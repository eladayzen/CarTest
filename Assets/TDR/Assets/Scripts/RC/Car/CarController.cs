// Description: CarController. Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CarController : MonoBehaviour, IVehicleStartLine<Vector3>, IGyroStartLine<Quaternion>
    {
        public bool                     autoInit = true;
        [HideInInspector]
        public bool                     isInitDone = false;

        [Header("Debug Options")]
        public bool                     isGizmosEnable = true;
        public bool                     editParametersAtRuntime = false;

        [Header("Car parameters")]
        public Rigidbody                rb;

        public Transform                centerOfMass;

        public float                    speedRotation = 30;
        public float                    speedRotationRef = 0;
        public AnimationCurve           rotationSpeedCurve;

        public float                    jumpDownForceApplied = 3500;

        public float                    forwardForceApplied = 6000;
        public AnimationCurve           forwardForceAppliedSpeedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve           amountOfForceCurve;
        public  float                   forwardForceAppliedRef = 6000;

        [System.Serializable]
        public class WheelParams
        {
            public string       name = "";
            public Transform    spring;
            public float        suspensionLength;
            public float        springMaxCompressionRatio;
            public float        springStiffness;
            [HideInInspector]
            public float        damperStiffeness;
            public float        damperStiffenessMin;
            public float        damperStiffenessMax;

            [HideInInspector]
            public float        maxLength;
            [HideInInspector]
            public float        lastLength;
            [HideInInspector]
            public float        springLength;
            [HideInInspector]
            public float        springVelocity;

            public float        lastSpringVelocity;

            [HideInInspector]
            public float        springForce;
            [HideInInspector]
            public float        damperForce;

            public float        wheelRadius;
            public float        WheelWidth = .13f;
            public float        wheelExtraWidth = 0;

            public GameObject   wheelObj;
            public float        wheelExtraOffsetWidth = 0;
            public float        wheelOffsetRadius = 0;
            public int          wheelRoationDir = 1;
            public bool         wheelAllowRotationY = false;
            [HideInInspector]
            public float        currentWheelRotationY = 0;
            public float        maxWheelRotationY = 30;

            [HideInInspector]
            public Vector3      hitPoint;

            public bool         isObstacle;
            [HideInInspector]
            public float        distanceToObstacle;

            [HideInInspector]
            public float        lastDistance =0;
            [HideInInspector]
            public RoadType     surface;

            public bool         curbDetection = false;

            public Transform    wheelAxisY;
            public Transform    wheelAxisX;

            public Vector3      capsuleCastOffset = Vector3.zero;
            public float        capsuleCastPos01 = 0;
            public float        capsuleCastPos02 = 0;

            public Rigidbody    rbSweepTest;
        }

        public List<WheelParams>        wheelsList = new List<WheelParams>();

        public LayerMask                wheelLayer;
        public List<int>                wheelLayerMaskList = new List<int>();   //  0 Default
        public float                    wheelModelRotationSpeed = 1;

        public float                    suspensionRuntimeOffset = .03f;

        public float                    sidewayDragLeft = .001f;
        float                           sidewayDragLeftRef = 0;
        public AnimationCurve           sidewayLeftDragCurve;

        public float                    maxSpeed = 90;              // = max car velocity         50 -> 182 km/h | 90 -> 320 km/h  |
        [HideInInspector]
        public float                    refMaxSpeed = 90;
        public float                    maxBackwardSpeedMag = 10;
    
        [HideInInspector]
        public float                    forwardRBVel = 0;

        [HideInInspector]
        public float                    smoothRotation = 0;
        public AnimationCurve           smoothRotCurve;
        public float                    smoothRotCurveSpeed = 2;
        [HideInInspector]
        public float                    rotBoost = 0;

        [HideInInspector]
        public CarState                 carState;
        [HideInInspector]
        public VehicleInfo              vehicleInfo;
       // [HideInInspector]
       // public bool                     isGrounded = false;

        public float                    sidewayDragSpeedCurve = 1;
        public AnimationCurve           sidewayLeftDragSpeedCurve;
        [HideInInspector]
        public bool                     isBrakePressed;

        public Transform                grpBrakeHeadlightModels;

        [HideInInspector]
        public float                    steer = 0;
        [HideInInspector]
        public float                    acceleration = 0;
        public float                    BrakeForce = .5f;

        public float                    speedRatioWhenCarIsStopped = .5f;

        public SurfaceData              surfaceData;

        Vector3                         refInitPos;

        Vector3                         currentRBVel = Vector3.zero;

        [HideInInspector]
        public bool                     lastFrameFrontalImpact = false;
        public Transform                ImpactDetector;
        public Transform                ImpactDetectorB;
        public Vector3                  lastDectorPos;

        public Transform                ImpactDetector2;
        public Transform                ImpactDetector2B;
        public Vector3                  lastDectorPos2;

        public bool                     collisionNextFrame = false;
        public Vector3                  lastVelocity;
        [HideInInspector]
        public bool                     increaseDrag = false;

        public List<Transform>          grp_Detector = new List<Transform>();
        [HideInInspector]
        public bool                     lockAcc = false;

        [HideInInspector]
        public bool                     isImpactDetected = false;
        [HideInInspector]
        public float                    impactMagnitude = 0;
        [HideInInspector]
        public bool                     waitSec = false;
        
        float                           currentDragLeft = 15;

        [HideInInspector]
        public float                    gripAmountDependingSurface = 0;
        [HideInInspector]
        public float                    speedAmountDependingSurface = 0;
        [HideInInspector]
        public int                      surfaceAverage = 0;
        [HideInInspector]
        public int                      mostUseSurface = 0;

        public RoadType                 averageSurface = RoadType.Default;
        int                             howManyDifferentSurface = 0;

        public float                    offsetSweepTestRaycastDistance = .05f;

        public VehicleDamage            vehicleDamage;
        public GameObject               grp_Body;
        public GameObject               grp_Colliders;

        float                           slowDownCar = 1;

        float                           lastSteerBackwardDir = 1;
        float                           steerBackwardDir = 0;

        Quaternion                      refInitRot;

        Vector3                         specialRespawnPosition = Vector3.zero;
        Vector3                         specialRespawnDirection = Vector3.zero;

        public LayerMask                RespawnLayer;
        public List<int>                respawnLayerMaskList = new List<int>();   //  0 Default | 14 Border | 16 DestructibleObject

        float                           respawnProgressDistance = 0;

        public float                    offsetMaxWheelPosition = 0;

        public float                    stiffSuspensionValue = -1.0f;

        public void OnDestroy()
        {
            #region
            if (vehicleDamage)
            {
                vehicleDamage.VehicleExplosionAction -= VehicleRespawnStartProcess;
                vehicleDamage.VehicleRespawnPart1 -= VehicleOutOfLimitZone;
                vehicleDamage.VehicleRespawnPart2 -= VehicleRespawn;
            }
            #endregion
        }
        void Start()
        {
            #region 
            if (autoInit) Init(); 
            #endregion
        }

        void Init()
        {
            #region
            wheelLayer = InitLayerMask(wheelLayerMaskList);
            RespawnLayer = InitLayerMask(respawnLayerMaskList);
            
            howManyDifferentSurface = System.Enum.GetValues(typeof(RoadType)).Length;

            if (vehicleDamage)
            {
                vehicleDamage.VehicleExplosionAction += VehicleRespawnStartProcess;
                vehicleDamage.VehicleRespawnPart1 += VehicleOutOfLimitZone;
                vehicleDamage.VehicleRespawnPart2 += VehicleRespawn;
            }

            rb.centerOfMass = centerOfMass.localPosition;

            for (var i = 0; i < wheelsList.Count; i++)
            {
                WheelParams wheelParams = wheelsList[i];
                if (wheelParams.spring)
                {
                    //wheelParams.minLength = wheelParams.suspensionLength - wheelParams.springMaxCompression;
                    wheelParams.maxLength = wheelParams.suspensionLength;
                }
            }
           
            carState = GetComponent<CarState>();
            vehicleInfo = GetComponent<VehicleInfo>();
            speedRotationRef = speedRotation;

            forwardForceAppliedRef = forwardForceApplied;
            sidewayDragLeftRef = sidewayDragLeft;
            refMaxSpeed = maxSpeed;

            isInitDone = true;
            #endregion
        }


        void Update()
        {
            #region 
            if (isInitDone)
            {
                if (editParametersAtRuntime)
                    RuntimeCarParamsEdition();

                CheckInputs();
                UpdateForwardForceAppliedToCar();
                CarRotationDependingSpeed();

                SpeedAmountDependingSurface();
                GripAmountDependingSurface();
                AverageSurface();
                CurbDetection();


            }
            #endregion
        }

        void FixedUpdate()
        {
            #region 
            if (isInitDone && !rb.isKinematic)
            {
                currentRBVel = rb.linearVelocity;

                CalculateRBVelocity();
                CalculateWheelPosition();
                CalculateSpringsUsingSweepTest();

                //TODO: AT the end if there is no issue with collision. Remove this method
                // + Remove ImpactDetector, ImpactDetectorB, ImpactDetector2, ImpactDetector2B,
                // + lastDectorPos, lastDectorPos2, Collision NextFrame,xPos01,xPos02
                // + grp_Detector + lockAcc
                // CheckImpact();

                CheckVelocityVariation();
            }     
            #endregion
        }


        void UpdateCenterOfMass()
        {
            #region
            /*  float yPos = centerOfMass.localPosition.y * 10;
                if (!IsCarOnTheGround(4))
                {
                    yPos = Mathf.MoveTowards(yPos, 6,Time.deltaTime*1);
                }
                else
                {
                    yPos = Mathf.MoveTowards(yPos, 2, Time.deltaTime*6);
                }
                yPos /= 10; 
                centerOfMass.localPosition = new Vector3(0, yPos, 0);*/ 
            #endregion
        }


        public  Vector3 FindCenter(List<RaycastHit> objs)
        {
            #region
            var bound = new Bounds(objs[0].point, Vector3.zero);
            for (int i = 1; i < objs.Count; i++)
                bound.Encapsulate(objs[i].point);
            return bound.center;
            #endregion
        }

        public void CalculateSpringsUsingSweepTest()
        {
            #region 
            for (var i = 0; i < wheelsList.Count; i++)
            {
                Transform spring = wheelsList[i].spring;
                if (spring)
                {
                    WheelParams wheelParams = wheelsList[i];
                    RaycastHit hit;

                    float radiusTotal = wheelParams.wheelRadius + wheelParams.wheelOffsetRadius;

                    if (wheelParams.rbSweepTest.SweepTest(-Vector3.up, out hit, wheelParams.suspensionLength + suspensionRuntimeOffset + radiusTotal + offsetSweepTestRaycastDistance, QueryTriggerInteraction.Ignore))
                    {
                        wheelParams.hitPoint = hit.point;
                        wheelParams.distanceToObstacle = hit.distance;
                        wheelParams.isObstacle = true;
                        
                        UpdateSpringAndMove(wheelParams.distanceToObstacle, wheelParams.hitPoint, wheelParams);
                        UpdateRotation(spring);

                        UpdateVelocity(spring);

                        UpdateWheelModelRotation(wheelParams);

                        if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
                        {                       
                            ReturnRoadSurface(wheelsList[i], hit.transform, hit.point);
                            ReturnCurbSurface(wheelsList[i], hit.transform, hit.point);
                        }
                           
                    }
                    else
                    {
                        wheelParams.isObstacle = false;
                        UpdateSpringAndMove(radiusTotal * 2 + wheelParams.maxLength + suspensionRuntimeOffset, wheelParams.hitPoint, wheelParams, false);
                        UpdateRotation(spring);

                        ApplyDownForceDuringJump(wheelParams);

                        UpdateWheelModelRotation(wheelParams);

                        if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
                        {
                            ReturnRoadSurface(wheelParams, null, Vector3.zero);
                            ReturnCurbSurface(wheelParams, null, Vector3.zero);
                        } 
                    }
                }
            } 
            #endregion
        }

        public void CalculateWheelPosition()
        {
            #region 
            //if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
            //{
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    Transform spring = wheelsList[i].spring;
                    if (spring)
                    {
                        WheelParams wheelParams = wheelsList[i];

                        float radiusTotal = wheelParams.wheelRadius;
                        float posY = -wheelParams.springLength + radiusTotal;

                        posY = Mathf.Clamp(posY, 0, wheelParams.springLength + radiusTotal - offsetMaxWheelPosition);

                        wheelParams.wheelObj.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, posY, 0);
                    }
                }
            //}
            
            #endregion
        }

        void ReturnRoadSurface(WheelParams wheel, Transform hit,Vector3 hitPoint)
        {
            #region
            if (hit && hit.GetComponent<RoadTag>())
            {
                wheel.surface = hit.GetComponent<RoadTag>().roadType;
            }
            else if (hit && hit.GetComponent<Terrain>())
            {
                wheel.surface = ReturnTerrainCurrentSurface(hit.GetComponent<Terrain>(),hitPoint);
            }
            else
            {
                wheel.surface = RoadType.Default;
            }
            #endregion
        }

       


        RoadType ReturnTerrainCurrentSurface(Terrain terrain, Vector3 hitPoint)
        {
            #region 
            if (surfaceData)
            {
                // Debug.Log("Hit pos: " + " : x: " + hitPoint + " : z: " + terrain.transform.position);
                Vector3 posFromTerrainPivotPos = hitPoint - terrain.transform.position;
                int reso = terrain.terrainData.alphamapResolution;
                Vector3 terrainSize = terrain.terrainData.size;

                int stepX = Mathf.FloorToInt(reso * posFromTerrainPivotPos.x / terrainSize.x);
                int stepZ = Mathf.FloorToInt(reso * posFromTerrainPivotPos.z / terrainSize.z);
                //Debug.Log(posFromTerrainPivotPos + " : x: " + stepX + " : z: " + stepZ);

                if (stepX >= reso) stepX = reso - 1;
                if (stepZ >= reso) stepZ = reso - 1;

                float[,,] alphaMaps = terrain.terrainData.GetAlphamaps(stepX, stepZ, 1, 1);
                TerrainLayer[] terrainLayers = terrain.terrainData.terrainLayers;

                if (terrainLayers.Length > 0)
                {
                    //Debug.Log("terrain");
                    int selectedLayer = 0;

                    for (var i = 1; i < terrainLayers.Length; i++)
                    {
                        if (alphaMaps[0, 0, i] > alphaMaps[0, 0, selectedLayer])
                            selectedLayer = i;
                    }

                    //  Debug.Log("selectedLayer: " + selectedLayer);

                    for (var i = 1; i < surfaceData.terrainLayerList.Count; i++)
                    {
                        if (surfaceData.terrainLayerList[i].layer == terrainLayers[selectedLayer])
                        {
                            // Debug.Log("selectedLayer: " + terrainLayers[selectedLayer].name + " : " + surfaceData.terrainLayerList[i].layer.name);
                            return surfaceData.terrainLayerList[i].roadType;
                        }
                    }
                }
            }

            return RoadType.Default; 
            #endregion
        }

        void UpdateWheelModelRotation(WheelParams wheelParams)
        {
            #region
            if (wheelParams.wheelObj)
            {
                Transform wheelXAxis = wheelParams.wheelObj.transform.GetChild(0).GetChild(0);
                wheelXAxis.Rotate(new Vector3(1, 0, 0) * forwardRBVel * wheelModelRotationSpeed, Space.Self);

                if (wheelParams.wheelAllowRotationY)
                {
                    
                    if (carState.steeringDir == CarSteeringDirection.Left)
                    {
                        wheelParams.currentWheelRotationY = Mathf.MoveTowards(wheelParams.currentWheelRotationY, -wheelParams.maxWheelRotationY * Mathf.Abs(steer), Time.fixedDeltaTime * 100);
                    }
                    else if (carState.steeringDir == CarSteeringDirection.Right)
                    {
                        wheelParams.currentWheelRotationY = Mathf.MoveTowards(wheelParams.currentWheelRotationY, wheelParams.maxWheelRotationY * Mathf.Abs(steer), Time.fixedDeltaTime * 100);
                    }
                    else
                    {
                        wheelParams.currentWheelRotationY = Mathf.MoveTowards(wheelParams.currentWheelRotationY, 0, Time.fixedDeltaTime * 100);
                    }

                    Transform wheelYAxis = wheelParams.wheelObj.transform.GetChild(0);
                    wheelYAxis.localEulerAngles = new Vector3(0, wheelParams.currentWheelRotationY, 0);
                }
            }
            #endregion
        }

        void UpdateSpringAndMove(float hitDistance, Vector3 hitPoint, WheelParams wheelParams, bool isGrounded = true)
        {
            #region
            Vector3 suspensionForce = GetSuspensionForce(hitDistance, wheelParams,isGrounded);
            Vector3 moveForwardForce = MoveForwardForce(wheelParams);
            if(isGrounded)
                rb.AddForceAtPosition(suspensionForce + moveForwardForce, hitPoint);


            if (rb.linearVelocity.y > 3)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.95f, rb.linearVelocity.z);
            }

           // Debug.Log(rb.velocity.y);
            #endregion
        }

        void UpdateRotation(Transform spring)
        {
            #region
            Quaternion deltaRotation = DeltaRotation(spring);
            rb.MoveRotation(rb.rotation * deltaRotation);
            #endregion
        }

        void UpdateVelocity(Transform spring)
        {
            #region
            float xVel = Vector3.Dot(spring.right, currentRBVel);

            currentRBVel -= GetSidewayDragLeft() * rb.transform.right * xVel + 0.0008f * rb.transform.forward * forwardRBVel;

            if (IsCarNeedsToBeStopped())
            {
                currentRBVel *= slowDownCar;
               
            }
            else
            {
                if (currentRBVel.magnitude > maxSpeed * speedAmountDependingSurface)
                {
                    //currentRBVel = currentRBVel.normalized * maxSpeed * speedAmountDependingSurface;
                    Vector3 targetVel = currentRBVel.normalized * maxSpeed * speedAmountDependingSurface;
                    Vector3 newV = Vector3.MoveTowards(currentRBVel, targetVel, Time.deltaTime*4);
                    currentRBVel = newV;
                }
                else if (currentRBVel.magnitude > maxBackwardSpeedMag * speedAmountDependingSurface && carState.carDirection == CarState.CarDirection.Backward)
                {
                    // currentRBVel = Vector3.MoveTowards(currentRBVel, currentRBVel.normalized * maxBackwardSpeedMag * speedAmountDependingSurface,Time.deltaTime *2);
                    Vector3 targetVel = currentRBVel.normalized * maxBackwardSpeedMag * speedAmountDependingSurface;
                    Vector3 newV = Vector3.MoveTowards(currentRBVel, targetVel, Time.deltaTime*4);
                    currentRBVel = newV;
                }
                else
                {

                }
            }

            rb.linearVelocity = currentRBVel;
            #endregion
        }

        bool IsCarNeedsToBeStopped()
        {
            #region
            if (currentRBVel.magnitude < 4 && carState.moveDir == CarMoveDirection.center)
            {
                slowDownCar = Mathf.MoveTowards(slowDownCar, 0, Time.deltaTime * .01f);
                return true;
            }

            slowDownCar = 1;
            
            return false;
            #endregion
        }

        void CalculateRBVelocity()
        {
            #region 
            forwardRBVel = Vector3.Dot(rb.transform.forward, currentRBVel); 
            #endregion
        }

        float GetSidewayDragLeft()
        {
            #region 
            return sidewayDragLeft * gripAmountDependingSurface; 
            #endregion
        }

        void ApplyDownForceDuringJump(WheelParams wheelParams)
        {
            #region 
            rb.AddForceAtPosition(-Vector3.up * jumpDownForceApplied, wheelParams.spring.position); 
            #endregion
        }

        Vector3 GetSuspensionForce(float hitDistance, WheelParams wheelParams,bool isGrounded)
        {
            #region
            wheelParams.lastLength = wheelParams.springLength;
            wheelParams.lastSpringVelocity = wheelParams.springVelocity;
            wheelParams.springLength = hitDistance -wheelParams.wheelRadius - wheelParams.wheelOffsetRadius;
            wheelParams.springLength = Mathf.Clamp(wheelParams.springLength, 0, wheelParams.maxLength + suspensionRuntimeOffset);


            wheelParams.springVelocity = (wheelParams.lastLength - wheelParams.springLength) / Time.fixedDeltaTime;

            if (wheelParams.springLength < (wheelParams.suspensionLength + suspensionRuntimeOffset) * (1 - wheelParams.springMaxCompressionRatio) && 
                wheelParams.springVelocity > 0 && 
                wheelParams.lastSpringVelocity > 0)
                wheelParams.damperStiffeness = wheelParams.damperStiffenessMax;
            else
                wheelParams.damperStiffeness = wheelParams.damperStiffenessMin;
          
            wheelParams.springForce = wheelParams.springStiffness * (wheelParams.suspensionLength + suspensionRuntimeOffset - wheelParams.springLength);


            if(!isGrounded)
                wheelParams.springForce = Mathf.Clamp(wheelParams.springForce, 0, 0);


           // Debug.Log(wheelParams.springForce);

            wheelParams.damperForce = wheelParams.damperStiffeness * wheelParams.springVelocity;

            Vector3 newSuspensionForce = (wheelParams.springForce + wheelParams.damperForce) * wheelParams.spring.transform.up;

            return newSuspensionForce;

            #endregion
        }

        public void CarMoveParameters(float steerRatio, float accelerationRatio)
        {
            #region 
            steer = steerRatio;
            acceleration = accelerationRatio; 
            #endregion
        }

        Vector3 MoveForwardForce(WheelParams wheelParams)
        {
            #region
            Vector3 force = Vector3.zero;
            if (wheelParams.isObstacle && !lockAcc)
            {
                float amountOfForce = AmountOfForceDependingCarSpeed();

                if (carState.carPlayerType == CarState.CarPlayerType.Human)
                {
                    if (acceleration > .1f)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * amountOfForce;
                    }
                    if (acceleration < -.1f && carState.carDirection != CarState.CarDirection.Backward)
                    {
                            force = forwardForceApplied * rb.transform.forward * acceleration * BrakeForce * amountOfForce * gripAmountDependingSurface;
                    }
                    if (acceleration < -.1f && carState.carDirection == CarState.CarDirection.Backward)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * 1.25f * amountOfForce;
                    }

                    if (carState.carForceApply == CarState.CarForceApply.Brake && carState.carDirection == CarState.CarDirection.Backward)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * 2f;
                    }

                }
                else
                {
                    if (acceleration > 0)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * amountOfForce;
                    }
                    if (acceleration < 0 && carState.carDirection != CarState.CarDirection.Backward)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * BrakeForce * amountOfForce * gripAmountDependingSurface;
                    }
                    if (acceleration < 0 && carState.carDirection == CarState.CarDirection.Backward)
                    {
                        force = forwardForceApplied * rb.transform.forward * acceleration * .5f * amountOfForce;
                    }
                }
            }
            return force; 
            #endregion
        }

        float AmountOfForceDependingCarSpeed()
        {
            #region
            float result = currentRBVel.magnitude / (refMaxSpeed * .75f);
            result = Mathf.Clamp(result, 0, 1 - speedRatioWhenCarIsStopped);
            result += speedRatioWhenCarIsStopped;
            result = amountOfForceCurve.Evaluate(result);
            return result;
            #endregion
        }

        public bool IsCarOnTheGround(int howManyWheels = 2)
        {
            #region
            int howManyWheelsOnGround = 0;
            for (var i = 0; i < wheelsList.Count; i++)
            {
                if (wheelsList[i].isObstacle)
                    howManyWheelsOnGround++;

                if (howManyWheelsOnGround >= howManyWheels)
                    return true;
            }

            return false;

            #endregion
        }

        Quaternion DeltaRotation(Transform spring)
        {
            #region
            float carOnGroundCoef = 1;
            if (!IsCarOnTheGround())
                carOnGroundCoef = .25f ;
            // Debug.Log("smoothRotation: " + smoothRotation + " -- " + "steer: " + steer);
            int steerBackwardDirTarget = 1;
            if (carState.carDirection == CarState.CarDirection.Backward && carState.moveDir == CarMoveDirection.backward)
                steerBackwardDirTarget = -1;

            if (carState.carDirection == CarState.CarDirection.Stop && carState.moveDir == CarMoveDirection.backward)
                steerBackwardDirTarget = -1;

            if (lastSteerBackwardDir == -1 && carState.moveDir == CarMoveDirection.center)
                steerBackwardDirTarget = -1;
            else if (lastSteerBackwardDir == 1 && carState.moveDir == CarMoveDirection.center)
                steerBackwardDirTarget = 1;

            lastSteerBackwardDir = steerBackwardDirTarget;

            float speedRatio = rb.linearVelocity.magnitude / 90;
            speedRatio *= 5;
            speedRatio = Mathf.Clamp(speedRatio, 1, 5);
            steerBackwardDir = Mathf.MoveTowards(steerBackwardDir, steerBackwardDirTarget, Time.deltaTime * speedRatio);

            // Adjust rotation depending surface
            float boostRotDependingSurface = AdjustRotationDependingSurface();

            return Quaternion.Euler(spring.up * (speedRotation * boostRotDependingSurface + rotBoost) * Time.fixedDeltaTime * smoothRotCurve.Evaluate(smoothRotation) * carOnGroundCoef * steer * steerBackwardDir);
            #endregion
        }

        float AdjustRotationDependingSurface()
        {
            #region
            float boostRotDependingSurface = 1 - gripAmountDependingSurface;
            if (carState.carPlayerType == CarState.CarPlayerType.AI)
                boostRotDependingSurface += 1;
            else
                boostRotDependingSurface = 1;

            return boostRotDependingSurface;
            #endregion
        }

        private void OnDrawGizmos()
        {
            #region 
            if (isGizmosEnable)
            {
                DrawSpringInfo();
                DrawSidewayForce();
                DrawWheelCollider();
                DrawPerpWithBorder();
            } 
            #endregion
        }

        void DrawSpringInfo()
        {
            #region
            for (var i = 0; i < wheelsList.Count; i++)
            {
                Transform spring = wheelsList[i].spring;
                if (spring)
                {
                   
                    Vector3 p1 = spring.position + spring.right * wheelsList[i].capsuleCastOffset.x /*+ spring.up * wheelsList[i].WheelWidth*/;

                    // Draw Spring Origin
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(p1, .025f);

                    // Draw ground hit position.
                    if (wheelsList[i].isObstacle)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(wheelsList[i].hitPoint, .1f);
                        Gizmos.DrawLine(wheelsList[i].hitPoint + spring.up * wheelsList[i].distanceToObstacle, wheelsList[i].hitPoint);

                       // Gizmos.DrawLine(spring.position - spring.up * wheelsList[i].distanceToObstacle, spring.position);

                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(wheelsList[i].rbSweepTest.position, .05f) ;
                        Gizmos.DrawSphere(wheelsList[i].rbSweepTest.position, .05f);
                    }
                }
            }
            #endregion
        }

        void DrawSpringGizmos()
        {
            #region
            for (var i = 0; i < wheelsList.Count; i++)
            {
                Transform spring = wheelsList[i].spring;
                if (spring)
                {
                    WheelParams wheelParams = wheelsList[i];

                    // Spring travel
                    Debug.DrawLine(spring.position, spring.position - spring.transform.up * wheelParams.springLength, Color.red);
                    // Spring pivot
                    Gizmos.color = Color.green;
                    Vector3 springPivotPos = spring.position - spring.up * wheelParams.springLength;
                    Gizmos.DrawSphere(springPivotPos, .02f);

                    // Wheel diameter
                    Debug.DrawLine(spring.position - spring.transform.up * wheelParams.springLength, spring.position - spring.transform.up * (wheelParams.springLength + wheelParams.wheelRadius * 2), Color.green);
                    // Wheel pivot
                    Gizmos.color = Color.red;
                    Vector3 wheelPivotPos = spring.position - spring.up * wheelParams.springLength - spring.up * wheelParams.wheelRadius;
                    Gizmos.DrawSphere(wheelPivotPos, .02f);
                }
            }
            #endregion
        }

        void DrawSidewayForce()
        {
            #region
            if (rb)
            {
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    Transform spring = wheelsList[i].spring;
                    Debug.DrawRay(spring.position, currentRBVel.normalized * 5, Color.blue);
                    //Gizmos.color = Color.blue;
                    float xVel = Vector3.Dot(spring.right, currentRBVel);

                    Debug.DrawRay(spring.position, rb.transform.right * xVel,Color.green);
                }
            }
            #endregion
        }

        void DrawForwardForce()
        {
            #region
            if (rb)
            {
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    Transform spring = wheelsList[i].spring;
                    Debug.DrawRay(spring.position, currentRBVel.normalized * 10);
                    Gizmos.color = Color.blue;

                    Debug.DrawRay(spring.position, rb.transform.forward * forwardRBVel);
                }
            }
            #endregion
        }

        void CheckImpact()
        {
            #region
          /*  if (counter == 0)
            {
                ImpactDetectorB.position = lastDectorPos;
                // lastDectorPos2 = ImpactDetector2.position;
                ImpactDetector2B.position = lastDectorPos2;
            }


           
            RaycastHit hit;
           // Vector3 dir = (ImpactDetector.position - lastDectorPos).normalized;
            if (Physics.Linecast(ImpactDetector.position, lastDectorPos, out hit,wheelLayer))
            {
                if(xPos01 > 20)
                {
                   //if(carF) carF.rotSpeedCoef = 0;
                    float ratio = currentRBVel.magnitude / maxSpeed;
                    if (!lastFrameFrontalImpact)
                    {
                       
                        if (lockAcc == false)
                        {
                            
                            StartCoroutine(LockAccelerationRoutine());
                        }
                    }
                    Debug.Log("imp 1");
                    currentRBVel -= currentRBVel * .1f * (1 - ratio);
                    rb.velocity = currentRBVel;
                    lastFrameFrontalImpact = true;
                    
                }

            }
            else if (Physics.Linecast(ImpactDetector2.position, lastDectorPos2, out hit, wheelLayer))
            {
                if (xPos01 > 20)
                {
                    //if (carF) carF.rotSpeedCoef = 0;
                    float ratio = currentRBVel.magnitude / maxSpeed;
                    if (!lastFrameFrontalImpact)
                    {
                        if (lockAcc == false)
                        {
                            StartCoroutine(LockAccelerationRoutine());
                        }
                    }
                    Debug.Log("imp 2");
                    currentRBVel -= currentRBVel * .1f * (1 - ratio);
                    rb.velocity = currentRBVel;
                    lastFrameFrontalImpact = true;
                }
            }
            else
            {
                lastFrameFrontalImpact = false;
            }



            if(counter == 0)
            {
                lastDectorPos = ImpactDetector.position;
                lastDectorPos2 = ImpactDetector2.position;
            }

            float ratio2 = currentRBVel.magnitude / maxSpeed;
            xPos01 = Mathf.MoveTowards(ImpactDetector.localPosition.z, 40f * ratio2, Time.deltaTime * 300);

            ImpactDetector.localPosition = new Vector3(ImpactDetector.localPosition.x, ImpactDetector.localPosition.y, xPos01);
            ImpactDetector2.localPosition = new Vector3(ImpactDetector2.localPosition.x, ImpactDetector2.localPosition.y, xPos01);

            xPos02 = Mathf.MoveTowards(grp_Detector[0].transform.localPosition.z, 1.6f * ratio2, Time.deltaTime * 20);

            for(var i = 0;i< grp_Detector.Count; i++)
            {
                grp_Detector[i].localPosition = new Vector3(grp_Detector[i].localPosition.x, grp_Detector[i].localPosition.y, xPos02);
            }

            counter++;
            counter %= 6;*/
            #endregion
        }

        // prevent weird jump when collide small object on floor 
        void CheckVelocityVariation()
        {
            #region 
            /*if (rb)
            {
                Vector3 localVelocity = transform.position;

                float diff = transform.position.y - lastVelocity.y;

                // Debug.Log( diff + " -> " + localVelocity + " -> " + lastVelocity);
                if (diff > .3f && !IsCarOnTheGround(2))
                {
                    increaseDrag = true;
                }
                else
                {

                }

                lastVelocity = localVelocity;

                //Debug.DrawRay(transform.position, rb.velocity.normalized*10,Color.green);

                if (increaseDrag)
                {
                    currentRBVel *= .25f;
                    rb.velocity = currentRBVel;
                    rb.angularDrag = .1f;
                    increaseDrag = false;
                }
                else
                {
                    rb.angularDrag = Mathf.MoveTowards(rb.angularDrag, 5f, Time.deltaTime * 1);
                }
            } */
            #endregion
        }
       
        private void OnCollisionEnter(Collision collision)
        {
            #region 
            if (collision.relativeVelocity.magnitude > 0 && !waitSec)
            {
                impactMagnitude = collision.relativeVelocity.magnitude;
                isImpactDetected = true;
            }

            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            } 
            #endregion
        }

        public IEnumerator WaitForSecondsAterPlayingCollisionSound(float sec = 1)
        {
            #region 
            waitSec = true;
            float t = 0;
            while (t < sec)
            {
                t += Time.deltaTime;
                yield return null;
            }

            waitSec = false;
            yield return null; 
            #endregion
        }

        private void OnCollisionStay(Collision collision)
        {
            #region 
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            } 
            #endregion
        }
       
        IEnumerator LockAccelerationRoutine()
        {
            #region 
            yield return null; 
            #endregion
        }

        void DrawPerpWithBorder()
        {
            #region
            RaycastHit hit;

            if (Physics.Raycast(transform.position, rb.transform.forward, out hit,10,wheelLayer))
            {
                
                Debug.DrawLine(transform.position, hit.point,Color.magenta);

                Debug.DrawRay(hit.point, hit.normal * 3, Color.yellow);

                Vector3 perpendicular2 = Vector3.Cross(hit.normal, rb.transform.forward);
                Debug.DrawRay(hit.point, perpendicular2, Color.green);

                Vector3 perpendicular3 = Vector3.Cross(hit.normal, -perpendicular2);
                Debug.DrawRay(hit.point, perpendicular3, Color.blue);
            }
            #endregion
        }

        void RuntimeCarParamsEdition()
        {
            #region
            if (editParametersAtRuntime)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }
            #endregion
        }
        
        void CheckInputs()
        {
            #region 
            if (carState.steeringDir == CarSteeringDirection.Left)
            {
                if (carState.lastSteeringDir != carState.steeringDir) smoothRotation = 0;

                float ratio = currentRBVel.magnitude / 4;
                ratio = Mathf.Clamp01(ratio);

                smoothRotation = Mathf.MoveTowards(smoothRotation, 1 * ratio, Time.deltaTime * smoothRotCurveSpeed);
            }
            else if (carState.steeringDir == CarSteeringDirection.Right)
            {
                if (carState.lastSteeringDir != carState.steeringDir) smoothRotation = 0;
                float ratio = currentRBVel.magnitude / 4;
                ratio = Mathf.Clamp01(ratio);
                smoothRotation = Mathf.MoveTowards(smoothRotation, 1 * ratio, Time.deltaTime * smoothRotCurveSpeed);
            }
            else
            {
                smoothRotation = 0;
                float ratio = currentRBVel.magnitude / 4;
                ratio = Mathf.Clamp01(ratio);
                smoothRotation = Mathf.MoveTowards(smoothRotation, 1 * ratio, Time.deltaTime * smoothRotCurveSpeed);
            }

            CheckDriftInput(); 
            #endregion
        }

        void CheckDriftInput()
        {
            #region 
            if (carState.carDrift == CarState.CarDrift.NoDrift && isBrakePressed)
                isBrakePressed = false;

            if (carState.carDrift == CarState.CarDrift.Drift)
            {
                currentDragLeft = Mathf.MoveTowards(currentDragLeft, sidewayDragLeftRef * 1000, Time.deltaTime * 250);
                sidewayDragLeft = currentDragLeft * .0001f;
                rotBoost = Mathf.MoveTowards(rotBoost, 7, Time.deltaTime * 4);
            }
            else
            {
                currentDragLeft = Mathf.MoveTowards(currentDragLeft, sidewayDragLeftRef * 10000, Time.deltaTime * 200);
                sidewayDragLeft = currentDragLeft * .0001f;
                rotBoost = Mathf.MoveTowards(rotBoost, 0, Time.deltaTime * 4);
            } 
            #endregion
        }

        public float carAIOffset = 0;
        void UpdateForwardForceAppliedToCar()
        {
            #region 
           float forwardForceAppliedDependingSpeed = forwardForceAppliedRef * forwardForceAppliedSpeedCurve.Evaluate(currentRBVel.magnitude / 50) + carAIOffset;
            if (carState.carDrift == CarState.CarDrift.Drift && !isBrakePressed)
            {
                forwardForceApplied = Mathf.MoveTowards(forwardForceApplied, forwardForceAppliedDependingSpeed * .33f, Time.deltaTime * 7500);
            }
            else if (carState.carDirection == CarState.CarDirection.Backward)
            {
                forwardForceApplied = Mathf.MoveTowards(forwardForceApplied, forwardForceAppliedDependingSpeed, Time.deltaTime * 6500);
            }
            else
            {
                forwardForceApplied = Mathf.MoveTowards(forwardForceApplied, forwardForceAppliedDependingSpeed, Time.deltaTime * 6500);
            }
            #endregion
        }

        void CarRotationDependingSpeed()
        {
            #region 
            float normalizedSpeed = currentRBVel.magnitude / maxSpeed;
            speedRotation = speedRotationRef * rotationSpeedCurve.Evaluate(normalizedSpeed); 
            #endregion
        }

        void BrakeFeedback()
        {
        }
       
        IEnumerator WaitBrakHeadlightRoutine()
        {
            #region 
            yield return null; 
            #endregion
        }

        float lastSpeedAmountDependingSurface = 0;
        void SpeedAmountDependingSurface()
        {
            #region
            float amount = lastSpeedAmountDependingSurface;
            if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
            {
                amount = 0;
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    bool found = false;
                    for (var j = 0; j < surfaceData.surfaceList.Count; j++)
                    {
                        if (wheelsList[i].surface == surfaceData.surfaceList[j].roadType)
                        {
                            amount += surfaceData.surfaceList[j].speedAmount;
                            found = true;
                            break;
                        }

                    }
                    if (!found)
                        amount += 1;
                }

                amount /= wheelsList.Count;
                lastSpeedAmountDependingSurface = amount;
            }
            speedAmountDependingSurface = Mathf.MoveTowards(speedAmountDependingSurface, amount, Time.deltaTime * .5f);
            #endregion
        }

        float lastGripAmountDependingSurface = 0;
        void GripAmountDependingSurface()
        {
            #region
            float amount = lastGripAmountDependingSurface;

            if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
            {
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    bool found = false;
                    for (var j = 0; j < surfaceData.surfaceList.Count; j++)
                    {
                        if (wheelsList[i].surface == surfaceData.surfaceList[j].roadType)
                        {
                            amount += surfaceData.surfaceList[j].gripAmount;
                            found = true;
                            break;
                        }

                    }
                    if (!found)
                        amount += 1;
                }

                amount /= wheelsList.Count;

                if (carState.carPlayerType == CarState.CarPlayerType.AI)
                    amount += .15f;
                amount = Mathf.Clamp(amount, 0, .9f);
                lastGripAmountDependingSurface = amount;
            }
           

            gripAmountDependingSurface = Mathf.MoveTowards(gripAmountDependingSurface, amount, Time.deltaTime * .5f);

            #endregion
        }

        void AverageSurface()
        {
            #region 
            if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
            {
                surfaceAverage = 0;

                int howManySurface = howManyDifferentSurface;

                List<int> surfaceByWheel = new List<int>();

                // Detect the current surface for each wheel
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    for (var j = 0; j < howManySurface; j++)
                    {
                        if ((int)wheelsList[i].surface == j)
                        {
                            surfaceByWheel.Add(j);
                            break;
                        }
                    }
                }

                // Check if the surface change from the previous frame
                int checkLastSurface = 0;
                for (var j = 0; j < surfaceByWheel.Count; j++)
                {
                    if (surfaceByWheel[j] == mostUseSurface)
                        checkLastSurface++;
                }

                // If the surface changed
                if (checkLastSurface < 2)
                {
                    mostUseSurface = 0;
                    for (var i = 0; i < howManySurface; i++)
                    {
                        int counter = 0;
                        for (var j = 0; j < surfaceByWheel.Count; j++)
                        {
                            if (surfaceByWheel[j] == i)
                                counter++;
                        }

                        if (counter >= 2)
                        {
                            mostUseSurface = i;
                            break;
                        }
                    }
                    surfaceAverage = mostUseSurface;
                }
                averageSurface = (RoadType)mostUseSurface;
            }
            #endregion
        }

        public void InitVehiclePosition(Vector3 pos)
        {
            #region 
            refInitPos = pos;
            transform.position = pos; 
            #endregion
        }

        public void InitVehicleOffsetPosition(Vector3 pos)
        {
            #region 
            StartCoroutine(InitVehicleOffsetPositionRoutine(pos));
            #endregion
        }

        public void InitVehicleGyroPosition(Quaternion quat)
        {
            #region 
            refInitRot = quat;
            transform.rotation = quat;
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region 
            yield return new WaitUntil(() => transform.position == refInitPos);
            yield return new WaitUntil(() => transform.rotation == refInitRot);
            rb.isKinematic = false;
            yield return null; 
            #endregion
        }

        public bool BEnableIsKinemetic()
        {
            #region 
            rb.isKinematic = true;
            return true; 
            #endregion
        }

        public bool BDisableIsKinemetic()
        {
            #region 
            rb.isKinematic = false;
            return true; 
            #endregion
        }

        public void RibodyStartConstraint()
        {
            #region
            rb.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
            #endregion
        }

        public void RibodyResetConstraint()
        {
            #region
            rb.constraints = RigidbodyConstraints.None;
            #endregion
        }

        private void DrawWheelCollider()
        {
            #region
            for (var i = 0; i < wheelsList.Count; i++)
            {
                Transform spring = wheelsList[i].spring;
                if (spring)
                {
                    WheelParams wheelParams = wheelsList[i];
                    //RaycastHit hit;
                    Vector3 extraRadiusUp = Vector3.up * wheelsList[i].wheelOffsetRadius;
                    Vector3 p1 = spring.position + extraRadiusUp + spring.right * wheelsList[i].capsuleCastOffset.x /*- spring.up * wheelsList[i].wheelRadius*/;

                    Gizmos.DrawSphere(p1, .025f);

                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(spring.position, new Vector3(.05f, .05f, .05f)) ;

                    // Draw A square to represent the wheel
                    Vector3 springExpansion = -spring.up * wheelsList[i].springLength;

                    Vector3 pos1 = p1 + springExpansion - wheelsList[i].wheelAxisY.forward * (wheelsList[i].wheelRadius + wheelsList[i].wheelOffsetRadius);
                    Vector3 pos2 = p1 + springExpansion + spring.up * (wheelsList[i].wheelRadius + wheelsList[i].wheelOffsetRadius);
                    Vector3 pos3 = p1 + springExpansion + wheelsList[i].wheelAxisY.forward * (wheelsList[i].wheelRadius + wheelsList[i].wheelOffsetRadius);
                    Vector3 pos4 = p1 + springExpansion - spring.up * (wheelsList[i].wheelRadius + wheelsList[i].wheelOffsetRadius);

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(pos1 - spring.right * wheelsList[i].WheelWidth, pos2 - spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos2 - spring.right * wheelsList[i].WheelWidth, pos3 - spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos3 - spring.right * wheelsList[i].WheelWidth, pos4 - spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos4 - spring.right * wheelsList[i].WheelWidth, pos1 - spring.right * wheelsList[i].WheelWidth);

                    Gizmos.DrawLine(pos1 + spring.right * wheelsList[i].WheelWidth, pos2 + spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos2 + spring.right * wheelsList[i].WheelWidth, pos3 + spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos3 + spring.right * wheelsList[i].WheelWidth, pos4 + spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(pos4 + spring.right * wheelsList[i].WheelWidth, pos1 + spring.right * wheelsList[i].WheelWidth);

                    // Draw a line to represent the spring
                    Gizmos.color = Color.red;
                    Vector3 posSpring = spring.position + spring.right * wheelsList[i].capsuleCastOffset.x + spring.up * wheelsList[i].WheelWidth;
                    Vector3 posWheelCenter = p1 + springExpansion;
                    Gizmos.DrawLine(posSpring - spring.right * wheelsList[i].WheelWidth, posWheelCenter - spring.right * wheelsList[i].WheelWidth);
                    Gizmos.DrawLine(posSpring + spring.right * wheelsList[i].WheelWidth, posWheelCenter + spring.right * wheelsList[i].WheelWidth);

                    Gizmos.color = Color.green;
                    springExpansion = -spring.up * (wheelsList[i].springLength - wheelsList[i].suspensionLength + suspensionRuntimeOffset);
                    Gizmos.DrawLine(posSpring - spring.right * wheelsList[i].WheelWidth, posWheelCenter - spring.right * wheelsList[i].WheelWidth + springExpansion);
                    Gizmos.DrawLine(posSpring + spring.right * wheelsList[i].WheelWidth, posWheelCenter + spring.right * wheelsList[i].WheelWidth + springExpansion);
                }
            }
            #endregion
        }

        public void VehicleCustomizationUpdateMaxSpeed(float percentage)
        {
            #region
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            // 1P
            if (howManyPlayer == 1 && vehicleInfo.playerNumber == 0)
            {
                float increaseMaxSpeed = maxSpeed + maxSpeed * percentage * .01f;
                maxSpeed = increaseMaxSpeed;
                refMaxSpeed = maxSpeed;
            }

            // 2P split screen
            if (howManyPlayer == 2 && 
                (vehicleInfo.playerNumber == 0 || vehicleInfo.playerNumber == 1))
            {
                float increaseMaxSpeed = maxSpeed + maxSpeed * percentage * .01f;
                maxSpeed = increaseMaxSpeed;
                refMaxSpeed = maxSpeed;
            }
            #endregion
        }

        public void VehicleCustomizationUpdateAcceleration(float percentage)
        {
            #region
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            // 1P
            if (howManyPlayer == 1 && vehicleInfo.playerNumber == 0)
            {
                float increaseForceApplied = forwardForceAppliedRef + forwardForceAppliedRef * percentage * .01f;
                forwardForceAppliedRef = increaseForceApplied;
            }

            // 2P split screen
            if (howManyPlayer == 2 &&
                (vehicleInfo.playerNumber == 0 || vehicleInfo.playerNumber == 1))
            {
                float increaseForceApplied = forwardForceAppliedRef + forwardForceAppliedRef * percentage * .01f;
                forwardForceAppliedRef = increaseForceApplied;
            }
            #endregion
        }

        public void VehicleCustomizationUpdateBrakeForce(float percentage)
        {
            #region
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            // 1P
            if (howManyPlayer == 1 && vehicleInfo.playerNumber == 0)
            {
                float increaseBrake = BrakeForce + BrakeForce * percentage * .01f;
                BrakeForce = increaseBrake;
            }

            // 2P split screen
            if (howManyPlayer == 2 &&
                (vehicleInfo.playerNumber == 0 || vehicleInfo.playerNumber == 1))
            {
                float increaseBrake = BrakeForce + BrakeForce * percentage * .01f;
                BrakeForce = increaseBrake;
            }
            #endregion
        }

        void VehicleOutOfLimitZone()
        {
            #region 
            //Debug.Log("Out of Limit");
            StartCoroutine(VehicleOutOfLimitZoneRoutine()); 
            #endregion
        }

        IEnumerator VehicleOutOfLimitZoneRoutine()
        {
            #region
            rb.isKinematic = true;

            carState.carSkid = CarState.CarSkid.NoSkid;

            GetComponent<CarPlayerInputs>().isInputEnabled = false;

            // Reset Wheels rotation
            steer = 0;
            acceleration = 0;
            for (var i = 0; i < wheelsList.Count; i++)
                wheelsList[i].wheelAxisY.rotation = Quaternion.identity;

            // Find spawn position
            VehiclePathFollow vehiclePathFollow = GetComponent<VehiclePathFollow>();

            if (vehiclePathFollow && 
                LapCounterAndPosition.instance &&
                LapCounterAndPosition.instance.posList.Count > vehicleInfo.playerNumber)
            {
                // Case Start Line
                float distanceToStartLine = 0;
                int dirRespawnDistance = 1;
                if (!vehiclePathFollow.Track.TrackIsLooped)
                {
                    distanceToStartLine += vehiclePathFollow.Track.checkpointsDistanceFromPathStart[2];

                    if (LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].howLapDone == 1)
                    {
                        respawnProgressDistance = distanceToStartLine - 20;
                        dirRespawnDistance = -1;
                        Debug.Log("Start: " + respawnProgressDistance);
                    }

                    else if (respawnProgressDistance < distanceToStartLine + 25 && 
                        respawnProgressDistance > -25)
                    {
                        respawnProgressDistance = distanceToStartLine + 20;
                        dirRespawnDistance = 1;
                    }
                        
                    int howManyCheckpoints = vehiclePathFollow.Track.checkpointsDistanceFromPathStart.Count;
                    distanceToStartLine = vehiclePathFollow.Track.checkpointsDistanceFromPathStart[howManyCheckpoints - 4];

                    if (respawnProgressDistance > distanceToStartLine - 50 )
                    {
                        respawnProgressDistance = distanceToStartLine - 60;
                        dirRespawnDistance = -1;
                         Debug.Log("Start 0");
                    }
                    //Debug.Log("Start 0");
                }
                else
                {
                    // Regular Mode
                    if (!vehiclePathFollow.Track.improveAIPathLoopMode)
                    {
                       // Debug.Log("Lock: " + LapCounterAndPosition.instance.lapCounterBadgeList[vehicleInfo.playerNumber].Lock );
                        //-> Vehicle is between Last Checkpoint and 1st checkpoint
                        if (LapCounterAndPosition.instance.lapCounterBadgeList[vehicleInfo.playerNumber].bIsPlayerIntoBufferZone)
                        {
                            respawnProgressDistance = vehiclePathFollow.Track.pathLength - 20;
                            dirRespawnDistance = -1;
                            //Debug.Log("Start: " + respawnProgressDistance);
                        }
                        //-> Vehicle is after startLine
                        else if (respawnProgressDistance > 0 && respawnProgressDistance < 30 
                            ||
                            LapCounterAndPosition.instance.lapCounterBadgeList[vehicleInfo.playerNumber].Lock)
                        {
                            respawnProgressDistance = 20;
                            dirRespawnDistance = 1;
                            //Debug.Log("Start 2: " + respawnProgressDistance);
                        }

                       // Debug.Log("Start 4: " + respawnProgressDistance);
                    }
                    // Game Mode 3 with 1 AI to improve Ai path
                    else
                    {
                        respawnProgressDistance = vehiclePathFollow.Track.improveAIPathStartID * vehiclePathFollow.Track.spotDifficultyDistance;
                        //Debug.Log("Start 3");
                    }
                }

                respawnProgressDistance += RespawnPosUpdateIfObstacleOnroad(vehiclePathFollow, dirRespawnDistance);

                vehiclePathFollow.progressDistance = respawnProgressDistance;

                vehiclePathFollow.offsetAIPos = Vector2.zero;
                vehiclePathFollow.currentTargetOffsetAIPos = Vector2.zero;

                yield return new WaitUntil(() => vehiclePathFollow.progressDistance == respawnProgressDistance);

                vehicleDamage.Invisibility();

                Vector3 spawnPos = vehiclePathFollow.Track.PositionOnPath(respawnProgressDistance, 0);
                Vector3 spawnLookAt = vehiclePathFollow.Track.PositionOnPath(respawnProgressDistance + 3, 0);

                // Force the vehicle to spawn in a special position;
                if(specialRespawnPosition != Vector3.zero){
                    spawnPos = specialRespawnPosition;
                    spawnLookAt = specialRespawnDirection;
                }

                // Find ground position
                if (Physics.Raycast(spawnPos + 5 * transform.up, -transform.up, out RaycastHit hit,100,RespawnLayer))
                {
                    Debug.Log(hit.transform.name);
                    float distance = Vector3.Distance(spawnPos, hit.point);
                    Vector3 dir = (hit.point - spawnPos).normalized;
                    spawnPos += distance * dir;
                    spawnLookAt += distance * dir;
                }

                // Move to spawn position
                transform.position = spawnPos;
                yield return new WaitUntil(() => transform.position == spawnPos);
                transform.rotation = Quaternion.identity;
                yield return new WaitUntil(() => transform.rotation == Quaternion.identity);
                transform.LookAt(spawnLookAt);

                // Allow the player to use the vehicle
                rb.isKinematic = false;
                GetComponent<CarPlayerInputs>().isInputEnabled = true;
            }

            yield return null;
            #endregion
        }

        void VehicleRespawn()
        {
            #region
            
            #endregion
        }

        void VehicleRespawnStartProcess()
        {
            #region
            respawnProgressDistance = LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].lastPathDistance;
            #endregion
        }

        float RespawnPosUpdateIfObstacleOnroad(VehiclePathFollow vehiclePathFollow , int dirRespawnDistance)
        {
            #region 
            bool objectDetected = false;
            int howManyObjectDetected = 0;
            int increaseRespawnDistance = 0;

            //Debug.Log("dir: " + dirRespawnDistance);

            while (!objectDetected)
            {
                Collider[] hitColliders = Physics.OverlapSphere(vehiclePathFollow.Track.PositionOnPath(respawnProgressDistance + increaseRespawnDistance, 0), 5, RespawnLayer);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.GetComponent<ExcludeRespawnZoneTag>())
                    {
                        increaseRespawnDistance = -100000;
                        objectDetected = true;
                        specialRespawnPosition = hitCollider.GetComponent<ExcludeRespawnZoneTag>().respawnPosition.position;
                        specialRespawnDirection = hitCollider.GetComponent<ExcludeRespawnZoneTag>().respawnPosition.position + .1f * hitCollider.GetComponent<ExcludeRespawnZoneTag>().respawnPosition.forward;
                        return 0;
                    }
                    if (!hitCollider.GetComponent<Terrain>() && !hitCollider.GetComponent<RoadTag>())
                    {
                        howManyObjectDetected = 1;
                        break;
                    }
                }

                if (howManyObjectDetected > 0)
                {
                    increaseRespawnDistance += dirRespawnDistance * 5;
                    objectDetected = false;
                }
                else if (howManyObjectDetected == 0)
                {
                    objectDetected = true;
                }
                // Debug.Log("HHH: " + howManyObjectDetected);
                howManyObjectDetected = 0;
            }
            specialRespawnPosition = Vector3.zero;
            return increaseRespawnDistance; 
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

        void ReturnCurbSurface(WheelParams wheel, Transform hit, Vector3 hitPoint)
        {
            #region
            if (hit && hit.GetComponent<CurbTag>())
            {
                wheel.curbDetection = true;
            }
            else
            {
                wheel.curbDetection = false;
            }
            #endregion
        }

        public bool isCurbDetected = false;
        void CurbDetection()
        {
            #region 
            if (Time.frameCount % 4 == vehicleInfo.playerNumber % 4)
            {
                isCurbDetected = false;

                for (var i = 0; i < wheelsList.Count; i++)
                {
                    if (wheelsList[i].curbDetection == true)
                    {
                        isCurbDetected = true;
                        break;
                    }
                }

            }
            #endregion
        }
        IEnumerator InitVehicleOffsetPositionRoutine(Vector3 pos)
        {
            #region 
            // Init Offset vehicle position during countdown
            while (vehicleInfo == null) { yield return null; }

            while (!vehicleInfo.b_InitDone) { yield return null; }

            int howManyPlayerInTheRace = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            if (howManyPlayerInTheRace == 1 && vehicleInfo.playerNumber == 0)
            {
                GetComponent<VehiclePathFollow>().offsetAIPos = Vector2.zero;
            }
            else if (howManyPlayerInTheRace == 2 && vehicleInfo.playerNumber == 1)
            {
                GetComponent<VehiclePathFollow>().offsetAIPos = Vector2.zero;
            }
            else
            {
                GetComponent<VehiclePathFollow>().offsetAIPos = new Vector2(pos.x, pos.y);
            }


            yield return null;
            #endregion
        }


        public void SetStiffSuspensionInMainMenu()
        {
            #region 
            if (stiffSuspensionValue != -1)
            {
                for (var i = 0; i < wheelsList.Count; i++)
                {
                    wheelsList[i].damperStiffenessMin = stiffSuspensionValue;
                    wheelsList[i].damperStiffenessMax = stiffSuspensionValue;
                }

            }
            #endregion
        }
    }
}

