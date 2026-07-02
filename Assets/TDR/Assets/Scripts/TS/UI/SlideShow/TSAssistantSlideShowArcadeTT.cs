// Description: TSAssistantSlideShowArcadeTT: Methods to manage slideshow behavior in Arcade and TT mode
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TS.Generics
{
    public class TSAssistantSlideShowArcadeTT : MonoBehaviour
    {
        public SlideShow        slideShow;

        public int              ListIDVehicleCategory = 0;
        public int              EntryIDVehicleCategory = 193;

        public Sprite           sWarning;
        public Sprite           sPadlock;

        public bool Init()
        {
            #region
            Debug.Log("Init TS assistant");
            return true; 
            #endregion
        }

        public bool NewEntry()
        {
            #region MyRegion
            Debug.Log("New entry TS assistant");
            return true; 
            #endregion
        }

        public bool GetSprite()
        {
            #region 
            GetSpriteInit();
            return true; 
            #endregion
        }

        public bool GetName()
        {
            #region 
            GetNameInit();
            return true; 
            #endregion
        }

        public int GetHowManyEntries()
        {
            #region 
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                //-> Display tracks using the data order
                if (DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
                {
                    // All categories are selected
                    if (slideShow.currentCategory == -1)
                        if (slideShow.currentCategory == -1)
                            return DataRef.instance.tracksData.listTrackParams.Count;


                    // A specific category is selected
                   /* int total = DataRef.instance.tracksData.listTrackParams.Count;

                    int counter = 0;
                    for (var i = 0; i < total; i++)
                        if (DataRef.instance.tracksData.listTrackParams[i].WhichVehicleCatogoryAllowed == slideShow.currentCategory)
                            counter++;
                    return counter;*/

                    return 0;
                }
                //-> Display tracks using a specific order
                else
                {
                    // All categories are selected
                    if (slideShow.currentCategory == -1)
                        return DataRef.instance.arcadeModeData.customTrackList.Count;


                    // A specific category is selected
                    /* int total = DataRef.instance.arcadeModeData.customTrackList.Count;

                     int counter = 0;
                     for (var i = 0; i < total; i++)
                     {
                         int id = DataRef.instance.arcadeModeData.customTrackList[i];
                         if (DataRef.instance.tracksData.listTrackParams[id].WhichVehicleCatogoryAllowed == slideShow.currentCategory)
                             counter++;
                     }
                     return counter;*/
                    return 0;
                }
            }
            //-> Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                //-> Display tracks using the data order
                if (DataRef.instance.timeTrialModeData.bDisplayTrackUsingTrackListOrder)
                {
                    // All categories are selected
                    if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams.Count;


                    // A specific category is selected
                  /*  int total = DataRef.instance.tracksData.listTrackParams.Count;

                    int counter = 0;
                    for (var i = 0; i < total; i++)
                        if (DataRef.instance.tracksData.listTrackParams[i].WhichVehicleCatogoryAllowed == slideShow.currentCategory)
                            counter++;
                    return counter;*/
                    return 0;
                }
                //-> Display tracks using a specific order
                else
                {
                    // All categories are selected
                    if (slideShow.currentCategory == -1)
                        return DataRef.instance.timeTrialModeData.customTrackList.Count;


                    // A specific category is selected
                    int total = DataRef.instance.timeTrialModeData.customTrackList.Count;

                    /* int counter = 0;
                     for (var i = 0; i < total; i++)
                     {
                         int id = DataRef.instance.timeTrialModeData.customTrackList[i];
                         if (DataRef.instance.tracksData.listTrackParams[id].WhichVehicleCatogoryAllowed == slideShow.currentCategory)
                             counter++;
                     }
                     return counter;*/
                    return 0;
                }
            }

            return 0; 
            #endregion
        }

        public int GetCurrentSelection()
        {
            #region
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                return GameModeArcade.instance.currentSelection;
            }
            //-> Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                return GameModeTimeTrial.instance.currentSelection;
            }

            return 0; 
            #endregion
        }

        public void SetCurrentSelection()
        {
            #region 
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                GameModeArcade.instance.currentSelection = slideShow.currentSelection;
                TracksData.trackParams tracksData = ReturnTrackParams(GameModeArcade.instance.currentSelection);

                //Debug.Log("test -> " + GameModeArcade.instance.currentSelection);

                GameModeGlobal.instance.currentSelectedTrack = tracksData.sceneName;
                GameModeGlobal.instance.categoryAllowedList = tracksData.catogoryAllowedList;

            }
            //-> Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                GameModeTimeTrial.instance.currentSelection = slideShow.currentSelection;
                TracksData.trackParams tracksData = ReturnTrackParams(GameModeTimeTrial.instance.currentSelection);
                GameModeGlobal.instance.currentSelectedTrack = tracksData.sceneName;
                GameModeGlobal.instance.categoryAllowedList = tracksData.catogoryAllowedList;
            }  
            #endregion
        }

        

        int ReturnDiff(int j)
        {
            #region 
            int diff = 0;
            if (j < slideShow.whichSelectedInList)
            {
                diff = Mathf.Abs(slideShow.whichSelectedInList - j);
                diff = slideShow.currentSelection - diff;
            }
            else if (j > slideShow.whichSelectedInList)
            {
                diff = Mathf.Abs(slideShow.whichSelectedInList - j);
                diff = slideShow.currentSelection + diff;
            }
            else
            {
                diff = slideShow.currentSelection;
            }

            return diff; 
            #endregion
        }

        TracksData.trackParams ReturnTrackParams(int diff)
        {
            #region 
            // Arcade
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
                {
                    int specialOrderID = DataRef.instance.arcadeModeData.customTrackList[diff];

                    // All categories are selected
                    //if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams[specialOrderID];

                    // A specific category is selected
                    //return DataRef.instance.tracksData.listTrackParams[ReturnTrackIDIfSpecificCategorySelected(specialOrderID)];
                }
                else
                {
                    // All categories are selected
                    //if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams[diff];

                    // A specific category is selected
                    //return DataRef.instance.tracksData.listTrackParams[ReturnTrackIDIfSpecificCategorySelected(diff)];
                }
            }
            // Time Trial
            else
            {
                if (!DataRef.instance.timeTrialModeData.bDisplayTrackUsingTrackListOrder)
                {
                    int specialOrderID = DataRef.instance.timeTrialModeData.customTrackList[diff];
                    // return DataRef.instance.tracksData.listTrackParams[specialOrderID];
                    // All categories are selected
                    //if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams[specialOrderID];

                    // A specific category is selected
                   // return DataRef.instance.tracksData.listTrackParams[ReturnTrackIDIfSpecificCategorySelected(specialOrderID)];
                }
                else
                {
                    // return DataRef.instance.tracksData.listTrackParams[diff];
                    // All categories are selected
                    //if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams[diff];

                    // A specific category is selected
                   // return DataRef.instance.tracksData.listTrackParams[ReturnTrackIDIfSpecificCategorySelected(diff)];
                }
            }


            #endregion
        }

   

        //-> Get Sprite for each track
        public void GetSpriteInit()
        {
            #region 
            for (var j = 0; j < slideShow.listSquare.Count; j++)
            {
                for (var k = 0; k < slideShow.listSquareRef.Count; k++)
                {
                    if (slideShow.listSquare[j] == slideShow.listSquareRef[k])
                    {
                        int diff = ReturnDiff(j);

                        if (diff >= 0 && diff < slideShow.howManyEntries)
                        {
                            TracksData.trackParams tracksData = ReturnTrackParams(diff);
                            slideShow.objsInSquareList[k].imagesList[0].sprite = tracksData.trackSprite;
                        }
                    }
                }
            } 
            #endregion
        }

        //-> Get Track Name
        public void GetNameInit()
        {
            #region 
            for (var j = 0; j < slideShow.listSquare.Count; j++)
            {
                for (var k = 0; k < slideShow.listSquareRef.Count; k++)
                {
                    if (slideShow.listSquare[j] == slideShow.listSquareRef[k])
                    {
                        int diff = ReturnDiff(j);

                        if (diff >= 0 && diff < slideShow.howManyEntries)
                        {
                            TracksData.trackParams tracksData = ReturnTrackParams(diff);
                            slideShow.objsInSquareList[k].txtsList[1].NewTextWithSpecificID(tracksData.NameIDMultiLanguage, tracksData.selectedListMultiLanguage);
                        }
                    }
                }
            } 
            #endregion
        }

        public bool GetSlideshowLock()
        {
            #region 
            GetSlideshowLockInit();
            return true; 
            #endregion
        }


        public void GetSlideshowLockInit()
        {
            #region
            for (var j = 0; j < slideShow.listSquare.Count; j++)
            {
                for (var k = 0; k < slideShow.listSquareRef.Count; k++)
                {
                    if (slideShow.listSquare[j] == slideShow.listSquareRef[k])
                    {
                        int diff = ReturnDiff(j);

                        if (diff >= 0 && diff < slideShow.howManyEntries)
                        {
                            TracksData.trackParams tracksData = ReturnTrackParams(diff);

                            SlideShowLockInfo objSlideLock = slideShow.listSquare[j].gameObject.GetComponentInChildren<SlideShowLockInfo>(true);
                            if (objSlideLock)
                            {

                                //-> Arcade Mode
                                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
                                {
                                    GameModeArcade gameMode = GameModeArcade.instance;
                                    //Debug.Log(("lock: " + tracksData.WhichVehicleCatogoryAllowed));

                                    if (gameMode.listArcadeTrackState[ReturnStateArcadeID(diff)] &&
                                        DoesThePlayerOwnedAVehicleInThoseCategory(tracksData.catogoryAllowedList))
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                       

                                        string txtToDisplay = "";

                                        if (!gameMode.listArcadeTrackState[ReturnStateArcadeID(diff)])
                                        {
                                            objSlideLock.Im_Lock.sprite = sPadlock;
                                            objSlideLock.Im_Lock.gameObject.SetActive(true);
                                            txtToDisplay = LanguageManager.instance.String_ReturnText(tracksData.listTexts[1].listID, tracksData.listTexts[1].EntryID);
                                        }
                                        else if (!DoesThePlayerOwnedAVehicleInThoseCategory(tracksData.catogoryAllowedList))
                                        {
                                            objSlideLock.Im_Lock.sprite = sWarning;
                                            objSlideLock.Im_Lock.gameObject.SetActive(true);
                                            txtToDisplay = LanguageManager.instance.String_ReturnText(ListIDVehicleCategory, EntryIDVehicleCategory);
                                        }

                                        objSlideLock.txtSlideShowLock.NewTextManageByScript(new List<TextEntry>() { new TextEntry(txtToDisplay) });
                                    }
                                }
                                //-> Time Trial Mode
                                else
                                {
                                    GameModeTimeTrial gameMode = GameModeTimeTrial.instance;
                                    // Check if the track is unlock
                                    if (gameMode.listTimeTrialTrackState[ReturnStateTimeTrialID(diff)] &&
                                         DoesThePlayerOwnedAVehicleInThoseCategory(tracksData.catogoryAllowedList))
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(true);

                                        string txtToDisplay = "";

                                        if (!gameMode.listTimeTrialTrackState[ReturnStateArcadeID(diff)])
                                            txtToDisplay = LanguageManager.instance.String_ReturnText(tracksData.listTexts[1].listID, tracksData.listTexts[1].EntryID);
                                        else if (!DoesThePlayerOwnedAVehicleInThoseCategory(tracksData.catogoryAllowedList))
                                        {
                                            txtToDisplay = LanguageManager.instance.String_ReturnText(ListIDVehicleCategory, EntryIDVehicleCategory);
                                            txtToDisplay += " ";
                                        }

                                        objSlideLock.txtSlideShowLock.NewTextManageByScript(new List<TextEntry>() { new TextEntry(txtToDisplay) });
                                    }
                                }
                            }

                        }
                    }
                }  
            }
            #endregion 
        }

        int ReturnStateArcadeID(int diff)
        {
            #region 
            if (!DataRef.instance.arcadeModeData.bDisplayTrackUsingTrackListOrder)
            {
                return DataRef.instance.arcadeModeData.customTrackList[diff];
            }
            else
            {
                return diff;
            } 
            #endregion
        }

        int ReturnStateTimeTrialID(int diff)
        {
            #region
            if (!DataRef.instance.timeTrialModeData.bDisplayTrackUsingTrackListOrder)
            {
                return DataRef.instance.timeTrialModeData.customTrackList[diff];
            }
            else
            {
                return diff;
            } 
            #endregion
        }

        //-> Call when button next is pressed to vehicle category selection
        public bool IsTrackAvailable()
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                GameModeArcade gameMode = GameModeArcade.instance;                                      // Check if the track is unlock 
                return gameMode.listArcadeTrackState[ReturnStateArcadeID(gameMode.currentSelection)];
            }
            else
            {
                GameModeTimeTrial gameMode = GameModeTimeTrial.instance;                                      // Check if the track is unlock 
                return gameMode.listTimeTrialTrackState[ReturnStateTimeTrialID(gameMode.currentSelection)];
            } 
            #endregion
        }

        public void InitScenName()
        {
            #region 
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                GameModeArcade.instance.currentSelection = slideShow.currentSelection;
                TracksData.trackParams tracksData = ReturnTrackParams(GameModeArcade.instance.currentSelection);
                GameModeGlobal.instance.currentSelectedTrack = tracksData.sceneName;
            }
            //-> Time Trial
            else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                GameModeTimeTrial.instance.currentSelection = slideShow.currentSelection;
                TracksData.trackParams tracksData = ReturnTrackParams(GameModeTimeTrial.instance.currentSelection);
                GameModeGlobal.instance.currentSelectedTrack = tracksData.sceneName;
            } 
            #endregion
        }


        public void InfoLockInit()
        {
            #region 
            for (var j = 0; j < slideShow.listSquare.Count; j++)
            {
                for (var k = 0; k < slideShow.listSquareRef.Count; k++)
                {
                    if (slideShow.listSquare[j] == slideShow.listSquareRef[k])
                    {
                        int diff = ReturnDiff(j);

                        if (diff >= 0 && diff < slideShow.howManyEntries)
                        {
                            TracksData.trackParams tracksData = ReturnTrackParams(diff);

                            SlideShowLockInfo objSlideLock = slideShow.listSquare[j].gameObject.GetComponentInChildren<SlideShowLockInfo>(true);
                            if (objSlideLock)
                            {
                                objSlideLock.ReturnLockInfoStright(0);
                            }
                        }
                    }
                }
            } 
            #endregion
        }


        bool DoesThePlayerOwnedAVehicleInThisCategory(int category)
        {
            #region
            InfoVehicle infoVehicle = InfoVehicle.instance;

            for (var i = 0; i < infoVehicle.vehicleParametersInGameList.Count; i++)
            {
                bool DoesThePlayerOwnTheVehicle = infoVehicle.vehicleParametersInGameList[i].isUnlocked;
                int VehicleCategory = infoVehicle.vehicleParametersInGameList[i].vehicleCategory;

                if (DoesThePlayerOwnTheVehicle && VehicleCategory == category)
                    return true;
            }

            return false;
            #endregion
        }

        int ReturnTrackIDIfSpecificCategorySelected(int diff)
        {
            #region
            /* int total = DataRef.instance.tracksData.listTrackParams.Count;
             int counter = -1;
             int id = 0;
             for (var i = 0; i < total; i++)
             {
                 if (DataRef.instance.tracksData.listTrackParams[i].WhichVehicleCatogoryAllowed == slideShow.currentCategory)
                     counter++;

                 if (diff == counter)
                 {
                     id = i;
                     break;
                 }

             }
             return id;*/
            return 0;
            #endregion
        }

        public void InitCurrentCategory()
        {
            #region
            if (slideShow.txtSelectedCategory)
            {
                string txt = "";

                if(slideShow.currentCategory == -1)
                    txt = LanguageManager.instance.String_ReturnText(0, 199);
                else
                {
                    int entryID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[slideShow.currentCategory].EntryID;
                    int listID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[slideShow.currentCategory].ListID;
                    txt = LanguageManager.instance.String_ReturnText(listID, entryID);
                }

                slideShow.txtSelectedCategory.DisplayTextComponent(slideShow.txtSelectedCategory.gameObject, txt);
            }
            #endregion
        }

        bool DoesThePlayerOwnedAVehicleInThoseCategory(List<int> categoryList)
        {
            #region
            InfoVehicle infoVehicle = InfoVehicle.instance;

            for (var j = 0; j < categoryList.Count; j++)
            {
                for (var i = 0; i < infoVehicle.vehicleParametersInGameList.Count; i++)
                {
                    bool DoesThePlayerOwnTheVehicle = infoVehicle.vehicleParametersInGameList[i].isUnlocked;
                    int VehicleCategory = infoVehicle.vehicleParametersInGameList[i].vehicleCategory;

                    if (DoesThePlayerOwnTheVehicle && VehicleCategory == categoryList[j])
                        return true;
                }
            }
            return false;
            #endregion
        }

        public bool IsCategoryAvailableBtnNext()
        {
            #region 
            return DoesThePlayerOwnedAVehicleInThoseCategory(GameModeGlobal.instance.categoryAllowedList); 
            #endregion
        }

        public void UpdateSliderIfBackFromCategory()
        {
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            TSAssistantSlideShowArcadeTT obj = GameObject.FindFirstObjectByType<TSAssistantSlideShowArcadeTT>();
            if (obj)
            {
                obj.GetComponent<SlideShow>().Init();
            }
        }
    } 
}
