using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class AudioWhoosh : MonoBehaviour
    {
        public AudioClip    acWhoosh;
        public float        volume = 1;
        public float        minSpeed = 40;

        public UnityEvent   OnTriggerEnterDoSomething;

        Collider            col;

        //public LayerMask vehicleLayer;

        private void OnTriggerEnter(Collider other)
        {
            #region
            col = other;

            //Debug.Log(other.name);

            OnTriggerEnterDoSomething?.Invoke();
            #endregion
        }

        public void PlayWhoosh()
        {
            #region
            //Debug.Log("Play A Whoosh A");
            if (IsItAPlayer())
            {
                if(CarSpeed() >= minSpeed)
                    SoundFxManager.instance.Play(acWhoosh, volume);

                //Debug.Log("Play A Whoosh B");
            }
            #endregion
        }

        bool IsItAPlayer()
        {
            #region
            if (col.GetComponent<VehicleTriggerTag>() &&
                col.transform.parent.parent.parent.GetComponent<VehicleInfo>().playerNumber == 0)
                return true;
            else
                return false;
            #endregion
        }

        float CarSpeed()
        {
            #region
            return col.transform.parent.parent.parent.GetComponent<Rigidbody>().linearVelocity.magnitude;
            #endregion
        }
    }

}
