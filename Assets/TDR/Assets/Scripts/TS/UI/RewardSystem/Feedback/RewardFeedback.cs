// Description: RewardFeedback: Attached to the result manager in each Mode
// (Arcade, Time Trial, Championship) to display feedback during rewards sequence.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics
{
    public class RewardFeedback : MonoBehaviour
    {
        public GameObject objBestScoreBig;
        public GameObject objBestScore;
        public GameObject objCoinBig;
        public GameObject objTimeBig;
        public GameObject objPlayerPosition;

        public Transform objPivotInstantiatePos;

        public void BestScoreFeedback()
        {
            #region
            StartCoroutine(BestScoreFeedbackRoutine());
            #endregion
        }
        public IEnumerator BestScoreFeedbackRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            int timeScore = (int)(LapCounterAndPosition.instance.posList[0].globalTime * 1000.0f);

            if (ReturnPlayerPositionInTheLeaderboard(timeScore) == 0)
            {
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

                Transform instantiatePos = transform.parent;

                if (objPivotInstantiatePos)
                    instantiatePos = objPivotInstantiatePos;
                GameObject gift = Instantiate(objBestScoreBig, instantiatePos);
                IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
                yield return new WaitUntil(() => rFeedback.ISGiftAnimDone());
                Destroy(gift);
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            }

            if (ReturnPlayerPositionInTheLeaderboard(timeScore) == 0)
                objBestScore.SetActive(true);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        public void TimeTrialAddCoinToTheWallet()
        {
            #region
            // Update Wallet
            int timeScore = (int)(LapCounterAndPosition.instance.posList[0].globalTime * 1000.0f);
            int coins  = Coins(ReturnPlayerPositionInTheLeaderboard(timeScore));
            if(coins > 0)InfoCoins.instance.UpdateCoins(coins);

            // Coin feedback
             StartCoroutine(TimeTrialAddCoinToTheWalletRoutine(coins));
            #endregion
        }

        public IEnumerator TimeTrialAddCoinToTheWalletRoutine(int coins)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            if (coins > 0)
            {

                Transform instantiatePos = transform.parent;

                if (objPivotInstantiatePos)
                    instantiatePos = objPivotInstantiatePos;
                GameObject gift = Instantiate(objCoinBig,instantiatePos);
                IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
                rFeedback.UpdateCoin(coins);
                yield return new WaitUntil(() => rFeedback.ISGiftAnimDone());
                Destroy(gift);
            }
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        int Coins(int playerPosInRace)
        {
            #region
            int coins = 0;
            if (playerPosInRace < DataRef.instance.tracksData.listTrackParams[ReturnCurrentTrackID()].listTimeTrialCoins.Count)
            {
                coins = DataRef.instance.tracksData.listTrackParams[ReturnCurrentTrackID()].listTimeTrialCoins[playerPosInRace];
            }
            return coins;
            #endregion
        }

        int ReturnPlayerPositionInTheLeaderboard(int timeScore)
        {
            #region
            for (var i = 0; i < GameModeTimeTrial.instance.listLeaderboardByTrack[ReturnCurrentTrackID()].listLeadEntry.Count; i++)
            {
                if (GameModeTimeTrial.instance.listLeaderboardByTrack[ReturnCurrentTrackID()].listLeadEntry[i].time > timeScore)
                    return i;
            }

            return GameModeTimeTrial.instance.listLeaderboardByTrack[ReturnCurrentTrackID()].listLeadEntry.Count;
            #endregion
        }

        int ReturnCurrentTrackID()
        {
            #region
            int currentSelectedTrack = GameModeTimeTrial.instance.currentSelection;

            if (!DataRef.instance.timeTrialModeData.bDisplayTrackUsingTrackListOrder)
            {
                int specialOrderID = DataRef.instance.timeTrialModeData.customTrackList[currentSelectedTrack];
                return specialOrderID;
            }
            else
            {
                return currentSelectedTrack;
            }
            #endregion
        }

        public void Podium()
        {
            #region
            StartCoroutine(PodiumRoutine());
            #endregion
        }

        public IEnumerator PodiumRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            yield return new WaitForSeconds(1);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        public void RaceTime()
        {
            #region
            StartCoroutine(RaceTimeRoutine());
            #endregion
        }

        public IEnumerator RaceTimeRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            Transform instantiatePos = transform.parent;

            if (objPivotInstantiatePos)
                instantiatePos = objPivotInstantiatePos;


            GameObject gift = Instantiate(objTimeBig, instantiatePos);


            IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
            yield return new WaitUntil(() => rFeedback.ISGiftAnimDone());
            Destroy(gift);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        public string FormatTimer(int newTime)
        {
            #region
            int FormatedTimer = newTime;
            int minutes = FormatedTimer / (60000);
            int seconds = (FormatedTimer % 60000) / 1000;
            int milliseconds = FormatedTimer % 1000;
            return String.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
            #endregion
        }

        public void PodiumGroupState(bool isEnabled)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            GrpPodiumTag[] podiums = FindObjectsByType<GrpPodiumTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0;i< podiums.Length; i++)
            {
                GrpPodiumTag podium = podiums[i];
                if (podium.id == 0)
                {
                    if (podium && podium.grp3DModels) podium.grp3DModels.gameObject.SetActive(isEnabled);
                    InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
                }
             
            }
            #endregion
        }

        public void CanvasInGameState(bool isEnabled)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            CanvasInGameTag.instance.gameObject.SetActive(isEnabled);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        public void CanvasMinimapState(bool isEnabled)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            MinimapManager.instance.gameObject.SetActive(isEnabled);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            #endregion
        }

        public void ArcadePlayersRacePosition()
        {
            #region
            StartCoroutine(ArcadePlayersRacePositionRoutine());
            #endregion
        }
        public IEnumerator ArcadePlayersRacePositionRoutine()
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            Transform instantiatePos = transform.parent;

            if (objPivotInstantiatePos)
                instantiatePos = objPivotInstantiatePos;

            GameObject gift = Instantiate(objPlayerPosition,instantiatePos);
            IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
            yield return new WaitUntil(() => rFeedback.ISGiftAnimDone());
            Destroy(gift);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        public void ArcadeAddCoinToTheWallet()
        {
            #region
            int currentSelectedTrack = GameModeArcade.instance.currentSelection;

            TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[currentSelectedTrack];
            if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
            {
                int specialOrderID = DataRef.instance.arcadeModeData.customTrackList[currentSelectedTrack];
                trackParams = DataRef.instance.tracksData.listTrackParams[specialOrderID];
            }

            int howManyVehicleInTheList = ArcadeResult.instance.vehicleList.Count;
            int playerPos = 0;
            for (var i = 0; i < howManyVehicleInTheList++; i++)
            {
                if (ArcadeResult.instance.vehicleList[i] == 0)
                {
                    playerPos = i;
                    break;
                }
            }

            int coins = 0;
            if (playerPos < trackParams.listArcadeCoins.Count)
                coins = trackParams.listArcadeCoins[playerPos];

            StartCoroutine(ArcadeAddCoinToTheWalletRoutine(coins));
            #endregion
        }

        public IEnumerator ArcadeAddCoinToTheWalletRoutine(int coins)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            if (coins > 0)
            {
                Transform instantiatePos = transform.parent;

                if (objPivotInstantiatePos)
                    instantiatePos = objPivotInstantiatePos;
                GameObject gift = Instantiate(objCoinBig, instantiatePos);
                IRewardFeedback rFeedback = gift.GetComponent<IRewardFeedback>();
                rFeedback.UpdateCoin(coins);
                yield return new WaitUntil(() => rFeedback.ISGiftAnimDone());
                Destroy(gift);
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }

        public void PodiumGroupCameraState(bool isEnabled)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            GrpPodiumTag[] podiums = FindObjectsByType<GrpPodiumTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < podiums.Length; i++)
            {
                GrpPodiumTag podium = podiums[i];
                if (podium.id == 0)
                {
                    if (podium && podium.grp_Camera) podium.grp_Camera.gameObject.SetActive(isEnabled);
                    InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
                }
            }
            #endregion
        }

        public void WaitBeforDoingSomething(float duration)
        {
            #region
            StartCoroutine(WaitBeforDoingSomethingRoutine(duration));
            #endregion
        }

        public IEnumerator WaitBeforDoingSomethingRoutine(float duration)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            var t = 0f;

            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }
    }

}
