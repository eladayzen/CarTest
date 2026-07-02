//Description: PreventGamepadNoSelection: If no button selected -> Select a new UI button
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class PreventGamepadNoSelection : MonoBehaviour
    {
        public static PreventGamepadNoSelection instance = null;

        string                                  HorizontalAxis = "";
        string                                  VerticalAxis = "";


        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this;

            else if (instance != this)
                Destroy(gameObject); 
            #endregion
        }

        private void Start()
        {
            #region
            HorizontalAxis = TS_EventSystem.instance.standaloneInputModule.horizontalAxis;
            VerticalAxis = TS_EventSystem.instance.standaloneInputModule.verticalAxis; 
            #endregion
        }

        void Update()
        {
            #region 
            // only Desktop | Other platforms
            if (AllowFindAButton())
                MPreventGamepadNoSelection(); 
            #endregion
        }

        // If any button are selected in the UI, select automaticaly a button if the player press an Axis
        public void MPreventGamepadNoSelection()
        {
            #region
            if (InfoPlayerTS.instance.returnCheckState(0)
                && !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)  // Check if the player can press a button
            {
                if (TS_EventSystem.instance.eventSystem.currentSelectedGameObject == null)
                {
                    if (Mathf.Abs(Input.GetAxisRaw(HorizontalAxis)) >= 0.4f ||
                    Mathf.Abs(Input.GetAxisRaw(VerticalAxis)) >= 0.4f)
                    {
                        GameObject newCurrentButton = findAButton();
                        if (newCurrentButton)
                        {
                            TS_EventSystem.instance.eventSystem.SetSelectedGameObject(newCurrentButton);
                            //Debug.Log(newCurrentButton.name);
                        }
                    };
                }
            }      
            #endregion
        }

        // return a button that can be selected in the current UI
        GameObject findAButton()
        {
            #region 
            Button[] availableButton = FindObjectsByType<Button>(FindObjectsSortMode.None);
            for (var i = 0; i < availableButton.Length; i++)
            {
                if (availableButton[i].enabled)
                {
                    //Selection.activeGameObject = availableButton[i].gameObject;
                    return availableButton[i].gameObject;
                }
            }

            return null; 
            #endregion
        }

        bool AllowFindAButton()
        {
            #region 
            // Gameplay
            if (IntroInfo.instance &&
                    IntroInfo.instance.globalDatas.selectedPlatform == 0 &&
                    PauseManager.instance &&
                    PauseManager.instance.Bool_IsGamePaused &&
                    LoadScene.instance &&
                    LoadScene.instance.b_IsLoadingFinished)
                return true;

            // Main menu
            if (IntroInfo.instance &&
                    IntroInfo.instance.globalDatas.selectedPlatform == 0 &&
                    !PauseManager.instance &&
                    LoadScene.instance &&
                    LoadScene.instance.b_IsLoadingFinished)
                return true;

            return false; 
            #endregion
        }
    }
}
