// Description: MouseManager. Access from any script to methods to manage mouse cursor.
using UnityEngine;

namespace TS.Generics
{
    public class MouseManager : MonoBehaviour
    {
        public static MouseManager instance = null;

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this;

            //-> If instance already exists and it's not this:
            else if (instance != this)
                Destroy(gameObject); 
            #endregion
        }

        public void CusrorVisibility(bool state)
        {
            #region 
            Cursor.visible = state; 
            #endregion
        }
    }
}

