// Description: CarPlayerInputs. Return input activity. Attached to the vehicle
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarPlayerInputs : MonoBehaviour
    {
        public bool             AutoInit = true;
        [HideInInspector]
        public bool             b_InitDone;
        private bool            b_InitInProgress;
        private CarState        carState;
        private CarController   carController;
        private VehicleInfo     vehicleInfo;
        public CarSide          carSide;

        public int              TSInputKeyAcceleration = 5;
        public int              TSInputKeyBrake = 6;
        public int              TSInputKeyLeft = 3;
        public int              TSInputKeyRight = 4;
        public int              TSInputKeyDrift = 8;
        public int              TSInputKeyRespawn = 7;
        public int              TSInputBoolInvertUpDown = 0;

        public bool             isInputEnabled = true;

        void Start()
        {
            #region 
            if (AutoInit)
                StartCoroutine(InitRoutine()); 
            #endregion
        }

        public bool bInitVehicleInputs()
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
            carState = GetComponent<CarState>();
            carController = GetComponent<CarController>();
            vehicleInfo = GetComponent<VehicleInfo>();

            b_InitDone = true;
            //Debug.Log("Init: VehicleInputs -> Done");
            yield return null; 
            #endregion
        }

        void Update()
        {
            #region 
            if (isInputsAllowed())
            {
                CheckInputs();
                MoveTheCar();
            } 
            #endregion
        }

        bool isInputsAllowed()
        {
            #region
            if (b_InitDone &&
                isPlayerAHuman() &&
                isInputEnabled)
                return true;

            return false;
            #endregion
        }

        void MoveTheCar()
        {
            #region

            carController.CarMoveParameters(ReturnPlayerSteerRatio(), ReturnPlayerAccelerationRatio());
            #endregion
        }

        void CheckInputs()
        {
            #region 
            CheckSteeringDir();
            CheckForwardBackwardDir();
            CheckDrifting(); 
            #endregion
        }

        bool isPlayerAHuman()
        {
            #region
            if (!InfoRememberMainMenuSelection.instance 
                ||
                
                InfoRememberMainMenuSelection.instance &&
                carState.carPlayerType == CarState.CarPlayerType.Human &&
                vehicleInfo.playerNumber < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer)
                return true;
            else
                return false;
            #endregion
        }

        void CheckForwardBackwardDir()
        {
            #region
            bool upKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyAcceleration].b_GetKeyDown;
            bool downKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyBrake].b_GetKeyDown;
           
            if (upKey)
            {
                carState.lastMoveDir = carState.moveDir;
                carState.moveDir = CarMoveDirection.forward;
            }

            if (downKey)
            {
                carState.lastMoveDir = carState.moveDir;
                carState.moveDir = CarMoveDirection.backward;
            }

            if (!upKey &&
               !downKey)
            {
                // lastMoveDir = moveDir;
                carState.moveDir = CarMoveDirection.center;
            }
            #endregion
        }

        void CheckSteeringDir()
        {
            #region
            bool leftKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyLeft].b_GetKeyDown;
            bool rightKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyRight].b_GetKeyDown;

            carState.lastSteeringDir = carState.steeringDir;
            if (leftKey/* && !carSide.wheelsList[5].isObstacle && !carSide.wheelsList[3].isObstacle*/)
            {
                carState.steeringDir = CarSteeringDirection.Left;
            }
            else if (rightKey/* && !carSide.wheelsList[4].isObstacle && !carSide.wheelsList[1].isObstacle*/)
            {
                carState.steeringDir = CarSteeringDirection.Right;
            }
            else
            {
                carState.steeringDir = CarSteeringDirection.Center;
            }
            #endregion
        }

        void CheckDrifting()
        {
            #region
            if (carState.moveDir != CarMoveDirection.backward)
            {
                bool driftKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyDrift].b_GetKeyDown;


                if (!driftKey && carState.carDrift != CarState.CarDrift.NoDrift)
                {
                    carState.carDrift = CarState.CarDrift.NoDrift;
                }
                if (driftKey &&
                    carState.carDrift == CarState.CarDrift.NoDrift)
                {
                    carState.carDrift = CarState.CarDrift.Drift;
                }
            }
            else if (carState.carDrift == CarState.CarDrift.Drift)
            {
                carState.carDrift = CarState.CarDrift.NoDrift;
            }

            #endregion
        }

        float ReturnPlayerAccelerationRatio()
        {
            #region
            bool upKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyAcceleration].b_GetKeyDown;
            bool downKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyBrake].b_GetKeyDown;

            if (downKey)
            {
                return ReturnDownInputValue() * -1;
            }
            else if (upKey)
            {
                return ReturnUpInputValue() * 1;
            }
            else
                return 0;
            #endregion
        }

        float ReturnPlayerSteerRatio()
        {
            #region
            bool leftKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyLeft].b_GetKeyDown;
            bool rightKey = InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyRight].b_GetKeyDown;

            if (leftKey/* && !carSide.wheelsList[2].isObstacle && !carSide.wheelsList[3].isObstacle*/)
            {
                return ReturnLeftInputValue() * -1;
            }
            else if (rightKey/* && !carSide.wheelsList[0].isObstacle && !carSide.wheelsList[1].isObstacle*/)
            {
                return ReturnRightInputValue() * 1;
            }
            else
                return 0;
            #endregion
        }

        public float ReturnLeftInputValue()
        {
            #region 
            return Mathf.Abs(InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyLeft]._AxisCurrentValue); 
            #endregion
        }
        public float ReturnRightInputValue()
        {
            #region 
            return Mathf.Abs(InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyRight]._AxisCurrentValue); 
            #endregion
        }

        public float ReturnUpInputValue()
        {
            #region 
            return Mathf.Abs(InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyAcceleration]._AxisCurrentValue); 
            #endregion
        }
        public float ReturnDownInputValue()
        {
            #region 
            return Mathf.Abs(InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyBrake]._AxisCurrentValue); 
            #endregion
        }

    }

}

