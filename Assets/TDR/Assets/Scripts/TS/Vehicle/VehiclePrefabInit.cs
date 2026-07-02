// Description: VehiclePrefabInit. Attached on the root of the vehicle prefab.
// Use to initialize the vehicle when the vehicle is instantiated.
// The init is different depending the game mode.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class VehiclePrefabInit : MonoBehaviour
    {
        public bool             b_AutoInit;
        public bool             b_InitDone;
        private bool            b_InitInProgress;
        [HideInInspector]
        public bool             SeeInspector;
        [HideInInspector]
        public bool             moreOptions;
        [HideInInspector]
        public bool             helpBox = true;

        public CallMethods_Pc   callMethods;                              // Access script taht allow to call public function in this script.

        public int              currentSelectedList;
        public int              currentDisplayedList;

        public int              vehicleDataID;

        public bool             improveInitDuration = true;


        [System.Serializable]
        public class initVehiclePrefab
        {
            public string name;
            public List<EditorMethodsList_Pc.MethodsList> methodsList       // Create a list of Custom Methods that could be edit in the Inspector
                = new List<EditorMethodsList_Pc.MethodsList>();
        }

        public List<initVehiclePrefab>  initVehiclePrefabList = new List<initVehiclePrefab>();


        public int                      playerNumber = 0;

        public VehicleInfo              vehicleInfo;

        public int                      startGridPosition = 0;

        public float                    garageZoomMinScrollOverride = -1;
        public float                    garageZoomMaxScrollOverride = -1;
        public List<Vector3>            orbitalSpecialPosOverride = new List<Vector3>();

        public enum CarModeFive { ReadyForGame, ReadyForModeFive}

        public CarModeFive              carModeFive = CarModeFive.ReadyForGame;

        private void Start()
        {
            #region
            if (b_AutoInit)
                StartCoroutine(InitVehicleRoutine(currentSelectedList)); 
            #endregion
        }

        private void OnDestroy()
        {
            
        }

        public bool bInitVehicleInfo(int whichInit,int playerID = 0, int vehicleID = 0)
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(InitVehicleRoutine(whichInit, playerID, vehicleID));
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        IEnumerator InitVehicleRoutine(int whichMethodist = 0, int playerID = 0, int vehicleID = 0)
        {
            #region
            b_InitDone = false;
            playerNumber = playerID;
            vehicleDataID = vehicleID;

            yield return new WaitUntil(() => playerNumber == playerID);
            yield return new WaitUntil(() => vehicleDataID == vehicleID);
          
            for (var i = 0; i < initVehiclePrefabList[whichMethodist].methodsList.Count; i++)
            {
                //Debug.Log(i +  " | " + initVehiclePrefabList[whichMethodist].name);
                if (improveInitDuration)
                    callMethods.Call_One_Bool_Method(initVehiclePrefabList[whichMethodist].methodsList, i);
                else
                    yield return new WaitUntil(() => callMethods.Call_One_Bool_Method(initVehiclePrefabList[whichMethodist].methodsList, i) == true);
            }

            b_InitDone = true;

            yield return null;
            #endregion
        }
    }
}

