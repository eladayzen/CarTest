// Description: CheckEndOfTheRace: When a vehicle finish the race check if the game must display the result menu
// In Hierarchy -> GAMEPLAY -> MANAGERS ->
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CheckEndOfTheRace : MonoBehaviour
    {
        public static CheckEndOfTheRace instance = null;
        public bool                     b_InitDone;
        private bool                    b_InitInProgress;
        [HideInInspector]
        public bool                     bEndRaceOnce = false;

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        private void Start()
        {
            #region 
            LapCounterAndPosition.instance.AVechicleFinishTheRace += CheckEndOfTheTheRace; 
            #endregion
        }

        private void OnDestroy()
        {
            #region 
            LapCounterAndPosition.instance.AVechicleFinishTheRace -= CheckEndOfTheTheRace; 
            #endregion
        }

        //-> Init Lap counter
        public bool bInitLapCounter()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(bInitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        IEnumerator bInitRoutine()
        {
            #region
            b_InitDone = false;

           
            b_InitDone = true;
            yield return null;
            #endregion
        }

        //-> When a vehicle finish the race check if the game must display the result menu (SceneStepsManager.instance.NextStep())     
        void CheckEndOfTheTheRace(int playerID)
        {
            #region 
            Debug.Log("The Race is Complete");
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            bool bIsRaceComplete = true;
            LapCounterAndPosition lapCounterAndPosition = LapCounterAndPosition.instance;

            for (var i = 0; i < howManyPlayer; i++)
            {
                if (!lapCounterAndPosition.posList[i].IsRaceComplete)
                {
                    bIsRaceComplete = false;
                    break;
                }
            }


            if (bIsRaceComplete && !bEndRaceOnce)
            {
                //Debug.Log("The Race is Complete: howManyPlayer ->" + howManyPlayer + " | bIsRaceComplete -> " + bIsRaceComplete);
                PauseManager.instance.isPauseModeEnable = false;
                bEndRaceOnce = true;
                SceneStepsManager.instance.NextStep();
            } 
            #endregion
        }
    }
}
