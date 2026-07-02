// Description : Work in association with OrbitalCam.cs.
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
//using System.Collections.Generic;

namespace TS.Generics
{
    [CustomEditor(typeof(OrbitalCam))]
    public class OrbitalCamEditor : Editor
    {
        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            #region 
            DrawDefaultInspector();
            serializedObject.Update();

            NewCamPosition();
            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        void NewCamPosition()
        {
            #region 
            OrbitalCam orbitalCam = (OrbitalCam)target;

            if (orbitalCam.PivotCam && orbitalCam.Cam)
            {
                //Vector3 inspectorRotation = TransformUtils.GetInspectorRotation(orbitalCam.PivotCam);

                EditorGUILayout.LabelField("");
                /* EditorGUILayout.LabelField(
                     "(Ref: New Cam Position " +
                     "X: " + Mathf.Round(orbitalCam.PivotCam.eulerAngles.x * 10) * .1f +
                     " | Y: " + Mathf.Round(orbitalCam.PivotCam.eulerAngles.y * 10) * .1f +
                     " | Z: " + Mathf.Round(orbitalCam.Cam.localPosition.z * 10) * -.1f + ")");
                */


                EditorGUILayout.LabelField(
                   "(Ref: New Cam Position " +
                   "X: " + Mathf.Round(orbitalCam.currentXRot * 10) * .1f +
                   " | Y: " + Mathf.Round(orbitalCam.currentYRot * 10) * .1f +
                   " | Z: " + Mathf.Round(orbitalCam.Cam.localPosition.z * 10) * -.1f + ")");
            }
            #endregion
        }

        void OnSceneGUI()
        {

        }
    }
}


#endif