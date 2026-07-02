// Desciption: BackButtonManagerAssistant: Attached to EventSystem object. 
using UnityEngine;

namespace TS.Generics
{
    public class BackButtonManagerAssistant : MonoBehaviour
    {
        public bool returnIfRemapIsInProgress()
        {
            #region
            if (InputRemapper.instance && InputRemapper.instance.IsRemapInProcess)
                return true;
            else
                return false;
            #endregion
        }

        public bool DisableBackButtonOnPageZero()
        {
            #region
            if (CanvasMainMenuManager.instance.currentSelectedPage == 0)
                return true;
            else
                return false;
            #endregion
        }

        public bool ConditionsWhenGameIsPaused()
        {
            #region
            if (PauseManager.instance &&
               CanvasMainMenuManager.instance.currentSelectedPage == 1)
                return true;

            else
                return false;
            #endregion
        }

        public bool DisableBackBtnOnCustomizePage()
        {
            #region
            CustomizationManager customizationManager = FindFirstObjectByType<CustomizationManager>();

            if (CanvasMainMenuManager.instance.currentSelectedPage == 19)
            {
                if (customizationManager &&
                customizationManager.isBackButtonAllowed)
                {
                    return false;
                }

                if (customizationManager &&
                !customizationManager.isBackButtonAllowed)
                {
                    return true;
                }
            }

            return false;
            #endregion
        }
    }

}
