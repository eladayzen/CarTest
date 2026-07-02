// Description: TSAssistantSlideShowCategory: Methods to manage slideshow behavior in Vehicle category page
using UnityEngine;
using System.Collections.Generic;

namespace TS.Generics
{
    public class TSAssistantSlideShowCategory : MonoBehaviour
    {
        public SlideShow        slideShow;

        public int              ListIDVehicleCategory = 0;
        public int              EntryIDVehicleCategory = 193;

        public bool Init()
        {
            #region 
            Debug.Log("Init TS assistant");
            return true; 
            #endregion
        }

        public bool NewEntry()
        {
            #region 
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
            return GameModeGlobal.instance.categoryAllowedList.Count; 
            #endregion
        }

        public int GetCurrentSelection()
        {
            #region 
            for (var i = 0; i < GameModeGlobal.instance.categoryAllowedList.Count; i++)
            {
                if (GameModeGlobal.instance.categoryAllowedList[i] == GameModeGlobal.instance.CurrentVehicleCategory)
                {
                    //Debug.Log("i: " + i);
                    return i;
                }
            }

            return 0; 
            #endregion
        }

        public void SetCurrentSelection()
        {
            #region 
            int category = 0;
            if (GameModeGlobal.instance.categoryAllowedList.Count > 0)
                category = GameModeGlobal.instance.categoryAllowedList[slideShow.currentSelection];

            GameModeGlobal.instance.CurrentVehicleCategory = category; 
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
                   // if (slideShow.currentCategory == -1)
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
                   // if (slideShow.currentCategory == -1)
                        return DataRef.instance.tracksData.listTrackParams[specialOrderID];

                    // A specific category is selected
                    //return DataRef.instance.tracksData.listTrackParams[ReturnTrackIDIfSpecificCategorySelected(specialOrderID)];
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
                            int category = GameModeGlobal.instance.categoryAllowedList[diff];
                            slideShow.objsInSquareList[k].imagesList[0].sprite =
                                DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[category].sprite;
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
                            int category = GameModeGlobal.instance.categoryAllowedList[diff];
                            // TracksData.trackParams tracksData = ReturnTrackParams(diff);

                            int txtList = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[category].ListID;
                            int txtID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[category].EntryID;

                            slideShow.objsInSquareList[k].txtsList[1].NewTextWithSpecificID(txtID, txtList);
                           // slideShow.objsInSquareList[k].txtsList[1].NewTextManageByScript(new List<TextEntry>() { new TextEntry("Bla") });
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
           // Debug.Log("Lock Init");
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
                                    int categoryChecked = GameModeGlobal.instance.categoryAllowedList[diff];

                                    if (DoesThePlayerOwnedAVehicleInThisCategory(categoryChecked))
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(true);
                                        string txtToDisplay = LanguageManager.instance.String_ReturnText(ListIDVehicleCategory, EntryIDVehicleCategory);
                                        objSlideLock.txtSlideShowLock.NewTextManageByScript(new List<TextEntry>() { new TextEntry(txtToDisplay) });
                                    }
                                }
                                //-> Time Trial Mode
                                else
                                {
                                    int categoryChecked = GameModeGlobal.instance.categoryAllowedList[diff];

                                    if (DoesThePlayerOwnedAVehicleInThisCategory(categoryChecked))
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        objSlideLock.Im_Lock.gameObject.SetActive(true);

                                        if (DoesThePlayerOwnedAVehicleInThisCategory(categoryChecked))
                                        {
                                            objSlideLock.Im_Lock.gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            objSlideLock.Im_Lock.gameObject.SetActive(true);
                                            string txtToDisplay = LanguageManager.instance.String_ReturnText(ListIDVehicleCategory, EntryIDVehicleCategory);
                                            objSlideLock.txtSlideShowLock.NewTextManageByScript(new List<TextEntry>() { new TextEntry(txtToDisplay) });
                                        }
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

        //-> Call when button next is pressed to go to vehicle selection
        public bool IsCategoryAvailableBtnNext()
        {
            #region MyRegion
            return DoesThePlayerOwnedAVehicleInThisCategory(GameModeGlobal.instance.CurrentVehicleCategory); 
            #endregion
        }

        public void InitScenName()
        {
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

    } 
}
