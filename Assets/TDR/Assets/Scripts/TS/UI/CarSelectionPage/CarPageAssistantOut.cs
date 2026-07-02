// Description: CarPageAssistantOut: Method called when the car page is closed (Main Menu).
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class CarPageAssistantOut : MonoBehaviour
    {
        public List<GameObject> btnList = new List<GameObject>();

        public void StateGrpCamP1(bool value)
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            CarSelectionManager.instance.StateGrpCamP1(value);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }
        public void StateGrpCamP2(bool value)
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            CarSelectionManager.instance.StateGrpCamP2(value);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }

        public void EnterCarSelection()
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            CarSelectionManager.instance.EnterCarSelection();
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
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
    }
}