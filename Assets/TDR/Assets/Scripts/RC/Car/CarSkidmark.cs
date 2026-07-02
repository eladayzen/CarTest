// Description: CarSkidmark. Attached to the vehicle
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarSkidmark : MonoBehaviour
    {
        public bool                 isInitDone = false;

        public float                OffsetYPosition = .16f;

        private CarState.CarSkid    lastFrameSkid;
        private CarState.CarDrift   lastFrameDrift;
        public GameObject           grpParticle;

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

        public CarState             carState;
        public CarAudio             carAudio;
        private Vector3             carLastPos = Vector3.zero;

        public bool                 isSkidmarkAllowed = true;


        private List<GameObject> skidmarksList = new List<GameObject>();

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

            for (var i = 0; i < carController.wheelsList.Count; i++)
            {
                Grp_Wheels.Add(carController.wheelsList[i].spring.gameObject);
                listCurrentTrail.Add(null);
                lastSurfaceList.Add(RoadType.Default);
            }

            isInitDone = true;
            #endregion
        }

        private void Update()
        {
            LimitSkidMarkList();
        }
        void FixedUpdate()
        {
            #region
            if (isInitDone && isSkidmarkAllowed)
            {
                float dist = Vector3.Distance(carController.transform.position, carLastPos);
                Vector3 dir = carController.transform.position - carLastPos;
                // Select and Instantiate the Trail depending the surface
                for (var i = 0; i < Grp_Wheels.Count; i++)
                {
                    if (carState.carSkid == CarState.CarSkid.Skid ||
                        carState.carDrift == CarState.CarDrift.Drift)
                    {
                        GameObject trail = SelectTrailDependingSurface(carController.wheelsList[i].surface);
                        //GameObject trail = SelectTrailDependingSurface(carController.averageSurface);
                        
                        if (trail && listCurrentTrail[i] == null)
                        {
                            //listCurrentTrail[i] = Instantiate(trail, Grp_Wheels[i].transform);
                            listCurrentTrail[i] = Instantiate(trail,
                          carController.wheelsList[i].hitPoint + new Vector3(0, OffsetYPosition, 0) + dir * dist,
                          Quaternion.identity);


                            skidmarksList.Add(listCurrentTrail[i]);

                        }

                        if(i==0)grpParticle.gameObject.SetActive(true);

                    }
                }
         
                // Unparent the trail if needed
                for (var i = 0; i < Grp_Wheels.Count; i++)
                {
                    if (carState.carSkid == CarState.CarSkid.NoSkid && lastFrameSkid == CarState.CarSkid.Skid ||
                        carController.wheelsList[i].surface != lastSurfaceList[i] ||
                        carState.carDrift == CarState.CarDrift.NoDrift && lastFrameDrift == CarState.CarDrift.Drift
                        /*carController.averageSurface != lastSurfaceList[i]*/)
                    {
                        if (listCurrentTrail[i])
                        {
                            listCurrentTrail[i].transform.SetParent(null);
                            listCurrentTrail[i] = null;
                        }
                        lastSurfaceList[i] = carController.wheelsList[i].surface;
                        //lastSurfaceList[i] = carController.averageSurface;

                        if (i == 0) grpParticle.gameObject.SetActive(false);
                    }
                }


                // Move the trail position to the wheel position using the next wheel position
            

                for (var i = 0; i < Grp_Wheels.Count; i++)
                {
                    if (listCurrentTrail[i])
                        listCurrentTrail[i].transform.position = carController.wheelsList[i].hitPoint + new Vector3(0, OffsetYPosition, 0) + dir * dist;
                }

                carLastPos = carController.transform.position;

                lastFrameSkid = carState.carSkid;
                lastFrameDrift = carState.carDrift;
            }
            #endregion
        }

        GameObject SelectTrailDependingSurface(RoadType currentSurface)
        {
            #region
            for (var i = 0;i< trailList.Count; i++)
            {
                if (currentSurface == trailList[i].surface)
                {
                    return trailList[i].objTrail;
                }      
            }

            return null;
            #endregion
        }
        public bool StopSkidMark()
        {
            #region 
            // Debug.Log("Stop Skidmarks");

            isSkidmarkAllowed = false;

            for (var i = 0; i < Grp_Wheels.Count; i++)
            {
                if (listCurrentTrail[i])
                {
                    listCurrentTrail[i].transform.SetParent(null);
                    listCurrentTrail[i] = null;
                }
                lastSurfaceList[i] = carController.wheelsList[i].surface;

                if (i == 0) grpParticle.gameObject.SetActive(false);
            }

            return true; 
            #endregion
        }

        void LimitSkidMarkList()
        {
            #region 
            if (skidmarksList.Count > 8)
            {
                int howManySKidToDelete = skidmarksList.Count - 8;
                for (var i = 0; i < howManySKidToDelete; i++)
                {
                    if (skidmarksList[0] != null)
                        Destroy(skidmarksList[i]);

                    skidmarksList.RemoveAt(0);
                }
            } 
            #endregion

        }
    }
}
