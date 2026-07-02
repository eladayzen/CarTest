// Description: CarState. Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarState : MonoBehaviour
    {
        public enum CarPlayerType   { Human,AI};
        public enum CarDirection    { Forward, Backward,Stop };
        public enum CarForceApply   { Accelerate, Brake, Nothing };
        public enum CarTouchedSurface { IsGround, InAir, OnBack };
        public enum CarDrift        { NoDrift, Drift };
        public enum CarSkid         { NoSkid, Skid };
        public enum CarGear         { Neutral, Gear1, Gear2, Gear3, Gear4, Gear5, Revers };

        public enum CarRespawn      { Nothing, Respawn };

        [Header("Car Type")]
        public CarPlayerType        carPlayerType = CarPlayerType.Human;

        // Additive control assist flag, independent of carPlayerType/currentGameMode.
        // When true, CarPlayerInputs suppresses its own free-steering loop so
        // CarPathFollowPlayerInput is the sole driver of this vehicle.
        [HideInInspector]
        public bool                 isPathFollowAssistEnabled = false;

        [Header("Car Inputs")]
        public CarSteeringDirection steeringDir;
        [HideInInspector]
        public CarSteeringDirection lastSteeringDir;
        public CarMoveDirection     moveDir;
        [HideInInspector]
        public CarMoveDirection     lastMoveDir;

        [Header("Car Behaviour")]
        public CarDirection         carDirection = CarDirection.Forward;
        public CarForceApply        carForceApply = CarForceApply.Nothing;
        public CarTouchedSurface    carTouchedSurface = CarTouchedSurface.IsGround;
        public CarDrift             carDrift = CarDrift.NoDrift;
        public CarSkid              carSkid = CarSkid.NoSkid;
        public CarGear              carGear = CarGear.Neutral;
        public CarRespawn           carRespawn = CarRespawn.Nothing;

        Rigidbody                   rb;
        CarController               carController;
        Vector3                     currentRBVelocity = Vector3.zero;

        public void Start()
        {
            #region 
            Init(); 
            #endregion
        }

        private void Init()
        {
            #region 
            rb = GetComponent<Rigidbody>();
            carController = GetComponent<CarController>(); 
            #endregion
        }

        private void Update()
        {
            #region 
            currentRBVelocity = rb.linearVelocity;
            CarDirectionCheck();
            CarForceApplyCheck();
            CarTouchedSurfaceCheck();
            CarSkidCheck(); 
            #endregion
        }

        void CarSkidCheck()
        {
            #region 
            float sidewayVel = Vector3.Dot(rb.transform.right, currentRBVelocity);

            if (Mathf.Abs(sidewayVel) >= 7 && carSkid == CarSkid.NoSkid)
                carSkid = CarSkid.Skid;
            else if (Mathf.Abs(sidewayVel) < 7 && carSkid == CarSkid.Skid)
                carSkid = CarSkid.NoSkid; 
            #endregion
        }

        void CarDirectionCheck()
        {
            #region 
            if (rb)
            {
                float forwardRBVel = carController.forwardRBVel;

                if (forwardRBVel > 1.1f)
                    carDirection = CarDirection.Forward;
                else if (forwardRBVel < -1.1f)
                    carDirection = CarDirection.Backward;
                else
                    carDirection = CarDirection.Stop;
            } 
            #endregion
        }

        void CarForceApplyCheck()
        {
            #region 
            if (carDirection == CarDirection.Forward && carController.acceleration > 0 ||
                   carDirection == CarDirection.Backward && carController.acceleration < 0)
                carForceApply = CarForceApply.Accelerate;
            else if (carDirection == CarDirection.Forward && carController.acceleration < 0 ||
               carDirection == CarDirection.Backward && carController.acceleration > 0)
                carForceApply = CarForceApply.Brake;
            else
                carForceApply = CarForceApply.Nothing; 
            #endregion
        }

        void CarTouchedSurfaceCheck()
        {
            #region
            if (carController)
            {
                if (carController.IsCarOnTheGround())
                    carTouchedSurface = CarTouchedSurface.IsGround;
                else if (!carController.IsCarOnTheGround())
                    carTouchedSurface = CarTouchedSurface.InAir;
                //else
                // Car on its back
            } 
            #endregion
        }
    }

    public enum CarSteeringDirection { Left, Right, Center }
    public enum CarMoveDirection { forward, backward, center }
}
