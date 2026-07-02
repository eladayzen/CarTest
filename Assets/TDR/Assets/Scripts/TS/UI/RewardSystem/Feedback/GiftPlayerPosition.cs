// Description: GiftPlayerPosition. Use to display player position at the end of the Arcade mode.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics
{
    public class GiftPlayerPosition : MonoBehaviour, IRewardFeedback
    {
        private bool                IsProcessDone = false;

        public Vector3              ScaleStart = Vector3.one;
        public Vector3              ScaleEnd = Vector3.one;

        public float                ScaleDuration = 1;

        public AudioSource          aSource;

        public List<GameObject>     playersList = new List<GameObject>();
        public List<CurrentText>    textPosList = new List<CurrentText>();

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
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            if(howManyPlayer == 1)
            {
                for (var i = 0; i < 2; i++)
                {
                    if (i < howManyPlayer)
                    {
                        playersList[i].transform.localScale = ScaleStart;
                        int playerRacePosition = LapCounterAndPosition.instance.posList[i].RacePos + 1;
                        textPosList[i].DisplayTextComponent(textPosList[i].gameObject, playerRacePosition.ToString());
                    }
                    else
                    {
                        playersList[i].transform.parent.gameObject.SetActive(false);
                    }
                }

                float t = 0;
                float duration = ScaleDuration;

                while (t < 1)
                {
                    t += Time.deltaTime / duration;

                    for (var i = 0; i < howManyPlayer; i++)
                        playersList[i].transform.localScale = Vector3.MoveTowards(playersList[i].transform.localScale, ScaleEnd, t);

                    yield return null;
                }


                yield return new WaitUntil(() => !aSource.isPlaying);
            }
            else if (howManyPlayer == 2)
            {
                for (var i = 0; i < 2; i++)
                {
                    playersList[i].transform.localScale = ScaleStart;
                    int playerRacePosition = LapCounterAndPosition.instance.posList[i].RacePos + 1;
                    textPosList[i].DisplayTextComponent(textPosList[i].gameObject, playerRacePosition.ToString());
                }

                float t = 0;
                float duration = ScaleDuration;

                while (t < 1)
                {
                    t += Time.deltaTime / duration;

                    for (var i = 0; i < howManyPlayer; i++)
                        playersList[i].transform.localScale = Vector3.MoveTowards(playersList[i].transform.localScale, ScaleEnd, t);

                    yield return null;
                }


                yield return new WaitUntil(() => !aSource.isPlaying);
            }


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
