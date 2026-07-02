// Description: ManualPathLimit. Generate Track limit manually
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class ManualPathLimit : MonoBehaviour
    {
        //public bool isInitDone = false;
        // public List<Vector3> spotPosList = new List<Vector3>();
        //[HideInInspector]
        public List<SpotInfoLimit> spotInfoLimitList = new List<SpotInfoLimit>();


        [HideInInspector]
        public float groundOffset = .1f;

        public float extraGround = 2;
        public float wallHeight = 10;

        public GameObject objMesh;
        [HideInInspector]
        public Vector3[] vertices;


        public int currentSelectedSpot = -1;

        //public Mesh mesh;

        //public MeshCollider meshCollider;

        void OnDrawGizmosSelected()
        {
            #region
            if (spotInfoLimitList.Count > 1)
            {
                Gizmos.color = Color.blue;
                Vector3 pos01 = spotInfoLimitList[spotInfoLimitList.Count - 1].posGround + transform.position;
                Vector3 pos02 = spotInfoLimitList[spotInfoLimitList.Count - 1].posUp + transform.position;
                Gizmos.DrawLine(pos01, pos02);

                for (var i = 0; i < spotInfoLimitList.Count - 1; i++)
                {
                    Gizmos.color = Color.blue;
                    pos01 = spotInfoLimitList[i].posGround + transform.position;
                    pos02 = spotInfoLimitList[i + 1].posGround + transform.position;
                    Gizmos.DrawLine(pos01, pos02);

                    pos01 = spotInfoLimitList[i].posGround + transform.position;
                    pos02 = spotInfoLimitList[i].posUp + transform.position;
                    Gizmos.DrawLine(pos01, pos02);

                    pos01 = spotInfoLimitList[i].posUp + transform.position;
                    pos02 = spotInfoLimitList[i+1].posUp + transform.position;
                    Gizmos.DrawLine(pos01, pos02);


                    if (i % 2 == 0 && i < spotInfoLimitList.Count - 1)
                    {
                        Gizmos.color = Color.red;
                        pos01 = spotInfoLimitList[i].posUp - Vector3.up + transform.position;
                        pos02 = spotInfoLimitList[i].posUp + transform.position;
                        var pos03 = spotInfoLimitList[i+1].posUp + transform.position;
                        Vector3 dir = (pos02 - pos01).normalized;
                        Vector3 dir2 = (pos03 - pos02).normalized;
                        Vector3 left = Vector3.Cross(dir, dir2).normalized;
                        Gizmos.DrawLine(pos02, pos02 + left * 4f);
                    }

                }
            }
            #endregion
        }
    }

    [System.Serializable]
    public class SpotInfoLimit
    {
        public Vector3 posGround;
        public Vector3 posUp;
        //public Vector3 dir;

        public SpotInfoLimit(Vector3 _posGround/*, Vector3 _dir*/,Vector3 _posUp)
        {
            posGround = _posGround;
            //dir = _dir;
            posUp = _posUp;
        }
    }

}
