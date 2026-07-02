// Description: WheelSurfaceParticle. Attached to the vehicle
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class WheelSurfaceParticle : MonoBehaviour
    {
        public bool                 isInitDone = false;

        public float                offsetYPosition = .16f;

        //public GameObject           grpParticle;

        [System.Serializable]
        public class TrailParams
        {
            public GameObject   objTrail;
            public RoadType     surface = RoadType.Default;
        }
        public List<TrailParams>    trailList = new List<TrailParams>();
        private List<GameObject>    Grp_Wheels = new List<GameObject>();
        private List<GameObject>    listCurrentTrail = new List<GameObject>();
        private List<RoadType>      lastSurfaceList = new List<RoadType>();
        private CarController       carController;

        [HideInInspector]
        public CarState             carState;

        Vector3                     carLastPos = Vector3.zero;

        public bool                 isParticlesAllowed = true;

        bool                        isRaceInProgress = false;

        [HideInInspector]
        public List<DustParticlesManager> particlesManagerList = new List<DustParticlesManager>();

        bool                        isChangeAllowed = true;

        List<bool>                  forceRefreshList = new List<bool>();
        private CarState.CarTouchedSurface lastFrameSurfaceGrounded;

        List<bool>                  allowedWheelsList = new List<bool>();

        float                       distanceBetweenFrontWheels = 0;
        float                       distanceBetweenRearWheels = 0;

        public enum UsedWheel { Centered,Both, Disabled};

        public UsedWheel            frontWheel = UsedWheel.Centered;
        public UsedWheel            rearWheel = UsedWheel.Centered;


        void Start()
        {
            #region 
            Init();
            #endregion
        }

        public void Init()
        {
            #region
            carState = GetComponent<CarState>();
            carController = GetComponent<CarController>();
            carLastPos = carController.transform.position;

            allowedWheelsList.Clear();

            for (var i = 0; i < carController.wheelsList.Count; i++)
            {
                Grp_Wheels.Add(carController.wheelsList[i].spring.gameObject);
                listCurrentTrail.Add(null);
                lastSurfaceList.Add(RoadType.Default);

                forceRefreshList.Add(false);

                //if (allowedWheelsList.Count <= i)
                //    allowedWheelsList.Add(true);

                allowedWheelsList.Add(false);
            }

            if (carController.wheelsList[0].spring && carController.wheelsList[1].spring)
                distanceBetweenFrontWheels = Vector3.Distance(carController.wheelsList[0].spring.position , carController.wheelsList[1].spring.position);

            if (carController.wheelsList[2].spring && carController.wheelsList[3].spring)
                distanceBetweenRearWheels = Vector3.Distance(carController.wheelsList[2].spring.position, carController.wheelsList[3].spring.position);

            if (Countdown.instance)
            {
                Countdown.instance.countdownStartAction += WaitUntilTheEndOfCountdown;
            }

            isInitDone = true;
            #endregion
        }

        private void OnDestroy()
        {
            #region 
            if (Countdown.instance)
            {
                Countdown.instance.countdownStartAction -= WaitUntilTheEndOfCountdown;
            } 
            #endregion
        }
       
        void FixedUpdate()
        {
            #region
            if (isInitDone && isParticlesAllowed && isRaceInProgress)
            {
                float dist = Vector3.Distance(carController.transform.position, carLastPos);
                Vector3 dir = carController.transform.position - carLastPos;


                for (var i = 0; i < Grp_Wheels.Count; i++)
                {
                    if ((listCurrentTrail[i] != null && 
                        isChangeAllowed &&
                    carController.wheelsList[i].surface != lastSurfaceList[i])
                    ||
                    (listCurrentTrail[i] != null && 
                    carState.carTouchedSurface == CarState.CarTouchedSurface.IsGround &&
                    lastFrameSurfaceGrounded == CarState.CarTouchedSurface.InAir)
                    )
                    {
                        lastSurfaceList[i] = carController.wheelsList[i].surface;
                        forceRefreshList[i] = true;
                    }
                }

                float currentVelocity = Mathf.Clamp01(carController.rb.linearVelocity.magnitude / 60);

                // Change rateOverDistance
                for (var i = 0; i < Grp_Wheels.Count; i++)
                {
                    if (listCurrentTrail[i] != null && forceRefreshList[i])
                    {
                        //Debug.Log("Change Particles");
                        for (var k = 0; k < particlesManagerList[i].particleList.Count; k++)
                        {
                            if (carController.wheelsList[i].surface == particlesManagerList[i].particleList[k].surface)
                            {
                                ParticleSystem.EmissionModule psEmission = particlesManagerList[i].particleList[k].particleSystem.emission;
                                psEmission.rateOverDistance = particlesManagerList[i].particleList[k].rateOverDistance;
                            }
                            else
                            {
                                ParticleSystem.EmissionModule psEmission = particlesManagerList[i].particleList[k].particleSystem.emission;
                                psEmission.rateOverDistance = 0;
                            }
                        }
                        forceRefreshList[i] = false;
                    }
                    else if (listCurrentTrail[i] != null && carState.carTouchedSurface == CarState.CarTouchedSurface.InAir && lastFrameSurfaceGrounded == CarState.CarTouchedSurface.IsGround)
                    {
                        for (var j = 0; j < particlesManagerList[i].particleList.Count; j++)
                        {
                            ParticleSystem.EmissionModule psEmission = particlesManagerList[i].particleList[j].particleSystem.emission;
                            psEmission.rateOverDistance = 0;
                        }
                    }

                    if (listCurrentTrail[i] != null)
                    {
                        if ((frontWheel == UsedWheel.Centered || frontWheel == UsedWheel.Both) && (i == 0 || i == 1))
                            listCurrentTrail[i].transform.position = ParticleFrontPosition(i, dir, dist);

                        if ((rearWheel == UsedWheel.Centered || rearWheel == UsedWheel.Both) && (i == 2 || i == 3))
                            listCurrentTrail[i].transform.position = ParticleRearPosition(i,dir,dist);
                       }   
                    }

                carLastPos = carController.transform.position;

                lastFrameSurfaceGrounded = carState.carTouchedSurface;
            }
            #endregion
        }

        Vector3 ParticleFrontPosition(int i, Vector3 dir, float dist)
        {
            #region 
            if (frontWheel == UsedWheel.Centered && i == 0)
            {
                return
                carController.wheelsList[i].spring.position
                + Vector3.up * carController.wheelsList[i].wheelRadius
                - carController.wheelsList[i].spring.right * distanceBetweenFrontWheels * .5f
                + new Vector3(0, offsetYPosition, 0) + dir * dist;
            }
            else if (frontWheel == UsedWheel.Both)
            {
                return
                carController.wheelsList[i].spring.position
                + Vector3.up * carController.wheelsList[i].wheelRadius
                + new Vector3(0, offsetYPosition, 0) + dir * dist;
            }
            else
            {
                return Vector3.zero;
            }
            #endregion
        }

        Vector3 ParticleRearPosition(int i, Vector3 dir,float dist)
        {
            #region 
            if (rearWheel == UsedWheel.Centered && i == 2)
            {
                return
                carController.wheelsList[i].spring.position
                + Vector3.up * carController.wheelsList[i].wheelRadius
                + carController.wheelsList[i].spring.right * distanceBetweenRearWheels * .5f
                + new Vector3(0, offsetYPosition, 0) + dir * dist;
            }
            else if (rearWheel == UsedWheel.Both)
            {
                return
                carController.wheelsList[i].spring.position
                + Vector3.up * carController.wheelsList[i].wheelRadius
                + new Vector3(0, offsetYPosition, 0) + dir * dist;
            }
            else
            {
                return Vector3.zero;
            }  
            #endregion
        }

        public bool StopSkidMark()
        {
            #region 
            // Debug.Log("Stop Skidmarks");

            isParticlesAllowed = false;

            for (var i = 0; i < Grp_Wheels.Count; i++)
            {
                if (listCurrentTrail[i])
                {
                    listCurrentTrail[i].transform.SetParent(null);
                    listCurrentTrail[i] = null;
                }
                lastSurfaceList[i] = carController.wheelsList[i].surface;

               // if (i == 0) grpParticle.gameObject.SetActive(false);
            }

            return true;
            #endregion
        }

        void WaitUntilTheEndOfCountdown()
        {
            #region 
            StartCoroutine(WaitUntilTheEndOfCountdownRoutine()); 
            #endregion
        }

        IEnumerator WaitUntilTheEndOfCountdownRoutine()
        {
            #region 
            yield return new WaitUntil(() => Countdown.instance.b_IsCountdownEnded);

            if (frontWheel == UsedWheel.Both)
            {
                allowedWheelsList[0] = true;
                allowedWheelsList[1] = true;
            }
            else if (frontWheel == UsedWheel.Centered)
            {
                allowedWheelsList[0] = true;
                allowedWheelsList[1] = false;
            }
            else
            {
                allowedWheelsList[0] = false;
                allowedWheelsList[1] = false;
            }

            if (rearWheel == UsedWheel.Both)
            {
                allowedWheelsList[2] = true;
                allowedWheelsList[3] = true;
            }
            else if (rearWheel == UsedWheel.Centered)
            {
                allowedWheelsList[2] = true;
                allowedWheelsList[3] = false;
            }
            else
            {
                allowedWheelsList[2] = false;
                allowedWheelsList[3] = false;
            }

            for (var i = 0; i < Grp_Wheels.Count; i++)
            {

                if (allowedWheelsList[i])
                {
                    float dist = Vector3.Distance(carController.transform.position, carLastPos);
                    Vector3 dir = carController.transform.position - carLastPos;
                    GameObject trail = trailList[0].objTrail;
                    listCurrentTrail[i] = Instantiate(trail,
                                  carController.wheelsList[i].hitPoint + new Vector3(0, offsetYPosition, 0) + dir * dist,
                                  Quaternion.identity);

                    particlesManagerList.Add(listCurrentTrail[i].GetComponent<DustParticlesManager>());
                }
                else
                    particlesManagerList.Add(null);
            }

            // Disabled Unused Particle 
            for (var i = 0; i < particlesManagerList.Count; i++)
            {
                if (particlesManagerList[i])
                {
                    for (var k = 0; k < particlesManagerList[i].particleList.Count; k++)
                    {
                        if (particlesManagerList[i].particleList[k].rateOverDistance == 0)
                        {
                            particlesManagerList[i].particleList[k].particleSystem.gameObject.SetActive(false);
                        }
                    }
                }
            }

            for (var i = 0; i < Grp_Wheels.Count; i++)
                forceRefreshList[i] = true;

            for (var i = 0; i < particlesManagerList.Count; i++)
            {
                if (particlesManagerList[i])
                {
                    for (var k = 0; k < particlesManagerList[i].particleList.Count; k++)
                    {
                        ParticleSystem.EmissionModule psEmission = particlesManagerList[i].particleList[k].particleSystem.emission;
                        psEmission.rateOverDistance = 0;
                    }
                }
            }

            isRaceInProgress = true;

            yield return null;
            #endregion
        }
    }
}
