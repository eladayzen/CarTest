// Description: TSAssistantSlideShowChampionship: Methods to manage slideshow behavior in Championship mode
using UnityEngine;
using System.Collections.Generic;

namespace TS.Generics
{
    public class TSAssistantSlideShowChampionship : MonoBehaviour
    {
        public SlideShow    slideShow;

        public int          ListIDVehicleCategory = 0;
        public int          EntryIDVehicleCategory = 193;

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
            //-> Display Championship using the data order
            if (DataRef.instance.championshipModeData.bDisplayChampionshipUsingListOrder)
            {
                return DataRef.instance.championshipModeData.listOfChampionship.Count;
            }
            //-> Display Championship using a specific order
            else
            {
                return DataRef.instance.championshipModeData.customChampionshipList.Count;
            } 
            #endregion
        }

        public int GetCurrentSelection()
        {
            #region 
            return GameModeChampionship.instance.currentSelection; 
            #endregion
        }

        public void SetCurrentSelection()
        {
            #region
            GameModeChampionship.instance.currentSelection = slideShow.currentSelection;

            ChampionshipModeData._Championship champioshipData = ReturnChampionshipGlobalParams(GameModeChampionship.instance.currentSelection);
            GameModeGlobal.instance.CurrentVehicleCategory = champioshipData.whichVehicleCatogoryAllowed; 
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
                            ChampionshipModeData._Championship championshipData = ReturnChampionshipGlobalParams(diff);
                            slideShow.objsInSquareList[k].imagesList[0].sprite = championshipData.championshipIcon;
                        }
                    }
                }
            } 
            #endregion
        }

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
                            ChampionshipModeData._Championship championshipData = ReturnChampionshipGlobalParams(diff);
                            slideShow.objsInSquareList[k].txtsList[1].NewTextWithSpecificID(championshipData.listTexts[0].EntryID, championshipData.listTexts[0].listID); ;
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
                            ChampionshipModeData._Championship championshipData = ReturnChampionshipGlobalParams(diff);
                            GameModeChampionship gameMode = GameModeChampionship.instance;                                      // Check if the championship is unlock 

                            SlideShowLockInfo objSlideLock = slideShow.listSquare[j].gameObject.GetComponentInChildren<SlideShowLockInfo>(true);
                            if (objSlideLock)
                            {
                                if (gameMode.listChampionshipState[ReturnListChampionshipStateID(diff)] &&
                                    DoesThePlayerOwnedAVehicleInThisCategory(DataRef.instance.championshipModeData.listOfChampionship[diff].whichVehicleCatogoryAllowed))
                                {
                                    objSlideLock.Im_Lock.gameObject.SetActive(false);
                                }
                                else
                                {
                                    objSlideLock.Im_Lock.gameObject.SetActive(true);

                                    string txtToDisplay = "";

                                    if (!gameMode.listChampionshipState[ReturnListChampionshipStateID(diff)])
                                        txtToDisplay = LanguageManager.instance.String_ReturnText(championshipData.listTexts[1].listID, championshipData.listTexts[1].EntryID);
                                    else if (!DoesThePlayerOwnedAVehicleInThisCategory(DataRef.instance.championshipModeData.listOfChampionship[diff].whichVehicleCatogoryAllowed))
                                    {

                                        txtToDisplay = LanguageManager.instance.String_ReturnText(ListIDVehicleCategory, EntryIDVehicleCategory);
                                        txtToDisplay += " ";

                                        int vehicleCategory = DataRef.instance.championshipModeData.listOfChampionship[diff].whichVehicleCatogoryAllowed;
                                        // Debug.Log("vehicleCategory: " + vehicleCategory);
                                        if (vehicleCategory >= DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList.Count)
                                            txtToDisplay += "ERROR: This category Doesn't Exist";
                                        else
                                        {
                                            int listID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[vehicleCategory].ListID;
                                            int entryID = DataRef.instance.vehicleGlobalData.VehicleCategoryParamsList[vehicleCategory].EntryID;

                                            txtToDisplay += LanguageManager.instance.String_ReturnText(listID, entryID);
                                        }
                                    }

                                    objSlideLock.txtSlideShowLock.NewTextManageByScript(new List<TextEntry>() { new TextEntry(txtToDisplay) });
                                }
                            }
                        }
                    }
                }
            } 
            #endregion
        }

        public bool GetBestRanking()
        {
            #region 
            GetBestRankingInit();
            return true; 
            #endregion
        }


        public void GetBestRankingInit()
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
                            GameModeChampionship gameMode = GameModeChampionship.instance;                                      // Check if the championship is unlock 

                            SlideShowMedal objSlideMedal = slideShow.listSquare[j].gameObject.GetComponentInChildren<SlideShowMedal>(true);

                            if (objSlideMedal)
                            {
                                int iMedal = gameMode.listChampionshipPosition[ReturnListChampionshipStateID(diff)];
                                if (iMedal == -1)    // No medal
                                    objSlideMedal.im.sprite = objSlideMedal.listMedal[0];
                                else if (iMedal == 2)  // Bronze
                                    objSlideMedal.im.sprite = objSlideMedal.listMedal[1];
                                else if (iMedal == 1) // Silver
                                    objSlideMedal.im.sprite = objSlideMedal.listMedal[2];
                                else if (iMedal == 0) // Gold
                                    objSlideMedal.im.sprite = objSlideMedal.listMedal[3];
                            }
                        }
                    }
                }
            } 
            #endregion
        }

        int ReturnListChampionshipStateID(int diff)
        {
            #region 
            if (!DataRef.instance.championshipModeData.bDisplayChampionshipUsingListOrder)
                return DataRef.instance.championshipModeData.customChampionshipList[diff];
            else
                return diff; 
            #endregion
        }

        //-> Call when button next is pressed to start championship
        public bool IsTrackAvailable()
        {
            #region 
            GameModeChampionship gameMode = GameModeChampionship.instance;
            if (gameMode.listChampionshipState[ReturnListChampionshipStateID(gameMode.currentSelection)] &&
            DoesThePlayerOwnedAVehicleInThisCategory(DataRef.instance.championshipModeData.listOfChampionship[ReturnListChampionshipStateID(gameMode.currentSelection)].whichVehicleCatogoryAllowed))
            {
                return true;
            }

            return false; 
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

        public void UpdateGarageSelectedCategory()
        {
            #region 
            GameModeChampionship gameMode = GameModeChampionship.instance;
            int vehicleCategory = DataRef.instance.championshipModeData.listOfChampionship[ReturnListChampionshipStateID(gameMode.currentSelection)].whichVehicleCatogoryAllowed;

            GameModeGlobal.instance.CurrentVehicleCategory = vehicleCategory;
            #endregion
        }
    } 
}
