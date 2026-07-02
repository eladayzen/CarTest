// Description: GiftBestScoreTT. Displayed Best Score at the end of the Time Trial mode.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics
{
    public class GiftBestScoreTT : MonoBehaviour, IRewardFeedback
    {
        private bool            IsProcessDone = false;

        public Vector3          ScaleStart = Vector3.one;
        public Vector3          ScaleEnd = Vector3.one;

        public float            ScaleDuration = 1;

        public AudioSource      aSource;

        public GameObject       objReminderReward;


        public void Start()
        {
            #region
            AnimProcess(); 
            #endregion
        }

        public void AnimProcess()
        {
            #region 
            StartCoroutine(AnimProcessRoutine()); 
            #endregion
        }

        public IEnumerator AnimProcessRoutine()
        {
            #region 
            //Debug.Log("Anim Starts");
            transform.localScale = ScaleStart;
            float t = 0;
            float duration = ScaleDuration;

            while (t < 1)
            {
                t += Time.deltaTime / duration;

                transform.localScale = Vector3.MoveTowards(transform.localScale, ScaleEnd, t);
                yield return null;
            }

            if (aSource)
                yield return new WaitUntil(() => !aSource.isPlaying);

            if (objReminderReward)
            {
                GiftPositonTag[] giftPositon = FindObjectsByType<GiftPositonTag>(FindObjectsSortMode.None);

                foreach (GiftPositonTag obj in giftPositon)
                {
                    if (obj.id == 1)
                    {
                        GameObject gift = Instantiate(objReminderReward, obj.transform);
                    }
                }
            }
            yield return new WaitForEndOfFrame();

            //Debug.Log("Anim ended");
            IsProcessDone = true;
            yield return null; 
            #endregion
        }

        public bool ISGiftAnimDone()
        {
            #region 
            return IsProcessDone; 
            #endregion
        }

        public void UpdateCoin(int value)
        {

        }
    }

}
