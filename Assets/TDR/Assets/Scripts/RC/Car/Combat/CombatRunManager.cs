// Description: CombatRunManager. Scene-scoped bootstrap for the "Combat Run" prototype mode:
// AI cars become enemy target vehicles with HP that the player destroys by ram contact (and,
// later phases, themed ability pickups). Attached to a dedicated GameObject in the track scene
// (like LaneGlowRenderer) - deleting that GameObject removes the mode entirely. All combat
// components are attached at runtime after the normal race init, so no existing script or
// prefab is modified. Per-instance tuning only - never mutates shared prefab defaults.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CombatRunManager : MonoBehaviour
    {
        public static CombatRunManager instance = null;

        [Header("Manual toggle (no menu integration) - read once at Start")]
        public bool                  enableCombatRun = true;

        [Header("Enemy Vehicles")]
        public float                 enemyMaxHP = 100f;
        // Per-instance top-speed scale for enemies so the assisted player (itself slowed to
        // ~0.6 max speed) can actually catch and hold contact with them. Applied once at init
        // to each enemy's own CarController/CarAI instances only. 1 = no change.
        public float                 enemySpeedMultiplier = 0.99f;

        [Header("Ram Damage")]
        public float                 ramDamagePerSecond = 20f;
        // OnCollisionEnter burst: relativeVelocity * scale, capped, so hard hits feel chunkier
        // than a light graze but bouncing off enemies never beats holding contact (DPS-primary).
        public float                 impactBurstScale = 0.5f;
        public float                 impactBurstCap = 15f;

        [Header("Wave Respawn (keep enemies near the player)")]
        // How often the engagement count gets checked - the main "tempo" knob.
        public float                 waveCheckInterval = 2f;
        // Distance ahead of the player (along the path) where recycled enemies reappear.
        public float                 spawnAheadDistance = 80f;
        // Engagement window relative to the player's path position: enemies inside
        // [player - engagementBehind, player + engagementAhead] count as "engaged".
        public float                 engagementBehind = 30f;
        public float                 engagementAhead = 120f;
        // Below this many engaged enemies, recycle enemies in - the density target. Out of the
        // fixed pool size (11 AI cars by default), so leave headroom for dead/cooldown enemies.
        public int                   minEngagedEnemies = 5;
        // A destroyed enemy stays gone at least this long before it can come back.
        public float                 respawnDelaySeconds = 5f;
        // When below minEngagedEnemies, recycles repeat within the same check (a "catch-up
        // burst") instead of one-per-check - this is what makes "almost always N nearby" actually
        // reachable after a kill streak instead of trickling back one every few seconds. This
        // just paces individual recycles within a burst so their portal FX don't all pop in on
        // the same frame - it does not throttle how many happen overall.
        public float                 recycleStaggerSeconds = 0.4f;

        [Header("Initial Clustering (start Combat Run with enemies near the player instead of the base game's wide starting grid)")]
        public bool                  initialClusterEnabled = true;
        // Path-distance ahead of the player where the first clustered enemy lands.
        public float                 initialClusterStartOffset = 20f;
        // Spacing between each subsequent clustered enemy so they don't overlap.
        public float                 initialClusterSpacing = 15f;

        [Header("Abilities (Phase C)")]
        // Single swap point for a different theme later (e.g. General vs TMNT); prototype just
        // assigns the TMNT asset here. CombatPickupSpawner reads this to know what to scatter.
        public CombatThemeDefinition  theme;
        // CombatBuffHUD is added via AddComponent at runtime (never through a prefab Inspector),
        // so its prefab reference and per-category idle icon (greyed out, shown before that
        // category's first pickup) are set here instead.
        public GameObject             buffHudPrefab;
        public Sprite                 defaultAttackIcon;
        public Sprite                 defaultSpeedIcon;

        [Header("Chase Assist (experimental - rubber-band the single nearest car ahead)")]
        public bool                  enableChaseAssist = false;
        // Target car's speed while it's "the chase target," relative to its own normal
        // (post-enemySpeedMultiplier) speed.
        public float                 chaseSlowdownMultiplier = 0.85f;
        // Only slow the nearest-ahead car if it's within this path-distance - a car essentially
        // a lap away shouldn't get artificially slowed for no visible reason.
        public float                 chaseAssistMaxRange = 60f;
        // Hysteresis: don't drop the current chase target for a new candidate unless the new
        // one is at least this much closer, so two nearly-tied cars don't flicker as the target.
        public float                 chaseTargetSwitchMargin = 5f;

        [HideInInspector] public List<EnemyVehicleHealth> activeEnemies = new List<EnemyVehicleHealth>();
        [HideInInspector] public int killCount = 0;

        VehiclePathFollow            playerPathFollow;
        CombatRamDamageDealer        playerRamDealer;
        EnemyVehicleHealth           currentChaseTarget = null;
        bool                         isReady = false;

        void Awake()
        {
            #region
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            #endregion
        }

        void Start()
        {
            #region
            if (enableCombatRun)
                StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            // Wait for the normal race spawn to fully finish.
            yield return new WaitUntil(() =>
                VehiclesRef.instance != null &&
                VehiclesRef.instance.b_InitDone &&
                InfoRememberMainMenuSelection.instance != null &&
                LapCounterAndPosition.instance != null);

            int howManyPlayer =
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            List<VehicleInfo> vehicles = VehiclesRef.instance.listVehicles;

            // Combat Run needs at least one AI slot to turn into an enemy.
            if (vehicles.Count <= howManyPlayer)
            {
                Debug.Log("[CombatRun] No AI vehicles in this race - mode inactive.");
                yield break;
            }

            // carPlayerType is only assigned in AssistantModesArcadeRC.bStep4 (post-countdown):
            // index < HowManyPlayer -> Human, rest -> AI. The last slot flipping to AI is the
            // signal that assignment has happened (default prefab value is Human).
            CarState lastCarState = vehicles[vehicles.Count - 1].GetComponent<CarState>();
            yield return new WaitUntil(() =>
                lastCarState.carPlayerType == CarState.CarPlayerType.AI);

            for (int i = 0; i < vehicles.Count; i++)
            {
                CarState carState = vehicles[i].GetComponent<CarState>();

                if (carState.carPlayerType == CarState.CarPlayerType.AI)
                {
                    EnemyVehicleHealth health = vehicles[i].gameObject.AddComponent<EnemyVehicleHealth>();
                    health.InitCombat(this, enemyMaxHP);
                    activeEnemies.Add(health);

                    vehicles[i].gameObject.AddComponent<EnemyHealthBar>().InitBar(health);

                    ApplyEnemySpeedMultiplier(vehicles[i].gameObject, health);
                }
                else
                {
                    playerPathFollow = vehicles[i].GetComponent<VehiclePathFollow>();
                    playerRamDealer = vehicles[i].gameObject.AddComponent<CombatRamDamageDealer>();
                    playerRamDealer.InitCombat(this);

                    CarAbilityController abilityController =
                        vehicles[i].gameObject.AddComponent<CarAbilityController>();
                    abilityController.InitCombat(this);

                    CombatBuffHUD hud = vehicles[i].gameObject.AddComponent<CombatBuffHUD>();
                    hud.hudPrefab = buffHudPrefab;
                    hud.defaultAttackIcon = defaultAttackIcon;
                    hud.defaultSpeedIcon = defaultSpeedIcon;
                    hud.InitCombat(this, abilityController);
                }
            }

            if (initialClusterEnabled)
                ClusterEnemiesNearPlayer();

            isReady = true;
            Debug.Log("[CombatRun] Active - enemies: " + activeEnemies.Count);

            StartCoroutine(WaveRespawnRoutine());
            StartCoroutine(ChaseAssistRoutine());
            #endregion
        }

        // Places the whole enemy pool just ahead of the player in a spread column instead of
        // wherever the base game's StartLine grid math put them (StartLine is shared with every
        // non-Combat-Run race on this track, so this reuses TeleportEnemyOnPath - the exact
        // mechanism WaveRespawnRoutine already trusts for mid-race repositioning - rather than
        // touching that shared system).
        void ClusterEnemiesNearPlayer()
        {
            #region
            if (playerPathFollow == null || playerPathFollow.Track == null) return;

            float dist = PlayerPathDistance() + initialClusterStartOffset;
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                EnemyVehicleHealth enemy = activeEnemies[i];
                if (enemy == null) continue;

                TeleportEnemyOnPath(enemy, dist);
                dist += initialClusterSpacing;
            }
            #endregion
        }

        // ---------------------------------------------------------------------------------
        // Wave respawn: every waveCheckInterval seconds, count enemies engaged around the
        // player; if too few, recycle enemies in (dead first, else farthest outside the window)
        // to spawnAheadDistance in front of the player. Below target, this recycles repeatedly
        // within the same check - a "catch-up burst," paced by recycleStaggerSeconds rather than
        // gated to one-per-check - so a big deficit (e.g. after several kills in a row) fills
        // back in over a couple seconds instead of trickling back one every several seconds.
        // ---------------------------------------------------------------------------------
        IEnumerator WaveRespawnRoutine()
        {
            #region
            while (true)
            {
                float t = 0f;
                while (t < waveCheckInterval)
                {
                    if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                    yield return null;
                }

                if (playerPathFollow == null || playerPathFollow.Track == null) continue;

                while (true)
                {
                    int engaged = EvaluateEngagement(out EnemyVehicleHealth bestCandidate);
                    if (engaged >= minEngagedEnemies || bestCandidate == null) break;

                    TeleportEnemyOnPath(bestCandidate, PlayerPathDistance() + spawnAheadDistance);
                    bestCandidate.ResetCombatHealth();
                    bestCandidate.gameObject.SetActive(true);
                    Debug.Log("[CombatRun] Wave respawn: " + bestCandidate.name
                        + " recycled ahead of player (engaged was " + engaged + ")");

                    float staggerT = 0f;
                    while (staggerT < recycleStaggerSeconds)
                    {
                        if (!PauseManager.instance.Bool_IsGamePaused) staggerT += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            #endregion
        }

        // Counts enemies inside the engagement window around the player and finds the single
        // best recycle candidate (dead-and-past-respawn-delay beats any alive-but-far car).
        // Pulled out of WaveRespawnRoutine so the catch-up burst above can call it repeatedly
        // without duplicating the wrap-aware gap math.
        int EvaluateEngagement(out EnemyVehicleHealth bestCandidate)
        {
            #region
            float pathLength = playerPathFollow.Track.pathLength;
            float playerDist = playerPathFollow.progressDistance;

            int engaged = 0;
            bestCandidate = null;
            float bestCandidateScore = -1f;

            foreach (EnemyVehicleHealth enemy in activeEnemies)
            {
                if (enemy == null) continue;

                if (enemy.isDead || !enemy.gameObject.activeSelf)
                {
                    // Dead: candidate once its respawn delay has passed. Dead beats any
                    // alive-but-far candidate (score above the wrap-gap maximum).
                    if (Time.time - enemy.deathTime >= respawnDelaySeconds &&
                        bestCandidateScore < pathLength + 1f)
                    {
                        bestCandidate = enemy;
                        bestCandidateScore = pathLength + 1f;
                    }
                    continue;
                }

                // Wrapped gap in [0, pathLength): 0..engagementAhead = ahead in window,
                // pathLength-engagementBehind..pathLength = behind in window. Works whether
                // progressDistance wraps at pathLength or accumulates.
                VehiclePathFollow vpf = enemy.GetComponent<VehiclePathFollow>();
                float gap = ((vpf.progressDistance - playerDist) % pathLength + pathLength) % pathLength;
                bool inWindow = gap <= engagementAhead || gap >= pathLength - engagementBehind;

                if (inWindow)
                {
                    engaged++;
                }
                else
                {
                    // Alive but out of range: score by how far behind the player it is.
                    float behindDistance = pathLength - gap;
                    if (behindDistance > bestCandidateScore)
                    {
                        bestCandidate = enemy;
                        bestCandidateScore = behindDistance;
                    }
                }
            }

            return engaged;
            #endregion
        }

        // Only ever touches this enemy's own component instances - same per-instance pattern as
        // CarPathFollowPlayerInput.InitRoutine. All three speed caches are scaled together so
        // CarAI's obstacle-slowdown restore logic (which writes carController.maxSpeed back from
        // its own refs) stays consistent. Also caches the resulting maxSpeedRef on the health
        // component as the "normal" baseline Chase Assist restores to.
        void ApplyEnemySpeedMultiplier(GameObject enemy, EnemyVehicleHealth health)
        {
            #region
            CarController carController = enemy.GetComponent<CarController>();
            CarAI carAI = enemy.GetComponent<CarAI>();

            if (enemySpeedMultiplier != 1f)
            {
                carController.maxSpeed *= enemySpeedMultiplier;
                carController.refMaxSpeed = carController.maxSpeed;
                if (carAI != null)
                    carAI.maxSpeedRef *= enemySpeedMultiplier;
            }

            if (carAI != null)
                health.baseMaxSpeedRef = carAI.maxSpeedRef;
            #endregion
        }

        // ---------------------------------------------------------------------------------
        // Chase Assist (experimental): rubber-band only the single nearest enemy car ahead of
        // the player, so the player can close the gap and hold a ram. Every other car on track
        // is unaffected. CarAI.cs continuously fights direct writes to CarController.maxSpeed
        // (TakeCareOfObstacles restores toward maxSpeedRef every FixedUpdate, UpdateAIState
        // force-resets it in the Overtaken state) - so this works the same way
        // ApplyEnemySpeedMultiplier does: temporarily lower carAI.maxSpeedRef on exactly the
        // targeted car, and restore it the moment that car stops being the target.
        // ---------------------------------------------------------------------------------
        IEnumerator ChaseAssistRoutine()
        {
            #region
            while (true)
            {
                float t = 0f;
                while (t < 0.25f)
                {
                    if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                    yield return null;
                }

                if (!enableChaseAssist)
                {
                    if (currentChaseTarget != null) ReleaseChaseTarget();
                    continue;
                }

                if (playerPathFollow == null || playerPathFollow.Track == null) continue;

                float pathLength = playerPathFollow.Track.pathLength;
                float playerDist = playerPathFollow.progressDistance;

                // Nearest ahead: same wrap-aware gap formula as WaveRespawnRoutine. Once the
                // player passes a car, its gap jumps from ~0 to ~pathLength, so it naturally
                // stops being "nearest" on its own - no special-case needed for that.
                EnemyVehicleHealth nearest = null;
                float nearestGap = float.MaxValue;

                foreach (EnemyVehicleHealth enemy in activeEnemies)
                {
                    if (enemy == null || enemy.isDead || !enemy.gameObject.activeSelf) continue;

                    VehiclePathFollow vpf = enemy.GetComponent<VehiclePathFollow>();
                    float gap = ((vpf.progressDistance - playerDist) % pathLength + pathLength) % pathLength;

                    if (gap < nearestGap)
                    {
                        nearestGap = gap;
                        nearest = enemy;
                    }
                }

                bool nearestInRange = nearest != null && nearestGap <= chaseAssistMaxRange;

                if (!nearestInRange)
                {
                    if (currentChaseTarget != null) ReleaseChaseTarget();
                    continue;
                }

                if (currentChaseTarget == null)
                {
                    SetChaseTarget(nearest);
                }
                else if (nearest == currentChaseTarget)
                {
                    // Re-apply every tick (not just on switch) so live-tuning
                    // chaseSlowdownMultiplier in the Inspector during Play Mode takes effect
                    // immediately on the currently-held target, same as the rest of this system.
                    SetChaseTarget(currentChaseTarget);
                }
                else
                {
                    VehiclePathFollow currentVpf = currentChaseTarget.GetComponent<VehiclePathFollow>();
                    float currentGap = ((currentVpf.progressDistance - playerDist) % pathLength + pathLength) % pathLength;

                    if (currentGap - nearestGap >= chaseTargetSwitchMargin)
                    {
                        ReleaseChaseTarget();
                        SetChaseTarget(nearest);
                    }
                }
            }
            #endregion
        }

        void SetChaseTarget(EnemyVehicleHealth target)
        {
            #region
            bool isNewTarget = target != currentChaseTarget;
            currentChaseTarget = target;
            CarAI carAI = target.GetComponent<CarAI>();
            if (carAI != null)
                carAI.maxSpeedRef = target.baseMaxSpeedRef * chaseSlowdownMultiplier;
            if (isNewTarget)
                Debug.Log("[CombatRun] Chase target: " + target.name);
            #endregion
        }

        void ReleaseChaseTarget()
        {
            #region
            if (currentChaseTarget != null)
            {
                CarAI carAI = currentChaseTarget.GetComponent<CarAI>();
                if (carAI != null)
                    carAI.maxSpeedRef = currentChaseTarget.baseMaxSpeedRef;
                Debug.Log("[CombatRun] Chase target released: " + currentChaseTarget.name);
            }
            currentChaseTarget = null;
            #endregion
        }

        // Disable-in-place, never Destroy: CarAI.NextCar() already skips inactive cars, while
        // LapCounterAndPosition reads every vehicle transform with no null check and would NRE
        // on a destroyed one. Explosion VFX deliberately deferred (user call) - death is just
        // the car vanishing for now.
        public void NotifyEnemyDestroyed(EnemyVehicleHealth enemy)
        {
            #region
            killCount++;
            Debug.Log("[CombatRun] Enemy destroyed (" + enemy.name + ") - kills: " + killCount);
            CombatExplosionFx.Spawn(enemy.transform.position + Vector3.up * 1f);

            // Restore immediately rather than waiting up to 0.25s for the next Chase Assist
            // tick - this enemy is about to be recycled by WaveRespawnRoutine and must not come
            // back still slowed.
            if (currentChaseTarget == enemy)
            {
                CarAI carAI = enemy.GetComponent<CarAI>();
                if (carAI != null)
                    carAI.maxSpeedRef = enemy.baseMaxSpeedRef;
                currentChaseTarget = null;
            }

            enemy.gameObject.SetActive(false);
            #endregion
        }

        // ---------------------------------------------------------------------------------
        // Teleport core - replicates the proven respawn placement of
        // CarController.VehicleOutOfLimitZoneRoutine: progressDistance + zeroed lateral offsets
        // + PositionOnPath placement + ground raycast on RespawnLayer + zeroed rb velocity.
        // Also refreshes LapCounterAndPosition.posList[..].lastPathDistance, because the
        // position tracker only searches +/-100m around the last known distance and would lose
        // a long-teleported car forever otherwise.
        // ---------------------------------------------------------------------------------
        public void TeleportEnemyOnPath(EnemyVehicleHealth enemy, float targetPathDistance)
        {
            #region
            VehiclePathFollow vpf = enemy.GetComponent<VehiclePathFollow>();
            CarController carController = enemy.GetComponent<CarController>();
            VehicleInfo vehicleInfo = enemy.GetComponent<VehicleInfo>();
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            Path track = vpf.Track;

            if (track == null) return;

            // Keep clear of the start line's own guard bands (CarController treats the first
            // ~30m specially during respawn) and wrap on looped tracks.
            if (track.TrackIsLooped)
            {
                targetPathDistance = (targetPathDistance % track.pathLength + track.pathLength) % track.pathLength;
                if (targetPathDistance < 30f)
                    targetPathDistance = 30f;
                if (targetPathDistance > track.pathLength - 30f)
                    targetPathDistance = track.pathLength - 30f;
            }
            else
            {
                targetPathDistance = Mathf.Clamp(targetPathDistance, 30f, track.pathLength - 30f);
            }

            vpf.progressDistance = targetPathDistance;
            vpf.offsetAIPos = Vector2.zero;
            vpf.currentTargetOffsetAIPos = Vector2.zero;

            Vector3 spawnPos = track.PositionOnPath(targetPathDistance, 0);
            Vector3 spawnLookAt = track.PositionOnPath(targetPathDistance + 3, 0);

            if (Physics.Raycast(spawnPos + 5 * Vector3.up, -Vector3.up, out RaycastHit hit, 100,
                carController.RespawnLayer))
            {
                float distance = Vector3.Distance(spawnPos, hit.point);
                Vector3 dir = (hit.point - spawnPos).normalized;
                spawnPos += distance * dir;
                spawnLookAt += distance * dir;
            }

            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;
            enemy.transform.LookAt(spawnLookAt);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Portal effect at the arrival point, facing across the track.
            CombatPortalFx.Spawn(enemy.transform.position + Vector3.up * 1.4f,
                enemy.transform.rotation);

            if (LapCounterAndPosition.instance != null &&
                vehicleInfo.playerNumber < LapCounterAndPosition.instance.posList.Count)
            {
                LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].lastPathDistance =
                    targetPathDistance;
            }
            #endregion
        }

        public float PlayerPathDistance()
        {
            #region
            return playerPathFollow != null ? playerPathFollow.progressDistance : 0f;
            #endregion
        }

        // -------------------------------------------------------------------------
        // Phase A de-risk spike: prove disable -> teleport ahead -> re-enable works
        // before the death/wave systems are built on top of it. Run from the
        // component context menu in play mode, or via MCP script-execute.
        // -------------------------------------------------------------------------
        [ContextMenu("Debug Spike: Recycle First Enemy")]
        public void DebugSpikeRecycleFirstEnemy()
        {
            #region
            if (!isReady || activeEnemies.Count == 0)
            {
                Debug.Log("[CombatRun] Spike: not ready or no enemies.");
                return;
            }
            StartCoroutine(SpikeRoutine(activeEnemies[0]));
            #endregion
        }

        IEnumerator SpikeRoutine(EnemyVehicleHealth enemy)
        {
            #region
            Debug.Log("[CombatRun] Spike: disabling " + enemy.name);
            enemy.gameObject.SetActive(false);

            float t = 0;
            while (t < 1f)
            {
                if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                yield return null;
            }

            float target = PlayerPathDistance() + spawnAheadDistance;
            TeleportEnemyOnPath(enemy, target);
            enemy.ResetCombatHealth();
            enemy.gameObject.SetActive(true);
            Debug.Log("[CombatRun] Spike: re-enabled " + enemy.name + " at path distance " + target);
            #endregion
        }
    }
}
