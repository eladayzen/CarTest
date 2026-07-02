// Description: CameraSelector. Attached to the vehicle. Manage the camera view selection during the game.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class CameraSelector : MonoBehaviour
    {
       [HideInInspector]
        public bool                 b_InitDone;
        private bool                b_InitInProgress;

        //[HideInInspector]
        /*     public int                  selectedPreset          = 0;
            public int                  defaultPreset           = 1;
            public bool                 saveLastSelectedPreset  = true;


            [System.Serializable]
            public class CamParams
            {
                public string           _Name;
                public bool             bypassThisCam = false;
                public List<UnityEvent> unityEventsList = new List<UnityEvent>();
            }

            public List<CamParams>      CamList = new List<CamParams>();

            public List<int>            PlayerViewList = new List<int>();
*/
          /*  [HideInInspector]
            public bool                 IsProcessDone = true;
            [HideInInspector]
            public bool                 IsSubProcessDone = true;

            public int                  TSInputKeyCameraView = 10;
        */
            VehicleInfo                 vehicleInfo;
/*
            // Start is called before the first frame update
            void Start()
            {
            }

            private void OnDestroy()
            {
                #region 
                VehicleInfo vehicleInfo = GetComponent<VehicleInfo>();

                //-> Camera View (TSInputKeyCameraView = 10)
                if (InfoRememberMainMenuSelection.instance && vehicleInfo.playerNumber < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer)
                    InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyCameraView].OnGetKeyDownReceived -= OnGetKeyPressedCameraAction; 
                #endregion
            }

            void CallUnityEvents(int iD)
            {
                #region 
                if (IsProcessDone)
                    StartCoroutine(CallUnityEventsRoutine(iD)); 
                #endregion
            }

            IEnumerator CallUnityEventsRoutine(int iD)
            {
                #region 
                IsProcessDone = false;
                for (var i = 0; i < CamList[iD].unityEventsList.Count; i++)
                {
                    CamList[iD].unityEventsList[i].Invoke();
                    yield return new WaitUntil(() => IsSubProcessDone);
                }
                IsProcessDone = true;
                yield return null; 
                #endregion
            }
          */
        //-> Initialisation
        public bool bInitCamSystem()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(InitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            VehiclePrefabInit vehiclePrefabInit = transform.parent.GetComponent<VehiclePrefabInit>();
            vehicleInfo = GetComponent<VehicleInfo>();
            VehicleCamPreset vehicleCamPreset = GetComponent<VehicleCamPreset>();

            if (vehicleInfo.playerNumber == 0 || vehicleInfo.playerNumber == 1)
            {
                //-> Case P1 P2: Init the Cam P1 or P2 depending the player
                CarF[] playerCameras = FindObjectsByType<CarF>(FindObjectsSortMode.None);

                foreach (CarF cam in playerCameras)
                {
                    if (cam.PlayerID == vehicleInfo.playerNumber)
                    {
                        vehicleCamPreset.PlayerCamera = cam;
                        cam.presetList = new List<CamPreset>(vehicleCamPreset.PresetList);
                        yield return new WaitUntil(() => cam.InitCamera(gameObject) == true);
                    }
                }
            }

            b_InitDone = true;
            yield return null;
            #endregion
        }
        /*
        void OnGetKeyPressedCameraAction()
        {
            #region
            if (IsProcessDone &&
                    !PauseManager.instance.Bool_IsGamePaused &&
                    InfoPlayerTS.instance.b_IsAvailableToDoSomething)
            {
                selectedPreset++;
                selectedPreset %= PlayerViewList.Count;

                while (CamList[selectedPreset].bypassThisCam)
                {
                    selectedPreset++;
                    selectedPreset %= PlayerViewList.Count;
                }

                CallUnityEvents(PlayerViewList[selectedPreset]);


                if (vehicleInfo.playerNumber == 0)
                    PlayerPrefs.SetInt("CamP1", selectedPreset);
                if (vehicleInfo.playerNumber == 1)
                    PlayerPrefs.SetInt("CamP2", selectedPreset);
            } 
            #endregion
        }*/
    }

}
