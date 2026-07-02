// Description: CarSelectionPageAssistant: attached to CarSelectionAssitantP1 an CarSelectionAssitantP2
// in Main Menu scene. Page Grp_Game_CarSelection
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CarSelectionPageAssistant : MonoBehaviour
    {
        public List<int>            pageIDToDisplayDependingGameMode = new List<int>();

        public List<GameObject>     objPlayerParamsList;
        public RectTransform        grpPlayerVehicle;
        public float                pivotV = .6f;

        public List<GameObject>     btnList = new List<GameObject>();

        public List<Button>         btnNavigationList = new List<Button>();

        [System.Serializable]
        public class ObjState
        {
            public GameObject Obj;
            public List<bool> listStateDependingPlayerNumber = new List<bool>(3) { true, true, true };
        }

        public List<ObjState>       listObjState = new List<ObjState>();

        public bool useOnlyOneVehicleCategory = false;

        //-> Use on Button_Championship (page Grp_Game_ChooseMode).
        public void OpenPageDependingGameMode()
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                PageIn currentMenu;
                switch (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode)
                {
                    case 0:
                        currentMenu = CanvasMainMenuManager.instance.listMenu[pageIDToDisplayDependingGameMode[0]].transform.parent.GetComponent<PageIn>();
                        currentMenu.DisplayNewPage(pageIDToDisplayDependingGameMode[0]);
                        break;
                    case 1:
                        currentMenu = CanvasMainMenuManager.instance.listMenu[pageIDToDisplayDependingGameMode[1]].transform.parent.GetComponent<PageIn>();
                        currentMenu.DisplayNewPage(pageIDToDisplayDependingGameMode[1]);
                        break;
                    case 2:
                        currentMenu = CanvasMainMenuManager.instance.listMenu[pageIDToDisplayDependingGameMode[2]].transform.parent.GetComponent<PageIn>();
                        currentMenu.DisplayNewPage(pageIDToDisplayDependingGameMode[2]);
                        break;
                }
            } 
            #endregion
        }

        public bool Init()
        {
            #region 
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            if (howManyPlayer == 1)
            {
                for (var i = 0; i < objPlayerParamsList.Count; i++)
                {
                    if (i == 0)
                        objPlayerParamsList[i].SetActive(true);
                    else
                        objPlayerParamsList[i].SetActive(false);

                    if (grpPlayerVehicle) grpPlayerVehicle.pivot = new Vector2(.5f, .5f);
                }


                for (var i = 0; i < listObjState.Count; i++)
                {
                    listObjState[i].Obj.SetActive(listObjState[i].listStateDependingPlayerNumber[0]);
                }
            }
            else
            {
                for (var i = 0; i < objPlayerParamsList.Count; i++)
                    objPlayerParamsList[i].SetActive(true);

                for (var i = 0; i < listObjState.Count; i++)
                {
                    listObjState[i].Obj.SetActive(listObjState[i].listStateDependingPlayerNumber[1]);
                }

                if (grpPlayerVehicle) grpPlayerVehicle.pivot = new Vector2(.5f, pivotV);
            }

            return true; 
            #endregion
        }

        public void ChooseButtonDependingGameMode()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            GameObject newButton = null;
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                newButton = btnList[0];
            }
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                newButton = btnList[1];
            }
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                newButton = btnList[2];
            }

            if (IntroInfo.instance.globalDatas.returnPageOutSetSelectedButtonAllowed())
                TS_EventSystem.instance.eventSystem.SetSelectedGameObject(newButton);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void StartChampionshipMode(int PageNumber)
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void StartArcadeOrTimeTrialMode()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            // Arcade
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                // Open the page Choose track when the player came back from a race
                GameModeGlobal.instance.lastSelectedMenuPage = 17;
                string trackName = GameModeGlobal.instance.currentSelectedTrack;
                LoadScene.instance.LoadSceneWithSceneNameAndSpecificCustomMethodList(trackName);
            }
            // Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                // Open the page Choose track when the player came back from a race
                GameModeGlobal.instance.lastSelectedMenuPage = 17;
                string trackName = GameModeGlobal.instance.currentSelectedTrack;
                LoadScene.instance.LoadSceneWithSceneNameAndSpecificCustomMethodList(trackName);
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void BackChooseMenuDependingGameMode()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            GameObject newButton = null;
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
               // if (!IsTheTrackAllowMultipleCategory())
              //      newButton = btnList[5];
               // else
                    newButton = btnList[3];
            }
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
             //   if (!IsTheTrackAllowMultipleCategory())
              //      newButton = btnList[5];
             //   else
                    newButton = btnList[3];
            }
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
                newButton = btnList[4];


            // Arcade
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                int newPageID = 20;
                 if (useOnlyOneVehicleCategory)
                     newPageID = 17;

                StartCoroutine(CanvasMainMenuManager.instance.listMenu[15].transform.parent.GetComponent<PageOut>().DisableCurrentPageAndSelectManualyANewPage(
                    newPageID,
                    false,
                    0,
                    newButton));
            }
            // Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                int newPageID = 20;
                if (useOnlyOneVehicleCategory)
                    newPageID = 17;

                StartCoroutine(CanvasMainMenuManager.instance.listMenu[15].transform.parent.GetComponent<PageOut>().DisableCurrentPageAndSelectManualyANewPage(
                    newPageID,
                    false,
                    0,
                    newButton));
            }
            // Championiship
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                StartCoroutine(CanvasMainMenuManager.instance.listMenu[15].transform.parent.GetComponent<PageOut>().DisableCurrentPageAndSelectManualyANewPage(
                    16,
                    false,
                    0,
                    newButton));
            }

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void GenerateVehicleList()
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {

            } 
            #endregion
        }

        public bool IsTheTrackAllowMultipleCategory()
        {
            #region 
            int currentSelectedTrack = 0;
            // Arcade
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                currentSelectedTrack = GameModeArcade.instance.currentSelection;
            }
            // Time Trial
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                currentSelectedTrack = GameModeTimeTrial.instance.currentSelection;
            }


            TracksData.trackParams trackParams = DataRef.instance.tracksData.listTrackParams[currentSelectedTrack];
            if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
            {
                int specialOrderID = DataRef.instance.arcadeModeData.customTrackList[currentSelectedTrack];
                trackParams = DataRef.instance.tracksData.listTrackParams[specialOrderID];
            }

            int howManyCategoryAvailableForThisTrack = trackParams.catogoryAllowedList.Count;

            if (howManyCategoryAvailableForThisTrack > 1)
                return true;
            else 
                return false;

            #endregion
        }
    }
}