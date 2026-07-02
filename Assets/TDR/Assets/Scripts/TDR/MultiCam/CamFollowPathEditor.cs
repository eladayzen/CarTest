// Description: CamFollowPathEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;

namespace TS.Generics
{
    [CustomEditor(typeof(CamFollowPath))]
    public class CamFollowPathEditor : Editor
    {
        SerializedProperty m_seeInspector;                                            // use to draw default Inspector


        SerializedProperty m_height;

        SerializedProperty m_heightDamping;

        SerializedProperty m_rotSpeed;
        SerializedProperty m_rotationDamping;

        SerializedProperty m_camSpeed;
        SerializedProperty m_distance;

        SerializedProperty m_lookAtTargetlocalPosition;

        SerializedProperty m_PlayerID;

        SerializedObject   serializedObjectCam;
        SerializedProperty m_cameraFocalLength;
        SerializedProperty m_cameraFOV;

        SerializedProperty m_editFOV;


        void OnEnable()
        {
            #region
            m_seeInspector = serializedObject.FindProperty("seeInspector");

            m_distance = serializedObject.FindProperty("distance");
            m_height = serializedObject.FindProperty("height");

            m_heightDamping = serializedObject.FindProperty("heightDamping");
            m_rotSpeed = serializedObject.FindProperty("rotSpeed");
            m_rotationDamping = serializedObject.FindProperty("rotationDamping");
            m_camSpeed = serializedObject.FindProperty("camSpeed");
            m_lookAtTargetlocalPosition = serializedObject.FindProperty("lookAtTargetlocalPosition");

            m_editFOV = serializedObject.FindProperty("editFOV");

            m_PlayerID = serializedObject.FindProperty("PlayerID");

            CamFollowPath myScript = (CamFollowPath)target;
            Camera refCam = myScript.transform.GetChild(0).GetChild(0).GetComponent<Camera>();
            serializedObjectCam = new UnityEditor.SerializedObject(refCam);
            serializedObjectCam.Update();
            m_cameraFocalLength = serializedObjectCam.FindProperty("m_FocalLength");
            m_cameraFOV = serializedObjectCam.FindProperty("field of view");

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
            EditorGUILayout.LabelField(new GUIContent("Height", "Cam height From Car"), GUILayout.Width(140));
            m_height.floatValue = EditorGUILayout.Slider(m_height.floatValue, 0, 150);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Rotation Speed", "Camera rotation speed"), GUILayout.Width(140));
            m_rotSpeed.floatValue = EditorGUILayout.Slider(m_rotSpeed.floatValue, 0, 50);
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Rotation Damping:", "Cam Rotation Damping"), GUILayout.Width(140));
            m_rotationDamping.floatValue = EditorGUILayout.Slider(m_rotationDamping.floatValue, 0, 20);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Camera speed:", "The speed used by the camera to follow path target"), GUILayout.Width(140));
            m_camSpeed.floatValue = EditorGUILayout.Slider(m_camSpeed.floatValue, 0, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Height Damping", "Cam height damping"), GUILayout.Width(140));
            m_heightDamping.floatValue = EditorGUILayout.Slider(m_heightDamping.floatValue, 0, 20);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Distance", "Cam Distance From target on path"), GUILayout.Width(140));
            m_distance.floatValue = EditorGUILayout.Slider(m_distance.floatValue, -10, 100);
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

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Camera Focal Length:", "Used to modify the camera field of view"), GUILayout.Width(140));
            m_editFOV.floatValue = EditorGUILayout.Slider(m_editFOV.floatValue, .1f, 50);


            string currentFOV = m_cameraFOV.floatValue.ToString("F1");
            EditorGUILayout.LabelField("FOV: " + currentFOV, GUILayout.Width(65));

            EditorGUILayout.EndHorizontal();


            if (EditorGUI.EndChangeCheck())
            {
                m_cameraFocalLength.floatValue = m_editFOV.floatValue;
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
