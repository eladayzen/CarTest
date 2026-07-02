// Description: CanvasMainMenuManager. Attached to CanvasManager_
// Allows to select and display a page in the editor.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CanvasMainMenuManager : CanvasManager
    {
        public static CanvasMainMenuManager instance = null;
        public string                       newPageName = "New Page";
        public GameObject                   objCanvasMainMenuManager;

        public bool                         b_IsPageInitProcessDone = true;     //-> Use to know if all the Pages are initialized (SceneInitManager)
        public bool                         b_WaitUntilPageInit = true;         // Use to know if a Page is initialized (SceneInitManager)
        public bool                         b_IsSwitchPageInProgress = false;   // Use for transition. Use to know if the new Page is displayed on screen.

        public int                          gamePageInGameplayScene = 0;
        public int                          menuPageInGameplayScene = 1;

        public int                          howManyObject;

        [System.Serializable]
        public class ComeBackFromPage
        {
            public string name;
            public int comeBackToPage = 0;                 // Use for special page. Those pages can be opened from any menu (Monetization, leaderboard)
            public GameObject selectedButtonWhenBackToPage;       // Use for special page. Remember the button that needed to be selected when the special page is closed
        }

        public List<ComeBackFromPage>       ComeBackFromPageList = new List<ComeBackFromPage>();

        private int                         loadInProgress = 0;

        private void Start()
        {
            #region 
            howManyObject = objCanvasMainMenuManager.transform.childCount; 
            #endregion
        }

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        //-> Start the coroutine that initialize ale the menu pages (SceneInitManager)
        public bool initAllPage()
        {
            #region
            for (var i = 0; i < listMenu.Count; i++)
            {
                if(currentSelectedPage != i)
                    listMenu[i].GetComponent<CanvasGroup>().interactable = false;
                else
                    listMenu[i].GetComponent<CanvasGroup>().interactable = true;
            }

            //-> Play the coroutine Once
            if (loadInProgress == 0)
            {
                loadInProgress = 1;
                StartCoroutine(InitAllPageOneByOne());
            }
            //-> Check if the coroutine is finished
            else if (loadInProgress == 2)
                loadInProgress = 3;

            if (loadInProgress == 3)
            {
                Debug.Log("All pages are initialized");
                loadInProgress = 0;
                return true;
            }
            else
                return false;
            #endregion
        }
        
        //-> Init the page one by one.
        IEnumerator InitAllPageOneByOne()
        {
            #region 
            for (var i = 0; i < listMenu.Count; i++)
            {
                b_WaitUntilPageInit = false;
                StartCoroutine(listMenu[i].transform.parent.GetComponent<PageInit>().IPageInit());

                yield return new WaitUntil(() => b_WaitUntilPageInit == true);
            }

            loadInProgress = 2;

            yield return null; 
            #endregion
        }

        //-> Return if all the pages are initialized (SceneInitManager)
        public bool checkIfAllPageAreInitialized()
        {
            #region 
            return b_IsPageInitProcessDone; 
            #endregion
        }

        public bool B_GamePageInGameplayScene()
        {
            #region 
            if (gamePageInGameplayScene == currentSelectedPage)
                return true;
            else
                return false; 
            #endregion
        }

        //-> Return if current selected page is the game page in gameplay scene
        public bool B_MenuPageInGameplayScene()
        {
            #region 
            if (menuPageInGameplayScene == currentSelectedPage)
                return true;
            else
                return false; 
            #endregion
        }
    }
}

