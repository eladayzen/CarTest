// Description: BoostShakeCam: Manage camera shake. Attached on BoosterShake object.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class BoostShakeCam : MonoBehaviour
    {
        public bool             b_ShakeEnable = true;
        public AnimationCurve   animCurveX;
        public AnimationCurve   animCurveY;
        public float            speedOn = 1;
        public float            speedOff = 100;
        public float            amplitudeX = .5f;
        public float            amplitudeY = .5f;

        public bool             loop = true;

        float                   t = 0;
        public bool             isShakeEnable = true;

        public void ShakeStart()
        {
            #region 
            if (b_ShakeEnable)
            {
                StopAllCoroutines();
                t = 0;
                StartCoroutine(ShakeRoutine(this.transform));
            } 
            #endregion
        }

        public void ShakeStop()
        {
            #region 
            if (!b_ShakeEnable)
            {
                StopAllCoroutines();
                StartCoroutine(ShakeResetRoutine(this.transform));
            } 
            #endregion
        }

        IEnumerator ShakeRoutine(Transform trans/*, CameraFx camFx*/)
        {
            #region 
            b_ShakeEnable = false;
            //Debug.Log("Shake");
            //-> VFX
            //t = 0;

            while (t != 1)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t = Mathf.MoveTowards(t, 1, Time.deltaTime * speedOn);
                    float newXPos = amplitudeX * animCurveX.Evaluate(t);
                    float newYPos = amplitudeY * animCurveY.Evaluate(t);
                    trans.localPosition = new Vector3(newXPos, newYPos, 0);
                }

                yield return null;
            }

            b_ShakeEnable = true;
            if (loop)
            {

                ShakeStart();
            }
            yield return null; 
            #endregion
        }

        IEnumerator ShakeResetRoutine(Transform trans)
        {
            #region 
            b_ShakeEnable = true;
            //-> VFX
            //t = 0;

            //Debug.Log("t: " + t);

            float posX = trans.localPosition.x;
            float posY = trans.localPosition.y;

            while (t != 0)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t = Mathf.MoveTowards(t, 0, Time.deltaTime * speedOff);
                    float newXPos = posX * t;
                    float newYPos = posY * t;
                    trans.localPosition = new Vector3(newXPos, newYPos, 0);
                }

                yield return null;
            }

            yield return null; 
            #endregion
        }

        public void ShakeWithParams(float x,float y,float speed)
        {
            #region 
            if (isShakeEnable)
            {
                speedOn = speed;
                amplitudeX = x;
                amplitudeY = y;
                ShakeStart();
            } 
            #endregion
        }

        public void ShakeForceStop()
        {
            #region 
            StopAllCoroutines();
            StartCoroutine(ShakeResetRoutine(this.transform)); 
            #endregion
        }

    }
}

