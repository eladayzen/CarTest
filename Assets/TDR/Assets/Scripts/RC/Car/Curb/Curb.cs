using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class Curb : MonoBehaviour
    {
        public bool SeeInspector;
        public bool moreOptions;
        public bool helpBox = true;

        public Bezier roadRefForCurb;

        public int selectStart = 0;
        public int selectStop = 0;

        public int accuracy = 5;
       

        public enum Direction { Left,Right};
        public Direction direction;

        public float gizmoSecondLineDistance = 1.5f;


        public float curbOffsetRight = 1;


        void OnDrawGizmosSelected()
        {
            #region
            if(roadRefForCurb)
                DisplayCurbZone();
            #endregion
        }

        void DisplayCurbZone()
        {
            #region
            var start = selectStart;
            start = Mathf.Clamp(start, 0, selectStop);

            var stop = selectStop;
            stop = Mathf.Clamp(stop, selectStart, roadRefForCurb.distVecList.Count - accuracy);


            for (var i = start; i < stop; i++)
            {
                if (i % accuracy == 0)
                {

                    switch (direction)
                    {
                        case Direction.Left:
                            DisplayBorder(i,-1);
                                break;

                        case Direction.Right:
                            DisplayBorder(i, 1);
                            break;
                    }
                }
            }
            #endregion
        }

        void DisplayBorder(int index,int direction)
        {
            var multiplier = direction;

            var point01 = roadRefForCurb.distVecList[index].spotPos;

            var dir01 = (roadRefForCurb.distVecList[index].spotPos - roadRefForCurb.distVecList[index + 1].spotPos).normalized;
            var left01 = multiplier * roadRefForCurb.roadSize * .5f * Vector3.Cross(dir01, Vector3.up).normalized;

            var point02 = roadRefForCurb.distVecList[index + accuracy].spotPos;

            var dir02 = (roadRefForCurb.distVecList[index + accuracy].spotPos - roadRefForCurb.distVecList[index + accuracy + 1].spotPos).normalized;
            var left02 = multiplier * roadRefForCurb.roadSize * .5f * Vector3.Cross(dir02, Vector3.up).normalized;

            //curb.GetComponent<Bezier>().pointsList.Add(new PointDescription(bezier.distVecList[i].spotPos + left, Quaternion.identity));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(point01 + left01 + transform.position, point02 + left02 + transform.position);
            Gizmos.DrawLine(point01 + left01 * gizmoSecondLineDistance + transform.position, point02 + left02 * gizmoSecondLineDistance + transform.position);
        }
    }

}
