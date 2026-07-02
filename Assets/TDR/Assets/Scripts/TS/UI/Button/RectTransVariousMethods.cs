// Description: RectTransVariousMethods:
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class RectTransVariousMethods : MonoBehaviour
    {
        [HideInInspector]
        public bool             SeeInspector;
        [HideInInspector]
        public bool             moreOptions;
        [HideInInspector]
        public bool             helpBox = true;

        //[Header("Change Scale")]
        public float            scaleSpeed = 1;
        public List<Vector3>    vectorThreeaList = new List<Vector3>(2) {new Vector3(1f,1f,1f),new Vector3(1.05f,1.05F,1f) };
        public bool             bShowEditorScale = true;

        //[Header("Change Pivot")]
        public float            pivotSpeed = 1;
        public List<Vector2>    pivotaList = new List<Vector2>(2) { new Vector2(.5f, .4f), new Vector2(.5f,0) };
        public bool             bShowEditorPivot = true;

        public void ChangePivotSmooth(int vectorID)
        {
            #region 
            if (bShowEditorPivot) ChangePivot(vectorID, true); 
            #endregion
        }
        public void ChangePivotStraight(int vectorID)
        {
            #region 
            if (bShowEditorPivot) ChangePivot(vectorID, false); 
            #endregion
        }
        public void ChangeScaleSmooth(int vectorID)
        {
            #region 
            if (bShowEditorScale) ChangeScale(vectorID, true); 
            #endregion
        }
        public void ChangeScaleStraight(int vectorID)
        {
            #region 
            if (bShowEditorScale) ChangeScale(vectorID, false); 
            #endregion
        }

        public void ChangeScale(int vectorID, bool bSmooth = true)
        {
            #region
            if (gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(ChangeScaleRoutine(vectorThreeaList[vectorID], bSmooth));
            }
            else
            {
                ScaleStraight(vectorThreeaList[vectorID]);
            } 
            #endregion
        }

        IEnumerator ChangeScaleRoutine(Vector3 target, bool bSmooth = true)
        {
            #region 
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();

            if (bSmooth)
            {
                while (rectTrans.localScale != target)
                {
                    rectTrans.localScale = Vector3.MoveTowards(rectTrans.localScale, target, Time.deltaTime * scaleSpeed);
                    yield return null;
                }
            }
            else
            {
                rectTrans.localScale = target;
            }

            yield return null; 
            #endregion
        }

        public void ChangePivot(int vectorID,bool bSmooth = true)
        {
            #region 
            if (gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(ChangePivotRoutine(pivotaList[vectorID], bSmooth));
            }
            else
            {
                PivotStraight(pivotaList[vectorID]);
            } 
            #endregion
        }

        IEnumerator ChangePivotRoutine(Vector2 target, bool bSmooth = true)
        {
            #region 
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();

            if (bSmooth)
            {
                while (rectTrans.pivot != target)
                {
                    rectTrans.pivot = Vector3.MoveTowards(rectTrans.pivot, target, Time.deltaTime * pivotSpeed);
                    yield return null;
                }
            }
            else
            {
                rectTrans.pivot = target;
            }
            yield return null; 
            #endregion
        }

        void PivotStraight(Vector2 target) {
            #region 
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            rectTrans.pivot = target; 
            #endregion
        }

        void ScaleStraight(Vector2 target)
        {
            #region
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
            rectTrans.localScale = target; 
            #endregion
        }
    }
}