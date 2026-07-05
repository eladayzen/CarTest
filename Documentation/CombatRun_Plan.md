# "Combat Run" — New Prototype Game Mode (TMNT-themed first)

## Context

New game mode built on top of the existing path-follow assist (Feature 1): the player steers only left/right within the lane band while the car auto-drives. AI cars become **enemy target vehicles with HP** — the player destroys them by holding contact (ram/grind damage) and via **time-limited themed ability pickups** scattered along the path. All attacks auto-inflict; steering remains the only input.

Decisions locked with the user:
- **Win condition:** reach the finish line — existing lap/finish system untouched. Kills are score/fun, not the end condition.
- **Player is invulnerable** in v1 (no player HP).
- **CarAI driving stays exactly as-is** — no changes to steering, avoidance, or anti-collision forces. User confirmed car contact already feels fine in play. Damage is a pure detection layer.
- **Prototype wiring:** scene GameObject + Inspector flag (like LaneGlowRenderer), no main-menu/`currentGameMode` integration.
- **Two themes, two builds planned:** "General" and "Teenage Mutant Ninja Turtles" — TMNT built first. Abilities are theme content: e.g. **Raphael pickup** → his icon appears on the back of the car and the car auto-fires explosive daggers at nearby enemies for the duration; **Donatello** → hi-tech electro attacks. All attacks auto-target by proximity. (Prototype uses placeholder art — simple meshes/colors/icons; real licensed art is a later concern.)

## Architecture (key findings from exploration)

**Zero modifications to existing scripts, zero CarRef.prefab edits.** Everything the mode needs is already public, and the mode attaches its components at runtime:

- One manager GameObject in `02_MautikiIsland.unity` (under `GAMEPLAY`, like LaneGlowRenderer). Its init coroutine waits for `VehiclesRef.instance.b_InitDone` + `CarState.carPlayerType` assignment, then `AddComponent`s combat components onto the spawned cars. Deleting the GameObject removes the mode entirely.
- **Damage architecture:** a *dealer* component only on the player, a *health* component only on enemies. "Only player hurts enemies" falls out structurally (enemy-vs-enemy contact does nothing; nothing damages the player). Single damage entry point: `EnemyVehicleHealth.ApplyDamage(float, DamageSource)` — ram, daggers, electro all funnel through it.
- **Not reusing `VehicleDamage.lifePoints`:** its `VehicleExplosionAction` semantically means "respawn to checkpoint" (CarRespawnV subscribes), its damage methods are empty stubs. New float-HP component keeps the two "explosion" concepts un-conflated.
- **Death = `SetActive(false)`, never `Destroy`:** `CarAI.NextCar()` already skips inactive cars (`activeSelf` checks, CarAI.cs:1225,1256); `LapCounterAndPosition` reads transforms with no null check (line 379) so Destroy would NRE. Minimap dot freezing at death spot is acceptable v1 cosmetics. `CheckEndOfTheRace` only loops human players — dead enemies can't block results.
- **Waves = recycle the grid pool, no runtime Instantiate:** all race systems snapshot `VehiclesRef.listVehicles` at race start (CarAI.CreateCarListRoutine, LapCounter init, minimap). Dead/left-behind enemies are teleported ahead of the player, HP reset, re-enabled — replicating the proven teleport core of `CarController.VehicleOutOfLimitZoneRoutine` (lines 1621-1658): set `VehiclePathFollow.progressDistance`, zero offsets, position via `Track.PositionOnPath`, ground raycast via public `carController.RespawnLayer`, zero rb velocity. **Must also write `LapCounterAndPosition.posList[id].lastPathDistance`** — position tracker only searches ±100m around last known distance (line 374).
- **Ram damage model: DPS-primary** (`OnCollisionStay` on player root — same GameObject where `CarController.OnCollisionEnter` already fires — `ramDPS * damageMultiplier * Time.fixedDeltaTime` into any `EnemyVehicleHealth` on `collision.rigidbody`), plus a capped `OnCollisionEnter` impact burst. Grinding is the core loop; per-impact-only would reward bouncing.

## Theme & ability layer (supports 2 builds)

