// Description: UITranslation: USed to create translation on Tutorial UI element.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class UITranslation : MonoBehaviour
    {
        public RectTransform    rect;
        public RectTransform    posStart;
        public RectTransform    posEnd;

        public AnimationCurve   curve;

        public bool             bLoop = true;
        public float            loopDuration = 1;
        public bool             bAutoStart = true;

       
        void Start()
        {
            #region 
            rect.localPosition = posStart.localPosition;
            if (bAutoStart) StartCoroutine(TranslationRoutine()); 
            #endregion
        }

        public void RectTranslation()
        {
            #region 
            StopAllCoroutines();
            StartCoroutine(TranslationRoutine()); 
            #endregion
        }

        IEnumerator TranslationRoutine()
        {
            #region 
            float t = 0;

            while (t < loopDuration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime / loopDuration;
                    rect.localPosition = Vector3.Lerp(posStart.localPosition, posEnd.localPosition, curve.Evaluate(t));
                }
                yield return null;
            }

            if (bLoop)
                StartCoroutine(TranslationRoutine());

            yield return null; 
            #endregion
        }
    }
}

