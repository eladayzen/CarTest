//Desciption: ButtonCustom.cs. Attached to buttons. This script allows to open a new page in the CanvasMainMenu.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TS.Generics
{
    public class ButtonCustom : MonoBehaviour
    {
        public bool             SeeInspector;
        public bool             moreOptions;
        public bool             helpBox = true;

        [HideInInspector]
        public int              whichInit = 0;

        public bool             b_IsCheckConditionsInProcess = false;

        public List<EditorMethodsList_Pc.MethodsList> methodsList       // Create a list of Custom Methods that could be edit in the Inspector
       = new List<EditorMethodsList_Pc.MethodsList>();

        public CallMethods_Pc   callMethods;                            // Access script taht allow to call public function in this script.

        [HideInInspector]
        public UnityEvent       OnClick;
        [HideInInspector]
        public UnityEvent       OnClickWrong;

        void Start()
        {
            #region
            Button btn = GetComponent<Button>();
            if (btn) btn.onClick.AddListener(OnClickTS); 
            #endregion
        }

        void OnClickTS()
        {
            #region 
            // Invoke OnClick() if all the condition return true
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
                StartCoroutine(CallAllTheMethodsOneByOne((checkCondition) =>
                {
                    if (checkCondition) { OnClick.Invoke(); }
                    else { OnClickWrong.Invoke(); }
                })); 
            #endregion
        }

        public void DisplayNewPage(int PageNumber)
        {
            #region
            //Debug.Log("returnCheckState: " + InfoPlayerTS.instance.returnCheckState(0));
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            } 
            #endregion
        }

        //-> Check if all the condition to press the button return true
        public IEnumerator CallAllTheMethodsOneByOne(System.Action<bool> callback)
        {
            #region
            b_IsCheckConditionsInProcess = false;

            bool b_ConditionReturnTrue = true;

            for (var i = 0; i < methodsList.Count; i++)
            {
                if (callMethods.Call_One_Bool_Method(methodsList, i) == false)
                {
                    b_ConditionReturnTrue = false;
                    break;
                }
            }

            callback(b_ConditionReturnTrue);
            b_IsCheckConditionsInProcess = true;
            yield return null;
            #endregion
        }

        //-> Go back to previous page
        public void GoBackToPreviousPage()
        {
            #region 
            //Debug.Log("returnCheckState: " + InfoPlayerTS.instance.returnCheckState(0));
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                int currentPage = CanvasMainMenuManager.instance.currentSelectedPage;
                StartCoroutine(CanvasMainMenuManager.instance.listMenu[currentPage].transform.parent.GetComponent<PageOut>().BackMenu());
            } 
            #endregion
        }

        //-> Load New Scene
        public void LoadNewScene(int newScene)
        {
            LoadScene.instance.LoadSceneWithSceneNumberAndSpecificCustomMethodList(newScene);
        }

        public bool TestCondition_True()
        {
            return true;
        }

        public bool TestCondition_False()
        {
            return false;
        }

        public void DisplayMonetization(int Price)
        {
            #region
            Debug.Log("returnCheckState: " + InfoPlayerTS.instance.returnCheckState(0));
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                //-> Display page are you sure to by this item
                if (InfoCoins.instance.currentPlayerCoins >= Price)
                {
                    PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[4].transform.parent.GetComponent<PageIn>();
                    currentMenu.DisplayNewPage(4);
                }
                //-> Display Screen Not Enough money
                else
                {
                    Debug.Log("5:");
                    PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[5].transform.parent.GetComponent<PageIn>();
                    currentMenu.DisplayNewPage(5);
                }

            } 
            #endregion
        }

        //-> Load New Scene
        public void LoadSceneUsingCurrentSelectedTrackName()
        {
            #region 
            string trackName = GameModeGlobal.instance.currentSelectedTrack;
            LoadScene.instance.LoadSceneWithSceneNameAndSpecificCustomMethodList(trackName); 
            #endregion
        }

        //-> Call pause
        public void CallPause()
        {
            #region 
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
            {  // Check if the player can press a button
                PauseManager.instance.Bool_IsGamePaused = !PauseManager.instance.Bool_IsGamePaused;
                PauseManager.instance.UnpauseGame(0);
            } 
            #endregion
        }

        //-> Load Main Menu Scene
        public void LoadMainMenuScene()
        {
            #region 
            int MainMenuID = DataRef.instance.mainMenuBuildInSceneID.mainMenuScenesInBuildID;
            LoadScene.instance.LoadSceneWithSceneNumberAndSpecificCustomMethodList(MainMenuID); 
            #endregion
        }
   
        public void QuitApplication()
        {
            #region 
            Application.Quit(); 
            #endregion
        }
    }

}
