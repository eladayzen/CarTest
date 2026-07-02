// Description: CarSetup. Attached to the vehicle. Use to setup the vehicle parameters.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarSetup : MonoBehaviour
    {
        // Custom Editor variables
        public List<GameObject>             modelsRefList = new List<GameObject>();
        [HideInInspector]
        public bool                         tab;
        [HideInInspector]
        public bool                         SeeInspector;
        [HideInInspector]
        public bool                         moreOptions;
        [HideInInspector]
        public bool                         helpBox = true;


        // Vehicle Performance
        public Transform                    refSize;

        public Transform                    mainCollider;
        public Transform                    carBody;

        public bool                         isGizmosEnable = true;
        public float                        gizmosRadius = .1f;

        public bool                         isCarSetupEnabled = true;

        [System.Serializable]
        public class ObjInfo
        {
            public Transform    refTransform;
            public Vector3      refPos;
            public Color        gizmoColor = Color.white;
        }
        public List<ObjInfo>                objList = new List<ObjInfo>();

        public List<ObjInfo>                wheelsList = new List<ObjInfo>();
        public List<Transform>              objWheelModelList = new List<Transform>();
        public List<Transform>              rbSweepTestList = new List<Transform>();

        [System.Serializable]
        public class ObjColliderInfo
        {
            public Transform    refTransform;
            public bool         editPosY = true;
            public Vector3      refScale;
            public bool         editScaleX = true;
            public bool         editScaleZ = true;
        }
        public List<ObjColliderInfo>        objColliderList = new List<ObjColliderInfo>();

        public CarController                carController;

        public float                        frontWheelDistanceZ = 1.24f;
        public float                        frontWheelDistanceX = .67f;

        public float                        rearWheelDistanceZ = 1.24f;
        public float                        rearWheelDistanceX = .67f;

        [System.Serializable]
        public class DetectorScaleInfo
        {
            public float refDistance;
            public float scaleFrontBack;
            public float scaleLeftRight;

            public DetectorScaleInfo(float _refDistance, float _scaleFrontBack,float _scaleLeftRight)
            {
                refDistance = _refDistance;
                scaleFrontBack = _scaleFrontBack;
                scaleLeftRight = _scaleLeftRight;
            }
        }
        public List<DetectorScaleInfo> detectorScaleList = new List<DetectorScaleInfo>();


        public bool useNewVersion = false;

        void OnDrawGizmos()
        {
            #region
            for (var i = 0; i < objList.Count; i++)
            {
                if (objList[i].refTransform && isGizmosEnable)
                {
                    Gizmos.color = objList[i].gizmoColor;
                    Gizmos.DrawSphere(objList[i].refTransform.position, gizmosRadius);
                }
            } 
            #endregion
        }
    }
}
