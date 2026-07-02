// Description: SteeringWheel. Manage steering wheel. Attached to Grp_SteeringWheel inside the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class SteeringWheel : MonoBehaviour
    {
        public CarState     carState;
        public Transform    steeringWheel;
        public Transform    steeringWheelRef01;
        public Transform    steeringWheelRef02;
        public Transform    steeringWheelRef03;

        public float        maxRotationAngle = 150;
        public float        speed = 360;
        
        void Update()
        {
            #region 
            if (carState.carPlayerType == CarState.CarPlayerType.Human)
            {
                Quaternion target = steeringWheelRef01.rotation;

                if (carState.steeringDir == CarSteeringDirection.Left)
                    target = steeringWheelRef02.rotation;
                else if (carState.steeringDir == CarSteeringDirection.Right)
                    target = steeringWheelRef03.rotation;

                steeringWheel.rotation = Quaternion.RotateTowards(steeringWheel.rotation, target, Time.deltaTime * speed);
            } 
            #endregion
        }
    }
}
