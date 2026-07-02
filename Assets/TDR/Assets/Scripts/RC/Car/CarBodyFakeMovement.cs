// Description: CarBodyFakeMovement. Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarBodyFakeMovement : MonoBehaviour
    {
        Rigidbody           rb;
        CarController       carController;
        CarState            carState;

        public Transform    grp_Body;
        float               bodyXAxis = 0;

        public float        fakeRollMax = 1.5f;
        public float        pitchSpeed = 5f;

        public float        fakePitchMax = 12;

        void Start()
        {
            #region
            rb = GetComponent<Rigidbody>();
            carController = GetComponent<CarController>();
            carState = GetComponent<CarState>();
            #endregion
        }

        void Update()
        {
            #region
            CarFakeRoll();
            CarFakePitch();
            #endregion
        }
    
        void CarFakeRoll()
        {
            #region
            float sidewayForce = SidewayForce();

            sidewayForce = Mathf.Clamp(sidewayForce, -fakeRollMax, fakeRollMax);

            grp_Body.localEulerAngles = new Vector3(grp_Body.localRotation.eulerAngles.x, grp_Body.localRotation.eulerAngles.y, -sidewayForce * .35f);

            #endregion
        }

        void CarFakePitch()
        {
            #region
            float carSpeedAmount = rb.linearVelocity.magnitude / carController.maxSpeed;

            carSpeedAmount = Mathf.Clamp(carSpeedAmount, 0, 1f);

            if (carState.moveDir == CarMoveDirection.forward)
                bodyXAxis = Mathf.MoveTowards(bodyXAxis, -fakePitchMax * carSpeedAmount, Time.deltaTime * pitchSpeed);
            else if (carState.moveDir == CarMoveDirection.backward)
                bodyXAxis = Mathf.MoveTowards(bodyXAxis, fakePitchMax * carSpeedAmount, Time.deltaTime * pitchSpeed);
            else
                bodyXAxis = Mathf.MoveTowards(bodyXAxis, 0, Time.deltaTime * 5);

            grp_Body.localEulerAngles = new Vector3(bodyXAxis, grp_Body.localRotation.eulerAngles.y, grp_Body.localRotation.eulerAngles.z);
            #endregion
        }

        float SidewayForce()
        {
            #region
            float xVel = 0;
            if (rb)
            {
                Transform spring = carController.wheelsList[0].spring;
                xVel = Vector3.Dot(spring.right, rb.linearVelocity);
            }
            return xVel;
            #endregion
        }

    }

}
