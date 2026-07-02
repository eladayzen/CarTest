// Description: Cam2DBehindEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;

namespace TS.Generics
{
    [CustomEditor(typeof(Cam2DBehind))]
    public class Cam2DBehindEditor : Editor
    {
        SerializedProperty m_seeInspector;                                            // use to draw default Inspector

        SerializedProperty m_camSpeed;
        SerializedProperty m_camRotSpeed;

        SerializedProperty m_objFollowingTargetSpeed;
        SerializedProperty m_objFollowingTargetRotSpeed;
        SerializedProperty m_targetlocalPosition;
        SerializedProperty m_camHeight;
        SerializedProperty m_PlayerID;
        
        SerializedProperty m_editFOV;
        SerializedProperty m_camRef;

        SerializedObject serializedObjectCam;
        SerializedProperty m_cameraOrthoSize;


        void OnEnable()
        {
            #region
            m_seeInspector = serializedObject.FindProperty("seeInspector");

            m_camSpeed = serializedObject.FindProperty("camSpeed");
            m_camRotSpeed = serializedObject.FindProperty("camRotSpeed");
            m_objFollowingTargetSpeed = serializedObject.FindProperty("objFollowingTargetSpeed");
            m_objFollowingTargetRotSpeed = serializedObject.FindProperty("objFollowingTargetRotSpeed");
            m_targetlocalPosition = serializedObject.FindProperty("targetlocalPosition");
            m_camHeight = serializedObject.FindProperty("camHeight");

            m_editFOV = serializedObject.FindProperty("editFOV");

            m_PlayerID = serializedObject.FindProperty("PlayerID");

            m_camRef = serializedObject.FindProperty("camRef");

           // Cam2DBehind myScript = (Cam2DBehind)target;
            //Camera refCam = myScript.transform.GetChild(0).GetChild(0).GetComponent<Camera>();
            serializedObjectCam = new UnityEditor.SerializedObject((Camera)m_camRef.objectReferenceValue);
            serializedObjectCam.Update();
            m_cameraOrthoSize = serializedObjectCam.FindProperty("orthographic size");

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
            EditorGUILayout.LabelField(new GUIContent("Camera Speed", "The speed used by the camera to follow the object that follow the car"), GUILayout.Width(140));
            m_camSpeed.floatValue = EditorGUILayout.Slider(m_camSpeed.floatValue, -10, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Height", "Cam height From Car"), GUILayout.Width(140));
            m_camHeight.floatValue = EditorGUILayout.Slider(m_camHeight.floatValue, 0, 300);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Cam Rotation Speed:", "The camera rotation speed."), GUILayout.Width(140));
            m_camRotSpeed.floatValue = EditorGUILayout.Slider(m_camRotSpeed.floatValue, 0, 50);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Obj That Follow Car| Speed:", "The speed used by the camera to follow the car"), GUILayout.Width(140));
            m_objFollowingTargetSpeed.floatValue = EditorGUILayout.Slider(m_objFollowingTargetSpeed.floatValue, 0, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Obj That Follow Car| Rotation Speed:", "The camera rotation speed."), GUILayout.Width(140));
            m_objFollowingTargetRotSpeed.floatValue = EditorGUILayout.Slider(m_objFollowingTargetRotSpeed.floatValue, 0, 30);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Target Local Position:", "The camera look at this position. This position is a local position"), GUILayout.Width(140));
            EditorGUILayout.PropertyField(m_targetlocalPosition, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            #endregion
        }

        void DisplayCamParameters()
        {
            #region 
            serializedObjectCam.Update();

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

      

        void DisplayPlayerTwoCustomEditor()
        {
            EditorGUILayout.HelpBox(" Camera modifications are made on Player 1's camera. " +
                "For more information on modifying the camera, see the documentation", MessageType.Info);

        }
    }
}


#endif
