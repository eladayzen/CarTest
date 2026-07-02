// Description: CarRaceEndedAssistant. Attached to the vehicle.
// Called by ActionWhenRaceEnded script attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarRaceEndedAssistant : MonoBehaviour
    {
        VehicleInfo vehicleInfo;

        public void EndOfTheRace()
        {
        }

        public void CongratulationSeq()
        {
            #region
            vehicleInfo = GetComponent<VehicleInfo>();
            int playerID = vehicleInfo.playerNumber;

            Congratulation[] congratulations = FindObjectsByType<Congratulation>(FindObjectsSortMode.None);

            foreach (Congratulation obj in congratulations)
            {
                if (obj.playerID == playerID)
                    obj.CongratulationSeq();
            } 
            #endregion
        }

        public void PlayMusic(int ID = 0)
        {
            #region MyRegion
            if (ID == -1)
                MusicManager.instance.MFadeOut(1);
            else
            {
                if (MusicList.instance.ListAudioClip.Count > ID)
                    MusicManager.instance.MCrossFade(1, MusicList.instance.ListAudioClip[ID]);
            } 
            #endregion
        }

        public void DisablePlayerInputs()
        {
            #region
            InfoPlayerTS.instance.b_IsAvailableToDoSomething = false; 
            #endregion
        }

      
        public void DrivedByAI()
        {
            #region
            StartCoroutine(DrivedByAIRoutine()); 
            #endregion
        }
       
        IEnumerator DrivedByAIRoutine()
        {
            #region
            float mag = GetComponent<Rigidbody>().linearVelocity.magnitude;
            GetComponent<CarAI>().currentspeedObstacle = mag;

            yield return new WaitUntil(() => GetComponent<CarAI>().currentspeedObstacle == mag);

            GetComponent<CarState>().carPlayerType = CarState.CarPlayerType.AI;

            GetComponent<CarState>().carDrift = CarState.CarDrift.NoDrift;

            GetComponent<CarAI>().StopAIIfTrackNotLooped();

            yield return null;
            #endregion
        }
    }
}
