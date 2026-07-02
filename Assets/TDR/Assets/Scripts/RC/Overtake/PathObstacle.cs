// Description: PathObstacle. Attached to ObstacleManager. Manage obstacles for AIs.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class PathObstacle : MonoBehaviour
    {
        public bool                     ShowGizmo = false;
        public Gradient                 Danger;
        public Path                     Path;

        [HideInInspector]
        public bool                     IsInitDone = false;
        [HideInInspector]
        public bool                     isProcessDone = false;

        [System.Serializable]
        public class DangerParam
        {
            public Vector3  Pos;
            public float    DangerRatioLeft = 0;
            public float    DangerRatioRight = 0;
            public float    DistanceToPath;

            public DangerParam(Vector3 _Pos, int _DangerRatioLeft, int _DangerRatioRight)
            {
                Pos = _Pos;
                DangerRatioLeft     = _DangerRatioLeft;
                DangerRatioRight    = _DangerRatioRight;
            }
        }

        List<DangerParam>               tmpDangerList = new List<DangerParam>();

        [System.Serializable]
        public class DangerListByPath
        {
            public List<DangerParam>    DangerList = new List<DangerParam>();
        }

        public List<DangerListByPath>   dangerListByPath = new List<DangerListByPath>();


        [System.Serializable]
        public class ObstacleParam
        {
            public ObstaclePosition obstaclePos;
            public int              closestCheckPoint = -1;
            public int              lastClosestCheckPoint;
            public int              dir = 1;
            public float            distToPath = 0;
            public Vector3          lastPos = Vector3.zero;

            public ObstacleParam(ObstaclePosition _obstaclePos, int _closestCheckPoint, int _dir,float _distToPath,Vector3 _lastPos)
            {
                obstaclePos         = _obstaclePos;
                closestCheckPoint   = _closestCheckPoint;
                dir                 = _dir;
                distToPath          = _distToPath;
                lastPos             = _lastPos;
            }
        }

        public List<ObstacleParam>      ObstacleList = new List<ObstacleParam>();

        public int                      showSpecificPath = 0;
        public AnimationCurve           dangerRatioCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));


        void Start()
        {
            #region 
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        public IEnumerator InitRoutine()
        {
            #region
            IsInitDone = false;

            yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);
          
            // Create a list of obstacles
            ObstaclePosition[] obstacles = FindObjectsByType<ObstaclePosition>(FindObjectsSortMode.None);
            ObstacleList.Clear();
            foreach (ObstaclePosition obstacle in obstacles)
                ObstacleList.Add(new ObstacleParam(obstacle, -1, 1, 0, Vector3.zero));

            yield return new WaitUntil(() => Path.IsInitDone);


            CreateDangerListByPath();

            IsInitDone = true;
            yield return null;
            #endregion
        }

        void CreateDangerListByPath()
        {
            #region
            dangerListByPath.Clear();

            int howManyPaths = Path.allPathList.Count;

            for(var j = 0;j< howManyPaths; j++)
            {
                dangerListByPath.Add(new DangerListByPath());

                // Create Path List
                int howManySegment = Mathf.RoundToInt(Path.allPathList[j].pathLength / 5f);//250;
                float segment = Path.allPathList[j].pathLength / howManySegment;

                for (var i = 0; i < howManySegment; i++)
                {
                    Vector3 pos = Path.PositionOnSpecificPath(segment * i, 0, j);
                    dangerListByPath[j].DangerList.Add(new DangerParam(pos, 0, 0));
                }
            }

            StartCoroutine(UpdateDangerPathListObstacleRoutine());
            #endregion
        }

        IEnumerator UpdateDangerPathListObstacleRoutine()
        {
            #region
            isProcessDone = false;
            yield return new WaitUntil(() => dangerListByPath.Count == Path.allPathList.Count);

            // Process for each danger path
            for (var j = 0; j < dangerListByPath.Count; j++)
            {
                tmpDangerList.Clear();
                // Create temporary Path List
                int howManySegment = Mathf.RoundToInt(Path.allPathList[j].pathLength / 5f);//250;
                float segment = Path.allPathList[j].pathLength / howManySegment;

                for (var i = 0; i < howManySegment; i++)
                    tmpDangerList.Add(new DangerParam(dangerListByPath[j].DangerList[i].Pos, 0, 0));


                yield return new WaitUntil(() => tmpDangerList.Count == howManySegment);

                // Update the temporary danger path list with obstacle on the road
                for (var i = 0; i < ObstacleList.Count; i++)
                    yield return new WaitUntil(() => CalculateAndSetThePositionOfTheObstacleOnPath(ObstacleList[i]));

                dangerListByPath[j].DangerList = new List<DangerParam>(tmpDangerList);
            }

            isProcessDone = true;
            yield return null;
            #endregion
        }

        bool CalculateAndSetThePositionOfTheObstacleOnPath(ObstacleParam obsParam)
        {
            #region
            Vector3 obstaclePos = obsParam.obstaclePos.transform.position;
            if (tmpDangerList.Count > 0)
            {
                float closestDistanceSqr = Mathf.Infinity;
                Vector3 currentPosition = obstaclePos;
                int checkPointID = 0;

                for (var i = 0; i < tmpDangerList.Count; i++)
                {
                    Vector3 directionToTarget = tmpDangerList[i].Pos - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        checkPointID = i;
                        obsParam.closestCheckPoint = i;
                    }
                }

                obsParam.dir = IsTheObstacleOnRightOrLeftOfThePath(
                    (tmpDangerList[(obsParam.closestCheckPoint + 1 + tmpDangerList.Count) % tmpDangerList.Count].Pos - tmpDangerList[obsParam.closestCheckPoint].Pos).normalized,
                    (obstaclePos - tmpDangerList[obsParam.closestCheckPoint].Pos).normalized,
                    Vector3.up);

                obsParam.obstaclePos.dir = obsParam.dir;

                obsParam.distToPath = Vector3.Distance(tmpDangerList[checkPointID].Pos, new Vector3(currentPosition.x, tmpDangerList[checkPointID].Pos.y, currentPosition.z));

                float distToPathRatio = Mathf.Clamp01(obsParam.distToPath/12);
               
                //Debug.Log(obsParam.distToPath + " -> distToPathRatio: " + distToPathRatio);
                if(distToPathRatio > .7f)
                {}
                else if (obsParam.dir == 1)
                    tmpDangerList[checkPointID].DangerRatioLeft = 1/* - distToPathRatio*/;
                else
                    tmpDangerList[checkPointID].DangerRatioRight = 1/* - distToPathRatio*/;

                // Smooth difficulty before the obstacle
                int steps = 20;
                for (var i = 0; i < steps; i++)
                {
                    float dangerRatio = i / (float)steps;

                    int id = (checkPointID - steps + i + tmpDangerList.Count) % tmpDangerList.Count;

                    // Left of the road
                    if (tmpDangerList[id].DangerRatioLeft < dangerRatio && obsParam.dir == 1)
                        tmpDangerList[id].DangerRatioLeft = tmpDangerList[checkPointID].DangerRatioLeft * dangerRatioCurve.Evaluate(dangerRatio);// dangerRatio;

                    // Right of the road
                    if (tmpDangerList[id].DangerRatioRight < dangerRatio && obsParam.dir == -1)
                        tmpDangerList[id].DangerRatioRight = tmpDangerList[checkPointID].DangerRatioRight * dangerRatioCurve.Evaluate(dangerRatio);//;
                }

                // Smooth difficulty after the obstacle
                steps = 10;
                for (var i = 0; i < steps; i++)
                {
                    float dangerRatio = i / (float)steps;

                    int id = (checkPointID + i + tmpDangerList.Count) % tmpDangerList.Count;

                    // Left of the road
                    if (tmpDangerList[id].DangerRatioLeft < dangerRatio && obsParam.dir == 1)
                        tmpDangerList[id].DangerRatioLeft = tmpDangerList[checkPointID].DangerRatioLeft * (1- dangerRatioCurve.Evaluate(dangerRatio)*.5f);// dangerRatio;

                    // Right of the road
                    if (tmpDangerList[id].DangerRatioRight < dangerRatio && obsParam.dir == -1)
                        tmpDangerList[id].DangerRatioRight = tmpDangerList[checkPointID].DangerRatioRight *(1- dangerRatioCurve.Evaluate(dangerRatio)*.5f);//;
                }
            }

            return true;
            #endregion
        }

        int IsTheObstacleOnRightOrLeftOfThePath(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            #region
            Vector3 right = Vector3.Cross(up, fwd);
            float dir = Vector3.Dot(right, targetDir);

            if (dir > 0f)
                return -1;
            else if (dir < 0f)
                return 1;
            else
                return 0;
            #endregion
        }

        private void OnDrawGizmos()
        {
            #region
            if (ShowGizmo)
            {
                if (dangerListByPath.Count > showSpecificPath)
                {
                    for (var i = 0; i < dangerListByPath[showSpecificPath].DangerList.Count; i++)
                    {
                        int nextID = (i + 1 + dangerListByPath[showSpecificPath].DangerList.Count) % dangerListByPath[showSpecificPath].DangerList.Count;

                        Vector3 dir = (dangerListByPath[showSpecificPath].DangerList[nextID].Pos - dangerListByPath[showSpecificPath].DangerList[i].Pos).normalized;
                        Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;

                        //Debug.Log("dir: " + dir.x + " : " + dir.y + " : " + dir.z + " : ");

                        Gizmos.color = Danger.Evaluate(dangerListByPath[showSpecificPath].DangerList[i].DangerRatioLeft);
                        Gizmos.DrawSphere(dangerListByPath[showSpecificPath].DangerList[i].Pos + left * 1, 1);

                        Gizmos.color = Danger.Evaluate(dangerListByPath[showSpecificPath].DangerList[i].DangerRatioRight);
                        Gizmos.DrawSphere(dangerListByPath[showSpecificPath].DangerList[i].Pos - left * 1, 1);
                    }
                }
            }
            #endregion
        }
    }

}
