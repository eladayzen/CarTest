using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DoSomethingEnableDisable : MonoBehaviour
    {
        public int camIndex = 0;
        void OnEnable()
        {
            #region Enabled Camera
            var camManager = MenuCameraManager.instance;
            if (camManager)
                camManager.EnableCamera(camIndex); 
            #endregion
        }

        void OnDisable()
        {
            #region Disabled Camera
            var camManager = MenuCameraManager.instance;
            if (camManager)
                camManager.DisableCamera(camIndex); 
            #endregion
        }
    }

}
