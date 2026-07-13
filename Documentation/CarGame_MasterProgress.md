# TDCars2 — Car Game Master Progress Doc

Scope: everything built so far on the car racing game itself (TDR asset). Written so another Claude
Code instance can pick up this specific thread without re-reading the whole conversation history.

## Project basics

- Unity project: `/Users/eladayzen/UnityProjects/TDCars2`, Unity 6000.0.78f1, URP.
- Git remote: `git@github.com:eladayzen/CarTest.git`, branch `main`.
- Based on the TDR ("Turbo Drift Racing"-style) third-party car-racing asset — stock modes are
  Championship, Arcade, Time Trial, plus internal Test Modes 3 and 5.
- Unity MCP (`ai-game-developer` server, via `.mcp.json` — gitignored, contains a secret token) is
  used throughout for direct Editor control (compile checks, prefab/scene edits, material creation,
  script-execute for one-off Editor operations) instead of hand-editing YAML.

## Feature 1: Path-Follow Control Assist

**Purpose:** an on-rails driving assist, not a race mode. The car auto-drives the track path
(throttle + steering reused from the AI), and the player nudges left/right within a lateral offset
from centerline. Reframed mid-project: since the assist makes it impossible to crash, the actual
player decision is *where* to position the car on the track (for future booster/barricade
placement by lane), not driving precision.

**Key file:** `Assets/TDR/Assets/Scripts/RC/Car/CarPathFollowPlayerInput.cs`, attached to
`CarRef.prefab` — confirmed the *only* vehicle prefab in the whole project (no variants), so no
coverage gaps.

**Architecture decisions (deliberate, confirmed with user):**
- Independent of `currentGameMode` — works in Arcade/TimeTrial/Championship/Test alike. A brand
  new game-mode value was considered and rejected: would've touched ~15-20 menu/UI files (car
  selection, track slideshow, results screens) unrelated to the mechanic itself.
- `CarState.CarPlayerType` stays `Human`/`AI` only, no third enum value — gated instead by a new
  `CarState.isPathFollowAssistEnabled` bool. This keeps `CarAI`'s own automatic driving loop
  (`IsPlayerAI()`-gated) fully inert on the assisted car without touching it.
- Every tuning knob is applied **per-vehicle-instance** at init (`InitRoutine()`), only ever
  mutating this car's own `CarController`/`CarAI` component instances — never the shared prefab
  default or anything an AI-driven car would read. This was verified end-to-end: AI cars are
  provably unaffected by any of this feature's tuning.

**Supporting access-modifier changes (zero behavior change on their own):**
- `CarAI.cs`: `DesiredSteer()`, `DesiredAcceleration()`, `CloseFromPathObstaclePosition()` made
  `public` (were private/package-default).
- `CarController.cs`: `speedRotationRef` made `public` (was private) — this is the real physical
  turn-rate base value, see tuning notes below.
- `CarPlayerInputs.cs`: `isInputsAllowed()` gated on `&& !carState.isPathFollowAssistEnabled`
  (also disables drift input, consistent with "on-rails, not free driving");
  `ReturnPlayerSteerRatio()` made `public`.
- `CarState.cs`: added `public bool isPathFollowAssistEnabled = false;`.

**Config knobs on `CarPathFollowPlayerInput`** (see the prefab for live current values — these
drift as playtesting continues, treat numbers below as a snapshot only):
- `holdOffsetOnRelease` (Config B, default `true`): lateral offset holds exactly where the player
  left it — no auto-recenter, lane choice persists through corners. Config A (`false`) drifts back
  to centerline at `centeringSpeed`.
- `turnRateMultiplier` (default `1.6`, started `1.3`): multiplies `CarController.speedRotationRef`
  once at init. This is a **real** physical turn-rate ceiling increase — important distinction:
  raising `CarAI.aiSteerSens` was tried and explicitly ruled out first, because the `[-1,1]` steer
  clamp already maps linearly to the car's actual max turn-rate (`CarController.DeltaRotation()`),
  not a soft/tunable threshold. There's no headroom to unlock via sensitivity; you have to raise
  the actual ceiling, which is what this does.
