using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class HingeAssistant : MonoBehaviour
    {
        private HingeJoint Hing;

        void Start()
        {
            #region 
            Hing = GetComponent<HingeJoint>(); 
            #endregion
        }

        public void NewlimitMin(float value)
        {
            #region 
            if (Hing)
            {
                JointLimits limits = Hing.limits;
                limits.min = value;
                Hing.limits = limits;
            } 
            #endregion
        }

        public void NewlimitMax(float value)
        {
            #region 
            if (Hing)
            {
                JointLimits limits = Hing.limits;
                limits.max = value;
                Hing.limits = limits;
            } 
            #endregion
        }

        public void ManuallyBreakHingJoint()
        {
            #region 
            Destroy(GetComponent<HingeJoint>());
            GetComponent<Rigidbody>().linearVelocity *= 0; 
            #endregion
        }
    }
}
