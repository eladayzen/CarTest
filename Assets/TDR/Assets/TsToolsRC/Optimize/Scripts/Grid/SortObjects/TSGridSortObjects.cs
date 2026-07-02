// Description: TSGridSortObjects. "This object is used to sort objects in a grid according to their positions in the scene."
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class TSGridSortObjects : MonoBehaviour
    {
        public bool             isInitDone = false;

        public bool             seeInspector = false;
        public bool             howItWorks = false;

        public int              terrainX = 2100;
        public int              terrainZ = 2100;

        public int              row = 5;
        public int              column = 10;

        public Transform        masterFolder;
        public Transform        sortFolder;

        public List<Transform>  zonesList = new List<Transform>();

        public float            gizmoSphereSize = 10;
        public bool             showGizmo = true;

        [System.Serializable]
        public class gridSectionParams
        {
            public GameObject       colRow;
            public List<GameObject> objsList = new List<GameObject>();
        }

        public List<gridSectionParams> zonesInfoList = new List<gridSectionParams>();

   
        void OnDrawGizmosSelected()
        {
            #region
            if (showGizmo)
                DisplayTheGrid();
            #endregion
        }

        void DisplayTheGrid()
        {
            #region
            for (var i = 0; i <= column; i++)
            {
                for (var j = 0; j <= row; j++)
                {
                    float ZPos = terrainZ / column * i;
                    float XPos = terrainX / row * j;

                    Vector3 point = new Vector3(XPos, 0, ZPos);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(point, gizmoSphereSize);
                }
            }
            #endregion
        }


    }


}
