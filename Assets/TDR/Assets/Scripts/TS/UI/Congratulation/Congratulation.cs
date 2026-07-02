// Description: Congratulation. Manage the Congratulation sequence.
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace TS.Generics
{
    public class Congratulation : MonoBehaviour
    {
        public int          playerID;
        public UnityEvent   CongratulationEvents = new UnityEvent();

        public void CongratulationSeq()
        {
            #region 
            CongratulationEvents?.Invoke(); 
            #endregion
        }

        public void CongratulationSeqWithDelay(float delay)
        {

            #region 
            bool isTrackLooped = VehiclesRef.instance.listVehicles[0].GetComponent<VehiclePathFollow>().Track.TrackIsLooped;
            if (!isTrackLooped)
                StartCoroutine(CongratulationSeqWithDelayRoutine(delay)); 
            #endregion
        }

        IEnumerator CongratulationSeqWithDelayRoutine(float delay)
        {
            #region 
            //Debug.Log("Congrat Part 2)");
            float t = 0;

            while (t < delay)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime;

                yield return null;
            }

            if (IsCongratulationPartTwoNeeded())
                CongratulationEvents?.Invoke();
            yield return null; 
            #endregion
        }

        bool IsCongratulationPartTwoNeeded()
        {
            #region 
            if (CheckEndOfTheRace.instance.bEndRaceOnce)
                return false;

            return true; 
            #endregion
        }
    }
}
