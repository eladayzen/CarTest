// Description: DetectCarAhead. Attached to DetectCarAhead_Use and DetectCarForAltPath_Use
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DetectCarAhead : MonoBehaviour
    {
        Collider                    m_Collider;
        public VehiclePathFollow    vehiclePath;

        public bool                 isCarAhead = false;

        public int                  cases = 0;

        public LayerMask            vehicleMask;
        public List<int>            vehicleLayerMaskList = new List<int>();   // 9 Vehicle
        public int                  layerRefVehicle = 9;


        void Start()
        {
            #region 
            vehicleMask = InitLayerMask(vehicleLayerMaskList);

            layerRefVehicle = LayersRef.instance.layersListData.listLayerInfo[layerRefVehicle].layerID;

            m_Collider = GetComponent<BoxCollider>(); 
            #endregion
        }

        public bool IsCollisionDetected()
        {
            #region 
            Collider[] allColliders = Physics.OverlapBox(m_Collider.bounds.center, m_Collider.transform.localScale * .5f, transform.rotation, vehicleMask);
            isCarAhead = false;
            if (allColliders != null)
            {
                foreach (Collider col in allColliders)
                {
                    //  Debug.Log("layer : " + col.gameObject.layer);
                    if (col.gameObject.layer == layerRefVehicle && col.GetComponent<VehicleTriggerTag>())
                    {
                        // Null guard: on scene load the other car's VehiclePathFollow.Track is
                        // null until its own init coroutine finishes - dereferencing unguarded
                        // here threw NullReference when called during that window.
                        VehiclePathFollow otherPathFollow = col.transform.parent.parent.parent.GetComponent<VehiclePathFollow>();
                        if (otherPathFollow == null || otherPathFollow.Track == null || vehiclePath.Track == null)
                            continue;
                        int otherCarPathFollow = otherPathFollow.Track.selectedId;
                        if (cases == 0 &&
                            (otherCarPathFollow != vehiclePath.Track.selectedId))
                        {
                            return true;
                            //  Debug.Log("Name: " + col.transform.parent.parent.name);
                        }
                        else if (cases == 1 && col.transform.parent.parent.parent.gameObject != vehiclePath.gameObject)
                        {
                            return true;
                            //  Debug.Log("Name: " + col.transform.parent.parent.name);
                        }
                    }
                }
            }

            return false; 
            #endregion
        }

        void OnDrawGizmos()
        {
            #region 
            Gizmos.color = Color.blue;

            if (m_Collider)
            {
                Matrix4x4 cubeTransform = Matrix4x4.TRS(m_Collider.transform.position, m_Collider.transform.rotation, m_Collider.transform.localScale);
                Gizmos.matrix = cubeTransform;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            } 
            #endregion
        }

        LayerMask InitLayerMask(List<int> layerList)
        {
            #region //-> Init LayerMask
            string[] layerUsed = new string[layerList.Count];
            for (var i = 0; i < layerList.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[layerList[i]].layerID);

            return LayerMask.GetMask(layerUsed);
            #endregion
        }
    }

}
