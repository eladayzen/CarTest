// Descritpion: OverrideAISpeed. Override the AI speed when the vehicle enter the trigger.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class OverrideAISpeed : MonoBehaviour
    {
        [Range(-1f, 1f)]
        public float offset = 0;

        void OnTriggerEnter(Collider other)
        {
            #region
            if (other.GetComponent<VehicleTriggerTag>())
            {
                //Debug.Log("Override AI Speed");
                other.transform.parent.parent.parent.GetComponent<VehiclePathFollow>().currentDiffOffset = -offset;

            }
            #endregion
        }
    }
}