- `maxSpeedMultiplier` (default `0.6`, started `0.85`): lowers top speed per-instance to buy more
  turn-rate budget on corners, at the cost of overall pace.
- `cornerBrakeStrength` (Config C, default `0.07`, was toggled fully off at one point): extra
  braking scaled by `corner-sharpness × held-offset-severity`, so a held lane is always physically
  reachable even on a hard corner instead of bleeding back toward centerline. Root cause this
  solves: `CarAI.CautiousNeededDependingOnCornerAngle()`'s corner-sharpness signal comes from the
  **centerline** tangent (`VehiclePathFollow`'s `target.rotation`), never recalculated for the
  current offset — so baseline braking under-brakes for an inside line's actually-tighter radius.
- `generalCornerBrakeStrength` (default `0.716`, added after cornerBrakeStrength alone still felt
  like "fighting the path" on corners even without nudging): extra braking scaled by corner
  sharpness *alone*, independent of offset. The two demands are combined via `Mathf.Max`, not
  summed.
- `minSpeedFloor` (default `6`, was `8`, tested at `4` which felt uncontrollable): braking from the
  two systems above never drops speed below this.
- `maxLateralOffset` (default `4`, was `3.5`): max nudge distance from centerline.
- `lateralInputSpeed` (default `6`, was `8`), `centeringSpeed` (default `3`, was `5`, only active
  in Config A).

**Bug found and fixed:** `CarAI.Update()` normally refreshes `closestObstaclePathPos` every 4
frames for obstacle-avoidance braking — that loop is gated behind `IsPlayerAI()` and never runs for
the assisted (`Human`-flagged) car, so it stayed stuck at `0`. On `02_MautikiIsland` (which has a
configured `PathObstacle`), that stale index went out of range, throwing an
`ArgumentOutOfRangeException` every physics frame — which aborted `FixedUpdate()` mid-frame and
froze the car's throttle/steering at stale values (looked like "the game broke" during playtesting,
traced back to this, not a config mistake). Fixed by calling `CarAI.CloseFromPathObstaclePosition()`
ourselves every frame in `CarPathFollowPlayerInput.FixedUpdate()`.

**Known follow-ups / explicitly not done:**
- No player-facing UI toggle — enabled via an Inspector-only bool (`enablePathFollowMode`) on the
  prefab. A real in-game toggle (menu checkbox etc.) was explicitly deferred.
- Alt-path branching (the game's existing shortcut-route system, `AltPathPlayerTrigger` /
  `TriggerAltPath` in `VehiclePathFollow.cs`) is **not** accounted for by the assist's corner-brake
  math — main path only. Confirmed via scene grep that alt paths exist on `02_MautikiIsland.unity`
  / `02_MautikiIslandRevers.unity` (not on the plain `Demo.unity` scene) and are already
  player-selectable today purely by which physical trigger collider you drive through — that part
  is pre-existing base-asset behavior, unrelated to anything built this session.
- `maxLateralOffset` has never been validated against actual track width on any track (confirmed
  no width/lane data exists anywhere in `Path.cs` or companion classes) — a large offset could
  theoretically push the car off the drivable surface on a narrow or very tight section. Not yet
  hit in practice, just flagged as unverified.
- Tuning values above are mid-iteration, not final.

Detailed design-intent memory (auto-loaded in future sessions on this project):
`~/.claude/projects/-Users-eladayzen-UnityProjects-TDCars2/memory/path_follow_assist_tuning.md`

## Feature 2: Lane-Edge Glow Lines

**Purpose:** visualize the playable band the assist allows — two glowing lines at a configurable
distance either side of centerline, marking the border of the playable area. Centerline itself is
not drawn, only the two edges (explicit user decision).

