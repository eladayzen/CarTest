// Description: PageInitAssistant: Methods called by PageInit.cs
using UnityEngine;


namespace TS.Generics {
    public class PageInitAssistant : MonoBehaviour
    {
        public bool SetSelectedGameObject(GameObject newButton)
        {
            #region
            if (IntroInfo.instance.globalDatas.returnPageInitSetSelectedButtonAllowed())
            {
                //Debug.Log("Select Button");
                GameObject SelectedCanvas = CanvasMainMenuManager.instance.listMenu[CanvasMainMenuManager.instance.currentSelectedPage];
                if (SelectedCanvas == transform.GetChild(0).gameObject)
                    TS_EventSystem.instance.eventSystem.SetSelectedGameObject(newButton);
            }

            return true; 
            #endregion
        }

        public bool InitCoinsValue(GameObject objBtnCoin)
        {
            #region 
            InfoCoins.instance.UpdateCoins();
            return true; 
            #endregion
        }

        public bool InitPageEventExample()
        {
            #region 
            Debug.Log("Main Page is initialized");
            return true; 
            #endregion
        }

        public void UpdateLastPageSelected(int value = 0)
        {
            #region 
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            GameModeGlobal.instance.lastSelectedMenuPage = value;
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }
    }
}
