// Description: VehicleRef: Access from anywhere to vehicleInfo.
// Use by CamDuringCountdownAssistant | CountdownAssistant | LapCOunterAndPosition | LapCounterBadge | MiniMapManager
// StepAssistantModes | VehicleAI | VehicleFlagManager | VehicleFlagOnCam | VehicleVisibleByTheCamList
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class VehiclesRef : MonoBehaviour
    {
        public static VehiclesRef   instance = null;
        public List<VehicleInfo>    listVehicles = new List<VehicleInfo>();
        public bool                 b_InitDone;
        private bool                b_InitInProgress;
        public VehicleGlobalData    vehicleGlobalData;
        public Vector3              InstantiateOffsetRotation = new Vector3(0, 180, 0);
        public float                giveCarsTimeToStabilizedSuspension = 1;

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        //-> Time Trial Step 0:  -> Instantiate Vehicle
        public bool bInstantiateVehicle(List<int> vehicles)
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(bInstantiateVehicleRoutine(vehicles));
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;


            return b_InitDone;
            #endregion
        }

        IEnumerator bInstantiateVehicleRoutine(List<int> vehicles)
        {
            #region
            b_InitDone = false;
            for(var i = 0; i < vehicles.Count; i++)
            {
                GameObject newVehicle = Instantiate(vehicleGlobalData.carParametersList[vehicles[i]].Prefab);
                newVehicle.name = "Player_" + i;

                //-> Path exist
                if (PathRef.instance.Track && PathRef.instance.Track.checkpoints.Count > 2)
                {
                    int posInGrid = vehicles.Count - 1 - i;

                    if (i == 0 && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 1)
                        posInGrid = 0;
                   
                    else if ((i == 0 || i == 1) && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                        posInGrid = i;

                    else  if (i != 0 && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 1)
                        posInGrid = vehicles.Count - i;

                    else if ((i != 0 || i != 1) && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                        posInGrid = vehicles.Count + 1 - i;


                    if (newVehicle.GetComponent<VehiclePrefabInit>())
                        newVehicle.GetComponent<VehiclePrefabInit>().startGridPosition = posInGrid;

                   yield return new WaitUntil(() => StartLine.instance.ReturnGridPosition(posInGrid, newVehicle.transform));
                }
                else
                {
                    newVehicle.transform.position = StartLine.instance.Grp_StartLineColliders.transform.position;
                    newVehicle.transform.rotation = StartLine.instance.Grp_StartLineColliders.transform.rotation;
                    newVehicle.transform.localEulerAngles += InstantiateOffsetRotation;
                }




                //-> Init vehicle for Test P1 + No Collision
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 5)
                {
                    //Debug.Log("New Car");
                    newVehicle.GetComponent<VehiclePrefabInit>().bInitVehicleInfo(2, i, vehicles[i]);
                }
                //-> Init vehicle for a race
                else
                    newVehicle.GetComponent<VehiclePrefabInit>().bInitVehicleInfo(0, i, vehicles[i]);

                // Disable input during countdown
                newVehicle.transform.GetChild(0).GetComponent<CarPlayerInputs>().isInputEnabled = false;

                yield return new WaitUntil(() => newVehicle.GetComponent<VehiclePrefabInit>().b_InitDone);

                listVehicles.Add(newVehicle.GetComponent<VehiclePrefabInit>().vehicleInfo);
            }

            // Give cars time to stabilized there suspension
            yield return new WaitForSeconds(giveCarsTimeToStabilizedSuspension);

           b_InitDone = true;

            //Debug.Log("Time Trial Step 0:  -> Instantiate Vehicle Done: " + vehicles.Count);

            yield return null;
            #endregion
        }
    }

}
