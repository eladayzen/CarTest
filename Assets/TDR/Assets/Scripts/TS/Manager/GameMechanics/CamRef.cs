// Description: CamRef: Access form anywhere to P1 | P2 cam and Post process profile
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CamRef : MonoBehaviour
    {
        public static CamRef            instance = null;

        public List<Camera>             listCameras = new List<Camera>();
        public List<TS_PostProcess>     listPostFxVolumeProfile = new List<TS_PostProcess>();

        public bool                     b_InitDone;

        void Awake()
        {
            #region
            //-> Check if instance already exists
            if (instance == null)
                instance = this;

            InitCamerasList();
            #endregion
        }

        public bool InitCamerasList()
        {
            #region 
            b_InitDone = true;

            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            int currentCam = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentCamStyle;

            CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

            for( var i = 0;i< grpCams.Length; i++)
            {
                if(grpCams[i].PlayerID == 0)
                {
                    listCameras[0] = grpCams[i].interfaceObjList[currentCam].transform.GetChild(0).GetChild(0).GetComponent<Camera>();
                }

                if (grpCams[i].PlayerID == 1 && howManyPlayer == 2)
                {
                    listCameras[1] = grpCams[i].interfaceObjList[currentCam].transform.GetChild(0).GetChild(0).GetComponent<Camera>();
                }
            }
            


            return b_InitDone; 
            #endregion
        }
    }

}
