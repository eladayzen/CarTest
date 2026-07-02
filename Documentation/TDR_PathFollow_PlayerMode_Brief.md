# TDR Asset — "Path-Follow Player Mode" — Initial Findings & Build Brief

## Goal
Add a new driving mode to the TDR (Unity car-racing asset) where the **car auto-drives
along the track path** (throttle + baseline steering handled automatically, like the AI),
but the **player retains limited lateral control** — able to nudge the car left/right within
a bounded offset from the path centerline. Think "on-rails" / lane-steer, not free driving.

## Key finding: this is NOT a built-in mode
I reviewed the TDR documentation (v1.0.001, 445 pp). There is no shipped game mode that
constrains player steering to a path. The stock modes are **Championship, Arcade, Time Trial**
(plus internal test Modes 3 / 5), and in all of them the player has full manual control.

Path-following logic exists — but it's wired to **AI cars only**. The good news: the pieces
we need already exist and can be repurposed.

## Relevant existing systems (find these in the project)

### Path / track
- **`Path.cs`** — attached to `Track_Path`. Manages the track path (checkpoint list) and
  contains the system to optimize the AI path.
  - Hierarchy: `GAMEPLAY → PATH → PathRef → Track_Path`
- **`PathRef.cs`** — global access point for path info. Used by both `VehicleAI.cs` and
  `VehiclePathFollow.cs`.
  - Hierarchy: `GAMEPLAY → PATH → PathRef`
- The track path is a series of **checkpoints** (demo track has 13). Each checkpoint has a
  direction ("Update Checkpoint Direction"). This is the centerline we constrain against.

### AI driving (the auto-drive source)
- **`VehicleAI.cs`** — makes an AI car autonomously follow the path (throttle + steering).
  On grippy surfaces (grip > 0.5) it follows the path well by itself.
- Start-of-race timing params live on **`LapCounterAndPosition`**
  (`GAMEPLAY → MANAGERS → LapCounterAndPosition`): `Override Start Part Duration`,
  `Override Start Part Speed` — controls the delay/speed before a car begins following the path.

### Player position on path (the key hook for lateral limits)
- **`VehiclePathFollow.cs`** — per docs, "gives info about the vehicle position on the path
  followed by the player **or AI**." This is the important one: it can report where the
  *player's* car sits relative to the path, which is exactly the signal needed to measure and
  clamp lateral offset.

### Possibly useful
- **Alt Path system** — `Grp_AltPath` contains an alternative path + a `PlayerTrigger`.
  The game already tracks whether a player is on the main path vs an alt path (used for
  respawn logic). Relevant if we ever want branching lanes.
- **`CarRespawnV`** — respawns the vehicle onto the path. Reference for how "snap to path" is done.
- Camera "Follow A Path" (Cam Style 03) is **camera-only** — ignore it, not related to control.

## Proposed implementation approach (starting point — please validate against actual code)
1. Create a new mode flag (mirror how the existing modes / `Current Game Mode` are set) so we
   can branch behavior at input/steering time.
2. **Auto-drive**: reuse the AI's path-following for throttle and baseline steering. Either
   attach a (possibly stripped-down) `VehicleAI`-style driver to the player car, or extract its
   path-steering routine so both AI and this mode share it.
3. **Player lateral input**: instead of feeding player steering directly to the wheels, treat it
   as a *lateral offset target* relative to the path centerline. Use `VehiclePathFollow` to read
   the current offset, and **clamp** the player-commanded offset to a configurable max (e.g. ±X
   meters, or ± a fraction of track width).
4. **Blend**: final steering = baseline path-steering (keeps car on the path) + clamped
   lateral correction (player nudge). Tune so the car always trends back toward the path when
   the player lets go.
5. Expose tuning params in the inspector: max lateral offset, lateral responsiveness /
   return-to-center strength, and whether throttle is auto or player-controlled.

## Open questions for the code exploration
- Does `VehicleAI.cs` expose its path-steering as a reusable method, or is it monolithic
  (needs refactor to share with the player)?
- What exactly does `VehiclePathFollow.cs` return — distance along path, signed lateral
  offset, nearest-checkpoint index? (Determines how directly we can clamp.)
- Where does player steering input enter the physics (which script applies torque/steer to the
  wheels)? That's the interception point for the offset-clamp.
- How is `Current Game Mode` read at runtime, and where's the cleanest place to branch?

## Environment note
Unity project; repos under `~/Particula/GoChess/` pattern — confirm actual TDR project path.
Docs source: `01_TDR_Documentation.pdf`.
