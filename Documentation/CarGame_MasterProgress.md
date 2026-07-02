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

## Git history (this thread's commits)

- `76101a6` Initial commit: TDCars2 Unity project
- `153e99c` Add path-follow control assist for player vehicles (V1: new component, prefab wiring)
- `9336cf6` Checkpoint: hold-offset lane assist + corner braking (Config B/C landed, obstacle-index
  crash fixed, `minSpeedFloor` tuning)
- `fd142a5` Add per-instance turn-rate boost for path-follow assist
- `36b7e02` Add max-speed reduction, general corner slowdown, and manual retune
- **Lane-Edge Glow Lines (Feature 2) — not committed yet as of this doc.** Run `git status` to see
  current uncommitted state before assuming anything is saved.

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
