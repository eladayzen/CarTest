// Description: ActionsWhenRaceEnded. Attached to the vehicle. 
// Called when the race ended
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class ActionsWhenRaceEnded : MonoBehaviour
    {
        public bool         b_InitDone;
        private bool        b_InitInProgress;
        VehicleInfo         vehicleInfo;

        public UnityEvent   finishTheRace;


        //-> Initialisation
        public bool bInitActionsWhenRaceEnded()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(InitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            if (LapCounterAndPosition.instance)
            {
                LapCounterAndPosition.instance.AVechicleFinishTheRace += VechicleFinishTheRace;
                vehicleInfo = GetComponent<VehicleInfo>();
            }

            b_InitDone = true;
            //Debug.Log("Init: VehicleDamage -> Done");
            yield return null; 
            #endregion
        }

        public void OnDestroy()
        {
            #region
            if (LapCounterAndPosition.instance && LapCounterAndPosition.instance.AVechicleFinishTheRace != null)
                LapCounterAndPosition.instance.AVechicleFinishTheRace -= VechicleFinishTheRace; 
            #endregion
        }

        public void VechicleFinishTheRace(int vehicleID)
        {
            #region
            if (vehicleID == vehicleInfo.playerNumber)
                finishTheRace?.Invoke(); 
            #endregion
        }
    }

}
