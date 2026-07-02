// Description: VehiclesVisibleByTheCamList: Allows any script to know if vehicles are visible by the camera P1 | P2
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics {
    public class VehiclesVisibleByTheCamList : MonoBehaviour
    {
        public static VehiclesVisibleByTheCamList   instance = null;

        public List<VehicleInfo>                    listVehicles = new List<VehicleInfo>();

        public List<int>                            listLayersUsedByLayerMask01 = new List<int>();

        public bool                                 b_InitDone;
        private bool                                b_InitInProgress;

        public float                                updateEverySeconds = .1f;
        int                                         firstEntryTested = 0;
        int                                         lastEntryTested = 0;
        int                                         howManyCheckByFrame = 2;

        [System.Serializable]
        public class VehiclesVisibleByCamera
        {
            public int              vehicleToIgnore = 0;
            public Camera           cam;
            public List<bool>       listVehiclesVisible = new List<bool>();
            public LayerMask        layerMask01;

            public VehiclesVisibleByCamera(Camera newCam, List<bool> listBool)
            {
                cam = newCam;
                listVehiclesVisible = listBool;
            }
        }

        public List<VehiclesVisibleByCamera>        listVehiclesVisibleByCamera = new List<VehiclesVisibleByCamera>();

       

        void Awake()
        {
            #region
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        //-> Initialisation
        public bool bInitVehiclesVisibleByCamera()
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

      
        private IEnumerator InitRoutine()
        {
            #region
            //-> Init LayerMask
            string[] layerUsed = new string[listLayersUsedByLayerMask01.Count];
            for (var i = 0; i < listLayersUsedByLayerMask01.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[listLayersUsedByLayerMask01[i]].layerID);

            for (var i = 0; i < listVehiclesVisibleByCamera.Count; i++)
                listVehiclesVisibleByCamera[i].layerMask01 = LayerMask.GetMask(layerUsed);


            yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone == true);

            for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
            {
                listVehicles.Add(VehiclesRef.instance.listVehicles[i]);

            }

            for (var j = 0; j < listVehiclesVisibleByCamera.Count; j++)
            {
                for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
                {
                    listVehiclesVisibleByCamera[j].listVehiclesVisible.Add(false);
                }
            }

           yield return new WaitUntil(()=> InitCamera());

            b_InitDone = true;
            //Debug.Log("Time Trial Step 1:  -> Init Game Modules Done");
            yield return null; 
            #endregion
        }

        void Update()
        {
            #region
            if (b_InitDone)
            {
                lastEntryTested = Mathf.Clamp(firstEntryTested + howManyCheckByFrame, 0, listVehicles.Count);
                for (var i = 0; i < listVehiclesVisibleByCamera.Count; i++)
                {
                    //Debug.Log(Time.frameCount % 3 + " ::: " + i % 3);
                    if (listVehiclesVisibleByCamera[i].cam)
                    {
                        for (var j = firstEntryTested; j < lastEntryTested; j++)
                        {
                            Camera tmpCam = listVehiclesVisibleByCamera[i].cam;
                            Transform target = listVehicles[j].transform;
                            //Debug.Log("visible");
                            //RaycastHit hit;

                            /*if (Physics.Linecast(tmpCam.transform.position, target.position, out hit, listVehiclesVisibleByCamera[i].layerMask01))
                            {
                                int vehicleLayer = LayersRef.instance.layersListData.listLayerInfo[9].layerID;
                            }*/

                            if (IsTargetVisible(tmpCam, target.gameObject) &&
                                    j != listVehiclesVisibleByCamera[i].vehicleToIgnore &&
                                    !listVehicles[j].b_IsRespawn
                                    )
                            {
                                if (!listVehiclesVisibleByCamera[i].listVehiclesVisible[j])
                                {
                                    listVehiclesVisibleByCamera[i].listVehiclesVisible[j] = true;
                                }
                            }
                            else
                            {
                                if (listVehiclesVisibleByCamera[i].listVehiclesVisible[j])
                                {
                                    listVehiclesVisibleByCamera[i].listVehiclesVisible[j] = false;
                                }
                            }
                        }
                    }
                   

                }
                firstEntryTested += howManyCheckByFrame;
                if (firstEntryTested >= listVehicles.Count) firstEntryTested = 0;

            } 
            #endregion
        }

        bool IsTargetVisible(Camera c, GameObject go)
        {
            #region
            var planes = GeometryUtility.CalculateFrustumPlanes(c);
            var point = go.transform.position;
            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < 0)
                    return false;
            }
            return true;
            #endregion
        }

        bool InitCamera()
        {
            #region 
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
            int currentCam = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentCamStyle;

            CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

            for (var i = 0; i < grpCams.Length; i++)
            {
                if (grpCams[i].PlayerID == 0)
                {
                    listVehiclesVisibleByCamera[0].cam = CamRef.instance.listCameras[0];// grpCams[i].interfaceObjList[currentCam].transform.GetChild(0).GetChild(0).GetComponent<Camera>();
                }

                if (grpCams[i].PlayerID == 1 && howManyPlayer == 2)
                {
                    listVehiclesVisibleByCamera[1].cam = CamRef.instance.listCameras[1];// grpCams[i].interfaceObjList[currentCam].transform.GetChild(0).GetChild(0).GetComponent<Camera>();
                }
            }

            return true; 
            #endregion
        }
    }
}