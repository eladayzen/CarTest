// Description: DiffManager. Attached to DifficultyManager.
// This script limit the distance between the player and AI.
// AIs speed up if the player is too far ahead.
// AIs slow down if the player is too late.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DiffManager : MonoBehaviour
    {
        public bool         isInitDone = false;

        public bool         chaseAllowed = true;
        public bool         waitPlayerAllowed = true;

        public float        chasePlayerOneDistance = 100;
        public float        waitplayerDistance = 100;

        public float        chaseForceAppliedOffset;
        public float        chaseSpeedOffset;

        public bool         usePlayerOneInfoAsRefForAI = true;

        void Start()
        {
            #region
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        void OnDestroy()
        {
            #region
            StopAllCoroutines(); 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            yield return new WaitUntil(() => LapCounterAndPosition.instance.b_InitDone);

            int currentDifficulty = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentDifficulty;
            chaseForceAppliedOffset = DataRef.instance.difficultyManagerData.difficultyParamsList[currentDifficulty].chaseForceAppliedOffset;
            chaseSpeedOffset = DataRef.instance.difficultyManagerData.difficultyParamsList[currentDifficulty].chaseSpeedOffset;

            StartCoroutine(CheckDistanceAIToPlayerRoutine());

            isInitDone = true;
            yield return null; 
            #endregion
        }

        IEnumerator CheckDistanceAIToPlayerRoutine()
        {
            #region
            int howManyVehicles = LapCounterAndPosition.instance.posList.Count;

            for (var i = 1; i < howManyVehicles; i++)
            {
                float playerOneDistance = LapCounterAndPosition.instance.posList[0]._progression;
                float vehicleTested = LapCounterAndPosition.instance.posList[i]._progression;

                // Chase Player One
                if (chaseAllowed)
                {
                    if (playerOneDistance - vehicleTested > chasePlayerOneDistance)
                    {
                        bool isChaising = LapCounterAndPosition.instance.listVehicles[i].carAI.isChasingPlayerOne;
                        if (!isChaising)
                        {
                            // Debug.Log("Chaising Start");
                            LapCounterAndPosition.instance.listVehicles[i].carAI.carController.forwardForceAppliedRef += chaseForceAppliedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.maxSpeedRef += chaseSpeedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.isChasingPlayerOne = true;
                        }
                    }
                    else
                    {
                        bool isChaising = LapCounterAndPosition.instance.listVehicles[i].carAI.isChasingPlayerOne;
                        if (isChaising)
                        {
                            // Debug.Log("Chaising Ended");
                            LapCounterAndPosition.instance.listVehicles[i].carAI.carController.forwardForceAppliedRef -= chaseForceAppliedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.maxSpeedRef -= chaseSpeedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.isChasingPlayerOne = false;
                        }
                    }
                }

                // Wait Player One
                if (waitPlayerAllowed)
                {
                    if (vehicleTested - playerOneDistance > waitplayerDistance)
                    {
                        bool isWaiting = LapCounterAndPosition.instance.listVehicles[i].carAI.isWaitingPlayerOne;
                        if (!isWaiting)
                        {
                            // Debug.Log("Waiting Start");
                            LapCounterAndPosition.instance.listVehicles[i].carAI.carController.forwardForceAppliedRef -= chaseForceAppliedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.maxSpeedRef -= chaseSpeedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.isWaitingPlayerOne = true;
                        }
                    }
                    else
                    {
                        bool isWaiting = LapCounterAndPosition.instance.listVehicles[i].carAI.isWaitingPlayerOne;
                        if (isWaiting)
                        {
                            //  Debug.Log("Waiting Ended");
                            LapCounterAndPosition.instance.listVehicles[i].carAI.carController.forwardForceAppliedRef += chaseForceAppliedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.maxSpeedRef += chaseSpeedOffset;
                            LapCounterAndPosition.instance.listVehicles[i].carAI.isWaitingPlayerOne = false;
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1);

            StartCoroutine(CheckDistanceAIToPlayerRoutine());
            yield return null; 
            #endregion
        }
    }

}
