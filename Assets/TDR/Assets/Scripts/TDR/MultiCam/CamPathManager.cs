// Description: An object follows a path. This object tries to be the closer as posible to the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CamPathManager : MonoBehaviour
    {
        [HideInInspector]
        public bool             seeInspector;

        public bool             isAutoInit = false;
        public bool             isInitDone = false;
        public List<Vector3>    spotPosList = new List<Vector3>();

        public List<SpotPathInfo> spotInfoList = new List<SpotPathInfo>();

        public Transform        targetOnPath;

        public List<Transform>  targetList = new List<Transform>();
        public List<Rigidbody>  targetRbList = new List<Rigidbody>();
        public int              playerID = 0;

        int                     closestTargetID = 0;

        [HideInInspector]
        public float            dist = 0;

        public float            distanceBetweenTwoSpots = .001f;

        [HideInInspector]
        public int              closestPoint = 0;
        [HideInInspector]
        public float            closestSqrDist = 1000000;

        [HideInInspector]
        public float            groundOffset = .1f;

        public Vector3          gizmotargetOnPathSize = Vector3.one;

        public float            targetHeight = 10;
        public float            speed = 30;

        public CamFollowPath    camFollowPath;
        public bool             firstSpotFound = false;

        bool                    isProcessDone = true;

        public int              offsetSpotOnPath = 0;

        [HideInInspector]
        public int              editorCurrentSelectedPoint = -1;

        public float            defaultTangentLength = 20;

        bool isTargetFoundProcessDone = false;
        bool isGenerateSpotPosListDone = false;
        
        void Start()
        {
            #region
            if(isAutoInit)
                StartCoroutine(InitRoutine());
            #endregion
        }


        public IEnumerator InitRoutine()
        {
            #region
            if (spotPosList.Count == 0 && playerID == 0)
            {
                isProcessDone = false;
                CreateDefaultPath();
            }
            else if(playerID == 1)
            {
                isProcessDone = false;
                CreatePathForPlayerTwoUsingPlayerOneInfo();
            }

            while (!isProcessDone) ;

            targetRbList.Clear();
            targetList.Clear();

            if (VehiclesRef.instance && CheckIfCurrentGameModeUseVehicleRefInit())
                yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);

            FindTargets();
            yield return new WaitUntil(() => isTargetFoundProcessDone );

            GenerateSpotPosList();
            yield return new WaitUntil(() => isGenerateSpotPosListDone );

            dist = PathLength();

            isInitDone = true;
            yield return null;
            #endregion
        }

        bool CheckIfCurrentGameModeUseVehicleRefInit()
        {
            #region
            List<int> gameModeList = VehiclesRef.instance.vehicleGlobalData.gameModeThatWaitVehicleRefInitList;
            for (var i = 0; i < gameModeList.Count; i++)
            {
                if (gameModeList[i] == InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode)
                    return true;
            }

            return false;
            #endregion
        }

        void FindTargets()
        {
            #region
         
            if (VehiclesRef.instance && CheckIfCurrentGameModeUseVehicleRefInit())
            {
                if(playerID == 0)
                {
                    targetRbList.Add(VehiclesRef.instance.listVehicles[0].GetComponent<Rigidbody>());
                    targetList.Add(VehiclesRef.instance.listVehicles[0].transform);
                }
             

                if (playerID == 1)
                {
                    targetRbList.Add(VehiclesRef.instance.listVehicles[1].GetComponent<Rigidbody>());
                    targetList.Add(VehiclesRef.instance.listVehicles[1].transform);
                }
            }
            isTargetFoundProcessDone = true;
            #endregion
        }

        float PathLength()
        {
            #region
            for (var i = 0; i < spotPosList.Count - 1; i++)
            {
                Vector3 posA = spotPosList[i] + transform.position;
                Vector3 posB = spotPosList[i + 1] + transform.position;
                dist += Vector3.Distance(posA, posB);
            }
            return dist;
            #endregion
        }

        void GenerateSpotPosList()
        {
            #region
            spotInfoList.Clear();

            ReturnTotalCurveDistance(.25f);
            #endregion
        }

        public static Vector3 GetPointPosition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            #region
            return (1f - t) * (1f - t) * (1f - t) * p0 + 3f * (1f - t) * (1f - t) * t * p1 + 3f * (1f - t) * t * t * p2 + t * t * t * p3;
            #endregion
        }

        public void ReturnTotalCurveDistance(float distanceBetweenTwoPoints = 1.5f)
        {
            #region
            // isProcessDone = false;
            if (spotPosList.Count > 0)
            {
                //  Undo.RegisterFullObjectHierarchyUndo(myScript, "Bezier");

                float dist = 0.0f;
                Vector3 lastPos = spotPosList[0];

                float multiplier = 1;

                // distVecList.Clear();

                for (int j = 0; j < spotPosList.Count - 1; j++)
                {
                    if (j % 3 == 0)
                    {
                        float currentPosOnCurve = 0;
                        while (currentPosOnCurve < 1)
                        {
                            Vector3 startPos = spotPosList[j] + transform.position;
                            Vector3 startTangent = spotPosList[j + 1] + transform.position;
                            Vector3 endTangent = spotPosList[j + 2] + transform.position;
                            Vector3 endtPos = spotPosList[j + 3] + transform.position;

                            Vector3 subPoint = GetPointPosition(startPos, startTangent, endTangent, endtPos, currentPosOnCurve);

                            dist += Vector3.Distance(lastPos, subPoint);

                            if (dist >= distanceBetweenTwoPoints * multiplier)
                            {
                                multiplier++;
                            }

                            lastPos = subPoint;
                            currentPosOnCurve += distanceBetweenTwoSpots;

                            spotInfoList.Add(new SpotPathInfo(subPoint, Vector3.zero));
                        }
                    }
                }
            }

            isGenerateSpotPosListDone = true;
            #endregion
        }

        void FixedUpdate()
        {
            #region
            if (spotPosList.Count > 1 && targetList.Count > 0)
            {
                bool isSpotFound = FindClosestPoint();

                TheFirstTimeASpotIsFoundInitTheCameraPosition(isSpotFound);

                if (!isSpotFound)
                {
                }
                else
                {
                    MoveTheTargetOnPathDependingPlayerPosition();
                }
            }
            #endregion
        }

        bool FindClosestPoint()
        {
            #region
            bool isSpotFound = false;
            closestSqrDist = 100000;

            var counter = 0;
            var start = 0;
            List<int> pointsCheckedList = new List<int>();


            if (spotInfoList.Count > 0)
            {
                start = (closestPoint - 40 + spotInfoList.Count) % spotInfoList.Count;

                for (var i = 0; i < 80; i++)
                    pointsCheckedList.Add((start + i) % spotInfoList.Count);
            }

            for (var j = 0; j < targetList.Count; j++)
            {
                for (var i = 0; i < pointsCheckedList.Count; i++)
                {
                    Vector3 pos01 = spotInfoList[pointsCheckedList[i]].pos;

                    Vector3 offset = targetList[j].position - pos01;
                    float sqrLen = offset.sqrMagnitude;
                    //Debug.Log(i + ": " + sqrLen);

                    if (sqrLen <= closestSqrDist && sqrLen < 3000)
                    {
                        closestSqrDist = sqrLen;
                        closestPoint = pointsCheckedList[i];
                        closestTargetID = j;
                        isSpotFound = true;
                        //Debug.Log("C");
                    }
                    counter++;
                }
            }

            if (!isSpotFound)
            {
                closestPoint = 0;
                closestSqrDist = 100000;
                for (var j = 0; j < targetList.Count; j++)
                {
                    for (var i = 0; i < spotInfoList.Count; i++)
                    {
                        Vector3 pos01 = spotInfoList[i].pos;

                        Vector3 offset = targetList[j].position - pos01;
                        float sqrLen = offset.sqrMagnitude;

                        if (sqrLen <= closestSqrDist)
                        {
                            closestSqrDist = sqrLen;
                            closestPoint = i;
                            closestTargetID = j;
                            isSpotFound = true;
                        }

                        counter++;
                    }
                }
            }

            //Debug.Log("Counter: " + counter);

            return isSpotFound;
            #endregion
        }



        void MoveTheTargetOnPathDependingPlayerPosition()
        {
            #region
            if (closestTargetID == 0 && targetRbList.Count > 0 && targetRbList[0] != null)
            {
                Vector3 closetSpot = spotInfoList[(closestPoint + offsetSpotOnPath) % spotInfoList.Count].pos + new Vector3(0,targetHeight,0);
                targetOnPath.position = Vector3.Lerp(targetOnPath.position, closetSpot, Time.deltaTime * speed);
            }
            #endregion
        }

       void TheFirstTimeASpotIsFoundInitTheCameraPosition(bool isSpotFound)
        {
            #region 
            if (!firstSpotFound && isSpotFound)
            {
                Vector3 closetSpot = spotInfoList[(closestPoint + offsetSpotOnPath)% spotInfoList.Count].pos + new Vector3(0, targetHeight, 0);
                targetOnPath.position = closetSpot;
                camFollowPath.ForceUpdateCameraPosition();
                firstSpotFound = true;
            } 
            #endregion
        }


    
        void CreateDefaultPath()
        {
            #region 
            spotPosList.Clear();
            PathRef pathRef = FindFirstObjectByType<PathRef>();
            if (pathRef)
            {
                List<Transform> checkpointsList = new List<Transform>(pathRef.Track.checkpoints);
                for (var i = 0; i < checkpointsList.Count; i++)
                {
                    if (i == 0)
                    {
                        Vector3 dir = (checkpointsList[i + 1].position - checkpointsList[i].position).normalized;
                        Vector3 tangent01Pos = checkpointsList[i].position + dir * defaultTangentLength;
                        spotPosList.Add(checkpointsList[i].position - transform.position);
                        spotPosList.Add(tangent01Pos - transform.position);
                    }
                    else if (i < checkpointsList.Count - 1)
                    {
                        Vector3 dir = (checkpointsList[i + 1].position - checkpointsList[i].position).normalized;

                        Vector3 tangent01Pos = checkpointsList[i].position - dir * defaultTangentLength;

                        Vector3 tangent02Pos = checkpointsList[i].position + dir * defaultTangentLength;

                        spotPosList.Add(tangent01Pos - transform.position);
                        spotPosList.Add(checkpointsList[i].position - transform.position);
                        spotPosList.Add(tangent02Pos - transform.position);
                    }
                    else
                    {
                        Vector3 dir = (checkpointsList[i - 1].position - checkpointsList[i].position).normalized;
                        Vector3 tangent01Pos = checkpointsList[i].position + dir * defaultTangentLength;
                        Vector3 tangent02Pos = checkpointsList[i].position - dir * defaultTangentLength;
                        spotPosList.Add(tangent01Pos - transform.position);
                        spotPosList.Add(checkpointsList[i].position - transform.position);


                        Path path = FindFirstObjectByType<Path>();

                        if (path.TrackIsLooped)
                        {
                            spotPosList.Add(tangent02Pos - transform.position);

                            dir = (checkpointsList[1].position - checkpointsList[0].position).normalized;
                            tangent01Pos = checkpointsList[0].position - dir * defaultTangentLength;
                            spotPosList.Add(tangent01Pos - transform.position);
                            spotPosList.Add(checkpointsList[0].position - transform.position);
                        }
                    }

                    isProcessDone = true;
                } 
            }
            #endregion
        }


        void CreatePathForPlayerTwoUsingPlayerOneInfo()
        {
            CamPathManager[] grpPath = FindObjectsByType<CamPathManager>(FindObjectsSortMode.None);

            for (var i = 0; i < grpPath.Length; i++)
            {
                if (grpPath[i].playerID == 0)
                {
                    // while (!grpPath[i].isInitDone) ;
                    if(grpPath[i].spotPosList.Count > 0){
                        spotPosList = new List<Vector3>(grpPath[i].spotPosList);
                        targetHeight = grpPath[i].targetHeight;
                        speed = grpPath[i].speed;
                        offsetSpotOnPath = grpPath[i].offsetSpotOnPath;
                        defaultTangentLength = grpPath[i].defaultTangentLength;
                    }
                    else
                    {
                        CreateDefaultPath();
                    }
                }
            }
            isProcessDone = true;
        }
      

        void OnDrawGizmosSelected()
        {
            #region
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(targetOnPath.position, gizmotargetOnPathSize);
            #endregion
        }

    }

    [System.Serializable]
    public class SpotPathInfo
    {
        public Vector3 pos;
        public Vector3 dir;
        public SpotPathInfo(Vector3 _pos, Vector3 _dir)
        {
            pos = _pos;
            dir = _dir;
        }
    }

}
