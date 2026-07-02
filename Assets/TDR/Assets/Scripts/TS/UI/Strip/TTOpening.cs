//-> TTOpening : Description:  This script is used to display after the race the score in Time Trial Mode.
// 1- Allows the player to enter his name and his score in the leaderboard.
// 2- Allows the player to go back to the main menu or restart the race

using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class TTOpening : MonoBehaviour
    {
        public float                scaleSpeed = .5f;
        public float                delay;
        public AnimationCurve       scaleCurve;
        private float               t;

        public RectTransform        rectGrp;
        public RectTransform        rectGrp2;

        public Vector2              initSizeDelta = new Vector2(0, 0);

        public RectTransform        rectGrp3;

        public GameObject           btnPlayAgain;

        public RectTransform        rectTime;
        public RectTransform        rectName;
        public GameObject           line;

        //-> Display the section where the player is able to enter his name.
        public void NewScale(float value = 1)
        {
            #region
            StartCoroutine(scaleRectTransformRoutine(value)); 
            #endregion
        }


        IEnumerator scaleRectTransformRoutine(float value = 1, bool waitUntilScaleFinished = false)
        {
            #region
            if (waitUntilScaleFinished)
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            else
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;


            t = 0;
            while (t < delay)
            {
                t += Time.deltaTime / delay;
                yield return null;
            }

            rectGrp.sizeDelta = new Vector2(0, 1);

            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectGrp.sizeDelta = new Vector2(localZEvaluate * 500, rectGrp.sizeDelta.y);
                yield return null;
            }


            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectGrp.sizeDelta = new Vector2(rectGrp.sizeDelta.x, localZEvaluate * 355);
                yield return null;
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null; 
            #endregion
        }


        public void InitScale()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            StopAllCoroutines();
            rectGrp.sizeDelta = new Vector2(500, 0);
            rectTime.localScale = new Vector2(1, 1);
            rectName.localScale = new Vector2(1, 1);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public bool BInitTitleScale()
        {
            #region
            rectGrp.sizeDelta = new Vector2(500, 355);
            rectTime.localScale = new Vector2(1, 1);
            rectName.localScale = new Vector2(1, 1);
            return true; 
            #endregion
        }


        //-> Display the section that allows the player to go back to the main menu or restart the race
        public void StepTwo()
        {
            #region
            StartCoroutine(StepTwoRoutine()); 
            #endregion
        }

        IEnumerator StepTwoRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;

            t = 0;
            while (t < delay)
            {
                t += Time.deltaTime / delay;
                yield return null;
            }

            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectGrp2.sizeDelta = new Vector2(rectGrp2.sizeDelta.x, 180 - localZEvaluate * 180);
                yield return null;
            }


            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectGrp2.sizeDelta = new Vector2(355 - localZEvaluate * 355, rectGrp2.sizeDelta.y);
                yield return null;
            }

            StartCoroutine(StepThreeRoutine());

            yield return null; 
            #endregion
        }


        
        IEnumerator StepThreeRoutine(float value = 1)
        {
            #region
            t = 0;
            while (t < delay)
            {
                t += Time.deltaTime / delay;
                yield return null;
            }

            rectTime.localScale = new Vector2(1, 1);
            rectName.localScale = new Vector2(1, 1);

            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectTime.localScale = new Vector2(1 + localZEvaluate * .3f, 1 + localZEvaluate * .3f);
                yield return null;
            }


            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectName.localScale = new Vector2(1 + localZEvaluate * .3f, 1 + localZEvaluate * .3f);
                yield return null;
            }

            line.transform.localScale = new Vector2(0, 1);
            line.SetActive(true);
            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                line.transform.localScale = new Vector2(localZEvaluate * 1f, line.transform.localScale.y);
                yield return null;
            }

            t = 0;
            while (t < scaleSpeed)
            {
                t += Time.deltaTime * .5f / scaleSpeed;
                float localZEvaluate = scaleCurve.Evaluate(t / scaleSpeed);
                rectGrp3.sizeDelta = new Vector2(rectGrp3.sizeDelta.x, 200 + localZEvaluate * 200);
                yield return null;
            }

            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(btnPlayAgain);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;

            yield return null; 
            #endregion
        }


        public void NewScaleWaitUntilScaleFinished(float value = 1)
        {
            #region
            StartCoroutine(scaleRectTransformRoutine(value, true)); 
            #endregion
        }
    }

}
