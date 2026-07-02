// Description: PathLaneGlowRenderer. Renders two glowing lines along the track path, offset
// left/right from centerline by laneHalfWidth, marking the border of the playable lane area for
// the Path-Follow Control Assist. Scene-level, per-track object - does not live on CarRef.prefab.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class PathLaneGlowRenderer : MonoBehaviour
    {
        [Header("Lane Edges")]
        // Distance from centerline for both lines - the border of the playable area. Kept
        // independent of any specific car's CarPathFollowPlayerInput.maxLateralOffset; keep them
        // in sync manually if that value changes.
        public float                 laneHalfWidth  = 4f;
        public float                 sampleSpacing  = 4f;    // distance between sample points along the path
        public float                 heightOffset   = 0.1f;  // lifts the line above the road surface to avoid z-fighting
        public float                 lineWidth      = 0.3f;
        public Material              glowMaterial;

        void Start()
        {
            #region
            StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            yield return new WaitUntil(() => PathRef.instance != null && PathRef.instance.Track != null);

            Path track = PathRef.instance.Track;

            List<Vector3> leftPoints = new List<Vector3>();
            List<Vector3> rightPoints = new List<Vector3>();

            int sampleCount = Mathf.Max(2, Mathf.CeilToInt(track.pathLength / sampleSpacing));
            for (int i = 0; i <= sampleCount; i++)
            {
                float dist = (track.pathLength * i) / sampleCount;

                Vector3 centerPos = track.TargetPositionOnPath(dist);
                Vector3 tangent = track.TargetRotationOnPath(dist);
                Vector3 left = Vector3.Cross(tangent, Vector3.up).normalized;

                leftPoints.Add(centerPos + left * laneHalfWidth + Vector3.up * heightOffset);
                rightPoints.Add(centerPos - left * laneHalfWidth + Vector3.up * heightOffset);
            }

            CreateEdgeLine("LaneEdgeLeft", leftPoints, track.TrackIsLooped);
            CreateEdgeLine("LaneEdgeRight", rightPoints, track.TrackIsLooped);
            #endregion
        }

        void CreateEdgeLine(string edgeName, List<Vector3> points, bool loop)
        {
            #region
            GameObject edgeObj = new GameObject(edgeName);
            edgeObj.transform.SetParent(transform, false);

            LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = loop;
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            if (glowMaterial) lr.material = glowMaterial;
            #endregion
        }
    }
}
