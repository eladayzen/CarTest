// Description: ButtonVariousMethods:
using UnityEngine;


namespace TS.Generics
{
    public class ButtonVariousMethods : MonoBehaviour
    {
        //-> Use on Button_Solo (page Grp_Page_HomePage).
        public void SetToSoloAndOpenPage(int PageNumber)
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer = 1;
                GameModeGlobal.instance.GenerateNameList();
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        //-> Use on Button_Versus (page Grp_Page_HomePage).
        public void SetToVersusAndOpenPage(int PageNumber)
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer = 2;
                GameModeGlobal.instance.GenerateNameList();
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        //-> Use on Button_Arcade (page Grp_Game_ChooseMode).
        public void SetToArcadeAndOpenPage(int PageNumber)
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode = 0;
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        //-> Use on Button_TimeTrial (page Grp_Game_ChooseMode).
        public void SetToTimeTrialAndOpenPage(int PageNumber)
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode = 1;
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        //-> Use on Button_Championship (page Grp_Game_ChooseMode).
        public void SetToChampionshipAndOpenPage(int PageNumber)
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode = 2;
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        public void Test()
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                Debug.Log("Start Race Arcade | Time Trial");
            } 
            #endregion
        }

        public void OpenQuitIGPage()
        {
            #region
            Quit_IGAssistant.instance.OpenQuitIGPage(); 
            #endregion
        }

        public void DeletePlayerPrefs(string name)
        {
            #region 
            PlayerPrefs.DeleteKey(name); 
            #endregion
        }

        public void UpdateCurrentChampionship()
        {
            #region 
            int currentChampionship = GameModeChampionship.instance.currentSelection;
            int howManyChampionship = GameModeChampionship.instance.listChampionshipPosition.Count;

            if (currentChampionship < howManyChampionship - 1) GameModeChampionship.instance.currentSelection++; 
            #endregion
        }
    }
}