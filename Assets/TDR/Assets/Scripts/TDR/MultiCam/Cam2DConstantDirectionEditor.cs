// Description: Cam2DConstantDirectionEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;

namespace TS.Generics
{
    [CustomEditor(typeof(Cam2DConstantDirection))]
    public class Cam2DConstantDirectionEditor : Editor
    {
        SerializedProperty m_seeInspector;                                            // use to draw default Inspector

        SerializedProperty m_camSpeed;
        SerializedProperty m_objLookAtConstantDirectionSpeed;
        SerializedProperty m_camHeight;
        SerializedProperty m_lookAtTargetlocalPosition;

        SerializedProperty m_PlayerID;

        SerializedProperty m_editFOV;
        SerializedProperty m_editYConstantDirection;

        SerializedObject serializedObjectCam;
        SerializedProperty m_cameraOrthoSize;


        SerializedObject serializedObjectSphereConstantDir;
        SerializedProperty m_localRotation;

        void OnEnable()
        {
            #region
            m_seeInspector = serializedObject.FindProperty("seeInspector");



            m_camSpeed = serializedObject.FindProperty("camSpeed");
            m_objLookAtConstantDirectionSpeed = serializedObject.FindProperty("objLookAtConstantDirectionSpeed");
            m_camHeight = serializedObject.FindProperty("camHeight");
            m_lookAtTargetlocalPosition = serializedObject.FindProperty("lookAtTargetlocalPosition");

            m_editFOV = serializedObject.FindProperty("editFOV");
            m_editYConstantDirection = serializedObject.FindProperty("editYConstantDirection");

            m_PlayerID = serializedObject.FindProperty("PlayerID");


            Cam2DConstantDirection myScript = (Cam2DConstantDirection)target;
            Camera refCam = myScript.transform.GetChild(0).GetChild(0).GetComponent<Camera>();
            serializedObjectCam = new UnityEditor.SerializedObject(refCam);
            serializedObjectCam.Update();
            m_cameraOrthoSize = serializedObjectCam.FindProperty("orthographic size");

            Transform refShpere = myScript.objLookAtConstantDirection.transform;
            serializedObjectSphereConstantDir = new UnityEditor.SerializedObject(refShpere);
            m_localRotation = serializedObjectSphereConstantDir.FindProperty("m_LocalRotation");

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            #endregion
        }



        public override void OnInspectorGUI()
        {
            #region
            if (m_seeInspector.boolValue)
                DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("See Inspector:", GUILayout.Width(140));
            EditorGUILayout.PropertyField(m_seeInspector, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();


            if (m_PlayerID.intValue == 0)
            {
                DisplayGlobalParms();

                EditorGUILayout.LabelField("");
                DisplayCamParameters();

                EditorGUILayout.LabelField("");

                DisplayObjLookAtConstantDirectionParameters();
                EditorGUILayout.LabelField("");
            }
            else
            {
                DisplayPlayerTwoCustomEditor();
            }



            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        void DisplayGlobalParms()
        {
            #region
            EditorGUILayout.HelpBox("Process to edit the camera:" +
                   "\n" +
                   "- Start PLAY MODE." + "\n" +
                   "- Modify parameters." + "\n" +
                   "- Press the vertical 3 dots on the top right of the script." + "\n" +
                   "- Choose Copy Component." + "\n" +
                   "- Quit PLAY MODE." + "\n" +
                   "- Press again the vertical 3 dots on the top right of the script." + "\n" +
                   "- Choose Paste Component Values.", MessageType.Info);

            EditorGUILayout.LabelField("");
          
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Camera Speed", "The speed used by the camera to follow the car"), GUILayout.Width(140));
            m_camSpeed.floatValue = EditorGUILayout.Slider(m_camSpeed.floatValue, -10, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Height", "Cam height From Car"), GUILayout.Width(140));
            m_camHeight.floatValue = EditorGUILayout.Slider(m_camHeight.floatValue, 0, 300);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Obj Constant Dir Speed:", "The speed used by the objLookAtConstantDirectionSpeed to follow the car"), GUILayout.Width(140));
            m_objLookAtConstantDirectionSpeed.floatValue = EditorGUILayout.Slider(m_objLookAtConstantDirectionSpeed.floatValue, 0, 150);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Target Local Position:", "The camera look at this position. This position is a local position"), GUILayout.Width(140));
            EditorGUILayout.PropertyField(m_lookAtTargetlocalPosition, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            #endregion
        }

        void DisplayCamParameters()
        {
            #region 
            serializedObjectCam.Update();
            /*
            m_editFOV = serializedObject.FindProperty("editFOV");
            m_editYConstantDirection = serializedObject.FindProperty("editYConstantDirection");
            */
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Camera Ortho Size:", "Used to modify the camera orthographic size"), GUILayout.Width(140));
            m_editFOV.floatValue = EditorGUILayout.Slider(m_editFOV.floatValue, .1f, 100);

            EditorGUILayout.EndHorizontal();


            if (EditorGUI.EndChangeCheck())
            {
                m_cameraOrthoSize.floatValue = m_editFOV.floatValue;
                serializedObjectCam.ApplyModifiedProperties();
            }



            #endregion
        }

        void DisplayObjLookAtConstantDirectionParameters()
        {
            #region 
            serializedObjectCam.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Cam Y Direction:", "Correspond to the direction the camera look at."), GUILayout.Width(140));
            m_editYConstantDirection.floatValue = EditorGUILayout.Slider(m_editYConstantDirection.floatValue, 0, 359);

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {

                serializedObjectSphereConstantDir.Update();
                Vector3 eulerAngle = m_localRotation.quaternionValue.eulerAngles;
                eulerAngle = new Vector3(eulerAngle.x, m_editYConstantDirection.floatValue, eulerAngle.z);
                m_localRotation.quaternionValue = Quaternion.Euler(eulerAngle);
                serializedObjectSphereConstantDir.ApplyModifiedProperties();
            }
            #endregion
        }

        void DisplayPlayerTwoCustomEditor()
        {
            EditorGUILayout.HelpBox(" Camera modifications are made on Player 1's camera. " +
                "For more information on modifying the camera, see the documentation", MessageType.Info);

        }
    }
}


#endif
