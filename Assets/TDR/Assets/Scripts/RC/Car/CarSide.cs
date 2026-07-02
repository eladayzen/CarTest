// Description: CarSide: dampen obstacle collision using springs.
// Attached to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarSide : MonoBehaviour
    {
        public bool             isInitDone = false;
        public Rigidbody        rb;
        public CarController    carController;

        [System.Serializable]
        public class WheelParams
        {
            public bool         isUsed = true;
            public string       name = "";
            public Transform    spring;
            public float        restLength;
            public float        springTravel;
            public float        springStiffness;
            public float        damperStiffeness;

            [HideInInspector]
            public float        minLength;
            [HideInInspector]
            public float        maxLength;
            [HideInInspector]
            public float        lastLength;
            //[HideInInspector]
            public float        springLength;
            [HideInInspector]
            public float        springVelocity;
            [HideInInspector]
            public float        springForce;
            [HideInInspector]
            public float        damperForce;

            public float        wheelRadius;

            public GameObject   wheelObj;
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

            public Vector3      spherecastOffsetPos = Vector3.zero;

            public float        lastDistance = 0;
        }

        public List<WheelParams>    wheelsList = new List<WheelParams>();
        public List<bool>           hitDetectedList = new List<bool>();

        public bool                 link = true;

        public LayerMask            wheelLayer;
        public List<int>            wheelLayerMaskList = new List<int>();   // 14 Border

        public float                ratio;
        public AnimationCurve       ratioCurve;

        public PredictNextPosition  predictNextPosition;

        private void Start()
        {
            #region 
            Init(); 
            #endregion
        }

        void Init()
        {
            #region 
            wheelLayer = InitLayerMask(wheelLayerMaskList);

            for (var i = 0; i < wheelsList.Count; i++)
            {
                WheelParams wheelParams = wheelsList[i];
                if (wheelParams.spring)
                {
                    wheelParams.minLength = wheelParams.restLength - wheelParams.springTravel;
                    wheelParams.maxLength = wheelParams.restLength;
                }

                hitDetectedList.Add(true);
            }

            UpdateValue();

            isInitDone = true; 
            #endregion
        }

        void UpdateValue()
        {
            #region 
            for (var i = 0; i < wheelsList.Count; i++)
            {
                WheelParams wheelParams = wheelsList[i];

                wheelParams.restLength = wheelsList[i].restLength;
                wheelParams.springTravel = wheelsList[i].springTravel;
                wheelParams.springStiffness = wheelsList[i].springStiffness;
                wheelParams.damperStiffeness = wheelsList[i].damperStiffeness;
                wheelParams.wheelRadius = wheelsList[i].wheelRadius;

                wheelParams.minLength = wheelParams.restLength - wheelParams.springTravel;
                wheelParams.maxLength = wheelParams.restLength;
            } 
            #endregion
        }

        void FixedUpdate()
        {
            #region 
            if (isInitDone)
            {
                CalculateSpringsUsingSingleRaycast();
                UpdateValue();
            } 
            #endregion
        }

        void CalculateSpringsUsingSingleRaycast()
        {
            #region
            for (var i = 0; i < wheelsList.Count; i++)
            {
                Transform spring = wheelsList[i].spring;
                if (spring && wheelsList[i].isUsed)
                {

                    if (Time.frameCount % 4 == carController.vehicleInfo.playerNumber % 4 || hitDetectedList[i])
                    {

                        WheelParams wheelParams = wheelsList[i];
                        if (Physics.Raycast(spring.position, spring.transform.forward, out RaycastHit hit, wheelParams.maxLength + wheelParams.wheelRadius * 2, wheelLayer))
                        {
                            if (hit.distance != wheelsList[i].lastDistance)
                            {
                                UpdateSpringAndMove(hit.distance, hit.point, wheelsList[i], i);
                                wheelsList[i].lastDistance = hit.distance;

                                ratio = rb.linearVelocity.magnitude / 60;
                                ratio = Mathf.Clamp01(ratio);
                                ratio = ratioCurve.Evaluate(ratio);

                                wheelsList[i].isObstacle = true;
                            }

                            hitDetectedList[i] = true;
                        }
                        else
                        {
                            wheelsList[i].isObstacle = false;
                            hitDetectedList[i] = false;
                        }
                    }
                    else
                    {
                        hitDetectedList[i] = false;
                    }
                }

                if (i == 0 || i == 2)
                {
                }
                else
                {
                    float speedRatio = rb.linearVelocity.magnitude / carController.maxSpeed;
                    speedRatio = Mathf.Clamp01(speedRatio);

                    wheelsList[i].springStiffness = 100000 * speedRatio;

                }
            }
            #endregion
        }

        void UpdateSpringAndMove(float hitDistance, Vector3 hitPoint, WheelParams wheelParams,int ID)
        {
            #region 
            Vector3 suspensionForce = GetSuspensionForce(hitDistance, wheelParams, ID);

            float ratio = rb.linearVelocity.magnitude / carController.maxSpeed;
            ratio = Mathf.Clamp01(ratio);
         
            float multiplier = 1;
            if (!predictNextPosition.collisionDetected)
                multiplier = .1f;

            rb.AddForceAtPosition(suspensionForce * 1f * ratio * multiplier, wheelParams.wheelObj.transform.position); 
            #endregion
        }

        Vector3 GetSuspensionForce(float hitDistance, WheelParams wheelParams, int ID)
        {
            #region
            //float ratio = rb.velocity.magnitude / 90;

            wheelParams.lastLength = wheelParams.springLength;
            wheelParams.springLength = hitDistance - wheelParams.wheelRadius * 2;
            wheelParams.springLength = Mathf.Clamp(wheelParams.springLength, wheelParams.minLength, wheelParams.maxLength);
            wheelParams.springVelocity = (wheelParams.lastLength - wheelParams.springLength) / Time.fixedDeltaTime;
            wheelParams.springForce = wheelParams.springStiffness * (wheelParams.restLength - wheelParams.springLength);
            wheelParams.damperForce = wheelParams.damperStiffeness * wheelParams.springVelocity;

            Vector3 left = Vector3.Cross(wheelParams.wheelObj.transform.forward, Vector3.up).normalized;
            Vector3 forceDir = Vector3.Cross(left, Vector3.up).normalized;
            Debug.DrawLine(wheelParams.spring.position + forceDir * wheelParams.restLength, wheelParams.spring.position, Color.yellow);

           Vector3 newSuspensionForce = (wheelParams.springForce + wheelParams.damperForce) * forceDir;

            if (wheelParams.lastLength > wheelParams.springLength)
            {
                //Debug.Log(wheelParams.lastLength + " :" + wheelParams.springLength);
                return newSuspensionForce;
            }
            else
            {
                return Vector3.zero;
            }
            #endregion
        }

        void OnDrawGizmos()
        {
            DrawSpringGizmos();
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

                    Vector3 rayDir = spring.transform.forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(wheelsList[i].spring.position + rayDir * (wheelsList[i].maxLength + wheelsList[i].wheelRadius * 2), wheelsList[i].spring.position);

                    Gizmos.DrawSphere(spring.position + rayDir * (wheelsList[i].springLength + wheelsList[i].wheelRadius * 2), .1f);
                }
            }
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
