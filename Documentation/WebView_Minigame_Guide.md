# Embedding Web Minigames in Unity via WebView — Integration Guide

How we got an externally-built web game (Vite/ES-module build) running inside this Unity app
through `gree/unity-webview`, including every failure mode we hit and its fix. Written so a
developer can integrate the next web game without rediscovering any of this.

Working reference implementation: `Assets/TDR/Assets/Scripts/TS/Minigame/MinigameWebViewLoader.cs`
+ `Assets/StreamingAssets/MinigameTest/index.html` (scene: `MinigameWebViewTest.unity`, build
index 4).

---

## The three killer issues (each one alone makes the game silently dead)

### 1. ES modules do not load from `file://`

Modern web builds (`Vite`, anything emitting `<script type="module">`) are subject to CORS, and
a `file://` page has a "null" origin — the engine **silently refuses to execute the module**. No
error dialog, page HTML/CSS renders fine, but zero game code runs. Symptom we saw for a long
time: page visible, buttons present, `canvas=NONE` forever.

**Fix (implemented):** serve the game folder over a local HTTP server and load
`http://127.0.0.1:<port>/index.html`. `MinigameWebViewLoader.StartLocalServer()` is a ~50-line
`HttpListener` static file server over the StreamingAssets folder (ports 8090-8099, auto-picks).

- MIME types matter: module scripts are **refused outright** if `.js` isn't served as
  `application/javascript` (unlike classic scripts, which tolerate wrong types).
- Bonus: over HTTP, absolute asset paths (`/assets/...`) resolve correctly, so builds no longer
  need the relative-path (`base: './'`) patching that `file://` required.
- Android caveat: this server reads via `System.IO`, which cannot see inside an APK's
  StreamingAssets. Works in Editor / desktop / iOS; Android needs a different file-access route
  when we get there.

### 2. WKWebView suspends the page's heartbeat when it thinks it's invisible

In the Unity **Editor** on macOS, the webview overlay is treated as an occluded window, so
WKWebView applies background-tab policy: **`requestAnimationFrame` never fires (0 forever)** and
timers (`setInterval`/`setTimeout`) are throttled to ~1Hz. Event-driven JS (our `EvaluateJS`
calls, DOM event handlers) executes normally — which makes this maddening to diagnose: the page
answers everything you ask, yet nothing time-driven ever advances. Symptom: countdown frozen,
game loaded-but-inert.

**Fix (implemented):** Unity becomes the heartbeat.
- `index.html` `<head>` installs a **rAF shim before the game bundle loads**: it replaces
  `window.requestAnimationFrame` with a queue, and exposes `window.__pumpFrames()` that flushes
  the queue with a `performance.now()` timestamp.
