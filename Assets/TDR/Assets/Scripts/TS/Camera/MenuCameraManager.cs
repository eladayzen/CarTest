using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class MenuCameraManager : MonoBehaviour
    {
        public static MenuCameraManager instance;

        public List<GameObject>         grpCamList = new List<GameObject>();

        public int selectedCam = 0;


        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this;
            #endregion
        }

        public void EnableCamera(int index)
        {
            #region 
            // Debug.Log("Enabled Camera");

            if (grpCamList[index])
                grpCamList[index].SetActive(true);

            selectedCam = index;
            #endregion
        }
        public void DisableCamera(int index)
        {
            #region 
            //Debug.Log("Disabled Camera");

            if (grpCamList[index] && selectedCam != index)
                grpCamList[index].SetActive(false);

            selectedCam = index;
            #endregion
        }
    }
}