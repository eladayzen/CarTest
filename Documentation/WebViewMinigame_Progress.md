# WebView Minigame Prototype — Progress

> **RESOLVED (2026-07-05).** The "Start doesn't visibly start the game" bug is fixed and the
> game now runs and plays inside the Editor webview. Root causes (all three were present at
> once): (1) ES-module game code silently blocked on `file://` — now served via a local HTTP
> server in the loader; (2) WKWebView suspends `requestAnimationFrame` entirely for the occluded
> Editor overlay — now driven by a rAF shim pumped from Unity's `Update()` at 60Hz; (3) keyboard
> input never reaches the overlay — now forwarded from Unity as synthetic DOM key events.
> Full write-up, do's & don'ts, and debugging playbook: `Documentation/WebView_Minigame_Guide.md`.
> The sections below are the original investigation notes, kept for history.

## Goal

Host a separately-built, non-Unity web minigame inside the Unity mobile app via an embedded
WebView, reachable from a new test-only button on the `01_MM` lobby menu. Longer-term motivation:
build games fast outside Unity, eventually feed them input from a custom Bluetooth gamepad SDK
(that Bluetooth bridging work is explicitly deferred, not started).

## What's built and working

- **Plugin**: `gree/unity-webview` installed via git-URL UPM package in `Packages/manifest.json`
  (`com.gree.unity-webview`). Namespace is `Gree.UnityWebView` (capital V), core API confirmed from
  actual source: `WebViewObject.Init(cb)`, `LoadURL`, `EvaluateJS`, `SetMargins`, `SetVisibility`.
  JS-to-C# bridge is `window.Unity.call(msg)`, arriving in the `Init` callback.
- **Loader script**: `Assets/TDR/Assets/Scripts/TS/Minigame/MinigameWebViewLoader.cs` — creates the
  `WebViewObject` on `Start()`, loads a local `file://` page from StreamingAssets, logs bridge
  messages to the console. Currently also has a temporary `Update()` block: `Input.anyKeyDown`
  triggers `EvaluateJS` to synthetically `.click()` the page's `#start-button` (added because the
  user couldn't get real mouse clicks to land in the WebView's native overlay — see open issue
  below). This is scaffolding, not a final input solution.
- **Test scene**: `Assets/TDR/Assets/Scenes/Minigame/MinigameWebViewTest.unity`, added to
  `ProjectSettings/EditorBuildSettings.asset` at build index 4.
- **New button on `01_MM`**: home page now has a "Test Minigame" button (duplicated from the
  Credits button, GameObjects renamed `Grp_Button_TestMinigame`/`Button_TestMinigame`, label text
  "Test Minigame"), wired to `ButtonCustom.LoadNewScene(4)`. **Important gotcha**: MCP's reflection
  tools (`gameobject-component-modify`, path patches, etc.) cannot read or write `UnityEvent`
  internals (`m_PersistentCalls`/`m_Calls` — errors with "Segment 'm_PersistentCalls' not found").
  The only way found to rewire a `UnityEvent`'s persistent listener programmatically is
  `script-execute` calling `UnityEditor.Events.UnityEventTools.RemovePersistentListener` /
  `AddIntPersistentListener` directly. Verified afterward by reading back
  `OnClick.GetPersistentEventCount()` / `GetPersistentMethodName()` the same way.
- **Real game swapped in**: the user built an actual game, **Ninja Turtle Tunnel**, source at
  `/Users/eladayzen/ninja-turtle-tunnel` (Vite-built, `dist/` is the static output, ~560KB). Its
  `dist/` contents were copied into `Assets/StreamingAssets/MinigameTest/` (replacing the original
  placeholder test page — same folder the loader script already pointed at, so no script change
  needed for this swap).
- **Fixed the predicted `file://` absolute-path problem**: the Vite build's `dist/index.html` used
  `<script type="module" crossorigin src="/assets/...">` and `<link crossorigin href="/assets/...">`
  — absolute root paths break under `file://` (resolve to filesystem root, not the html's folder).
  Patched to relative paths (`./assets/...`) and dropped `crossorigin`. This is exactly the
  constraint flagged as a risk before any real content existed — now confirmed real and fixed for
  this one file. **The eventual `Documentation/WebGame_TechBrief.md` for the other Claude Code
  instance building web games needs to say: build with a relative base path (Vite `base: './'`),
  not the default absolute-root output.**
- Confirmed via console logs: page loads without asset-404 errors, `Start` button's JS click
  handler (`Cu()` in the minified bundle) fires correctly (hides both overlays, sets state to
  `playing`) whether triggered by the synthetic `EvaluateJS` click or presumably a real click.

## Open problem (unresolved as of this writing)

Two distinct, possibly-related symptoms reported by the user:

1. **Real mouse clicks don't register in the WebView.** This is why the `Input.anyKeyDown` →
   synthetic-click workaround was added to the loader script at all.
2. **Even the synthetic click (confirmed executing successfully via the bridge log
   `"start-button clicked"`) doesn't make the game visibly start.** The click handler runs and
   hides the overlays server-side (JS-side), but nothing appears to happen on screen afterward.

Current hypothesis: a silent JS runtime error inside the game's actual start/init logic (Phaser
scene start, WebGL context creation, `AudioContext` unlock, etc.) that isn't visible to us — the
`Gree.UnityWebView` bridge only surfaces messages explicitly sent via `window.Unity.call(...)`,
there's no console/error passthrough by default. **Just added** (last edit before this summary,
not yet tested): a `window.addEventListener('error', ...)` and `window.addEventListener(
'unhandledrejection', ...)` block injected directly into
`Assets/StreamingAssets/MinigameTest/index.html`'s `<head>`, forwarding any JS error/rejection
through `window.Unity.call('JS ERROR: ...')` so it shows up in the Unity console via the existing
`OnJavaScriptMessage` logger. **Next step for whoever picks this up: re-enter Play Mode, trigger
Start, and read the Unity console for any `"JS ERROR"` / `"JS UNHANDLED REJECTION"` log lines** —
that should finally reveal what's actually failing.

Also worth knowing: the WebView renders as a **native OS overlay window on top of the Unity Game
view**, not through Unity's own render/camera pipeline. This means Unity screenshot/render-capture
MCP tools (`screenshot-isolated`, etc.) **cannot** see WebView content at all — verifying anything
visual requires the user to look at the actual Editor Game view themselves, or extending the
JS↔Unity bridge to report more state back explicitly.

## Also relevant

- Unity MCP (`ai-game-developer`) disconnected/needed re-auth multiple times during this work,
  same recurring issue as the car-game work. Fix each time: **Window → AI Game Developer — MCP**
  (⌘⌥A) in the Editor, then resume the Claude Code session.
- Entering/exiting Play Mode was driven via `script-execute` calling
  `UnityEditor.EditorApplication.isPlaying = true/false` (no dedicated MCP play-mode tool exists).
  Editing any C# script requires exiting Play Mode first, letting it recompile
  (`assets-refresh` to confirm clean), then re-entering — editing scripts live during Play Mode
  risks the native WebView's underlying state/handle.
- None of this WebView prototype work is committed yet — `Packages/manifest.json`, the new script,
  scene, build-settings change, the `01_MM` button, and the StreamingAssets game files are all
  currently uncommitted local changes.
