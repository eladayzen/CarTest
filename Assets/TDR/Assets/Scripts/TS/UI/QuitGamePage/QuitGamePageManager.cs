// Description: QuitGamePageManager: QUit Application method
using UnityEngine;

namespace TS.Generics
{
    public class QuitGamePageManager : MonoBehaviour
    {
        public void QuitApplication()
        {
            #region 
            Application.Quit(); 
            #endregion
        }
    }
}
