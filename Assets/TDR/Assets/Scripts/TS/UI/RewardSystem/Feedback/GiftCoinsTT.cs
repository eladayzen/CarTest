// Description: GiftCoinsTT. Use to display coins at the end of the Time Trial mode.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics
{
    public class GiftCoinsTT : MonoBehaviour, IRewardFeedback
    {
        private bool            IsProcessDone = false;

        public Vector3          ScaleStart = Vector3.one;
        public Vector3          ScaleEnd = Vector3.one;

        public float            ScaleDuration = 1;

        public AudioSource      aSource;

        public GameObject       objReminderReward;


        public CurrentText      txtCoin;
        [HideInInspector]
        public int              howManyCoins = -1;

        public GameObject       grp_UI;

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
            transform.localScale = ScaleStart;

            grp_UI.SetActive(false);

            yield return new WaitUntil(() => howManyCoins != -1);

            int coinsWon = howManyCoins;

            txtCoin.DisplayTextComponent(txtCoin.gameObject, "+" + coinsWon);

            grp_UI.SetActive(true);
            //Debug.Log("Anim Starts");

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
                    if (obj.id == 2)
                    {
                        GameObject gift = Instantiate(objReminderReward, obj.transform);
                        IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
                        rFeedback.UpdateCoin(coinsWon);
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
            Debug.Log("howManyCoins: " + value);
            howManyCoins = value;
        }
    }

}
