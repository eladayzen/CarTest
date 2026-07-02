// Description: PagePauseAssistant
using UnityEngine;

namespace TS.Generics
{
    public class PagePauseAssistant : MonoBehaviour
    {
        public void DisablePausePage()
        {
            #region 
            PauseManager.instance.GetComponent<DisplayPauseMenu>().OnPauseAction();
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false; 
            #endregion
        }
    }
}

