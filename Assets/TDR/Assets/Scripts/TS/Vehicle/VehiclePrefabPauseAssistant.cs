// Description: VehiclePrefabPauseAssistant. Methods called by VehiclePrefabPause (Connected via the Inspector)
using UnityEngine;

namespace TS.Generics
{
    public class VehiclePrefabPauseAssistant : MonoBehaviour
    {
        public Rigidbody    vRb;
        public bool         remKinematicState;
        public Vector3      remVelocity;

       public bool PauseVehicle()
        {
            #region
            //Debug.Log("Pause Vehicle");
            return true; 
            #endregion
        }

        public bool UnPauseVehicle()
        {
            #region
            //Debug.Log("UnPause Vehicle");
            return true; 
            #endregion
        }

        public bool PauseMovement()
        {
            #region
            remKinematicState = vRb.isKinematic;
            remVelocity = vRb.linearVelocity;
            vRb.linearVelocity = Vector3.zero;
            vRb.isKinematic = true;

            return true; 
            #endregion
        }

        public bool UnpauseMovement()
        {
            #region
            vRb.isKinematic = remKinematicState;
            vRb.linearVelocity = remVelocity;
            return true; 
            #endregion
        }
    }
}