// Description: CarReflectionProbeManager. Optimize the vehicles ReflectionProbes. attached to CarReflectionProbeManager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarReflectionProbeManager : MonoBehaviour
    {
        public List<ReflectionProbe>    vehicleList = new List<ReflectionProbe>();
        List<float>                     intensityRefList = new List<float>();

        int                             firstVehicleCheck = 0;
        int                             howManyPlayer = 0;
        List<Vector3>                   playerPos = new List<Vector3>();

        public float                    detectionDistance = 30;

        bool                            isInitDone = false;

        void Start()
        {
            #region
            StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);
            
            // Create the list of vehicles
            vehicleList.Clear();
            for (var i=0;i< VehiclesRef.instance.listVehicles.Count; i++)
            {
                ReflectionProbe carReflectionProbe = VehiclesRef.instance.listVehicles[i].GetComponentInChildren<ReflectionProbe>(true);
                vehicleList.Add(carReflectionProbe);
                intensityRefList.Add(carReflectionProbe.intensity);
            }

            // One player
            if(InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 1)
            {
                howManyPlayer = 1;
                firstVehicleCheck = 1;
                playerPos.Add(Vector3.zero);
            }
            // Two Players
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
            {
                howManyPlayer = 2;
                firstVehicleCheck = 2;
                playerPos.Add(Vector3.zero);
                playerPos.Add(Vector3.zero);
            }

            isInitDone = true;
            yield return null;
            #endregion
        }

        void Update()
        {
            #region 
            if(isInitDone)
                UpdateReflectionProbeState(); 
            #endregion
        }

        void UpdateReflectionProbeState()
        {
            #region 
            for (var i = 0; i < howManyPlayer; i++)
                playerPos[i] = vehicleList[i].transform.position;

            for (var i = firstVehicleCheck; i < VehiclesRef.instance.listVehicles.Count; i++)
            {
                Vector3 aiPos = VehiclesRef.instance.listVehicles[i].transform.position;

                bool found = false;
                float distanceToTarget = 0;

                for (var j = 0; j < howManyPlayer; j++)
                {
                    distanceToTarget = Vector3.Distance(playerPos[j], aiPos);
                    if (distanceToTarget < detectionDistance)
                    {
                        found = true;
                        break;
                    }
                }

                if (found != vehicleList[i].gameObject.activeSelf)
                {
                    vehicleList[i].gameObject.SetActive(found);
                }
            } 
            #endregion
        }
    }
}