**Key files:**
- `Assets/TDR/Assets/Scripts/TS/Track/Race/PathLaneGlowRenderer.cs` — new component. At `Start()`,
  waits for `PathRef.instance`/`.Track`, then samples the centerline every `sampleSpacing` units via
  `Track.TargetPositionOnPath(dist)`/`Track.TargetRotationOnPath(dist)` (tangent), computes
  `left = Vector3.Cross(tangent, Vector3.up).normalized` (same convention as the pre-existing
  gizmo-only code in `Path.cs:470-471`), and builds two `LineRenderer`s (left/right edge) offset by
  `laneHalfWidth`. Runtime-generated, not baked — `LineRenderer` was chosen over `TrailRenderer`
  (used by `CarSkidmark.cs` for decaying skid trails — wrong tool, that's for a moving point over
  time) and over a hand-rolled mesh strip (no precedent for that in the project).
- `Assets/TDR/Assets/Materials/PathLaneGlow.mat` — URP Lit, black base color, HDR cyan emission
  (`_EmissionColor` set via script to a >1 HDR intensity, `_EMISSION` keyword enabled) so Bloom
  picks it up — confirmed Bloom is active in
  `Assets/TDR/Assets/RP/URP/DefaultVolumeProfile.asset`. No existing "glow" material was reusable
  (checked 178 materials with emission fields, all effectively zeroed).

**Placement:** GameObject "LaneGlowRenderer" added under `GAMEPLAY → PATH` in
`02_MautikiIsland.unity` only (`laneHalfWidth = 4`, matching the assist's default
`maxLateralOffset`). Not yet added to other track scenes (`02_MautikiIslandRevers.unity`,
`Demo.unity`) — this is opt-in per scene, nothing wires it in automatically.

**Known limitation:** `laneHalfWidth` is a fully independent field from
`CarPathFollowPlayerInput.maxLateralOffset` — no automatic sync. If the car's clamp value changes,
update this manually too. Main path only, same as Feature 1 (no alt-path awareness).

## Feature 3: "Combat Run" Prototype Mode (TMNT-themed, in progress)

**Purpose:** a new game mode layered on the path-follow assist: AI cars become enemy target
vehicles with HP; the player destroys them primarily by holding contact (ram/grind damage), plus
time-limited themed ability pickups (planned). Player input stays steering left/right only — all
attacks auto-inflict. Full plan: `~/.claude/plans/purrfect-meandering-gray.md`.

**Design decisions locked with user:**
- Win condition = reach the finish line (existing lap/finish system untouched); kills are
  score/fun, not the end condition.
- Player is invulnerable in v1 (no player HP).
- CarAI driving/avoidance logic untouched — user confirmed car contact already feels fine.
- Prototype wiring only: scene GameObject + Inspector flag, no main-menu/`currentGameMode`
  integration.
- **Two themes planned for two builds** — "General" and "Teenage Mutant Ninja Turtles", TMNT
  first. Abilities are theme content (Raphael pickup → rear icon + auto-fired explosive daggers;
  Donatello → electro attacks), all auto-targeting by proximity. Placeholder art only for now.

**Architecture (key insight: zero modifications to existing scripts, zero prefab edits):**
- `CombatRunManager` on a `CombatRunMode` GameObject under `--> GAMEPLAY <--` in
  `02_MautikiIsland.unity` (same opt-in-per-scene pattern as LaneGlowRenderer). Its init
  coroutine waits for `VehiclesRef.b_InitDone` + the last car's `carPlayerType` flipping to `AI`
  (that assignment happens post-countdown in `AssistantModesArcadeRC.bStep4`), then
  `AddComponent`s combat components onto the spawned cars at runtime. Deleting the GameObject
  removes the mode entirely.
- Damage is structural: a *dealer* component only on the player's car, a *health* component only
  on enemies — so enemy-vs-enemy contact does nothing and nothing can damage the player.
- Deliberately NOT reusing `VehicleDamage.lifePoints`: its `VehicleExplosionAction` semantically
  means "respawn to checkpoint" (CarRespawnV subscribes), its damage methods are empty stubs.
