// Description: CarSelectionManager: Select a vehicle in the car selection menu (Main Menu scene)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TS.Generics
{
    public class CarSelectionManager : MonoBehaviour
    {
        public static CarSelectionManager   instance;
        public int                          TSInputKeyLeft = 3;
        public int                          TSInputKeyRight = 4;

        public CarSelectionAssistant        carSelectionAssistantP1;
        public CarSelectionAssistant        carSelectionAssistantP2;

        public GameObject                   grpP1;
        public GameObject                   grpP2;

        //public GameObject                   grpCamP1;
        //public GameObject                   grpCamP2;

        bool                                bInitDone = false;

        public List<int>                    vehicleAvailableList = new List<int>();

        int                                 lastSelectedVehicleCategory = -1;

        void Awake()
        {
            #region
            //Check if instance already exists
            if (instance == null)
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)
                Destroy(gameObject);
            #endregion
        }

        public void EnterCarSelection()
        {
            #region 
            StartCoroutine(EnterCarSelectionRoutine()); 
            #endregion
        }

        public void ExitCarSelection()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            //grpCamP1.SetActive(false);
            //if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
            //    grpCamP2.SetActive(false);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void PlayerOneLeft()
        {
            #region 
            if (grpP1.activeInHierarchy)
            {
                carSelectionAssistantP1.DisplayNewVehicle(-1);
            } 
            #endregion
        }

        public void PlayerOneRight()
        {
            #region 
            if (grpP1.activeInHierarchy)
            {
                carSelectionAssistantP1.DisplayNewVehicle(1);
            } 
            #endregion
        }

        public void PlayerTwoLeft()
        {
            #region 
            if (grpP2.activeInHierarchy)
            {
                carSelectionAssistantP2.DisplayNewVehicle(-1);
            } 
            #endregion
        }

        public void PlayerTwoRight()
        {
            #region 
            if (grpP2.activeInHierarchy)
            {
                carSelectionAssistantP2.DisplayNewVehicle(1);
            } 
            #endregion
        }

        public void StateGrpCamP1(bool value,bool bDisplayCar = true)
        {
            #region 
           // grpCamP1.SetActive(value);
            if (bDisplayCar) carSelectionAssistantP1.DisplayNewVehicle(0); 
            #endregion
        }
        public void StateGrpCamP2(bool value, bool bDisplayCar = true)
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
            {
                //grpCamP2.SetActive(value);
                if (bDisplayCar) carSelectionAssistantP2.DisplayNewVehicle(0);
            } 
            #endregion
        }

        public void OnDestroy()
        {
            #region 
            if (bInitDone)
            {
                //-> Left (TSInputKeyLeft = 3) | Right (TSInputKeyRight = 4)
                InfoInputs.instance.ListOfInputsForEachPlayer[1].listOfButtons[TSInputKeyLeft].OnGetKeyDownReceived -= PlayerTwoLeft;
                InfoInputs.instance.ListOfInputsForEachPlayer[1].listOfButtons[TSInputKeyRight].OnGetKeyDownReceived -= PlayerTwoRight;
            }  
            #endregion
        }

        public IEnumerator EnterCarSelectionRoutine()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            if (!bInitDone)
            {
                //-> Left (TSInputKeyLeft = 3) | Right (TSInputKeyRight = 4)
                InfoInputs.instance.ListOfInputsForEachPlayer[1].listOfButtons[TSInputKeyLeft].OnGetKeyDownReceived += PlayerTwoLeft;
                InfoInputs.instance.ListOfInputsForEachPlayer[1].listOfButtons[TSInputKeyRight].OnGetKeyDownReceived += PlayerTwoRight;
                bInitDone = true;
            }

            // Create the list of vehicles available
            vehicleAvailableList.Clear();
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            int vehicleTotal = 0; 
            int counter = 0;


            //-> Use list Order
            vehicleTotal = InfoVehicle.instance.vehicleParametersInGameList.Count;
            if (!vehicleData.OrderUsingCustomList)
            {
                for (var i = 0; i < vehicleTotal; i++)
                {
                    int category = InfoVehicle.instance.vehicleParametersInGameList[i].vehicleCategory;
                    if (InfoVehicle.instance.vehicleParametersInGameList[i].isUnlocked &&
                            ReturnCurrentVehicleCategory() == category)
                    {
                        vehicleAvailableList.Add(i);
                        counter++;
                    }
                }
            }
            //-> Use Custom Order
            vehicleTotal = vehicleData.customList.Count;
            if (vehicleData.OrderUsingCustomList)
            {
                for (var i = 0; i < vehicleTotal; i++)
                {
                    int vehicleId = vehicleData.customList[i];
                    int category = InfoVehicle.instance.vehicleParametersInGameList[vehicleId].vehicleCategory;
                    if (InfoVehicle.instance.vehicleParametersInGameList[vehicleId].isUnlocked &&
                            ReturnCurrentVehicleCategory() == category)
                    {
                        vehicleAvailableList.Add(vehicleId);
                        counter++;
                    }
                }
            }

           // grpCamP1.SetActive(true);

            if (lastSelectedVehicleCategory == -1 || lastSelectedVehicleCategory != ReturnCurrentVehicleCategory())
                carSelectionAssistantP1.bForceReloadingVehicleInVehicleChoose = true;

            carSelectionAssistantP1.DisplayNewVehicle(0);

            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
            {
                //grpCamP2.SetActive(true);

                if (lastSelectedVehicleCategory == -1 || lastSelectedVehicleCategory != ReturnCurrentVehicleCategory())
                    carSelectionAssistantP2.bForceReloadingVehicleInVehicleChoose = true;

                carSelectionAssistantP2.DisplayNewVehicle(0);
            }

            lastSelectedVehicleCategory = ReturnCurrentVehicleCategory();
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null; 
            #endregion
        }

        int ReturnCurrentVehicleCategory()
        {
            #region 
            int currentGameMode = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode;
            // Arcade
            if (currentGameMode == 0)
            {
                return GameModeGlobal.instance.CurrentVehicleCategory;
            }
            // Time Trial
            if (currentGameMode == 1)
            {
                return GameModeGlobal.instance.CurrentVehicleCategory;
            }
            // Championship
            if (currentGameMode == 2)
            {
                int currentChampionship = GameModeChampionship.instance.currentSelection;
                ChampionshipModeData._Championship championshipData = ReturnChampionshipGlobalParams(currentChampionship);
                GameModeGlobal.instance.CurrentVehicleCategory = championshipData.whichVehicleCatogoryAllowed;
                return championshipData.whichVehicleCatogoryAllowed;
            }
            return 0; 
            #endregion
        }

        ChampionshipModeData._Championship ReturnChampionshipGlobalParams(int diff)
        {
            #region 
            if (!DataRef.instance.championshipModeData.bDisplayChampionshipUsingListOrder)
            {
                int specialOrderID = DataRef.instance.championshipModeData.customChampionshipList[diff];
                return DataRef.instance.championshipModeData.listOfChampionship[specialOrderID];
            }
            else
            {
                return DataRef.instance.championshipModeData.listOfChampionship[diff];
            } 
            #endregion
        }

        TracksData.trackParams ReturnTrackParams(int diff)
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
                {
                    int specialOrderID = DataRef.instance.arcadeModeData.customTrackList[diff];
                    return DataRef.instance.tracksData.listTrackParams[specialOrderID];
                }
                else
                {
                    return DataRef.instance.tracksData.listTrackParams[diff];
                }
            }
            else
            {
                if (!DataRef.instance.timeTrialModeData.bDisplayTrackUsingTrackListOrder)
                {
                    int specialOrderID = DataRef.instance.timeTrialModeData.customTrackList[diff];
                    return DataRef.instance.tracksData.listTrackParams[specialOrderID];
                }
                else
                {
                    return DataRef.instance.tracksData.listTrackParams[diff];
                }
            } 
            #endregion
        }
    }
}