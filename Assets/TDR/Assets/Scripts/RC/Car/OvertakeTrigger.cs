// Description: OvertakeTrigger. Attached to Overtake prefab. 
// Give info to the AI about avertaking. Is the AI allowed to overtake on the or left side of the road
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class OvertakeTrigger : MonoBehaviour
    {
        public Overtake allowOvertake = Overtake.None;

        void OnTriggerEnter(Collider other)
        {
            #region
            if (other.GetComponent<VehicleTriggerTag>())
            {
                CarAI carAI = other.transform.parent.parent.parent.GetComponent<CarAI>();
                carAI.overtakeZone = allowOvertake;
            } 
            #endregion
        }

        void OnDrawGizmos()
        {
            #region
            Gizmos.color = Color.white;

            Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);      // Allow the gizmo to fit the position, rotation and scale of the gameObject

            Gizmos.matrix = cubeTransform;

            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            if (allowOvertake == Overtake.Left)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(new Vector3(0, 0, -.25f), new Vector3(1, 1, .5f));
            }
            if (allowOvertake == Overtake.Right)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(new Vector3(0, 0, .25f), new Vector3(1, 1, .5f));
            }

            Gizmos.color = Color.white;
            Gizmos.DrawCube(new Vector3(-1.5f, 0, 0), new Vector3(1.5f, .1f, .1f));
            Gizmos.DrawWireCube(new Vector3(-1.5f, 0, 0), new Vector3(1.5f, .1f, .1f)); 
            #endregion
        }
    }
    public enum Overtake { Left, Right, None };
}
