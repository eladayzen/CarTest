// Description: CarAI. Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class CarAI : MonoBehaviour
    {
        bool                                isInitDone = false;
        public Rigidbody                    m_Rigidbody;
        public CarController                carController;

        public float                        maxSpeedRef = 0;
        public bool                         isChasingPlayerOne = false;
        public bool                         isWaitingPlayerOne = false;

        float                               aiCurrentSpeedFactor = .6f;                 // Increase or reduce maximum vehicle speed depending the part of the track
        float                               aiLastSpeedFactor;

        [HideInInspector]
        public Transform                    targetOne;                                  // target on path to determine the angle between vahicle and track
        [HideInInspector]
        public Transform                    targetTwo;                                  // Second target on path to determine the angle between vahicle and track

        public float                        aiSteerSens = 0.2f;
        public float                        aiAccelSens = 0.075f;
        public float                        aiBrakeSens = .001f;

        public float                        desiredSpeed = 0;
        float                               steer;
        float                               acceleration = 0;

        float                               multiplier = 1;
        
        VehiclePathFollow                   vehiclePathFollow;
        VehicleInfo                         carControllerVehicleInfo;
        CarState                            carState;
        public Overtake                     overtakeZone = Overtake.None;

        public CarController                carThatOvertakeThisCar;
        [HideInInspector]
        public VehiclePathFollow            carThatOvertakeThisCarVehiclePathFollow;

        public CarController                nextCarOnTrack;
        [HideInInspector]
        public float                        nextCarOnTrackDistance;
        VehiclePathFollow                   nextCarOnTrackVehiclePathFollow;
        CarAI                               nextCarOnTrackCarAI;
        VehicleInfo                         nextCarOnTrackVehicleInfo;


        [System.Serializable]
        public class DetectorParams
        {
            public float            frontRayDistance = 3;
            public List<Transform>  frontDetector = new List<Transform>();
            public float            backRayDistance = 3;
            public List<Transform>  backDetector = new List<Transform>();
            public float            leftRayDistance = 3;
            public List<Transform>  leftDetector = new List<Transform>();
            public float            rightRayDistance = 3;
            public List<Transform>  rightDetector = new List<Transform>();
        }

        public DetectorParams               detectors;
        public float                        offsetLength = 40;

        public LayerMask                    sideDetectionLayer;
        public List<int>                    sideDetectionLayerMaskList = new List<int>();   // Default | Vehicle

        public float                        overtakeOffsetPath  = 0f;
        float                               currentOffsetObstacle = 0;
        [HideInInspector]
        public float                        currentspeedObstacle = 0;
        int                                 dir = 1;
        [HideInInspector]
        public float                        checkMag = 0;

        public bool                         IsCarStuck = false;
        float                               chekIfCarStuckTimer = 0;

        public float                        TimerStart = 5;
        public float                        offsetDuringStartTimer = 1.5f;
        public float                        offsetDuringStartTimerSpeed = 2.5f;
        //[HideInInspector]
        public float                        timerStartCounter = 0;
        [HideInInspector]
        public float                        StartDir = 1;

        public bool                         IsStartPhaseDone = false;

        // TODO: Delete at the end if no issue
        public List<Transform>              frontList = new List<Transform>();

        public LayerMask                    detectionLayer;
        public List<int>                    detectionLayerMaskList = new List<int>();   //  Vehicle

        public float                        distanceDetector = 5;
        public AnimationCurve               detectorCurve;

        public enum CarAiState              { Nothing, Follow, Overtake, Overtaken };
        public CarAiState                   aiState = CarAiState.Nothing;
       
        [HideInInspector]
        public int                          closestObstaclePathPos = 0;
        int                                 closestSpotOnObstaclePath = 0;
        [HideInInspector]
        public PathObstacle                 PathObs;

        public enum ObstacleState           { Neutral, OneWayFree, NoFreeWay };
        public ObstacleState                ObsState = ObstacleState.Neutral;

        public LayerMask                    obstacleLayer;
        public List<int>                    obstacleLayerMaskList = new List<int>();   //  9 Vehicle | 14 Border

        [HideInInspector]
        public bool                         IsTouched = false;

        [HideInInspector]
        public List<CarController>          carList = new List<CarController>();
        [HideInInspector]
        public List<VehiclePathFollow>      carPathFollowList = new List<VehiclePathFollow>();
        public int                          carListID = 0;                          // Refer to Id of this car in carList 

        public DetectCarAhead               detectCarAhead;
        public DetectCarAhead               detectCarAltPath;

        float                               forwardOnlyObstacleDistance = 0;

        public int                          updateAiStateInterval = 3;
        float                               distanceObstacleLeft = 0;
        float                               distanceObstacleRight = 0;
        float                               distanceObstacleBack = 0;
       
        public ObstacleState                currentObsState;

        public float                        avoidObstaclePathOffset = 4.5f;

        float                               ObstacleSlowDown = 0;

        public LayerMask                    LayerVehicle;
        public List<int>                    LayerVehicleMaskList = new List<int>();   //  9 Vehicle

        public AnimationCurve               PreventCollisionCurve;
        float                               ratioLeft = 0;
        float                               ratioRight = 0;
        public Transform                    ApplyLeft;
        public Transform                    ApplyRight;
        public Transform                    ApplyFront;
        public Transform                    ApplyBack;

        [HideInInspector]
        public float                        gridPosition = 0;

        int                                 howManyPlayerInTheRace;

        public Transform                    refCarSize;

        float                               forwardForceAppliedRef = 0;

        public float                        OffsetForceApplyDependingDifficulty = 0;
        public float                        extraBoostAI00 = 0;
        float                               refExtraBoostAI00 = 0;
        enum ReverseState                   { None,Backward,Forward};
        ReverseState                        reversSequence = ReverseState.None;
        bool                                respawnAutomaticaly = false;

        float                               lastAccSens = .75f;

        float                               currentOffset = 0;
        float                               forceResetDuration = 0;

        [HideInInspector]
        public float                        increaseForceDependingTurnAngle = 0;

        void Start()
        {
            #region
            Init();
            #endregion
        }

        void Init()
        {
            #region
            sideDetectionLayer          = InitLayerMask(sideDetectionLayerMaskList);
            detectionLayer              = InitLayerMask(detectionLayerMaskList);
            obstacleLayer               = InitLayerMask(obstacleLayerMaskList);
            LayerVehicle                = InitLayerMask(LayerVehicleMaskList);

            carState                    = GetComponent<CarState>();
            vehiclePathFollow           = carController.GetComponent<VehiclePathFollow>();
            carControllerVehicleInfo    = GetComponent<VehicleInfo>();

            int currentDifficulty       = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentDifficulty;
            float difficultySpeedOffset = DataRef.instance.difficultyManagerData.difficultyParamsList[currentDifficulty].globalSpeedOffset;
            maxSpeedRef                 = carController.maxSpeed  + difficultySpeedOffset;



            PathObs                     = FindFirstObjectByType<PathObstacle>();

            overtakeOffsetPath          = overtakeOffsetPath + refCarSize.transform.localScale.x * 2f;

            if (LapCounterAndPosition.instance)
            {
                refExtraBoostAI00 = extraBoostAI00 + LapCounterAndPosition.instance.extraForceAppliedToAiInThisRace;

                TimerStart = LapCounterAndPosition.instance.overrideStartPart_Duration;
                offsetDuringStartTimerSpeed = LapCounterAndPosition.instance.overrideStartPart_Speed;
            }

            StartCoroutine(CreateCarListRoutine());

            isInitDone = true;
            #endregion
        }

        IEnumerator CreateCarListRoutine()
        {
            #region 
            if (VehiclesRef.instance)
            {
                howManyPlayerInTheRace = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

                carList.Clear();
                carPathFollowList.Clear();
                yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);
                for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
                {
                    carList.Add(VehiclesRef.instance.listVehicles[i].GetComponent<CarController>());
                    carPathFollowList.Add(VehiclesRef.instance.listVehicles[i].GetComponent<VehiclePathFollow>());
                    if (VehiclesRef.instance.listVehicles[i].gameObject == this.gameObject)
                    {
                        carListID = i;
                    }
                }

                int carID = transform.parent.GetComponent<VehiclePrefabInit>().startGridPosition;
                int startGridOffsetGridLength = StartLine.instance.listOffsetOnGrid.Count;
                int gridPos = carID % startGridOffsetGridLength;
                float startLineXOffsetDependingPositionOnGrid = StartLine.instance.listOffsetOnGrid[gridPos].x * -1;
                offsetDuringStartTimer = startLineXOffsetDependingPositionOnGrid;

                int currentDifficulty = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentDifficulty;
                OffsetForceApplyDependingDifficulty = DataRef.instance.difficultyManagerData.difficultyParamsList[currentDifficulty].forceAppliedOffset;

                yield return new WaitUntil(() => transform.parent.GetComponent<VehiclePrefabInit>().b_InitDone);
                if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 1 && howManyPlayerInTheRace == 1) 
                    extraBoostAI00 += 1000;
                if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 2 && howManyPlayerInTheRace == 2)
                    extraBoostAI00 += 1000;

                float difficultySpeedOffset = DataRef.instance.difficultyManagerData.difficultyParamsList[currentDifficulty].globalSpeedOffset;
                DiffManager diffManager = FindFirstObjectByType<DiffManager>();

                if(diffManager && diffManager.usePlayerOneInfoAsRefForAI)
                {
                    yield return new WaitUntil(() => transform.GetComponent<CarController>().isInitDone);
                    carController.forwardForceApplied = VehiclesRef.instance.listVehicles[0].GetComponent<CarController>().forwardForceAppliedRef;
                    carController.forwardForceAppliedRef = VehiclesRef.instance.listVehicles[0].GetComponent<CarController>().forwardForceAppliedRef;
                    forwardForceAppliedRef = VehiclesRef.instance.listVehicles[0].GetComponent<CarController>().forwardForceAppliedRef;
                    carController.maxSpeed = VehiclesRef.instance.listVehicles[0].GetComponent<CarController>().maxSpeed;
                }
                maxSpeedRef = carController.maxSpeed + difficultySpeedOffset;

            }
            yield return null; 
            #endregion
        }

        public bool IsPlayerAI()
        {
            #region
            if (carState.carPlayerType == CarState.CarPlayerType.AI)
                return true;
            else
                return false;
            #endregion
        }

        void Update()
        {
            #region
            if(isInitDone && IsPlayerAI() && !PauseManager.instance.Bool_IsGamePaused)
            {
                if (Time.frameCount % 4 == carControllerVehicleInfo.playerNumber % 4)
                {
                    closestSpotOnObstaclePath = CloseFromPathObstaclePosition();

                    forwardOnlyObstacleDistance = ForwardOnlyObstacleDistance();

                    distanceObstacleLeft = IsObstacleOnCarSidesDistance(carController, -1);
                    distanceObstacleRight = IsObstacleOnCarSidesDistance(carController, 1);
                    distanceObstacleBack = BackOnlyObstacleDistance();

                    UpdateCarState();

                    NextCar();

                    // if (Time.frameCount % updateAiStateInterval == 0)
                    UpdateAIState();

                    if (!LapCounterAndPosition.instance.posList[carListID].IsRaceComplete)
                    {
                        CheckIfCarIsStuck();
                        StartPart();
                    }

                    UpdateForceAppliedToVehicleDependingDifficulty();
                }
                    
            }
            #endregion
        }

        public float extraBoostDependingSurface = 0;
        void UpdateForceAppliedToVehicleDependingDifficulty()
        {
            #region 
            if (carListID >= howManyPlayerInTheRace)
            {
                extraBoostDependingSurface = 900 - carController.surfaceData.surfaceList[carController.mostUseSurface].gripAmount * 1000;
                extraBoostDependingSurface = Mathf.Clamp(extraBoostDependingSurface, 0, extraBoostDependingSurface);

                float totalForceAppliedToAI = OffsetForceApplyDependingDifficulty + extraBoostAI00 + extraBoostDependingSurface;

                if (forwardForceAppliedRef + totalForceAppliedToAI != carController.forwardForceAppliedRef)
                    carController.forwardForceAppliedRef = Mathf.MoveTowards(
                        carController.forwardForceAppliedRef, 
                        forwardForceAppliedRef + totalForceAppliedToAI, 
                        Time.deltaTime * (totalForceAppliedToAI) * .1f);

             
            } 
            #endregion
        }

        void StartPart()
        {
            #region
            if(!IsStartPhaseDone && timerStartCounter < TimerStart)
            {
                timerStartCounter += Time.deltaTime;
                vehiclePathFollow.UpdateOffsetPathPosition( offsetDuringStartTimer);
            }
            else if(!IsStartPhaseDone && currentObsState != ObstacleState.Neutral)
            {
                vehiclePathFollow.UpdateOffsetPathPosition(0, 0);

                IsStartPhaseDone = true;

                extraBoostAI00 = refExtraBoostAI00;
            }
            else if(!IsStartPhaseDone)
            {
                if (carListID == howManyPlayerInTheRace
                ||
                VehiclesRef.instance.listVehicles[carListID - 1].GetComponent<CarAI>().IsStartPhaseDone
                ||
                !IsObstacleOnCarSides(carController, 0))
                {
                    if (offsetDuringStartTimer < 0 && !IsObstacleOnCarSides(carController, -1))
                        offsetDuringStartTimer = Mathf.MoveTowards(offsetDuringStartTimer, 0, Time.deltaTime* offsetDuringStartTimerSpeed);

                    if (offsetDuringStartTimer > 0 && !IsObstacleOnCarSides(carController, 1))
                        offsetDuringStartTimer = Mathf.MoveTowards(offsetDuringStartTimer, 0, Time.deltaTime* offsetDuringStartTimerSpeed);

                    vehiclePathFollow.UpdateOffsetPathPosition(offsetDuringStartTimer, 0);

                    if (offsetDuringStartTimer == 0)
                        IsStartPhaseDone = true;

                    extraBoostAI00 = refExtraBoostAI00;
                }
            }
            #endregion
        }

        void CheckIfCarIsStuck()
        {
            #region
            if (carState.carDirection == CarState.CarDirection.Stop &&
                !IsCarStuck &&
                !carController.vehicleInfo.b_IsRespawn)
            {
                chekIfCarStuckTimer += Time.deltaTime;
                if(chekIfCarStuckTimer > 2)
                {
                   
                    if (respawnAutomaticaly)
                    {
                        IsCarStuck = false;
                        chekIfCarStuckTimer = 0;
                        carController.vehicleDamage.VehicleExplosionAction.Invoke();
                    }
                    else
                    {
                        IsCarStuck = true;
                        StartCoroutine(CarIsStuckRoutine());
                    }
                }    
            }
            #endregion
        }


        private void FixedUpdate()
        {
            #region
            if (targetOne && isInitDone && IsPlayerAI() && !PauseManager.instance.Bool_IsGamePaused)
            {
                checkMag = m_Rigidbody.linearVelocity.magnitude;

                TakeCareOfObstacles();

                AISensitivity();

                if (reversSequence != ReverseState.Backward)
                    acceleration = DesiredAcceleration();

                if(reversSequence != ReverseState.Backward)
                    steer = DesiredSteer();

                UpdateCarMoveParameters(steer, acceleration);

                AddForceSideToPreventCollision();
                AddForceFrontToPreventCollision();
            }
            #endregion
        }

        void AISensitivity()
        {
            #region
            /*float speedRatio = m_Rigidbody.velocity.magnitude / 70;
             if (aiState == CarAiState.Overtake)
             {
                 aiSteerSens = .05f;
             }
             else
             {

             }*/
            /* aiSteerSens = (speedRatio) * .1f;
             aiSteerSens = Mathf.Clamp(aiSteerSens, .05f, .1f);*/
            #endregion
        }

        void UpdateCarMoveParameters(float _steer, float _acceleration)
        {
            #region 
            if (IsAiOvertakenByACar() && IsStartPhaseDone)
                carController.CarMoveParameters(_steer, _acceleration * .75f);
            else
                carController.CarMoveParameters(_steer, _acceleration); 
            #endregion
        }

        public float DesiredSteer()
        {
            #region
            Vector3 offsetTargetPos = targetOne.position;

            Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            float desiredSteer = Mathf.Clamp(targetAngle * aiSteerSens, -1, 1) * Mathf.Sign(m_Rigidbody.linearVelocity.magnitude);

            return desiredSteer;
            #endregion
        }

        public float DesiredAcceleration()
        {
            #region
            // Choose the desired speed
            desiredSpeed = CautiousNeededDependingOnCornerAngle();

            // Choose Brake sensibility
            float accelBrakeSensitivity = (desiredSpeed < m_Rigidbody.linearVelocity.magnitude) ? aiBrakeSens: aiAccelSens;

            desiredSpeed = CautiousNeededDependingPathDifficultyList();

            desiredSpeed = DesiredSpeedDependingNextCarOnTrackDistance();

            // Choose the amount of acceleration
            float desiredAcceleration = Mathf.Clamp((desiredSpeed - m_Rigidbody.linearVelocity.magnitude) * accelBrakeSensitivity, -1, 1);

            desiredAcceleration = DesiredAccelerationDependingObstacleOnTrack(desiredAcceleration);

            desiredAcceleration = Mathf.Clamp(desiredAcceleration, -1, 1);
       
            // Check if the car is touched
            if (IsTouched)
            {
                m_Rigidbody.linearVelocity *= 0f;
                if (m_Rigidbody.linearVelocity.magnitude > 20)
                    desiredAcceleration = -2;
                else
                    desiredAcceleration = 0;
            }
           
            // Check if there is no car when altPath Ended 
            if (detectCarAhead.IsCollisionDetected())
            {
                if (m_Rigidbody.linearVelocity.magnitude > 20)
                    desiredAcceleration = -2;
                else
                    desiredAcceleration = 0;
            }
           
            return desiredAcceleration;
            #endregion
        }
        float CautiousNeededDependingOnCornerAngle()
        {
            #region
            float maxSpeed = carController.maxSpeed + 10;

            Vector3 fwd = transform.forward;
            if (m_Rigidbody.linearVelocity.magnitude > maxSpeed * 0.1f)
                fwd = m_Rigidbody.linearVelocity;

            // Check if there is a big angle between vehicle and the first target
            float angleBetweenVehicleAndTrack = Vector3.Angle(targetOne.forward, fwd);

            float speedRatio = m_Rigidbody.linearVelocity.magnitude / 70;
            speedRatio = 1 - speedRatio;
            float angle = Mathf.Lerp(1, 180, speedRatio);

            float cautiousnessRequired = Mathf.InverseLerp(0, angle, angleBetweenVehicleAndTrack);
            // Check if there is a big angle between vehicle and the second target (increase the ability to know if the AI is close to an area with big corners)
            float secondAngleBetweenVehicleAndTrack = Vector3.Angle(targetOne.forward, targetTwo.forward);
            float cautiousnessRequiredSecondTarget = Mathf.InverseLerp(0, 180, secondAngleBetweenVehicleAndTrack);

            //ratio = (1000 * carController.forwardForceAppliedRef) / 6000;
            increaseForceDependingTurnAngle = 1000 * (cautiousnessRequiredSecondTarget + cautiousnessRequired);
            carController.carAIOffset = increaseForceDependingTurnAngle;

            float speedFactorTarget = 1 - Mathf.Abs(DesiredSteer());

            speedFactorTarget = Mathf.Clamp(speedFactorTarget, 0.1f, 1f);

            float tmp = aiCurrentSpeedFactor * 10;

            tmp = Mathf.MoveTowards(tmp,
                 Mathf.Clamp01(speedFactorTarget * 1.1f - cautiousnessRequiredSecondTarget) * 10,
               Time.deltaTime * multiplier);

            aiCurrentSpeedFactor = tmp * .1f;

            // Turn decrease
            if (aiLastSpeedFactor < aiCurrentSpeedFactor)
                multiplier = 1f;
            // Turn increase
            else
                multiplier = 5f;

            aiLastSpeedFactor = aiCurrentSpeedFactor;

            maxSpeed = Mathf.Lerp(maxSpeed, maxSpeed * aiCurrentSpeedFactor, cautiousnessRequired);
            maxSpeed = Mathf.Clamp(maxSpeed, 0, carController.maxSpeed);

            return maxSpeed;
            #endregion
        }
        public float maxCoefPathDiff = .3f;
        float CautiousNeededDependingPathDifficultyList()
        {
            #region
            float currentDifficultyValue = Mathf.Clamp(vehiclePathFollow.difficultyValue, 0, maxCoefPathDiff);
            currentDifficultyValue += vehiclePathFollow.currentDiffOffset;

            // Max Speed Depending Corner Angle and surface Grip
            float speedConstant = 15;// + 10 * (1-carController.surfaceData.surfaceList[carController.mostUseSurface].gripAmount);

            float refMaxCornerCarSpeed = 90;
            if (carController.maxSpeed < refMaxCornerCarSpeed)
                refMaxCornerCarSpeed = carController.maxSpeed;

            float maxSpeedDependingCornerAngle = speedConstant + (refMaxCornerCarSpeed + 10 - speedConstant) * (1- currentDifficultyValue);
            if (currentDifficultyValue <.1f)
                maxSpeedDependingCornerAngle = carController.maxSpeed;

            maxSpeedDependingCornerAngle = Mathf.Clamp(maxSpeedDependingCornerAngle, 0, carController.maxSpeed);

            maxSpeedDependingCornerAngle = Mathf.Clamp(maxSpeedDependingCornerAngle, speedConstant, 1000);
            //  Debug.Log("maxSpeedDependingCornerAngle: " + maxSpeedDependingCornerAngle);

            var cautiousAmount = Mathf.Clamp(desiredSpeed, 0, maxSpeedDependingCornerAngle);
          
            return cautiousAmount; 
            #endregion
        }
        float DesiredSpeedDependingNextCarOnTrackDistance()
        {
            #region

            if (nextCarOnTrack && nextCarOnTrack.carState.carPlayerType == CarState.CarPlayerType.AI)
            {
                float dist01 = vehiclePathFollow.progressDistance;
                float dist02 = nextCarOnTrackVehiclePathFollow.progressDistance;
                dist01 = dist02 < dist01 ? -(vehiclePathFollow.Track.pathLength - dist01) : dist01;
                float total = dist02 - dist01;

                //Debug.Log("Total: " + total + " | " + "dist01: " + dist01 + " | " + "dist02: " + dist02 + " | ");

                float currentDifficultyValue = nextCarOnTrackVehiclePathFollow.difficultyValue + nextCarOnTrackVehiclePathFollow.currentDiffOffset;

                float speedRatio = Mathf.Clamp01(carController.rb.linearVelocity.magnitude / 50);

                float refValue = (20 + 20 * currentDifficultyValue) * speedRatio;
                if (nextCarOnTrackCarAI.desiredSpeed <= desiredSpeed && 
                    total < refValue)
                {
                    if(aiState != CarAiState.Overtake 
                        ||
                       (aiState == CarAiState.Overtake && ForwardOnlyObstacleDistance(10) != 0)
                       )
                        desiredSpeed = nextCarOnTrackCarAI.desiredSpeed * (.9f);
                }
                else
                {
                    lastAccSens = 100 * aiAccelSens;
                    lastAccSens = Mathf.MoveTowards(lastAccSens, 75f, Time.deltaTime * 10);

                    aiAccelSens = lastAccSens * .01f;

                    lastAccSens = aiAccelSens;
                }
            }
          
            return desiredSpeed;
            #endregion
        }
        float DesiredAccelerationDependingObstacleOnTrack(float desiredAcc)
        {
            #region
            float desiredAcceleration = desiredAcc;
            // Bounds guard: PathObstacle builds dangerListByPath in a coroutine after vehicle
            // init - on scene load there's a window where PathObs exists but the lists are
            // still empty, and closestObstaclePathPos/selectedId can also briefly be stale.
            // Indexing unguarded here threw ArgumentOutOfRange every physics frame.
            bool obstacleDataReady = PathObs &&
                vehiclePathFollow.Track.selectedId < PathObs.dangerListByPath.Count &&
                closestObstaclePathPos < PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList.Count;
            if (obstacleDataReady &&
              PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft > 0 &&
               PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight > 0 &&
               m_Rigidbody.linearVelocity.magnitude > 20)
            {
                float ratio = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft;
                if (PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight < PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft)
                    ratio = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight;
                desiredAcceleration -= 1.5f * ratio;
            }

            else if(PathObs &&
                ObstacleSlowDown != 0)
            {
                float ratio = m_Rigidbody.linearVelocity.magnitude / 30;
                ratio = Mathf.Clamp01(ratio);
                //Debug.Log("Ratio: " +ratio);
                    desiredAcceleration -= 1.5f  * ratio;
            }

            return desiredAcceleration;
            #endregion
        }
        void TakeCareOfObstacles()
        {
            #region
            currentObsState = ReturnObstacleState();

            if (currentObsState == ObstacleState.NoFreeWay)
            {
                float speedRatio = m_Rigidbody.linearVelocity.magnitude / maxSpeedRef;
                if (ForwardObstacle())
                    currentOffsetObstacle = Mathf.MoveTowards(currentOffsetObstacle, dir * 8f, Time.deltaTime * (15 * (1 - speedRatio)));
                else
                    currentOffsetObstacle = Mathf.MoveTowards(currentOffsetObstacle, 0f, Time.deltaTime * (5f - (5f * speedRatio)));

                if (IsStartPhaseDone)
                    vehiclePathFollow.UpdateOffsetPathPosition(currentOffsetObstacle);


                float targetSpeed = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft + PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight;
               // Debug.Log(carController.surfaceData.surfaceList[carController.mostUseSurface].gripAmount);
                float currentGrip = carController.surfaceData.surfaceList[carController.mostUseSurface].gripAmount;
                targetSpeed *= .5f;
                if (ForwardObstacle())
                {
                    float distRatio = DistanceObstacle() / 150;
                    distRatio = Mathf.Clamp(distRatio, .01f, .99f);

                    currentspeedObstacle = Mathf.MoveTowards(currentspeedObstacle, (40 - (30 * targetSpeed))* currentGrip, Time.deltaTime * (30 + 20 * speedRatio) * (1 - distRatio));
                    carController.maxSpeed = currentspeedObstacle;
                }
                else
                {
                    float distRatio = DistanceObstacle() / 150;
                    distRatio = Mathf.Clamp(distRatio, .01f, .99f);
                    currentspeedObstacle = Mathf.MoveTowards(currentspeedObstacle, (50 - (40 * targetSpeed))*currentGrip, Time.deltaTime * 10 * (1 - distRatio));
                    carController.maxSpeed = currentspeedObstacle;
                }
            }
            else
            {

                if (aiState == CarAiState.Follow)
                {
                    if (nextCarOnTrack)
                        carController.maxSpeed = nextCarOnTrack.maxSpeed;
                }
                else
                    carController.maxSpeed = maxSpeedRef;

                currentspeedObstacle = Mathf.MoveTowards(currentspeedObstacle, maxSpeedRef, Time.deltaTime * 30);
                carController.maxSpeed = currentspeedObstacle;

                CheckObstacle();
            }
            #endregion
        }
        ObstacleState ReturnObstacleState()
        {
            #region
            if (PathObs)
            {
                closestObstaclePathPos = closestSpotOnObstaclePath;

                int forceRoadSide = IsPlayerSlowAndOnTheSameSideOfTheRoad();

                if (PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft >= 0.5f &&
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight >= 0.5f 
                    ||
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft >= 0.5f &&
                    forceRoadSide == 1
                    ||
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight >= 0.5f
                    &&
                    forceRoadSide == -1
                    )
                {
                    ObsState = ObstacleState.NoFreeWay;
                    return ObstacleState.NoFreeWay;
                }

                else if (PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft > 0 &&
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight < 0.5f 
                    ||
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight > 0 &&
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft < 0.5f
                    ||
                     forceRoadSide == 1
                    ||
                     forceRoadSide == -1
                    )
                {
                    ObsState = ObstacleState.OneWayFree;
                    return ObstacleState.OneWayFree;
                }

                if (IsCarStuck)
                    return ObstacleState.NoFreeWay;
            }
            ObsState = ObstacleState.Neutral;
            return ObstacleState.Neutral;
            #endregion
        }
        public void CheckObstacle()
        {
            #region
            if (PathObs)
            {
                ObstacleSlowDown = 0;
                closestObstaclePathPos = closestSpotOnObstaclePath;

                float speedRatio = m_Rigidbody.linearVelocity.magnitude / maxSpeedRef;
                int forceRoadSide = IsPlayerSlowAndOnTheSameSideOfTheRoad();
               

                if ((PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft > 0 &&
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft > PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight)
                    ||
                    forceRoadSide == -1)
                {
                    float ratio = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft;

                    float offsetTarget = avoidObstaclePathOffset * PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft - PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DistanceToPath;
                    if (forceRoadSide == -1)
                    {
                        offsetTarget = avoidObstaclePathOffset * .5f;
                    }

                    if (vehiclePathFollow.whichSideOfThePath == VehiclePathFollow.WhichSideOfThePath.LeftSide)
                        ObstacleSlowDown = ratio;

                    currentOffsetObstacle = Mathf.MoveTowards(currentOffsetObstacle, offsetTarget, Time.deltaTime * 5/* (4 - 2 * speedRatio)*/);

                    if (forceRoadSide == -1) IsStartPhaseDone = true;

                    // Debug.Log("forceRoadSide Left: " + forceRoadSide + " -> " + currentOffsetObstacle);

                    if (IsStartPhaseDone)
                        vehiclePathFollow.UpdateOffsetPathPosition(currentOffsetObstacle);

                }
                else if ((PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight > 0 &&
                    PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight > PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioLeft)
                     ||
                    forceRoadSide == 1)
                {
                    float ratio = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight;

                    float offsetTarget = -avoidObstaclePathOffset * PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DangerRatioRight + PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[closestObstaclePathPos].DistanceToPath;
                    if (forceRoadSide == 1)
                    {
                        offsetTarget = -avoidObstaclePathOffset *.5f;
                    }

                    if (vehiclePathFollow.whichSideOfThePath == VehiclePathFollow.WhichSideOfThePath.RightSide)
                        ObstacleSlowDown = ratio;

                    currentOffsetObstacle = Mathf.MoveTowards(currentOffsetObstacle, offsetTarget, Time.deltaTime *5/* (4 - 2 * speedRatio)*/);

                    if (forceRoadSide == 1) IsStartPhaseDone = true;
                      //Debug.Log("forceRoadSide Right: " + forceRoadSide + " -> " + currentOffsetObstacle);
                    if (IsStartPhaseDone)
                        vehiclePathFollow.UpdateOffsetPathPosition(currentOffsetObstacle);
                }
                else
                {              
                    currentOffsetObstacle = Mathf.MoveTowards(currentOffsetObstacle, 0f, Time.deltaTime * (8f - (5f * speedRatio)));

                    if (IsStartPhaseDone && aiState != CarAiState.Overtake && aiState != CarAiState.Overtaken)
                        vehiclePathFollow.UpdateOffsetPathPosition(currentOffsetObstacle);
                }
               
            }
            #endregion
        }
        int IsPlayerSlowAndOnTheSameSideOfTheRoad()
        {
            #region 
            // Check if they are on the same path
            for (var i = 0; i < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer; i++)
            {
                if (m_Rigidbody.linearVelocity.magnitude > carList[i].rb.linearVelocity.magnitude &&
                   carPathFollowList[i].distanceToPath < 6 &&
                    DistanceBetweenTwoVehicles(i) < 100)
                {
                    float dist01 = vehiclePathFollow.progressDistance;
                    float dist02 = carPathFollowList[i].progressDistance;
                    dist01 = dist02 < dist01 ? -(vehiclePathFollow.Track.pathLength - dist01) : dist01;
                    float total = dist02 - dist01;

                    if (total < 100 && total > 0 && HasThePlayerCrossedTheLineAtLeastOnce(i))
                    {
                        if (carPathFollowList[i].whichSideOfThePath == VehiclePathFollow.WhichSideOfThePath.LeftSide)
                            return -1;

                        if (carPathFollowList[i].whichSideOfThePath == VehiclePathFollow.WhichSideOfThePath.RightSide)
                            return 1;
                    }
                }
            }
            return 0; 
            #endregion
        }
        bool HasThePlayerCrossedTheLineAtLeastOnce(int playerID)
        {
            #region 
            if (LapCounterAndPosition.instance.posList[playerID].howLapDone > 1)
                return true;
            else
                return false; 
            #endregion
        }
        float DistanceBetweenTwoVehicles(int index)
        {
            #region 
            float dist = Vector3.Distance(transform.position, carPathFollowList[index].transform.position);
            return dist; 
            #endregion
        }

        int lastObstaclePos = 0;
        bool checkAllPos = true;
        public int CloseFromPathObstaclePosition()
        {
            #region
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = transform.position;
            int checkPointID = 0;
            int dangerListSize = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList.Count;
            // int endPosChecked = (dangerListSize * 2 + 30) % dangerListSize;

            // bool isPostFound = false;


            if (!checkAllPos)
            { 
                checkPointID = lastObstaclePos;
                int startPosChecked = (checkPointID + dangerListSize * 2 - 30) % dangerListSize;

                bool isPosFound = false;
                for (var i = 0; i < 60; i++)
                {
                    int checkedPos = (startPosChecked + i) % dangerListSize;
                    Vector3 directionToTarget = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[checkedPos].Pos - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        checkPointID = checkedPos;
                        isPosFound = true;
                    }
                }

                if (!isPosFound)
                    checkAllPos = true;
            }

            if (checkAllPos)
            {
                for (var i = 0; i < dangerListSize; i++)
                {
                    Vector3 directionToTarget = PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList[i].Pos - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        checkPointID = i;
                    }
                }
                checkAllPos = false;
            }


            lastObstaclePos = checkPointID;

           // Debug.Log(checkPointID);
            return checkPointID;
            #endregion
        }
        private void UpdateAIState()
        {
            #region
            if (IsAiOvertakenByACar())
            {
                float carDistance = vehiclePathFollow.progressDistance + vehiclePathFollow.Track.pathLength;
                float carOvertakeDistance = carThatOvertakeThisCarVehiclePathFollow.progressDistance + vehiclePathFollow.Track.pathLength;

                carController.maxSpeed = maxSpeedRef;
                 
                if (carOvertakeDistance - carDistance > 10
                    ||
                    carThatOvertakeThisCarVehiclePathFollow.Track.selectedId != vehiclePathFollow.Track.selectedId)
                {
                    aiState = CarAiState.Nothing;
                    if (IsStartPhaseDone)
                        vehiclePathFollow.UpdateOffsetPathPosition(0f);
                    carThatOvertakeThisCar = null;
                }

                if (nextCarOnTrack && nextCarOnTrackVehiclePathFollow.Track.selectedId != vehiclePathFollow.Track.selectedId)
                    nextCarOnTrack = null;
            }
            else if(IsAiAllowedToOvertakeCar())
            {
               // Debug.Log("Allow Overtake");
                float twoCarsDistanceToCompare = 40;
                float carDistance = vehiclePathFollow.progressDistance + vehiclePathFollow.Track.pathLength;
                float carOvertakeDistance = nextCarOnTrackVehiclePathFollow.progressDistance + vehiclePathFollow.Track.pathLength;

                if (IsStartPhaseDone) nextCarOnTrackVehiclePathFollow.UpdateOffsetPathPosition(0f);

                nextCarOnTrackCarAI.aiState = CarAiState.Overtaken;

               // Debug.Log(transform.parent.GetComponent<VehiclePrefabInit>().playerNumber + " | " + nextCarOnTrackVehiclePathFollow.transform.parent.GetComponent<VehiclePrefabInit>().playerNumber);

                nextCarOnTrackCarAI.carThatOvertakeThisCar = carController;
                nextCarOnTrackCarAI.carThatOvertakeThisCarVehiclePathFollow = vehiclePathFollow;

                if (carOvertakeDistance - carDistance < twoCarsDistanceToCompare)
                {
                    if (IsStartPhaseDone)
                    {
                        currentOffset = Mathf.MoveTowards(currentOffset, overtakeOffsetPath, Time.deltaTime);
                        if (overtakeZone == Overtake.Left) vehiclePathFollow.UpdateOffsetPathPosition(-overtakeOffsetPath);
                        if (overtakeZone == Overtake.Right) vehiclePathFollow.UpdateOffsetPathPosition(overtakeOffsetPath);
                    }
                }
                else if (IsStartPhaseDone)
                {
                    vehiclePathFollow.UpdateOffsetPathPosition(0);
                }

                aiState = CarAiState.Overtake;
            }
            else if (!CheckContinueOvertake())
            {
                ResetToNothing();
            }
            else if (!CheckContinueOvertaken())
            {
                ResetToNothing();
            }
            else if (IsAiFollowingCar())
            {
                //Debug.Log("IsAiFollowingCar");
                aiState = CarAiState.Follow;
                carThatOvertakeThisCar = null;
            }
            else if (!CheckContinueFollowing())
            {
                ResetToNothing();
            }
            else
            {
                forceResetDuration += Time.deltaTime;

                if(forceResetDuration > 3)
                {
                    ResetToNothing();
                    forceResetDuration = 0;
                }
            }
            #endregion
        }
        void ResetToNothing()
        {
            #region
            //Debug.Log("Nothing");
            aiState = CarAiState.Nothing;
            nextCarOnTrack = null;
            carThatOvertakeThisCar = null;
            #endregion
        }
        bool CheckContinueOvertake()
        {
            #region
           
            if (aiState == CarAiState.Overtake &&
                nextCarOnTrack && 
              nextCarOnTrackVehiclePathFollow.Track.selectedId !=vehiclePathFollow.Track.selectedId &&
               currentObsState != ObstacleState.Neutral)
                return false;

            return true;
            #endregion
        }
        bool CheckContinueOvertaken()
        {
            #region
            if (aiState == CarAiState.Overtaken &&
                carThatOvertakeThisCar &&
                carThatOvertakeThisCarVehiclePathFollow.Track.selectedId != vehiclePathFollow.Track.selectedId &&
                currentObsState != ObstacleState.Neutral)
                return false;

                return true;
            #endregion
        }
        bool CheckContinueFollowing()
        {
            #region
            if (aiState == CarAiState.Follow &&
                nextCarOnTrack &&
              nextCarOnTrackVehiclePathFollow.Track.selectedId != vehiclePathFollow.Track.selectedId)
                return false;

            return true;
            #endregion
        }
        bool IsAiOvertakenByACar()
        {
            #region
            if (aiState == CarAiState.Overtaken &&
                carThatOvertakeThisCar &&
                carThatOvertakeThisCarVehiclePathFollow.Track.selectedId == vehiclePathFollow.Track.selectedId &&
                carThatOvertakeThisCar.GetComponent<CarAI>().aiState == CarAiState.Overtake)
            {
               // if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 1)
               //    Debug.Log("eeee");


                return true;
            }
               
            else
                return false;
            #endregion
        }
        public bool IsAiAllowedToOvertakeCar()
        {
            #region
            if (

                nextCarOnTrack &&
                !IsObstacleOnCarSides(nextCarOnTrack) &&
                (nextCarOnTrackCarAI.carThatOvertakeThisCar == null &&
                nextCarOnTrackCarAI.aiState != CarAiState.Overtake &&
                carController.maxSpeed >= nextCarOnTrack.maxSpeed + 10 )
               )
            {

                //if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 1)
                //Debug.Log("1");
                   // Debug.Break();
                return true;
            }
            else if (
                nextCarOnTrack &&
               (nextCarOnTrackVehicleInfo.playerNumber > carControllerVehicleInfo.playerNumber &&
               nextCarOnTrackVehiclePathFollow.Track.selectedId == vehiclePathFollow.Track.selectedId &&
               currentObsState == ObstacleState.Neutral &&
               overtakeZone != Overtake.None)
                )
            {
                //Debug.Log("2");
                //if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 1)
               // Debug.Break();
                return true;
            }
            else if (
                  nextCarOnTrack &&
                !IsObstacleOnCarSides(nextCarOnTrack) &&
                ( nextCarOnTrackCarAI.carThatOvertakeThisCar == carController &&
               nextCarOnTrackVehiclePathFollow.Track.selectedId == vehiclePathFollow.Track.selectedId &&
               currentObsState == ObstacleState.Neutral &&
               overtakeZone != Overtake.None)
               )
            {
                //Debug.Log("3");
                //if (transform.parent.GetComponent<VehiclePrefabInit>().playerNumber == 1)
                //Debug.Break();
                return true;
            }

            else
                return false;
            #endregion
        }
        public bool IsObstacleOnCarSides(CarController car,int leftRightBoth = 0)
        {
            #region
            CarAI carAI = car.GetComponent<CarAI>();

            if(leftRightBoth == -1 || leftRightBoth == 0)
            {
                for (var i = 0; i < carAI.detectors.leftDetector.Count; i++)
                {
                    Transform detector = carAI.detectors.leftDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit,  carAI.detectors.leftRayDistance, sideDetectionLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return false;
                            else
                                return true;
                        }
                    }
                }
            }

            if (leftRightBoth == 1 || leftRightBoth == 0)
            {
                for (var i = 0; i < carAI.detectors.rightDetector.Count; i++)
                {
                    Transform detector = carAI.detectors.rightDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit,carAI.detectors.rightRayDistance, sideDetectionLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return false;
                            else
                            {
                                //Debug.Log("On right");
                                return true;
                            }
                               
                        }
                    }
                }
            }

            return false;
            #endregion
        }
        bool IsAiFollowingCar()
        {
            #region
            if (nextCarOnTrack &&
               CheckIfPlayerMustBeFollowedByAI() && 
               carController.maxSpeed > nextCarOnTrack.maxSpeed - 2 &&
               carController.maxSpeed <= nextCarOnTrack.maxSpeed + 2 &&
               nextCarOnTrackVehiclePathFollow.Track.selectedId == vehiclePathFollow.Track.selectedId)
                return true;
            else
                return false;
            #endregion
        }
        bool CheckIfPlayerMustBeFollowedByAI()
        {
            #region 
            if (nextCarOnTrack.carState.carPlayerType == CarState.CarPlayerType.Human &&
                    nextCarOnTrack.rb.linearVelocity.magnitude < 20)
            {
                return true;
            }

            return true; 
            #endregion
        }
        CarController NextCar()
        {
            #region
            CarController nextCar = null;

            for(var i = 0;i< carList.Count; i++)
            {
                if(carList[i].gameObject.activeSelf && carList[i] != carController)
                {
                    float carLapDistance = vehiclePathFollow.progressDistance;
                    float carTestedLapDistance = carPathFollowList[i].progressDistance;

                    if (carTestedLapDistance - carLapDistance < 50 && carTestedLapDistance > carLapDistance && !nextCar &&
                        carList[i].carState.carPlayerType == CarState.CarPlayerType.AI)
                    {
                        nextCar = carList[i];
                       // break;
                    }

                    if (carTestedLapDistance - carLapDistance < 50 && nextCar &&
                        carList[i].carState.carPlayerType == CarState.CarPlayerType.AI)
                    {
                        float nextCarLapDistance = nextCar.GetComponent<VehiclePathFollow>().progressDistance;

                        if (carTestedLapDistance  < nextCarLapDistance && carTestedLapDistance > carLapDistance)
                        {
                            nextCar = carList[i];
                            //break;
                        }
                    }
                }
            }

            // Prevent the case that the car is at the end of the track. 
            if (!nextCar)
            {
                for (var i = 0; i < carList.Count; i++)
                {
                    if (carList[i].gameObject.activeSelf && carList[i] != carController)
                    {
                        float trackLength = vehiclePathFollow.Track.pathLength;
                        float carLapDistance = vehiclePathFollow.progressDistance;
                        float carTestedLapDistance = carPathFollowList[i].progressDistance;

                        if (carTestedLapDistance + trackLength - carLapDistance < 50 && carTestedLapDistance < carLapDistance && !nextCar &&
                        carList[i].carState.carPlayerType == CarState.CarPlayerType.AI)
                        {
                            nextCar = carList[i];
                            //break;
                        }

                        if (carTestedLapDistance + trackLength - carLapDistance < 50 && nextCar &&
                        carList[i].carState.carPlayerType == CarState.CarPlayerType.AI)
                        {
                            float nextCarLapDistance = nextCar.GetComponent<VehiclePathFollow>().progressDistance;

                            if (carTestedLapDistance < nextCarLapDistance && carTestedLapDistance < carLapDistance)
                            {
                                nextCar = carList[i];
                                //break;
                            }
                        }
                    }
                }
            }
         
            if (nextCar)
                nextCarOnTrackDistance = Vector3.Distance(carController.transform.position, nextCar.transform.position);
            else
                nextCarOnTrackDistance = 1;

            if (nextCar)
            {
                nextCarOnTrack = nextCar;
                nextCarOnTrackCarAI = nextCar.GetComponent<CarAI>();
                nextCarOnTrackVehiclePathFollow = nextCar.GetComponent<VehiclePathFollow>();
                nextCarOnTrackVehicleInfo = nextCar.GetComponent<VehicleInfo>();
            }
            else
            {
                nextCarOnTrack = null;
                nextCarOnTrackCarAI = null;
                nextCarOnTrackVehiclePathFollow = null;
                nextCarOnTrackVehicleInfo = null;
            }

            return nextCar;
            #endregion
        }

        // Select the car state (Moving Forward/backward | Turn Left | Right)
        void UpdateCarState()
        {
            #region
            UpdateMoveDirState();
            UpdateSteeringDirState();
            #endregion
        }

        void UpdateMoveDirState()
        {
            #region
            if (acceleration > 0)
            {
                carState.lastMoveDir = carState.moveDir;
                carState.moveDir = CarMoveDirection.forward;
            }
            else if (acceleration < 0)
            {
                carState.lastMoveDir = carState.moveDir;
                carState.moveDir = CarMoveDirection.backward;
            }
            else
            {
                carState.moveDir = CarMoveDirection.center;
            }
            #endregion
        }
        void UpdateSteeringDirState()
        {
            #region
            if (steer > 0)
            {
                carState.lastSteeringDir = carState.steeringDir;
                carState.steeringDir = CarSteeringDirection.Right;
            }
            else if (steer < 0)
            {
                carState.lastSteeringDir = carState.steeringDir;
                carState.steeringDir = CarSteeringDirection.Left;
            }
            else
            {
                carState.steeringDir = CarSteeringDirection.Center;
            }
            #endregion
        }
        void OnDrawGizmosSelected()
        {
            #region
            DrawDetectorsGizmos();
            #endregion
        }
        void DrawDetectorsGizmos()
        {
            #region
            for (var i = 0; i < detectors.frontDetector.Count; i++)
            {
                Transform detector = detectors.frontDetector[i];
                if (detector)
                {
                    Vector3 rayDir = detector.transform.forward;
                    float rayDistance = (m_Rigidbody.linearVelocity.magnitude / maxSpeedRef) * (offsetLength + detectors.frontRayDistance);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(detector.position,detector.transform.forward * rayDistance);
                }
            }

            for (var i = 0; i < detectors.backDetector.Count; i++)
            {
                Transform detector = detectors.backDetector[i];
                if (detector)
                {
                    Vector3 rayDir = detector.transform.forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(detector.position, detector.transform.forward * detectors.backRayDistance);
                }
            }

            for (var i = 0; i < detectors.leftDetector.Count; i++)
            {
                Transform detector = detectors.leftDetector[i];
                if (detector)
                {
                    Vector3 rayDir = detector.transform.forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(detector.position, detector.transform.forward * detectors.leftRayDistance);
                }
            }

            for (var i = 0; i < detectors.rightDetector.Count; i++)
            {
                Transform detector = detectors.rightDetector[i];
                if (detector)
                {
                    Vector3 rayDir = detector.transform.forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(detector.position, detector.transform.forward * detectors.rightRayDistance);
                }
            }

            #endregion
        }
        bool ForwardObstacle()
        {
            #region
            for (var i = 0; i < detectors.frontDetector.Count; i++)
            {
                Transform detector = detectors.frontDetector[i];
                if (detector)
                {
                    float rayDistance = (m_Rigidbody.linearVelocity.magnitude / maxSpeedRef) * (offsetLength + detectors.frontRayDistance);
                    if (Physics.Raycast(detector.position, detector.forward, out RaycastHit hit, rayDistance, obstacleLayer))
                    {
                        if (hit.transform.GetComponent<ObstaclePosition>())
                        {
                            dir = hit.transform.GetComponent<ObstaclePosition>().dir;

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
            #endregion
        }
        float DistanceObstacle()
        {
            #region
            float minDistance = 0;
            for (var i = 0; i < detectors.frontDetector.Count; i++)
            {
                Transform detector = detectors.frontDetector[i];
                if (detector)
                {
                    if (Physics.Raycast(detector.position, detector.forward, out RaycastHit hit, detectors.frontRayDistance, obstacleLayer))
                    {
                        if(minDistance > hit.distance || minDistance == 0)
                            minDistance = hit.distance;
                    }
                }
            }
          
            return minDistance;
            #endregion
        }
        private void OnCollisionEnter(Collision collision)
        {
            #region
            if (collision.relativeVelocity.magnitude > 0 && collision.gameObject.layer == LayerVehicle)
            {
                if(!IsTouched)
                    StartCoroutine(TouchedRoutine(collision.relativeVelocity.magnitude));
            }

            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }
            #endregion
        }
        IEnumerator TouchedRoutine(float impactMag)
        {
            #region
            IsTouched = true;

            float t = 0;
            float duration = impactMag / 40f; ;

            while (t < duration)
            {
                if(!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime;
                
                yield return null;
            }

            IsTouched = false;
            yield return null;
            #endregion
        }
        float IsObstacleOnCarSidesDistance(CarController car, int leftRightBoth = 0)
        {
            #region
            CarAI carAI = car.GetComponent<CarAI>();

            if (leftRightBoth == -1 || leftRightBoth == 0)
            {
                for (var i = 0; i < carAI.detectors.leftDetector.Count; i++)
                {
                    Transform detector = carAI.detectors.leftDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit,2 /* carAI.detectors.leftRayDistance*/, obstacleLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return 0;
                            else
                                return hit.distance/2f;
                        }

                    }
                }
            }

            if (leftRightBoth == 1 || leftRightBoth == 0)
            {
                for (var i = 0; i < carAI.detectors.rightDetector.Count; i++)
                {
                    Transform detector = carAI.detectors.rightDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit, 2 /*carAI.detectors.rightRayDistance*/, obstacleLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return 0;
                            else
                                return hit.distance/2f;
                        }
                    }
                }
            }

            return 0;
            #endregion
        }

        float ForwardOnlyObstacleDistance(float distanceToCheck = 2)
        {
            #region
            for (var i = 0; i < 2; i++)
            {
                if (detectors.frontDetector.Count > i)
                {
                    Transform detector = detectors.frontDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit, distanceToCheck, obstacleLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return 0;
                            else
                                return hit.distance / 2f;
                        }
                    }
                }
            }

            return 0;
            #endregion
        }
        float BackOnlyObstacleDistance()
        {
            #region
            for (var i = 0; i < 2; i++)
            {
                if (detectors.backDetector.Count > i)
                {
                    Transform detector = detectors.backDetector[i];
                    if (detector)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(detector.position, detector.transform.forward, out hit, 2 /*carAI.detectors.rightRayDistance*/, obstacleLayer))
                        {
                            if (hit.transform.gameObject == this.gameObject)
                                return 0;
                            else
                                return hit.distance / 2f;
                        }
                    }
                }
            }

            return 0;
            #endregion
        }
        void AddForceSideToPreventCollision()
        {
            #region
            if (distanceObstacleLeft > 0 && !IsTouched)
            {
                ratioLeft = (1 - distanceObstacleLeft);
                m_Rigidbody.AddForceAtPosition(m_Rigidbody.transform.right * 20000 * PreventCollisionCurve.Evaluate(ratioLeft), ApplyRight.position);
            }
            else
                ratioLeft = 1;


            if (distanceObstacleRight > 0 && !IsTouched)
            {
                ratioRight = (1 - distanceObstacleRight);
                m_Rigidbody.AddForceAtPosition(-m_Rigidbody.transform.right * 20000 * PreventCollisionCurve.Evaluate(ratioRight), ApplyLeft.position);
            }
            else
                ratioRight = 1;

            #endregion
        }
        void AddForceFrontToPreventCollision()
        {
            #region
            if (forwardOnlyObstacleDistance > 0)
            {
                float ratio = (1 - forwardOnlyObstacleDistance);
                m_Rigidbody.AddForceAtPosition(-m_Rigidbody.transform.forward * 50000 * PreventCollisionCurve.Evaluate(ratio), ApplyFront.position);
            }
           
            if (distanceObstacleBack > 0 && !IsTouched)
            {
                float ratio = (1 - distanceObstacleBack);
                m_Rigidbody.AddForceAtPosition(m_Rigidbody.transform.forward * 50000 * PreventCollisionCurve.Evaluate(ratio), ApplyBack.position);
            }
            #endregion
        }
        public void StopAIIfTrackNotLooped()
        {
            #region 
            if (!vehiclePathFollow.Track.TrackIsLooped)
                StartCoroutine(StopAIIfTrackNotLoopedRoutine()); 
            #endregion
        }
        IEnumerator StopAIIfTrackNotLoopedRoutine()
        {
            #region
            IsStartPhaseDone = true;
            int trackPosID = vehiclePathFollow.Track.checkpointsDistanceFromPathStart.Count - 2;
            while (vehiclePathFollow.progressDistance < vehiclePathFollow.Track.checkpointsDistanceFromPathStart[trackPosID]-20)
            {
                yield return null;
            }

            carController.rb.isKinematic = true;

            Vector3 carPos = carController.transform.position;
            Vector3 newCarPos = new Vector3(carPos.x + 10 * carController.vehicleInfo.playerNumber, -1000 , carPos.z);
            carController.transform.position = newCarPos;

            yield return null;
            #endregion
        }
        IEnumerator CarIsStuckRoutine()
        {
            #region 
           
            reversSequence = ReverseState.Backward;
            acceleration = -1;
            steer = -steer;
            currentObsState = ObstacleState.NoFreeWay;

            float t = 0;
            while (t < 2)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime;

                yield return null;
            }

            reversSequence = ReverseState.Forward;

            chekIfCarStuckTimer = 0;
            IsCarStuck = false;

            respawnAutomaticaly = true;

            t = 0;
            while (t < 6)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime;

                yield return null;
            }

            respawnAutomaticaly = false;
            yield return null; 
            #endregion
        }
        LayerMask InitLayerMask(List<int> layerList)
        {
            #region //-> Init LayerMask
            string[] layerUsed = new string[layerList.Count];
            for (var i = 0; i < layerList.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[layerList[i]].layerID);

            return LayerMask.GetMask(layerUsed);
            #endregion
        }

    }
}
