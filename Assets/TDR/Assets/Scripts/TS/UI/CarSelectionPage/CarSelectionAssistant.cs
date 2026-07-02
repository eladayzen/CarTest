// Description: CarSelectionAssistant: called when a vehicle is selected in the car selection menu (Main Menu scene)
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CarSelectionAssistant : MonoBehaviour
    {
        public bool                 b_IsInitGarageInProcess = false;

        public bool                 bActionAvailable;

        public UnityEvent           newVechicleEvent;                   // List of events use when a new vehicle is loaded
        public UnityEvent           instantiateNewVehicleEvent;         // Manage the vehicle instantiation

        public CurrentText          HowManyVehicleAvailable;
        public CurrentText          VehicleName;

        //-> Use to set up the transition (duration and curve)
        public float                duration = 1;                       // the transition duration
        public AnimationCurve       animSpeedCurve;

        public int                  selectedPlayer = 0;
        //[HideInInspector]
        //public GarageTagPivot       garageTagPivot;
        //public Transform            vehicleInstantiatePosition;

        public int                  direction;

        public int                  posInList;
        [HideInInspector]
        public bool                 bForceReloadingVehicleInVehicleChoose;
        //public bool                 EnableIsKiniematicWhenNewVehicleSpanwed = true;

        //public Transform            selectionCamP1;

        public Image                thumbnailCar;


        //-> Call by Page Init (garage page) | Call when Button_PreviousVehicle or Button_NextVehicle are pressed
        public void DisplayNewVehicle(int Direction)
        {
            #region 
            if (bActionAvailable) StartCoroutine(DisplayNextVehicleRoutine(Direction)); 
            #endregion
        }

        //-> direction 0: Init menu | 1: next vehicle | -1: Previous vehicle
        IEnumerator DisplayNextVehicleRoutine(int _direction)
        {
            #region 
            direction = _direction;
            bActionAvailable = false;
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            List<int> vehicleAvailableList = CarSelectionManager.instance.vehicleAvailableList;

            //-> Find the vehicle to display. Check if bShow in vehicle parameters is set to True.
            bool bNewEntry = false;
            while (bNewEntry != true)
            {
                posInList += direction + vehicleAvailableList.Count + vehicleAvailableList.Count;
                posInList %= vehicleAvailableList.Count;

                infoVehicle.listSelectedVehicles[selectedPlayer - 1] = vehicleAvailableList[posInList];


                if (InfoVehicle.instance.vehicleParametersInGameList[infoVehicle.listSelectedVehicles[selectedPlayer - 1]].bShow &&
                    InfoVehicle.instance.vehicleParametersInGameList[infoVehicle.listSelectedVehicles[selectedPlayer - 1]].isUnlocked)
                    bNewEntry = true;

                yield return null;
            }

         
            instantiateNewVehicleEvent?.Invoke();

            yield return null; 
            #endregion
        }

        //-> Display vehicle info 
        void UpdateVehicleInfo(int currentVehicle, VehicleGlobalData vehicleData)
        {
            #region 
            newVechicleEvent?.Invoke();
            //-> Display Vehicle name
            VehicleName.DisplayTextComponent(VehicleName.gameObject, InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].name, false);

            string newTxt = (posInList + 1) + "/" + CarSelectionManager.instance.vehicleAvailableList.Count;

            HowManyVehicleAvailable.DisplayTextComponent(VehicleName.gameObject, newTxt, false);

            // Display Vehicle Sprite
            if (InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].img)
                thumbnailCar.sprite = InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].img;

            Debug.Log("Update Info: ");

            #endregion
        }

        public void InstantiateNewVehicle()
        {
            #region
            StartCoroutine(InstantiateNewVehicleRoutine()); 
            #endregion
        }

        IEnumerator InstantiateNewVehicleRoutine()
        {
            #region 
            CarSelectionAssistant[] objsSel = FindObjectsByType<CarSelectionAssistant>(FindObjectsSortMode.None);
            CarSelectionAssistant gm = null;
            for (var i = 0; i < objsSel.Length; i++)
            {
                if (objsSel[i].selectedPlayer == selectedPlayer)
                {
                    gm = objsSel[i];
                    break;
                }
            }

            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Use list Order
            int currentVehicle = infoVehicle.listSelectedVehicles[selectedPlayer - 1];
            //-> Use Custom Order
           // if (vehicleData.OrderUsingCustomList)
             //   currentVehicle = vehicleData.customList[infoVehicle.listSelectedVehicles[selectedPlayer - 1]];

            Debug.Log("currentVehicle: " + currentVehicle);

            //-> Display the new vehicle info in UI
            UpdateVehicleInfo(currentVehicle, vehicleData);

            //-> IMPORTANT: Next 2 lines closed the process. Always add those 2 lines.
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            gm.bActionAvailable = true;
            yield return null; 
            #endregion
        }

    }
}