- Death = `SetActive(false)` (disable-in-place), never `Destroy`: `CarAI.NextCar()` already
  skips inactive cars (activeSelf checks), while `LapCounterAndPosition` reads every vehicle
  transform with no null check and would NRE on a destroyed one.

**Full Phase C/D/E design (theme/ability framework, wave spawning, UI): `Documentation/CombatRun_Plan.md`**
(in-repo copy of the approved plan — a fresh session should read it before building Phase C).

**Built so far (Phases A + B complete — compiled clean, core loop playtested by user):**
- `Assets/TDR/Assets/Scripts/RC/Car/Combat/CombatRunManager.cs` — bootstrap + tuning knobs
  (`enemyMaxHP` 100, `enemySpeedMultiplier` 0.6 applied per-instance to
  `CarController.maxSpeed/refMaxSpeed` AND `CarAI.maxSpeedRef` so all three speed caches stay
  consistent, `ramDamagePerSecond` 20, impact burst scale/cap). Also contains
  `TeleportEnemyOnPath()` (replicates `CarController.VehicleOutOfLimitZoneRoutine`'s placement:
  progressDistance + zeroed lateral offsets + `PositionOnPath` + ground raycast on `RespawnLayer`
  + zeroed rb velocity + **writes `LapCounterAndPosition.posList[..].lastPathDistance`** — the
  position tracker only searches ±100m around the last known distance) and the Phase-A de-risk
  spike (`[ContextMenu] Debug Spike: Recycle First Enemy` — disable → teleport +80m ahead of
  player → re-enable). Spike NOT yet confirmed by user playtest.
- `Combat/EnemyVehicleHealth.cs` — float HP, single `ApplyDamage(amount, CombatDamageSource)`
  entry point all damage funnels through, one-shot `isDead` guard, throttled HP logging.
- `Combat/CombatRamDamageDealer.cs` — player-only; `OnCollisionStay` DPS
  (`ramDPS × damageMultiplier × fixedDeltaTime`, DPS-primary so grinding beats bouncing) +
  capped `OnCollisionEnter` burst; `damageMultiplier` is the future Ram Frenzy ability hook.
  Enemy resolution via `collision.rigidbody?.GetComponent<EnemyVehicleHealth>()`.
- `Combat/EnemyHealthBar.cs` — worldspace bar 2.6m above each enemy's roof, billboards to
  `Camera.main`, polls HP in LateUpdate, green→red color lerp. Fully code-built canvas, no
  scene/prefab UI edits.
- `Combat/CombatExplosionFx.cs` — code-built one-shot death explosion (no prefab/assets): fire
  burst + smoke puffs on procedural soft-circle texture via `Sprites/Default` (URP-safe), HDR
  emissive flash sphere (Bloom picks it up) + fading point light; spawned detached in
  `NotifyEnemyDestroyed` right before `SetActive(false)`; self-destroys after 3s.
- Confirmed working in user playtest: ram damage ticks HP down, kill flow fires, death
  disables the car, HP bars visible.
- **The "many errors" from playtesting were triaged and fixed** — all were load-order races
  exposed because the assist starts driving earlier than AI cars ever did, hitting latent
  unguarded code in the stock asset. Three MINIMAL existing-file changes now exist (the
  "zero modifications" claim above no longer holds, but all three are pure safety guards):
  - `CarAI.DesiredAccelerationDependingObstacleOnTrack`: bounds-check `selectedId`/
    `closestObstaclePathPos` before indexing `dangerListByPath` (empty-on-load + alt-path).
  - `DetectCarAhead.IsCollisionDetected`: skip cars whose `VehiclePathFollow`/`Track` are
    still null during init.
  - `CarPathFollowPlayerInput.InitRoutine`: also waits for `VehiclesRef.b_InitDone` (whole
    fleet) before driving; obstacle-index refresh guarded per-path.
