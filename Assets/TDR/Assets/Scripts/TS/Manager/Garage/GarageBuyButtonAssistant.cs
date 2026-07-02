// Description: GarageBuyButtonAssistant. Attached to Button_Simple_Buy in Page Grp_Garage  (Main Menu)
using UnityEngine;
using System.Collections;

namespace TS.Generics
{
    public class GarageBuyButtonAssistant : MonoBehaviour
    {
        public bool CheckIfConditions()
        {
            #region
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Use list Order
            int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
            //-> Use Custom Order
            if (vehicleData.OrderUsingCustomList)
                currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];

           

            // Vehicle is not already unlocked
            if (InfoCoins.instance.currentPlayerCoins >= InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].cost &&
                !InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked
                )
            {
                return true;
            }

            // The vehicle is unlocked and the vehicle custimization is available
            if (GarageManager.instance.customizationAvailable &&
               InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked
               )
            {
                return true;
            }

            return false;
            #endregion
        }

        public void BuyVehicle()
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
            {
                //Debug.Log("EEE");
                GarageManager gm = GarageManager.instance;
                if (!gm.bActionAvailable || gm.displayVehiclecleThumbnailInProgress)
                {

                }
                else
                {

                    VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
                    InfoVehicle infoVehicle = InfoVehicle.instance;

                    //-> Use list Order
                    int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
                    //-> Use Custom Order
                    if (vehicleData.OrderUsingCustomList)
                        currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];

                    // Vehicle is not already unlocked
                    if (!InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked)
                    {
                        GarageManager.instance.UnlockVehicle();
                        // Force to reload the vehicle in the vehicle selection page to prevent issu if a vehicle has been earned by the player
                        CarSelectionManager.instance.carSelectionAssistantP1.bForceReloadingVehicleInVehicleChoose = true;
                    }
                }
            }

            #endregion
        }

        public void WrongResult()
        {
            #region
            Debug.Log("JJJ");
            VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
            InfoVehicle infoVehicle = InfoVehicle.instance;

            //-> Use list Order
            int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
            //-> Use Custom Order
            if (vehicleData.OrderUsingCustomList)
                currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];


            if (InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked)
            {
                //-> Feedback Already bought
                GarageManager.instance.BuyButtonFeedback(1);
            }
            else
            {
                //-> Feedback Not Enough Credit
                GarageManager.instance.BuyButtonFeedback(0);
            }
            #endregion
        }

        public void OpenVehicleCustomizationPage(int PageNumber)
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                 && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
            {
                //Debug.Log("HHH");
                GarageManager gm = GarageManager.instance;
                if (!gm.bActionAvailable || gm.displayVehiclecleThumbnailInProgress)
                {

                }
                else
                {
                    VehicleGlobalData vehicleData = DataRef.instance.vehicleGlobalData;
                    InfoVehicle infoVehicle = InfoVehicle.instance;

                    //-> Use list Order
                    int currentVehicle = infoVehicle.currentVehicleDisplayedInTheGarage;
                    //-> Use Custom Order
                    if (vehicleData.OrderUsingCustomList)
                        currentVehicle = vehicleData.customList[infoVehicle.currentVehicleDisplayedInTheGarage];

                    // Vehicle is already unlocked
                    if (InfoVehicle.instance.vehicleParametersInGameList[currentVehicle].isUnlocked)
                    {
                        if (InfoPlayerTS.instance.returnCheckState(0)
                            && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
                        {
                            PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                            currentMenu.DisplayNewPage(PageNumber);
                        }
                    }
                }
            }
           
           
            #endregion
        }

    }

}
