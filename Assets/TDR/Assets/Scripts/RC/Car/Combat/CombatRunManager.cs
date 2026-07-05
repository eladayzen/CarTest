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
        // Distance ahead of the player (along the path) where recycled enemies reappear.
        public float                 spawnAheadDistance = 80f;
        // Engagement window relative to the player's path position: enemies inside
        // [player - engagementBehind, player + engagementAhead] count as "engaged".
        public float                 engagementBehind = 30f;
        public float                 engagementAhead = 120f;
        // Below this many engaged enemies, one gets recycled ahead of the player.
        public int                   minEngagedEnemies = 2;
        // A destroyed enemy stays gone at least this long before it can come back.
        public float                 respawnDelaySeconds = 5f;
        // Minimum spacing between two consecutive recycles.
        public float                 recycleCooldownSeconds = 4f;

        [HideInInspector] public List<EnemyVehicleHealth> activeEnemies = new List<EnemyVehicleHealth>();
        [HideInInspector] public int killCount = 0;

        VehiclePathFollow            playerPathFollow;
        CombatRamDamageDealer        playerRamDealer;
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

                    ApplyEnemySpeedMultiplier(vehicles[i].gameObject);
                }
                else
                {
                    playerPathFollow = vehicles[i].GetComponent<VehiclePathFollow>();
                    playerRamDealer = vehicles[i].gameObject.AddComponent<CombatRamDamageDealer>();
                    playerRamDealer.InitCombat(this);
                }
            }

            isReady = true;
            Debug.Log("[CombatRun] Active - enemies: " + activeEnemies.Count);

            StartCoroutine(WaveRespawnRoutine());
            #endregion
        }

        // ---------------------------------------------------------------------------------
        // Wave respawn: every couple of seconds, count enemies engaged around the player;
        // if too few, recycle one (dead first, else the one farthest outside the window) to
        // spawnAheadDistance in front of the player - so the player is never alone for long.
        // ---------------------------------------------------------------------------------
        IEnumerator WaveRespawnRoutine()
        {
            #region
            float lastRecycleTime = -999f;

            while (true)
            {
                float t = 0f;
                while (t < 2f)
                {
                    if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                    yield return null;
                }

                if (playerPathFollow == null || playerPathFollow.Track == null) continue;
                if (Time.time - lastRecycleTime < recycleCooldownSeconds) continue;

                float pathLength = playerPathFollow.Track.pathLength;
                float playerDist = playerPathFollow.progressDistance;

                int engaged = 0;
                EnemyVehicleHealth bestCandidate = null;
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
                    // pathLength-engagementBehind..pathLength = behind in window. Works
                    // whether progressDistance wraps at pathLength or accumulates.
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

                if (engaged >= minEngagedEnemies || bestCandidate == null) continue;

                lastRecycleTime = Time.time;
                TeleportEnemyOnPath(bestCandidate, PlayerPathDistance() + spawnAheadDistance);
                bestCandidate.ResetCombatHealth();
                bestCandidate.gameObject.SetActive(true);
                Debug.Log("[CombatRun] Wave respawn: " + bestCandidate.name
                    + " recycled ahead of player (engaged was " + engaged + ")");
            }
            #endregion
        }

        // Only ever touches this enemy's own component instances - same per-instance pattern as
        // CarPathFollowPlayerInput.InitRoutine. All three speed caches are scaled together so
        // CarAI's obstacle-slowdown restore logic (which writes carController.maxSpeed back from
        // its own refs) stays consistent.
        void ApplyEnemySpeedMultiplier(GameObject enemy)
        {
            #region
            if (enemySpeedMultiplier == 1f) return;

            CarController carController = enemy.GetComponent<CarController>();
            CarAI carAI = enemy.GetComponent<CarAI>();

            carController.maxSpeed *= enemySpeedMultiplier;
            carController.refMaxSpeed = carController.maxSpeed;
            if (carAI != null)
                carAI.maxSpeedRef *= enemySpeedMultiplier;
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