- **`CombatThemeDefinition` (ScriptableObject):** theme name + list of `AbilityDefinition` assets + pickup visual tint set. `CombatRunManager` references exactly one theme asset — that's the single swap point for the General-theme build later (per-build scene variant or a scripting define can select it; prototype just assigns the TMNT asset in the Inspector).
- **`AbilityDefinition` (ScriptableObject):** display name, icon sprite (shown on car rear), duration, plus auto-attack params: `attackType` enum {`ProjectileDagger`, `ElectroZap`, `RamFrenzy`}, range, fire interval, damage, AoE radius, projectile speed, color. Adding an ability = new asset (+ a case in the attack executor if it's a new attackType).
- **Auto-targeting:** while a buff is active, `CarAbilityController` ticks a fire-interval timer (pause-aware, `CarInvisibility.InvisibilityRoutine` pattern) and each shot targets the **nearest active enemy within range** from `CombatRunManager.activeEnemies`. No input, no aiming.
- **v1 TMNT abilities:**
  - **Raphael — Explosive Daggers:** auto-fires dagger projectiles (stretched primitive + red emissive material) at the nearest enemy; on hit/proximity, small AoE explosion (`ApplyDamage` with falloff in radius) + burst FX.
  - **Donatello — Electro Strike:** instant-hit electro zap (purple emissive LineRenderer beam from car to target, brief flash) at fire interval; optionally chains to a 2nd enemy in radius at reduced damage.
  - **Ram Frenzy (theme-neutral 3rd, optional):** ×3 `damageMultiplier` on the ram dealer for the duration — cheapest ability, good baseline test of the buff plumbing.
- **Rear icon:** while a buff is active, a small billboard quad above/on the car's rear shows the ability's icon sprite (placeholder colored emblem per turtle for the prototype). Lives on `CarAbilityController`, created at init, sprite+visibility swapped per buff.

## New files

All under `Assets/TDR/Assets/Scripts/` (project conventions: car scripts in `RC/Car/`, track-side in `TS/Track/Race/`):

| File | Role |
|---|---|
| `RC/Car/Combat/CombatRunManager.cs` | Scene singleton; `enableCombatRun` bool + tuning knobs + theme reference; attaches components post-spawn; tracks `activeEnemies`, kill counter; wave loop (Phase D); runtime overlay canvas for kill count. |
| `RC/Car/Combat/EnemyVehicleHealth.cs` | float `maxHP` (~100), `ApplyDamage`, one-shot `isDead` guard, death sequence, owns its health bar. |
| `RC/Car/Combat/CombatRamDamageDealer.cs` | Player-only. `OnCollisionStay` DPS + `OnCollisionEnter` capped burst. Public `damageMultiplier` (Ram Frenzy hook). |
| `RC/Car/Combat/CarAbilityController.cs` | Player-only. One active buff at a time (new pickup replaces current). Pause-aware duration + fire-interval loops; executes attackType cases; manages rear icon quad. |
| `RC/Car/Combat/AbilityDefinition.cs` | ScriptableObject as above. |
| `RC/Car/Combat/CombatThemeDefinition.cs` | ScriptableObject: theme = named set of abilities + visuals. |
| `RC/Car/Combat/AbilityProjectile.cs` | Dagger projectile: move toward target, explode (AoE ApplyDamage + FX) on proximity/hit, self-destroy. |
| `RC/Car/Combat/CombatPickup.cs` | Pickup prefab logic: slow rotation; `OnTriggerEnter` → `VehicleTriggerTag` check → walk `parent.parent.parent` to root (OvertakeTrigger.cs pattern) → Human-only → `Activate(definition)`; hide + ~20s pause-aware cooldown respawn (2-lap races need lap 2 to have pickups too). |
| `TS/Track/Race/CombatPickupSpawner.cs` | Waits for `PathRef.instance.Track` (PathLaneGlowRenderer pattern); scatters N pickups: sample path every `spacing`+jitter, lateral offset `Vector3.Cross(tangent, Vector3.up)` clamped inside the assist's ±4 lane band, ground raycast; cycles theme's abilities; tints per ability. |
| `RC/Car/Combat/EnemyHealthBar.cs` | Runtime worldspace canvas above roof, billboards to camera, shows after first damage. |
| `RC/Car/Combat/AutoDestroyFx.cs` | `Destroy(gameObject, lifetime)` FX helper. |

**Modified existing files: none** (first feature here needing zero access-modifier changes — note this in the progress doc).

**Assets authored via Unity MCP** (no hand-edited YAML): pickup glow materials (HDR-emission recipe proven by `PathLaneGlow.mat`; Bloom confirmed active), `CombatPickup.prefab` (trigger SphereCollider r≈1.5 + primitive mesh), `EnemyExplosionFX.prefab` (particle burst + light flash + AutoDestroyFx; debris later via `DestructiblePropsTag` template if wanted), dagger/electro FX materials, `CombatRunMode` GameObject in `02_MautikiIsland.unity`, TMNT theme + ability assets.

## Phases (each independently playable)

**Phase A — Enemy HP + ram damage + de-risk spike.** Manager (attach-only), `EnemyVehicleHealth` (HP + Debug.Log, no death), `CombatRamDamageDealer`. Include the **spike**: a debug method (ContextMenu / MCP script-execute) that disables an AI car, teleports it +80m ahead of the player, re-enables — proving the two riskiest mechanics before anything depends on them. Verify: clean compile via MCP console logs; grinding drains HP at ~ramDPS; AI-vs-AI contact logs nothing; spiked car resumes driving.

**Phase B — Death sequence.** Explosion FX prefab; death = guard → notify manager → spawn FX detached → `SetActive(false)` on vehicle root. Verify: kill an enemy → explosion, zero console errors over a full subsequent lap, race finishes normally with results screen.

**Phase C — Pickups + theme/ability framework.** Theme + ability SOs, `CarAbilityController` + rear icon, `CombatPickup` + spawner + materials, Raphael daggers + Donatello electro (+ Ram Frenzy). Verify: pickups reachable via nudge-only steering and bloom-glow along the lane; AI cars can't collect; each ability auto-targets and damages through `ApplyDamage`; pickups respawn; pause freezes buff timers.

**Phase D — Waves near the player.** Manager loop (~2s): count active enemies in engagement window `[player−30, player+120]` (wrap via `% pathLength` on looped tracks); below threshold → teleport a dead (or farthest) enemy to player+80m with HP reset, writing `posList.lastPathDistance`, avoiding ±30m of the start line (CarController's own guard bands, lines 1595-1607). Verify: kill everything nearby → reinforcements appear ahead and drive correctly; full race with no position/UI corruption across lap wrap.

**Phase E — UI + polish.** Health bars (show on first damage) + kill counter only. Tuning pass (ramDPS, enemy speed multiplier, ability numbers). Document in `Documentation/CarGame_MasterProgress.md`.

## Risks & mitigations

1. **Re-enabling a disabled car** (coroutines on it die at SetActive(false)) — front-loaded as the Phase A spike. Fallback: "ghost mode" death (keep active, hide body via CarInvisibility's bodyList approach, park kinematic underground, unhide to recycle).
2. **Teleport correctness** — posList ±100m window (write `lastPathDistance`), start-line guard band, looped-track wrap. Covered by spike + Phase D lap test.
3. **Engagement pacing** — assisted player runs at ~0.6 max speed; stock AI will outrun them. Mitigate: per-instance `enemySpeedMultiplier` (~0.55–0.6) at mode init (established per-instance pattern); if `DiffManager` rubber-banding fights it, zero its offsets on the scene instance (data, not code).
4. **Unity MCP flakiness** — known: re-auth via Window → AI Game Developer, relaunch CLI session, check `.claude/settings.local.json` for contradictory enable/disable entries.

## Key reference files

- `Assets/TDR/Assets/Scripts/RC/Car/CarController.cs` — collision entry points (1034/1066), teleport template (1532-1663), `RespawnLayer` (212)
- `Assets/TDR/Assets/Scripts/RC/Car/CarAI.cs` — `NextCar()` activeSelf tolerance (1218-1280), `CreateCarListRoutine` (221-272)
- `Assets/TDR/Assets/Scripts/TS/Manager/GameMechanics/LapCounterAndPosition.cs` — posList, ±100m window (374), unguarded transform read (379)
- `Assets/TDR/Assets/Scripts/TS/Vehicle/VehiclePathFollow.cs` — `progressDistance`, offset fields
- `Assets/TDR/Assets/Scripts/TS/Track/Race/PathLaneGlowRenderer.cs` — path sampling + lateral offset + HDR glow pattern
- `Assets/TDR/Assets/Scripts/RC/Car/CarInvisibility.cs` — pause-aware timed-effect coroutine pattern
- `Assets/TDR/Assets/Scripts/RC/Car/OvertakeTrigger.cs` / `OnTriggerFeedback.cs` — drive-through trigger pattern
