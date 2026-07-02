// Description: MinimapManager: Allows to display vehicles position on map. Attached to Grp_Minimap object.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace TS.Generics
{
    public class MinimapManager : MonoBehaviour
    {
        public static MinimapManager        instance = null;
        public Transform                    objLineRenderer;
        public List<Transform>              vehicleList    = new List<Transform>();
        public List<Transform>              spotList     = new List<Transform>();

        public GameObject                   refSpot;
        public bool                         b_InitDone;
        private bool                        b_InitInProgress;
        public VehicleUIColorsDatas         vehicleUIColorsData;

        public int                          spotSize = 124;
        int                                 firstEntryTested = 0;
        int                                 lastEntryTested = 0;
        int                                 howManyCheckByFrame = 8;

        [System.Serializable]
        public class UISpotList
        {
            public List<RectTransform> spotList = new List<RectTransform>();
        }

        public List<UISpotList>             allUISpotList = new List<UISpotList>();
        public RectTransform                uiRefSpot;
        public List<RectTransform>          minimapList = new List<RectTransform>();

        public Camera                       minmapCam;
        Vector3                             minimapCamPos = Vector3.zero;
        float                               minimapOrthoSize = 0;

        public bool                         isVersionOneUsed = false;

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        //-> Init Lap counter
        public bool bInitMiniMap()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(bInitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;


            return b_InitDone;
            #endregion
        }

        IEnumerator bInitRoutine()
        {
            #region
            b_InitDone = false;

            minimapOrthoSize = minmapCam.orthographicSize;
            minimapCamPos = minmapCam.transform.position;

            if (isVersionOneUsed)
            {
                //-> First Init the vehicle List
                vehicleList.Clear();
                spotList.Clear();
                for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
                {
                    vehicleList.Add(VehiclesRef.instance.listVehicles[i].transform);

                    GameObject newSpot = Instantiate(refSpot, objLineRenderer);
                    if (vehicleUIColorsData)
                    {
                        newSpot.transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().color
                            = vehicleUIColorsData.listVehicleUIColorsDatas[i % vehicleUIColorsData.listVehicleUIColorsDatas.Count];
                    }

                    newSpot.name = i.ToString();


                    float ratio = GetComponent<CreateMinimap>().cam.orthographicSize * 100 / 700 * .01f;

                    newSpot.transform.GetChild(0).localScale = new Vector3(spotSize * ratio, spotSize * ratio, 1);

                    spotList.Add(newSpot.transform);

                    newSpot.transform.GetChild(0).transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = -i;
                    newSpot.transform.GetChild(0).transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = -i;
                }

                int counter = 0;
                for (var i = spotList.Count - 1; i >= 0; i--)
                {
                    if (spotList[i])
                    {
                        spotList[i].transform.GetChild(0).localPosition = new Vector3(0, counter, 0);
                    }
                    counter++;
                }
            }
            else
            {
                //-> Version 2:
                //First Init the vehicle List
                vehicleList.Clear();
                spotList.Clear();
                for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
                {
                    vehicleList.Add(VehiclesRef.instance.listVehicles[i].transform);
                }

                allUISpotList.Clear();

                ImageMinimapTag[] allImages = FindObjectsByType<ImageMinimapTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (var i = 0; i< allImages.Length; i++)
                {
                    if (allImages[i].id == 0)
                        minimapList[0] = allImages[i].GetComponent<RectTransform>();
                    if (allImages[i].id == 1)
                        minimapList[1] = allImages[i].GetComponent<RectTransform>();
                }

                for (var i = 0; i < minimapList.Count; i++)
                {
                    allUISpotList.Add(new UISpotList());
                    for (var j = 0; j < VehiclesRef.instance.listVehicles.Count; j++)
                    {
                        GameObject newSpot = Instantiate(uiRefSpot.gameObject, minimapList[i].transform);
                        if (vehicleUIColorsData)
                        {
                            newSpot.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().color
                                = vehicleUIColorsData.listVehicleUIColorsDatas[j % vehicleUIColorsData.listVehicleUIColorsDatas.Count];
                        }

                        newSpot.name = j.ToString();
                        allUISpotList[i].spotList.Add(newSpot.GetComponent<RectTransform>());
                    }
                }
            }

            

            b_InitDone = true;
            yield return null;
            #endregion
        }

        // Update is called once per frame
        void Update()
        {
            #region 
            if (!isVersionOneUsed)
                UpdateSpotPositionOnMinimapV2();
            else
                UpdateSpotPositionOnMinimap(); 
            #endregion
        }

        private void UpdateSpotPositionOnMinimap()
        {
            #region 
            if (vehicleList.Count > 0 && b_InitDone)
            {
                lastEntryTested = Mathf.Clamp(firstEntryTested + howManyCheckByFrame, 0, vehicleList.Count);
                for (var i = firstEntryTested; i < lastEntryTested; i++)
                {
                    if (vehicleList[i])
                    {
                        if (spotList[i])
                        {
                            if (!spotList[i].gameObject.activeSelf)
                                spotList[i].gameObject.SetActive(true);

                            spotList[i].position = new Vector3(vehicleList[i].position.x, objLineRenderer.position.y + 50, vehicleList[i].position.z);
                        }
                    }
                    else
                    {
                        spotList[i].gameObject.SetActive(false);
                    }
                }

                firstEntryTested += howManyCheckByFrame;
                if (firstEntryTested >= vehicleList.Count) firstEntryTested = 0;

            } 
            #endregion
        }

        private void UpdateSpotPositionOnMinimapV2()
        {
            #region 
            if (vehicleList.Count > 0 && b_InitDone)
            {
                lastEntryTested = Mathf.Clamp(firstEntryTested + howManyCheckByFrame, 0, vehicleList.Count);
                for (var j = 0; j < allUISpotList.Count; j++)
                {
                    for (var i = firstEntryTested; i < lastEntryTested; i++)
                    {

                        if (vehicleList[i])
                        {
                            if (allUISpotList[j].spotList[i])
                            {
                                if (!allUISpotList[j].spotList[i].gameObject.activeSelf)
                                    allUISpotList[j].spotList[i].gameObject.SetActive(true);

                                Vector3 newPos = (vehicleList[i].position - minimapCamPos) / (minimapOrthoSize * 2);
                                allUISpotList[j].spotList[i].pivot = new Vector2(.5f - newPos.x, .5f - newPos.z);
                            }
                        }
                        else
                        {
                            if (allUISpotList[j].spotList[i].gameObject.activeSelf)
                                allUISpotList[j].spotList[i].gameObject.SetActive(false);
                        }
                    }
                }

                firstEntryTested += howManyCheckByFrame;
                if (firstEntryTested >= vehicleList.Count) firstEntryTested = 0;
            } 
            #endregion
        }
    }
}
