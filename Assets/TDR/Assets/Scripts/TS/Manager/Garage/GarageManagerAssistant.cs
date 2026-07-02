//Description: GarageManagerAssistant: Attached to buttons that open/close the garage
using UnityEngine;

namespace TS.Generics
{
    public class GarageManagerAssistant : MonoBehaviour
    {
        //-> Call when button back is pressed and page Grp_Garage is closed (call from pageOut.cs)
        public void ExitGarage()
        {
            #region
            if (Leaderboard.instance)
            {
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
                GameObject newSelectedButton = CanvasMainMenuManager.instance.ComeBackFromPageList[1].selectedButtonWhenBackToPage;
               // Debug.Log("Exit Garage");

                StartCoroutine(CanvasMainMenuManager.instance.listMenu[GarageManager.instance.garagePageID].transform.parent.GetComponent<PageOut>().DisableCurrentPageAndSelectManualyANewPage(
                    CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage,
                    false,
                    0,
                    newSelectedButton));


                //-> Go back to car categories. 
                if (CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage == 20)
                {
                    TSAssistantSlideShowCategory obj = GameObject.FindFirstObjectByType<TSAssistantSlideShowCategory>();
                    if (obj)
                    {
                        obj.GetComponent<SlideShow>().Init();
                    }
                }

                //-> Go back to car selection. 
                if (CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage == 15)
                {
                    CarSelectionManager.instance.StartCoroutine(CarSelectionManager.instance.EnterCarSelectionRoutine());
                }

                //-> Go back to Championship Mode. 
                if (CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage == 16)
                {
                    TSAssistantSlideShowChampionship obj = GameObject.FindFirstObjectByType<TSAssistantSlideShowChampionship>();
                    if (obj)
                    {
                        obj.GetComponent<SlideShow>().Init();
                    }
                }

                //-> Go back to Arcade/ Time Trial Mode. 
                if (CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage == 17)
                {
                    TSAssistantSlideShowArcadeTT obj = GameObject.FindFirstObjectByType<TSAssistantSlideShowArcadeTT>();
                    if (obj)
                    {
                        obj.GetComponent<SlideShow>().Init();
                    }
                }


                if (GarageManager.instance.objScrollViewCategory)
                    GarageManager.instance.objScrollViewCategory.SetActive(true);


                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            }
            #endregion
        }

        // Use by the buttons Button_Garage
        public void OpenGarage()
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed &&
                GarageManager.instance.bActionAvailable)  // Check if the player can press a button
            {
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
                //Debug.Log("Open Garage");
                CanvasMainMenuManager.instance.ComeBackFromPageList[1].comeBackToPage = CanvasMainMenuManager.instance.currentSelectedPage;
                CanvasMainMenuManager.instance.ComeBackFromPageList[1].selectedButtonWhenBackToPage = TS_EventSystem.instance.eventSystem.currentSelectedGameObject;

                StartCoroutine(GarageManager.instance.OpenGarageRoutine());
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            }
            #endregion
        }

        // Use by the buttons Button_Garage
        public void OpenGarageWithSpecificCategory()
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed &&
                GarageManager.instance.bActionAvailable)  // Check if the player can press a button
            {
                int category = GameModeGlobal.instance.CurrentVehicleCategory;

                for (var i = 0; i < InfoVehicle.instance.vehicleParametersInGameList.Count; i++)
                {
                    if (InfoVehicle.instance.vehicleParametersInGameList[i].vehicleCategory == category)
                    {
                        if (GarageManager.instance.objScrollViewCategory)
                            GarageManager.instance.objScrollViewCategory.SetActive(false);

                        InfoVehicle.instance.currentVehicleDisplayedInTheGarage = i;
                        GarageManager.instance.CurrentCategory = category;
                        GarageManager.instance.direction = 1;

                        break;
                    }
                }

                OpenGarage();
            }
            #endregion
        }

        public void OpenGarageWithSpecificCategoryTrackPage()
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed &&
                GarageManager.instance.bActionAvailable)  // Check if the player can press a button
            {
                /*int category = GameModeGlobal.instance.CurrentVehicleCategory;

                for (var i = 0; i < InfoVehicle.instance.vehicleParametersInGameList.Count; i++)
                {
                    if (InfoVehicle.instance.vehicleParametersInGameList[i].vehicleCategory == category)
                    {
                        if (GarageManager.instance.objScrollViewCategory)
                            GarageManager.instance.objScrollViewCategory.SetActive(false);

                        InfoVehicle.instance.currentVehicleDisplayedInTheGarage = i;
                        GarageManager.instance.CurrentCategory = category;
                        GarageManager.instance.direction = 1;

                        break;
                    }
                }*/

                OpenGarage();
            }
            #endregion
        }
    }
}
