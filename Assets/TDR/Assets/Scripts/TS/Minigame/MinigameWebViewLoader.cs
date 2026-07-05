// Description: MinigameWebViewLoader. Prototype loader that embeds a WebView (gree/unity-webview)
// pointed at a locally-bundled web page, to prove the Unity <-> WebView <-> JS pipeline works
// end-to-end before a real externally-built minigame exists. Test-only, not the final integration.
using UnityEngine;
using Gree.UnityWebView;

namespace TS.Generics
{
    public class MinigameWebViewLoader : MonoBehaviour
    {
        WebViewObject webViewObject;

        void Start()
        {
            #region
            webViewObject = gameObject.AddComponent<WebViewObject>();
            webViewObject.Init(cb: OnJavaScriptMessage);

            string localPagePath = System.IO.Path.Combine(Application.streamingAssetsPath, "MinigameTest/index.html");
            webViewObject.LoadURL("file://" + localPagePath);

            webViewObject.SetMargins(0, 0, 0, 0);
            webViewObject.SetVisibility(true);
            #endregion
        }

        void OnJavaScriptMessage(string message)
        {
            #region
            Debug.Log("[MinigameWebViewLoader] Message from web page: " + message);
            #endregion
        }

        void Update()
        {
            #region
            // Mouse clicks aren't reliably reaching the WebView's native overlay in Editor,
            // so any keypress simulates clicking the page's Start button as a workaround.
            if (Input.anyKeyDown)
            {
                Debug.Log("[MinigameWebViewLoader] Key detected, sending click to #start-button");
                webViewObject.EvaluateJS("var b=document.getElementById('start-button'); if (b) { b.click(); window.Unity.call('start-button clicked'); } else { window.Unity.call('start-button not found'); }");
            }
            #endregion
        }
    }
}