- ~~"Test Minigame" buttons added to `02_MautikiIsland`~~ — the WebView minigame experiments
  (and their test buttons/scenes/package dependency) were removed on 2026-07-12; the project
  is now focused solely on the car game.

**Phase D (wave respawn) done** (commit `1113c8c`): `CombatRunManager.WaveRespawnRoutine` counts
engaged enemies in `[player−engagementBehind, player+engagementAhead]` every ~2s; below
`minEngagedEnemies`, recycles a dead-or-farthest enemy to `player+spawnAheadDistance` via
`TeleportEnemyOnPath` (respawn-delay + recycle-cooldown gated). `CombatPortalFx.cs` plays a
code-built cyan portal flash at the arrival point. **The Phase-A teleport/recycle debug spike
was never explicitly confirmed playtested on its own** — if wave respawn ever looks flaky,
check that first.

**Experimental Chase Assist added** (commit `21ccc4c`, opt-in via `enableChaseAssist`, off by
default): rubber-bands only the single nearest enemy ahead of the player (temporarily lowers
just that car's `CarAI.maxSpeedRef`) so the player can close the gap and hold a ram, without
touching any other car. Hysteresis via `chaseTargetSwitchMargin` avoids target flicker.

**Phase C framework landed 2026-07-12** (commit `123c2f4`): `AbilityDefinition` +
`CombatThemeDefinition` ScriptableObjects (`Assets > Create > Combat Run` menu),
`CarAbilityController` (player-only, structural typing like the ram dealer, one buff active at
a time, pause-aware duration, worldspace billboard rear icon), `CombatPickup` (OvertakeTrigger-
pattern trigger + pause-aware respawn cooldown), `CombatPickupSpawner` (`TS/Track/Race/`,
path-sampling scatter, builds pickup visuals procedurally at runtime — no prefab/material assets
needed). Only **RamFrenzy** (×3 damage multiplier on `CombatRamDamageDealer`) is wired
end-to-end; `ProjectileDagger`/`ElectroZap` exist in the `CombatAttackType` enum but log a clear
"not implemented" warning if picked — Raphael daggers / Donatello electro still need building.

**Playtested and one real bug found + fixed** (commit `8bd3d3f`): user reported enemies dying
almost instantly and no visible feedback on pickup collection. Root cause from the console log:
`CarAbilityController.Activate()` touched `iconRoot.gameObject` *before* starting the
gameplay-critical `BuffRoutine` coroutine; when `iconRoot` was stale the resulting
`MissingReferenceException` aborted the method early, which (a) left the RamFrenzy ×3 damage
multiplier permanently stuck on since its revert-coroutine never got scheduled, and (b) — since
the exception happened inside `CombatPickup.OnTriggerEnter`'s call to `Activate()` — prevented
the following `StartCoroutine(RespawnRoutine())` from ever running, so pickups never hid or went
on cooldown. Fixed: gameplay state now applies unconditionally before any icon UI touch,
`iconRoot` lazily rebuilds if stale, and `CombatPickup` hides/cooldowns before calling
`Activate()` so pickup-vanish feedback can't be blocked by ability logic. Added
activation/expiry/collection logs. **Not yet re-confirmed by a follow-up playtest** — verify the
fix actually resolves both symptoms before trusting Ram Frenzy is done.

**In-Editor wiring for Phase C done** (commit `be74dbb`): `Assets/TDR/Assets/Datas/CombatRun/`
has `Ability_RamFrenzy.asset` (×3 damage multiplier, 8s duration) and `Theme_TMNT.asset`
(references it); `CombatRunManager.theme` assigned, `CombatPickupSpawner` added to the
`CombatRunMode` GameObject in `02_MautikiIsland`. **Not yet confirmed by an actual playtest** —
drive the track, run over a spawned pickup, confirm ram damage triples and the rear icon shows
for ~8s before doing anything else with Phase C.

**Still to do:**
1. Playtest the Ram Frenzy pickup loop (see above) before building further on top of it.
2. Implement `ProjectileDagger` (Raphael) and `ElectroZap` (Donatello) attack execution in
   `CarAbilityController.ApplyEffect` + a new `AbilityProjectile.cs` for the dagger.
3. Phase E: kill-counter UI, tuning pass. All per `Documentation/CombatRun_Plan.md`.

**Committed & pushed** through `be74dbb` (Phase C in-Editor wiring). Earlier: Phase D + portal FX +
enemy speed multiplier in `1113c8c`, Chase Assist in `21ccc4c`, original combat run in `7a5a217`,
minigame buttons + crash guards in `b47ac79`/`f7f5c01`. Working tree is clean as of this update —
run `git status` to confirm before assuming anything is uncommitted.

## Git history (this thread's commits)

- `76101a6` Initial commit: TDCars2 Unity project
- `153e99c` Add path-follow control assist for player vehicles (V1: new component, prefab wiring)
- `9336cf6` Checkpoint: hold-offset lane assist + corner braking (Config B/C landed, obstacle-index
  crash fixed, `minSpeedFloor` tuning)
- `fd142a5` Add per-instance turn-rate boost for path-follow assist
- `36b7e02` Add max-speed reduction, general corner slowdown, and manual retune
- `7205137` Add lane-edge glow lines and master progress doc (Feature 2 landed here)
- `7a5a217` Combat Run Feature 3: Phases A+B (enemy HP, ram damage, death sequence)
- `b47ac79`/`f7f5c01` Minigame buttons + load-order crash guards (minigame work since removed)
- `1113c8c` Combat Run Phase D: wave respawn + portal FX, enemy speed multiplier bump
- `21ccc4c` Combat Run: experimental Chase Assist
- `ccfa034`/`0cc3be0`/`d008983` Strip project to car-game-only content (removed WebView
  minigames, stock Unity template boilerplate, TDR asset pack's bundled demo/tutorial content)
- `123c2f4`/`e0fd82a` Combat Run Phase C framework (pickups + ability buffs, Ram Frenzy wired)
- `5774763` Add unity-mcp-reconnect skill (Cloud-mode connection troubleshooting)
- `be74dbb` Phase C in-Editor wiring: Ram Frenzy asset + TMNT theme + spawner attached
- `123c2f4` Combat Run Phase C framework: pickups + ability buffs (Ram Frenzy wired)
- Run `git status`/`git log` to confirm nothing has drifted since this was last updated.

## Environment / working notes for whoever picks this up

- **Unity MCP (`ai-game-developer`) is flaky in this environment** — it repeatedly needed
  re-authorization mid-session. If tools report "requires re-authorization" or vanish from the
  tool list: (1) re-auth via the in-Editor window — **Window → AI Game Developer — MCP** (⌘⌥A on
  Mac) — (2) then the *running* Claude Code process needs to be relaunched
  (`claude --resume <session-id>` from the project directory) since a live session won't pick up a
  reconnected server on its own. Also check `.claude/settings.local.json` for
  `enabledMcpjsonServers`/`disabledMcpjsonServers` both listing `ai-game-developer` at once — that
  contradictory state silently blocks the connection regardless of auth state, and it happened once
  already this session.
- Only vehicle prefab in the project: `Assets/TDR/Assets/Prefabs/AS/Vehicle/Prefabs/Vehicle/CarRef.prefab`.
- Main test track: `Assets/TDR/Assets/Scenes/Tracks/Demo/02_MautikiIsland.unity` (has a configured
  `PathObstacle` and alt path — the plain `Demo.unity` scene has neither, don't use it for testing
  obstacle/alt-path-adjacent behavior).
- The user has twice accidentally modified terrain data (`Assets/TDR/Assets/Terrain/Terrain_Track/*.asset`)
  while clicking around the Scene view — check `git status` for stray `TerrainData_*.asset` diffs
  before assuming everything unexpected is intentional; these have been reverted via
  `git restore` each time so far, not committed.
- Original design brief that kicked this all off: `Documentation/TDR_PathFollow_PlayerMode_Brief.md`.
