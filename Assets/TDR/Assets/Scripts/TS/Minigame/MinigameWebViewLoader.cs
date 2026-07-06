// Description: MinigameWebViewLoader. Prototype loader that embeds a WebView (gree/unity-webview)
// pointed at a locally-bundled web page, to prove the Unity <-> WebView <-> JS pipeline works
// end-to-end before a real externally-built minigame exists. Test-only, not the final integration.
//
// Serves the page over a local HTTP server instead of file:// - REQUIRED, not an optimization:
// the game is a Vite ES-module build (<script type="module">), and module scripts are silently
// blocked on file:// pages (null-origin CORS) in WKWebView and Chrome alike. That was the root
// cause of the long-standing "game never visibly starts / canvas=NONE" bug: the game's module
// never executed at all under file://.
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using Gree.UnityWebView;

namespace TS.Generics
{
    public class MinigameWebViewLoader : MonoBehaviour
    {
        // StreamingAssets subfolder to serve - set per-scene in the Inspector so multiple
        // minigames can each get their own MinigameWebViewLoader scene instance.
        public string                 minigameFolderName = "MinigameTest";

        WebViewObject webViewObject;
        HttpListener  httpListener;
        string        rootDir;
        int           port;

        void Start()
        {
            #region
            rootDir = System.IO.Path.Combine(Application.streamingAssetsPath, minigameFolderName);
            port = StartLocalServer();
            if (port < 0)
            {
                Debug.LogError("[MinigameWebViewLoader] Could not start local HTTP server.");
                return;
            }

            webViewObject = gameObject.AddComponent<WebViewObject>();
            webViewObject.Init(cb: OnJavaScriptMessage);

            string url = "http://127.0.0.1:" + port + "/index.html";
            Debug.Log("[MinigameWebViewLoader] Serving minigame at " + url);
            webViewObject.LoadURL(url);

            webViewObject.SetMargins(0, 0, 0, 0);
            webViewObject.SetVisibility(true);
            #endregion
        }

        void OnDestroy()
        {
            #region
            if (httpListener != null)
            {
                try { httpListener.Stop(); httpListener.Close(); } catch { }
                httpListener = null;
            }
            #endregion
        }

        void OnJavaScriptMessage(string message)
        {
            #region
            Debug.Log("[MinigameWebViewLoader] " + message);
            #endregion
        }

        // Keys forwarded into the page as synthetic keydown/keyup - the occluded WebView
        // never receives real keyboard focus in the Editor.
        static readonly (KeyCode key, string code, string jsKey)[] forwardedKeys =
        {
            (KeyCode.LeftArrow,  "ArrowLeft",  "ArrowLeft"),
            (KeyCode.RightArrow, "ArrowRight", "ArrowRight"),
            (KeyCode.UpArrow,    "ArrowUp",    "ArrowUp"),
            (KeyCode.DownArrow,  "ArrowDown",  "ArrowDown"),
            (KeyCode.A,          "KeyA",       "a"),
            (KeyCode.D,          "KeyD",       "d"),
            (KeyCode.W,          "KeyW",       "w"),
            (KeyCode.S,          "KeyS",       "s"),
            (KeyCode.Space,      "Space",      " "),
            (KeyCode.Return,     "Enter",      "Enter"),
        };

        float pumpTimer = 0f;
        const float pumpInterval = 1f / 60f;

        void Update()
        {
            #region
            if (webViewObject == null) return;

            // Heartbeat: flush the page's queued requestAnimationFrame callbacks - WKWebView
            // suspends its own rAF for this occluded view, so Unity drives the game loop
            // instead (see the shim in index.html). Capped at 60Hz: pumping once per Unity
            // frame ties the game's speed to the Editor's (uncapped, variable) framerate.
            pumpTimer += Time.unscaledDeltaTime;
            if (pumpTimer >= pumpInterval)
            {
                pumpTimer = Mathf.Min(pumpTimer - pumpInterval, pumpInterval);
                webViewObject.EvaluateJS("window.__pumpFrames && window.__pumpFrames();");
            }

            // Forward key transitions as synthetic DOM keyboard events.
            foreach (var (key, code, jsKey) in forwardedKeys)
            {
                if (Input.GetKeyDown(key)) SendKeyEvent("keydown", code, jsKey);
                if (Input.GetKeyUp(key)) SendKeyEvent("keyup", code, jsKey);
            }

            #endregion
        }

        void SendKeyEvent(string type, string code, string jsKey)
        {
            #region
            webViewObject.EvaluateJS(
                "window.dispatchEvent(new KeyboardEvent('" + type + "',{code:'" + code +
                "',key:'" + jsKey + "',bubbles:true}));");
            #endregion
        }

        // ---------------------------------------------------------------------------------
        // Minimal static file server over the MinigameTest folder. Editor/desktop prototype
        // only (reads StreamingAssets via System.IO - fine on macOS/iOS/desktop, would need
        // rework for Android where StreamingAssets lives inside the APK).
        // ---------------------------------------------------------------------------------
        int StartLocalServer()
        {
            #region
            for (int tryPort = 8090; tryPort < 8100; tryPort++)
            {
                try
                {
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add("http://127.0.0.1:" + tryPort + "/");
                    httpListener.Start();
                    ThreadPool.QueueUserWorkItem(_ => ServeLoop());
                    return tryPort;
                }
                catch
                {
                    try { httpListener.Close(); } catch { }
                    httpListener = null;
                }
            }
            return -1;
            #endregion
        }

        void ServeLoop()
        {
            #region
            while (httpListener != null && httpListener.IsListening)
            {
                HttpListenerContext ctx;
                try { ctx = httpListener.GetContext(); }
                catch { break; }   // listener stopped

                try
                {
                    string rel = ctx.Request.Url.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrEmpty(rel)) rel = "index.html";
                    string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, rel));

                    // No escaping the served folder.
                    if (!fullPath.StartsWith(System.IO.Path.GetFullPath(rootDir)) || !File.Exists(fullPath))
                    {
                        ctx.Response.StatusCode = 404;
                        ctx.Response.Close();
                        continue;
                    }

                    byte[] bytes = File.ReadAllBytes(fullPath);
                    ctx.Response.ContentType = MimeFor(fullPath);
                    ctx.Response.ContentLength64 = bytes.Length;
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    ctx.Response.Close();
                }
                catch
                {
                    try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { }
                }
            }
            #endregion
        }

        // Correct JS MIME type is mandatory: module scripts are refused outright on a wrong
        // content type (unlike classic scripts).
        static string MimeFor(string path)
        {
            #region
            switch (System.IO.Path.GetExtension(path).ToLowerInvariant())
            {
                case ".html": return "text/html; charset=utf-8";
                case ".js":   return "application/javascript; charset=utf-8";
                case ".css":  return "text/css; charset=utf-8";
                case ".json": return "application/json";
                case ".png":  return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".svg":  return "image/svg+xml";
                case ".wasm": return "application/wasm";
                case ".mp3":  return "audio/mpeg";
                case ".ogg":  return "audio/ogg";
                default:      return "application/octet-stream";
            }
            #endregion
        }
    }
}