- A native-rAF loop also drains the queue, so **in a real browser the page behaves exactly as
  before** (native rAF drives; the Unity pump simply doesn't exist there). Same files work in
  both environments.
- Unity's `Update()` calls `EvaluateJS("window.__pumpFrames && window.__pumpFrames();")`,
  **capped at 60Hz** — pumping every Unity frame ties game speed to the Editor's uncapped,
  variable framerate (fast-forward on a fast frame, slow-motion under load).

### 3. Real input never reaches the webview overlay

The webview is a **native OS overlay window on top of the Unity Game view**, not part of
Unity's rendering. In the Editor it never gets keyboard focus, and mouse clicks are unreliable.
(This is also why Unity screenshot / render-capture tools — including MCP capture — **cannot see
webview content at all**; only a human looking at the screen can verify visuals.)

**Fix (implemented):** forward input from Unity into the page as synthetic DOM events:
`Input.GetKeyDown/Up` → `EvaluateJS(new KeyboardEvent('keydown'/'keyup', {code, key}))` for
the gameplay keys (arrows, WASD, Space, Enter). Games listening to standard `keydown`/`keyup`
on `window` work unmodified. For mouse/touch-driven games, the same pattern with
`MouseEvent`/`PointerEvent` dispatch applies.

---

## Do's

- **DO serve over `http://127.0.0.1`, never `file://`.** Kills the module-CORS problem and the
  path-format problem in one move.
- **DO put the rAF shim + error hooks in `<head>` BEFORE the game bundle script tag.** Order
  matters: the game must see the shimmed `requestAnimationFrame` at load time.
- **DO bake error forwarding into the page** (we inject it; better: put it in the game's own
  source): `window.addEventListener('error'|'unhandledrejection')` →
  `window.Unity.call('JS ERROR: ...')`. The bridge is the ONLY way to see JS failures — there
  is no devtools console in this setup.
- **DO write game logic delta-time-based** (scale by elapsed ms between frames), never
  frame-count-based — the pumped frame rate is not guaranteed to be a steady 60.
- **DO tell the web-game team to build with `base: './'` in Vite config** anyway — costs
  nothing, keeps the build double-clickable and file://-tolerant for quick checks.
- **DO use the `window.__diag` counter pattern when anything is mysterious** (see Debugging
  below) — it definitively separates "code never ran" / "code ran but heartbeat frozen" /
  "everything runs, problem is elsewhere".
- **DO exit Play Mode before editing any C# script** — recompiling under a live native webview
  risks its state/handle.

## Don'ts

- **DON'T trust "the page renders" as evidence the game code runs.** Static HTML/CSS renders
  even when the module was silently refused. Only a live canvas / moving countdown / bridge
  message from game code proves execution.
- **DON'T rely on `requestAnimationFrame` or timers firing in the Editor webview.** They won't
  (rAF) or barely will (~1Hz timers). Anything time-driven must ride the Unity pump.
- **DON'T synthesize input blindly/unconditionally.** Our first "brute force" clicked Start AND
  Restart on every keypress — it restarted the game mid-run constantly and looked like "the
  game is broken". Check overlay/menu visibility in the page before firing synthetic events.
- **DON'T use Unity screenshot tooling to verify webview visuals** — the overlay is invisible
  to it. Human eyes only.
- **DON'T try to rewire `UnityEvent` listeners (menu buttons etc.) via MCP reflection tools** —
  UnityEvent internals aren't reachable. Use `UnityEditor.Events.UnityEventTools` via
  script-execute (see the "Test Minigame" buttons wiring).
- **DON'T forget the dependency key in `Packages/manifest.json` must equal the package's real
  name**: `net.gree.unity-webview` (not `com.gree...`). A mismatch fails package resolution,
  which cascades into project-wide compile failure — and with compile errors, NO script runs,
  which looks like "all input is broken everywhere".

## Debugging playbook (what actually cornered the bug)

1. `EvaluateJS` + `window.Unity.call` round-trips always work (event-driven) — use them as the
   probe channel even when the page seems dead.
2. Install counters at page load: `__diag = {t0, raf: 0, iv: 0, to: 0, moduleRan: 0}`, with a
   rAF loop / 250ms interval / 500ms timeout incrementing them, and an inline
   `<script type="module">__diag.moduleRan=1</script>` probe.
3. Poll from Unity every 2s and read the story:
   - `moduleRan=0` → modules blocked (issue 1).
   - `moduleRan=1, canvas exists, raf=0, iv` crawling → heartbeat suspended (issue 2).
   - Everything ticking but no visuals → repaint/overlay problem (we never hit this).
4. Keep an on-screen `OnGUI` log strip (leave a `SetMargins` gap for it) when iterating —
   faster than console archaeology. Strip it when done.

## Reimporting a new game build (until automated)

1. Copy new `dist/` contents over `Assets/StreamingAssets/MinigameTest/` (html + assets).
2. Re-add to `index.html` `<head>`, in this order, before the bundle script tag: error hooks,
   rAF shim, `__diag` counters (all in the current file — copy the whole block).
3. With HTTP serving, path rewriting is no longer needed.
4. Unity → assets refresh; run scene; console should show
   `[MinigameWebViewLoader] Serving minigame at http://127.0.0.1:809x/index.html`.
