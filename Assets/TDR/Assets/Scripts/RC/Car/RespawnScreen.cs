// Description: RespawnScreen. Called by CarRespawnV
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class RespawnScreen : MonoBehaviour
    {
        public int          PlayerID = 0;
        public CanvasGroup  canvasGroup;
        [HideInInspector]
        public bool         IsProcessDone = true;

        public void FadeOut()
        {
            #region
            StartCoroutine(FadeOutRoutine());
            #endregion
        }

        public IEnumerator FadeOutRoutine()
        {
            #region
            float t = 0;
            float duration = 1;
            float start = canvasGroup.alpha;

            while (t < .25f)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime / duration;


                yield return null;
            }

            t = 0;

            while (t < 1)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime / duration;

                canvasGroup.alpha = Mathf.Lerp(start, 0, t);
                yield return null;
            }
           
            yield return null;
            #endregion
        }

        public void FadeIn()
        {
            #region
            StartCoroutine(FadeInRoutine());
            #endregion
        }

        public IEnumerator FadeInRoutine()
        {
            #region
            IsProcessDone = false;
            float t = 0;
            float duration = .5f;
            float start = canvasGroup.alpha;

          
            while (t < 1)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime / duration;

                canvasGroup.alpha = Mathf.Lerp(start, 1, t);

                yield return null;
            }
            IsProcessDone = true;
            yield return null;
            #endregion
        }
    }
}
