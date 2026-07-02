// Description: Path. Info about the path of the track.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class Path : MonoBehaviour
    {
        public bool                     IsInitDone = false;

        [HideInInspector]
        public List<Texture2D>          listTex = new List<Texture2D>();
        [HideInInspector]
        public List<GUIStyle>           listGUIStyle = new List<GUIStyle>();
        [HideInInspector]
        public bool                     SeeInspector;
        [HideInInspector]
        public bool                     moreOptions;
        [HideInInspector]
        public bool                     helpBox = true;
        [HideInInspector]
        public int                      currentSelectedCheckpoint;
        [HideInInspector]
        public bool                     showCheckpoints = true;

         [HideInInspector]
        public List<Transform>          checkpointsRef = new List<Transform>();                 // Use to remember the default Main Path
        public List<Transform>          checkpoints = new List<Transform>();                    // The current checkpoints use as path for the player or AI.
        [HideInInspector]
        public List<Vector3>            checkpointsPosition = new List<Vector3>();              // The position of each checkpoint of checkpoints list (current path for AI |Players)
       // [HideInInspector]
        public List<float>              checkpointsDistanceFromPathStart = new List<float>();   // The distance from start for each checkpoint of checkpoints list (current path for AI |Players)

        public float                    pathLength;                                             // The lenght of the current path

        private List<int>               checkpointsIDList = new List<int>() { 0, 0, 0, 0 };         // Use for the CatmullRom calculation

        public int                      curveSmoothness = 100;                                  // Use to have smoother representation of the track path curve

        public List<AltPath>            AltPathList = new List<AltPath>();                      // References to Alternative path connected to the main path

        public GameObject               prefabCheckpoint;                                       // Use as reference prefab when a new checkpoint is added to the path using the Inspector Editor 

        public float                    gizmoCheckpointSize = 10;
        public bool                     gizmoShowPath = true;

        public bool                     TrackIsLooped = false;

        public Gradient                 trackDifficulty;

        // Show vehicle path that use offset
        [System.Serializable]
        public class AdditionalPath
        {
            public Vector3 offset;
            public Color color = Color.red;
            public bool b_Show = true;
        }

        public List<AdditionalPath>     additionalPathsList = new List<AdditionalPath>();

        public float                    thresholdAngle = 80.41f;
        public float                    decreaseDifficultyDamping = .02f;
        public int                      howManyPositionCheck = 10;

        public List<float>              difficultyPathList = new List<float>();
        public float                    spotDifficultyDistance = 2; // Distance between 2 difficulty spot allong the path.
        public float                    ratioForward = 1f;

        public List<float>              difficultyReversPathList = new List<float>();
        public float                    ratioRevers = 1f;

        public bool                     updateCurve = true;
        public int                      smoothDifficultyDependingGripSurface = 60;
        public bool                     difficultyGizmoCurve = true;

        [System.Serializable]
        public class SpeedGrip
        {
            public List<float>      speedRatio = new List<float>();
        }

        [System.Serializable]
        public class SpeedRatioDependingGrip
        {
            public int              checkpointID = 0;
            public int              altPathID = -1;
            public List<SpeedGrip>  speedGridList = new List<SpeedGrip>();
        }

        public List<SpeedRatioDependingGrip> speedRatioDependingGripList = new List<SpeedRatioDependingGrip>();

        public SurfaceData              surfaceData;

        public int                      currentGizmoSurface = 0;

        public int                      currentCheckpointID = 0;
        public int                      currentAltPathID = 0;

        public int                      selectedId = 0;
        public bool                     improveAIPathLoopMode = false;
        public int                      improveAIPathStartID = 0;
        public int                      improveAIPathEndID = 0;
        public float                    improveAITimer = 0;
        public float                    lastLap = 0;
        public string                   improveLastTwoLaps = "Last and Penultimate Laps:";

        [System.Serializable]
        public class TrackPathList
        {
            public List<Transform>  spotList = new List<Transform>();
            public List<Vector3>    spotpositionList = new List<Vector3>();
            public List<float>      distanceFromPathList = new List<float>();
            public float            pathLength = 0;
            public int              checkpointId = 0;
            public int              altPathId = 0;
            public int              indexOfLastAltPathSpotInList = 0;
        }

        public List<TrackPathList>      allPathList = new List<TrackPathList>();

        public bool                     isUsingAltPath = false;


        [System.Serializable]
        public class DiffOffsetParam
        {
            public int spotID = 0;
            public float difficultyOffset = 0;
            public DiffOffsetParam(int _spotID, float _difficultyOffset)
            {
                spotID = _spotID;
                difficultyOffset = _difficultyOffset;
            }
        }

        public List<DiffOffsetParam>    difficultyOffset = new List<DiffOffsetParam>();

        [HideInInspector]
        public float                    minDifficultyOffset = -1;
        [HideInInspector]
        public float                    maxDifficultyOffset = 1;
        public int                      selectedIDOffsetDifficulty = -1;

        public float gizmoSpotSize = 5;

        // Use this for initialization
        private void Awake()
        {
            #region
            if (CheckpointAvailable() && checkpoints.Count > 3)
            {
                checkpointsRef = new List<Transform>(checkpoints);
                ModifyThePath();
            }
            else Debug.Log("Checkpoints are missing in this path: " + this.name);
            #endregion
        }

        public Vector3 TargetPositionOnPath(float dist)
        {
            #region Use to find the new position of the AI target
            return PositionOnPath(Mathf.Clamp(dist, 0, pathLength));
            #endregion
        }
        
        public Vector3 TargetRotationOnPath(float dist, bool invertY = false)
        {
            #region  Use to find the new rotation of the AI target
            Vector3 p1 = PositionOnPath(Mathf.Clamp(dist, 0, pathLength));
            Vector3 p2 = PositionOnPath(Mathf.Clamp(dist + 0.1f, 0, pathLength));
            Vector3 delta = p2 - p1;
            if (invertY) delta = p1 - p2;
            return delta.normalized;
            #endregion
        }

        public bool isModifiedPathDone = true;

        public void ModifyThePath()
        {
            #region
            // This section update info used to calculate the path.
            // It used the current checkpoints list to update the list of position for each checkpoint of the path
            // It used the current checkpoints list to update the list of distances from the first checkpoint the other checkpoints of the path
            // Clear list of checkpoints positions and distance from the start of the path
            //Debug.Log("CHange The Path");
            isModifiedPathDone = false;

            checkpointsPosition = new List<Vector3>();
            checkpointsDistanceFromPathStart = new List<float>();

            float distanceFromStart = 0;
            for (int i = 0; i < checkpoints.Count + 1; ++i)
            {
                if (checkpoints[i % checkpoints.Count] && checkpoints[(i + 1) % checkpoints.Count])
                {
                    Vector3 checkpoint1 = checkpoints[i % checkpoints.Count].position;
                    Vector3 checkpoint2 = checkpoints[(i + 1) % checkpoints.Count].position;
                    // Save the position of each checkpoint
                    checkpointsPosition.Add(checkpoints[i % checkpoints.Count].position);
                    // Save the distance from the start for each checkpoint
                    checkpointsDistanceFromPathStart.Add(distanceFromStart);
                    Vector3 diff = checkpoint1 - checkpoint2;
                    distanceFromStart += diff.magnitude;
                }
            }

            pathLength = checkpointsDistanceFromPathStart[checkpointsDistanceFromPathStart.Count - 1];

            selectedId = FindTheCurrentAltPathList();
            isModifiedPathDone = true;
            //StartCoroutine(WaitBeforeTheEndOfThePath());

            #endregion
        }

        public Vector3 PositionOnPath(float dist, int _checkpoint = 0)
        {
            #region Return a point position on the track using CatmullRom equation

            for (var i = 0; i < checkpointsDistanceFromPathStart.Count; i++)
            {
                //-> Find the closest checkpoint to the position we want to find on the path.
                if (checkpointsDistanceFromPathStart[i] < dist) _checkpoint++;
                else break;
            }

            //-> Find the four checkpoints needed to use catmulRom equation
            checkpointsIDList[0] = (_checkpoint - 2 + checkpoints.Count) % checkpoints.Count;
            checkpointsIDList[1] = (_checkpoint - 1 + checkpoints.Count) % checkpoints.Count;
            checkpointsIDList[2] = _checkpoint;
            checkpointsIDList[3] = (_checkpoint + 1) % checkpoints.Count;

            if (checkpointsPosition.Count > checkpointsIDList[0] &&
                checkpointsPosition.Count > checkpointsIDList[1] &&
                checkpointsPosition.Count > checkpointsIDList[2] &&
                checkpointsPosition.Count > checkpointsIDList[3] &&
                checkpointsDistanceFromPathStart.Count > 4)
            {
                float clampedDist = Mathf.Clamp(dist, checkpointsDistanceFromPathStart[checkpointsIDList[1]], checkpointsDistanceFromPathStart[checkpointsIDList[2]]);
                // Scale the distance between 0 and 1. It gives the distance from checkpointsIDList[1] to the point we want to find.
                float scaledDistP1P2 = (clampedDist - checkpointsDistanceFromPathStart[checkpointsIDList[1]]) / (checkpointsDistanceFromPathStart[checkpointsIDList[2]] - checkpointsDistanceFromPathStart[checkpointsIDList[1]]);
                checkpointsIDList[2] %= checkpoints.Count;
                return CatmullRom(checkpointsPosition[checkpointsIDList[0]], checkpointsPosition[checkpointsIDList[1]], checkpointsPosition[checkpointsIDList[2]], checkpointsPosition[checkpointsIDList[3]], scaledDistP1P2);
            }
            return Vector3.zero;
            #endregion
        }

        public Vector3 PositionOnSpecificPath(float dist, int _checkpoint = 0,int pathIndex = 0)
        {
            #region Return a point position on the track using CatmullRom equation using a specific path

            List<Transform>     spotList = allPathList[pathIndex].spotList;
            List<Vector3>       spotPositionList = allPathList[pathIndex].spotpositionList;
            List<float>         distanceList = allPathList[pathIndex].distanceFromPathList;

            for (var i = 0; i < distanceList.Count; i++)
            {
                //-> Find the closest checkpoint to the position we want to find on the path.
                if (distanceList[i] < dist) _checkpoint++;
                else break;
            }

            //-> Find the four checkpoints needed to use catmulRom equation
            checkpointsIDList[0] = (_checkpoint - 2 + spotList.Count) % spotList.Count;
            checkpointsIDList[1] = (_checkpoint - 1 + spotList.Count) % spotList.Count;
            checkpointsIDList[2] = _checkpoint;
            checkpointsIDList[3] = (_checkpoint + 1) % spotList.Count;

            if (spotPositionList.Count > checkpointsIDList[0] &&
                spotPositionList.Count > checkpointsIDList[1] &&
                spotPositionList.Count > checkpointsIDList[2] &&
                spotPositionList.Count > checkpointsIDList[3])
            {
                float clampedDist = Mathf.Clamp(dist, distanceList[checkpointsIDList[1]], distanceList[checkpointsIDList[2]]);
                // Scale the distance between 0 and 1. It gives the distance from checkpointsIDList[1] to the point we want to find.
                float scaledDistP1P2 = (clampedDist - distanceList[checkpointsIDList[1]]) / (distanceList[checkpointsIDList[2]] - distanceList[checkpointsIDList[1]]);
                checkpointsIDList[2] %= spotPositionList.Count;
                return CatmullRom(spotPositionList[checkpointsIDList[0]], spotPositionList[checkpointsIDList[1]], spotPositionList[checkpointsIDList[2]], spotPositionList[checkpointsIDList[3]], scaledDistP1P2);
            }
            return Vector3.zero;
            #endregion
        }

        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float scaledDistP1P2)
        {
            #region
            return 0.5f * ((2 * p1) + (-p0 + p2) * scaledDistP1P2 + (2 * p0 - 5 * p1 + 4 * p2 - p3) * scaledDistP1P2 * scaledDistP1P2 + (-p0 + 3 * p1 - 3 * p2 + p3) * scaledDistP1P2 * scaledDistP1P2 * scaledDistP1P2);
            #endregion
        }

        public Vector3 VelocityOnPath(float dist, int _checkpoint = 0)
        {
            #region
            for (var i = 0; i < checkpointsDistanceFromPathStart.Count; i++)
            {
                //-> Find the closest checkpoint to the position we want to find on the path.
                if (checkpointsDistanceFromPathStart[i] < dist) _checkpoint++;
                else break;
            }

            //-> Find the four checkpoints needed to use catmulRom equation
            checkpointsIDList[0] = (_checkpoint - 2 + checkpoints.Count) % checkpoints.Count;
            checkpointsIDList[1] = (_checkpoint - 1 + checkpoints.Count) % checkpoints.Count;
            checkpointsIDList[2] = _checkpoint;
            checkpointsIDList[3] = (_checkpoint + 1) % checkpoints.Count;


            float clampedDist = Mathf.Clamp(dist, checkpointsDistanceFromPathStart[checkpointsIDList[1]], checkpointsDistanceFromPathStart[checkpointsIDList[2]]);
            // Scale the distance between 0 and 1. It gives the distance from checkpointsIDList[1] to the point we want to find.
            float scaledDistP1P2 = (clampedDist - checkpointsDistanceFromPathStart[checkpointsIDList[1]]) / (checkpointsDistanceFromPathStart[checkpointsIDList[2]] - checkpointsDistanceFromPathStart[checkpointsIDList[1]]);

            checkpointsIDList[2] %= checkpoints.Count;

            return GetVelocity(checkpointsPosition[checkpointsIDList[0]], checkpointsPosition[checkpointsIDList[1]], checkpointsPosition[checkpointsIDList[2]], checkpointsPosition[checkpointsIDList[3]], scaledDistP1P2);
            #endregion
        }

        private Vector3 GetVelocity(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float scaledDistP1P2)
        {
            #region
            return 0.5f * ((-p0 + p2) + (2 * p0 - 5 * p1 + 4 * p2 - p3) * scaledDistP1P2 + (-p0 + 3 * p1 - 3 * p2 + p3) * scaledDistP1P2 * scaledDistP1P2);
            #endregion
        }

        //-> Prevent bug if checkpoints are missing in the checkpoint array
        bool CheckpointAvailable()
        {
            #region
            for (var i = 0; i < checkpoints.Count; i++)
            {
                if (!checkpoints[i])
                    return false;
            }
            return true;
            #endregion
        }

        public float DistanceFromCheckpointToStart(int checkpointID)
        {
            #region
            //Debug.Log("Yep");
            return checkpointsDistanceFromPathStart[checkpointID];
            #endregion
        }

        private void OnDrawGizmos()
        {
            # region 
            if (gizmoShowPath)
            {
                //-> Track path is drawned only if there are at least 3 checkpoints and no checkpoint are missing in the list
                if (CheckpointAvailable() && checkpoints.Count > 3)
                {
                    //-> In edit Mode generate the checkpoint list
                    if (!Application.isPlaying)
                    {
                        ModifyThePath();
                        pathLength = checkpointsDistanceFromPathStart[checkpointsDistanceFromPathStart.Count - 1];
                    }

                    GizmosDrawMainPath();
                    GizmosDrawAILimitPath();
                }
            }
            #endregion
        }

        private void OnDrawGizmosSelected()
        {
            # region 
            GizmosDrawDifficultyGizmoCurve();
            GizmosDrawDifficultyOffset();
            #endregion
        }

        void GizmosDrawDifficultyOffset()
        {
            #region 
            for (var i = 0; i < difficultyOffset.Count; i++)
            {
                Vector3 pos = PositionOnPath(difficultyOffset[i].spotID * spotDifficultyDistance);
                if (selectedIDOffsetDifficulty == i && selectedIDOffsetDifficulty < difficultyOffset.Count)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.blue;

                Gizmos.DrawSphere(pos, gizmoSpotSize);
            }

            if (improveAIPathLoopMode)
            {
                Vector3 startPos;
                if (AltPathList.Count > 0)
                    startPos = PositionOnSpecificPath(improveAIPathStartID * spotDifficultyDistance, 0, 0);
                else
                    startPos = PositionOnPath(improveAIPathStartID * spotDifficultyDistance);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(startPos, 10);

                Vector3 endPos;

                if (AltPathList.Count > 0)
                    endPos = PositionOnSpecificPath(improveAIPathEndID * spotDifficultyDistance, 0, 0);
                else
                    endPos = PositionOnPath(improveAIPathEndID * spotDifficultyDistance);


                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(endPos, 10);
            } 
            #endregion
        }

        void GizmosDrawMainPath()
        {
            #region
            //-> Draw the main path
            Vector3 prev = checkpoints[0].position;

            float newPathLength = pathLength;
            if (!TrackIsLooped) newPathLength = checkpointsDistanceFromPathStart[checkpointsDistanceFromPathStart.Count - 2];

            for (float dist = 0; dist < newPathLength; dist += newPathLength / curveSmoothness)
            {
                Vector3 next = PositionOnPath(Mathf.Clamp(dist + 1, 0, newPathLength));
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            if (TrackIsLooped)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(prev, checkpoints[0].position);
            }

            for (var i = 0; i < checkpoints.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(checkpoints[i].position, 3);
            }
            #endregion
        }

        void GizmosDrawAILimitPath()
        {
            #region
            //-> Draw the path AI limit.
            // Those limit are useful to know if the AI vehicles will no enter in collision with walls.
            for (var i = 0; i < additionalPathsList.Count; i++)
            {
                if (additionalPathsList[i].b_Show)
                {
                    float newPathLength = pathLength;
                    if (!TrackIsLooped) newPathLength = checkpointsDistanceFromPathStart[checkpointsDistanceFromPathStart.Count - 2];

                    Vector3 prevAlt = checkpoints[0].position;
                    Vector3 offset = additionalPathsList[i].offset;
                    Vector3 pos = new Vector3();
                    for (float dist = 0; dist < newPathLength; dist += newPathLength / curveSmoothness)
                    {
                        Vector3 nextAlt = PositionOnPath(Mathf.Clamp(dist + 1, 0, newPathLength));
                        Gizmos.color = additionalPathsList[i].color;

                        Vector3 dir = (nextAlt - prevAlt).normalized;
                        Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;

                        Vector3 Up = Vector3.Cross(left, dir).normalized;

                        pos = left * offset.x + Up * offset.y + dir * offset.z;

                        Gizmos.DrawLine(prevAlt + pos, nextAlt + pos);
                        prevAlt = nextAlt;
                    }

                    if (TrackIsLooped)
                        Gizmos.DrawLine(prevAlt + pos, checkpoints[0].position + pos);
                }
            }
            #endregion
        }

        List<float> ReturnDifficultyOfTheTurnInTheDirectionOfThePath(int pathIndex)
        {
            #region
            Vector3 prev = allPathList[pathIndex].spotList[0].position;

            float newPathLength = allPathList[pathIndex].pathLength;
            if (!TrackIsLooped) newPathLength = allPathList[pathIndex].distanceFromPathList[allPathList[pathIndex].distanceFromPathList.Count - 2];

            int difficultyCurveSmoothness = Mathf.RoundToInt(newPathLength / spotDifficultyDistance);

            // Create a list of angles for each position of the path
            List<float> angleList = new List<float>();
            int counter = 0;
            for (float dist = 0; dist < newPathLength; dist += newPathLength / difficultyCurveSmoothness)
            {
                // Next Pos
                Vector3 next = PositionOnSpecificPath(Mathf.Clamp(dist + 1, 0, newPathLength),0, pathIndex);

                // First Direction
                Vector3 dir = (next - prev).normalized;

                // Next Next Pos
                Vector3 nextNext = PositionOnSpecificPath(Mathf.Clamp(dist + (newPathLength / difficultyCurveSmoothness) + 1, 0, newPathLength), 0, pathIndex);

                // First Direction
                Vector3 dirNext = (nextNext - next).normalized;

                // Find the differance between the 2 angles
                float angle = Vector3.Angle(dir, dirNext);

                angleList.Add(angle*.9f);

                prev = next;
                counter++;
            }


            // Create the list of angles difficulty for each position on the path
            counter = 0;
            List<float> scaledValueList = new List<float>();
            for (float dist = 0; dist < newPathLength; dist += newPathLength / difficultyCurveSmoothness)
            {
                float totalAngle = 0;
                for (var i = 0; i < howManyPositionCheck; i++)
                {
                    totalAngle += angleList[(counter + i) % angleList.Count];
                }

                var scaledValue = totalAngle / thresholdAngle;

                scaledValueList.Add(scaledValue * ratioForward);

                counter++;
            }


            // Smooth the difficulty after a turn.
            counter = 0;
            List<float> forwardList = new List<float>();
            for (float dist = 0; dist < newPathLength; dist += newPathLength / difficultyCurveSmoothness)
            {
                var scaledValue = scaledValueList[counter];

                scaledValueList[counter] = scaledValue;
                forwardList.Add(Mathf.Clamp01(scaledValueList[counter]));

                counter++;
            }

            return forwardList;
            #endregion
        }

        List<float> ReturnDifficultyOfTheTurnInTheOppositeDirectionOfThePath(int pathIndex)
        {
            #region
            Vector3 prev = allPathList[pathIndex].spotList[0].position;

            float newPathLength = allPathList[pathIndex].pathLength;
            int difficultyCurveSmoothness = Mathf.RoundToInt(allPathList[pathIndex].pathLength / spotDifficultyDistance);

            // Create a list of angles for each position on the path
            List<float> angleList = new List<float>();
            int counter = 0;
            for (float dist = newPathLength; dist >= 0; dist -= newPathLength / difficultyCurveSmoothness)
            {
                // Next Pos
                Vector3 next = PositionOnSpecificPath(Mathf.Clamp(dist - 1, 0, newPathLength),0,pathIndex);

                // First Direction
                Vector3 dir = (next - prev).normalized;

                // Next Next Pos
                Vector3 nextNext = PositionOnSpecificPath(Mathf.Clamp(dist - (newPathLength / difficultyCurveSmoothness) - 1, 0, newPathLength),0,pathIndex);

                // First Direction
                Vector3 dirNext = (nextNext - next).normalized;

                // Find the differance between the 2 angles
                float angle = Vector3.Angle(dir, dirNext);

                angleList.Add(angle*.9f);

                prev = next;
                counter++;
            }

            // Create the list of angles difficulty for each position on the path
            counter = 0;
            List<float> scaledValueList = new List<float>();

            for (float dist = newPathLength; dist >= 0; dist -= newPathLength / difficultyCurveSmoothness)
            {
                float totalAngle = 0;
                for (var i = 0; i < howManyPositionCheck; i++)
                {
                    totalAngle += angleList[(counter + i) % angleList.Count];
                }

                var scaledValue = totalAngle / thresholdAngle;

                scaledValueList.Add(scaledValue * ratioRevers);

                counter++;
            }

            // Smooth the difficulty after a turn.
            counter = 0;
            List<float> reversList = new List<float>();
            for (float dist = newPathLength; dist >= 0; dist -= newPathLength / difficultyCurveSmoothness)
            {
                // Next Pos
                Vector3 next = PositionOnSpecificPath(Mathf.Clamp(dist - 1, 0, newPathLength),0,pathIndex);

                var scaledValue = scaledValueList[counter];

                scaledValueList[counter] = scaledValue;

                reversList.Add(Mathf.Clamp01(scaledValueList[counter]));
                counter++;
            }

            reversList.Reverse();

            return reversList;
            #endregion
        }

        void GeneratePathsInfo()
        {
            #region
            speedRatioDependingGripList.Clear();

            for(var u = 0; u < allPathList.Count; u++)
            {
                List<float> forwardList = ReturnDifficultyOfTheTurnInTheDirectionOfThePath(u);
                List<float> reversList  = ReturnDifficultyOfTheTurnInTheOppositeDirectionOfThePath(u);

                speedRatioDependingGripList.Add(new SpeedRatioDependingGrip());

                speedRatioDependingGripList[u].checkpointID = allPathList[u].checkpointId;
                speedRatioDependingGripList[u].altPathID = allPathList[u].altPathId;

                for (var l = 0; l < surfaceData.surfaceList.Count; l++)
                {
                    speedRatioDependingGripList[u].speedGridList.Add(new SpeedGrip());

                    int entries = forwardList.Count;

                    for (var i = 0; i < forwardList.Count; i++)
                    {
                        if (forwardList[i] > reversList[(i + entries * 2 + 10) % entries])
                            speedRatioDependingGripList[u].speedGridList[l].speedRatio.Add(forwardList[i]);
                        else
                            speedRatioDependingGripList[u].speedGridList[l].speedRatio.Add(reversList[(i + entries * 2 + 10) % entries]);
                    }

                    // Find checkpoint closest position
                    for (var i = 0; i < checkpoints.Count; i++)
                    {
                        float dist = DistanceFromCheckpointToStart(i);

                        var closest = dist / pathLength;
                        closest *= speedRatioDependingGripList[u].speedGridList[l].speedRatio.Count;

                        closest = Mathf.RoundToInt(closest);

                        var scaledValue = 1 - (surfaceData.surfaceList[l].gripAmount - .25f) / (1 - .25f);
                        scaledValue = Mathf.Clamp01(scaledValue);
                        smoothDifficultyDependingGripSurface = Mathf.RoundToInt(scaledValue * 100);
                        // 30 suface .5f | 60 surface .2f
                        // smoothDifficultyDependingGripSurface -> Value modified by CarAI.cs
                        for (var j = 1; j < smoothDifficultyDependingGripSurface; j++)
                        {
                            int spot = (-j + (int)closest + speedRatioDependingGripList[u].speedGridList[l].speedRatio.Count * 2) % speedRatioDependingGripList[u].speedGridList[l].speedRatio.Count;

                            float newValue = speedRatioDependingGripList[u].speedGridList[l].speedRatio[(int)closest] - .007f * j;
                            if (newValue > speedRatioDependingGripList[u].speedGridList[l].speedRatio[spot])
                            {
                                speedRatioDependingGripList[u].speedGridList[l].speedRatio[spot] = newValue;
                                speedRatioDependingGripList[u].speedGridList[l].speedRatio[spot] = Mathf.Clamp01(speedRatioDependingGripList[u].speedGridList[l].speedRatio[spot]);
                            }
                        }
                    }
                }
            }
            #endregion
        }

        public int FindTheCurrentAltPathList()
        {
            #region
            for(var i= 0;i< allPathList.Count; i++)
            {
                if (Mathf.Round(pathLength) == Mathf.Round(allPathList[i].pathLength))
                    return i;
            }
            return 0;
            #endregion
        }

        void GizmosDrawDifficultyGizmoCurve()
        {
            #region
            // Display Gizmos
            if (difficultyGizmoCurve && IsInitDone)
            {
                var     counter         = 0;
                int     currentPath     = selectedId;
                float   newPathLength   = allPathList[currentPath].pathLength;
                int     difficultyCurveSmoothness = Mathf.RoundToInt(newPathLength / spotDifficultyDistance);

                for (float dist = 0; dist < newPathLength; dist += newPathLength / difficultyCurveSmoothness)
                {
                    Vector3 next = PositionOnSpecificPath(Mathf.Clamp(dist + 1, 0, newPathLength),0,currentPath);

                    if (counter == 0)
                        Gizmos.color = Color.green;

                    var scaledValue =
                      speedRatioDependingGripList[currentPath].speedGridList[currentGizmoSurface].speedRatio[
                          Mathf.Clamp(counter, 0, speedRatioDependingGripList[currentPath].speedGridList[currentGizmoSurface].speedRatio.Count - 1)];

                    Gizmos.color = trackDifficulty.Evaluate(scaledValue);
                    
                    if(TrackIsLooped)
                        Gizmos.DrawSphere(next, 1f);
                    else if(dist <= allPathList[currentPath].distanceFromPathList[allPathList[currentPath].distanceFromPathList.Count-2])
                            Gizmos.DrawSphere(next, 1f);


                    counter++;
                }
            }
            #endregion
        }

        public IEnumerator InitRoutine()
        {
            #region
            IsInitDone = false;
           
            yield return new WaitUntil(() => CreateAllTrackPathList());

            GeneratePathsInfo();

            IsInitDone = true;
            yield return null;
            #endregion
        }

        bool CreateAllTrackPathList()
        {
            #region
            allPathList.Clear();

            // Default path
            allPathList.Add(new TrackPathList());
            for (var i = 0; i < checkpoints.Count; i++)
            {
                allPathList[0].spotList.Add(checkpoints[i]);
                allPathList[0].spotpositionList.Add(checkpoints[i].position);
                allPathList[0].checkpointId = 0;
                allPathList[0].altPathId = 0;
            }
            
            allPathList[0].spotpositionList.Add(checkpoints[0].position);

            // Prevent bug with the default path (Main path). Prevent bug when the vehicle go back to the main path.
            if (allPathList[0].indexOfLastAltPathSpotInList == 0)
                allPathList[0].indexOfLastAltPathSpotInList = allPathList[0].spotpositionList.Count - 1;


            // Alt paths
            for (var j = 0; j < checkpoints.Count; j++)
            {
                if (IsAltPathEnabled(checkpoints[j].transform))
                {
                   
                    Transform refTrans = checkpoints[j].transform;
                    TriggerAltPath triggerAltPath = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>();
                    int HowManyAltPath = triggerAltPath.AltPathList.Count;

                  


                      //  Debug.Log(j + " -> HowManyAltPath: " + HowManyAltPath);
                   
                    for (var l = 0; l < HowManyAltPath; l++)
                    {
                        allPathList.Add(new TrackPathList());
                   
                        allPathList[allPathList.Count-1].checkpointId = j;
                        allPathList[allPathList.Count - 1].altPathId = l+1;

                        List<int> mainPathCheckpointToRemove = CheckpointBetweenAltPathStartAndEnd(triggerAltPath, l);

                        for (var i = 0; i < checkpoints.Count; i++)
                        {
                            if (AddThisCheckpointAllowed(i, mainPathCheckpointToRemove))
                            {
                                allPathList[allPathList.Count - 1].spotList.Add(checkpoints[i]);
                                allPathList[allPathList.Count - 1].spotpositionList.Add(checkpoints[i].position);
                            }
                          
                            if (i == j)
                            {
                                int howManySubPoints = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList[l].tmpCheckpoints.Count;
                                for (var k = 0; k < howManySubPoints; k++)
                                {
                                    Transform AltPath = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList[l].tmpCheckpoints[k].transform;
                                    allPathList[allPathList.Count - 1].spotList.Add(AltPath);
                                    allPathList[allPathList.Count - 1].spotpositionList.Add(refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList[l].tmpCheckpoints[k].position);
                                }
                                allPathList[allPathList.Count - 1].indexOfLastAltPathSpotInList = i + howManySubPoints + 1;
                            }
                        }
                       
                        allPathList[allPathList.Count - 1].spotpositionList.Add(checkpoints[0].position);

                       

                    }
                }
            }

            for (var i = 0; i < allPathList.Count; i++)
                allPathList[i].distanceFromPathList = CreateListDistanceFromPath(allPathList[i].spotList);


            for (var i = 0; i < allPathList.Count; i++)
                allPathList[i].pathLength = ReturnPathLength(i);

            return true;
            #endregion
        }

        bool IsAltPathEnabled(Transform checkpoint)
        {
            #region
            GameObject AltPath = checkpoint.GetChild(0).GetChild(0).gameObject;

            if (AltPath.activeSelf)
                return true;

            return false;
            #endregion
        }

        public List<float> CreateListDistanceFromPath(List<Transform> spot)
        {
            #region
            // Clear list of checkpoints positions and distance from the start of the path
            List<Vector3> spotsPosition = new List<Vector3>();
            List<float> distanceFromPathStartList = new List<float>();

            float distanceFromStart = 0;
            for (int i = 0; i < spot.Count + 1; ++i)
            {
                if (spot[i % spot.Count] && spot[(i + 1) % spot.Count])
                {
                    Vector3 checkpoint1 = spot[i % spot.Count].position;
                    Vector3 checkpoint2 = spot[(i + 1) % spot.Count].position;
                    // Save the position of each checkpoint
                    spotsPosition.Add(spot[i % spot.Count].position);
                    // Save the distance from the start for each checkpoint
                    distanceFromPathStartList.Add(distanceFromStart);
                    Vector3 diff = checkpoint1 - checkpoint2;
                    distanceFromStart += diff.magnitude;
                }
            }
            return distanceFromPathStartList;
            #endregion
        }

        public float ReturnPathLength(int index)
        {
            #region
            return allPathList[index].distanceFromPathList[allPathList[index].distanceFromPathList.Count - 1];
            #endregion
        }

        List<int> CheckpointBetweenAltPathStartAndEnd(TriggerAltPath triggerAltPath, int altPathID)
        {
            #region 
            List<int> mainPathCheckpointToRemove = new List<int>();
            Transform objAltPathStart = triggerAltPath.AltPathList[altPathID].checkpointStart;
            Transform objAltPathEnd = triggerAltPath.AltPathList[altPathID].checkpointEnd;

            int startAltPathCheckpointID = 0;
            int endAltPathCheckpointID = 0;

            for (var m = 0; m < checkpoints.Count; m++)
            {
                if (objAltPathStart == checkpoints[m])
                {
                    startAltPathCheckpointID = m;
                    break;
                }
            }

            for (var m = 0; m < checkpoints.Count; m++)
            {
                if (objAltPathEnd == checkpoints[m])
                {
                    endAltPathCheckpointID = m;
                    break;
                }
            }


            for (var m = 0; m < checkpoints.Count; m++)
            {
                if (m > startAltPathCheckpointID && m < endAltPathCheckpointID)
                {
                    mainPathCheckpointToRemove.Add(m);
                    // Debug.Log("m: " + m);
                }

            }

            return mainPathCheckpointToRemove; 
            #endregion
        }

        bool AddThisCheckpointAllowed(int spotIDToCheck, List<int> mainPathCheckpointToRemove)
        {
            #region
            for(var i = 0;i< mainPathCheckpointToRemove.Count;i++)
            {
                if (mainPathCheckpointToRemove[i] == spotIDToCheck)
                    return false;
            }

            return true;
            #endregion
        }

       /* public IEnumerator WaitBeforeTheEndOfThePath()
        {
            #region
            float t = 0;
            float duration = .5f;
            while (t < duration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime;
                }
                yield return null;
            }
            isModifiedPathDone = true;



            yield return null;
            #endregion
        }*/
    }
}
   

