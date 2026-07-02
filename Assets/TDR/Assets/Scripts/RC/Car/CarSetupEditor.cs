// Description: CarSetupEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    [CustomEditor(typeof(CarSetup))]
    public class CarSetupEditor : Editor
    {
        SerializedProperty m_modelsRefList;

        SerializedProperty SeeInspector;                                            // use to draw default Inspector
        SerializedProperty moreOptions;
        SerializedProperty helpBox;
        SerializedProperty m_isCarSetupEnabled;

        SerializedProperty m_CarController;

        SerializedProperty m_frontWheelDistanceZ;
        SerializedProperty m_frontWheelDistanceX;

        SerializedProperty m_rearWheelDistanceZ;
        SerializedProperty m_rearWheelDistanceX;

        Vector3 refSize = Vector3.zero;
        public Vector3 refPos = Vector3.zero;
        float refSuspensionLength = 0;
        float refWheelRadius = 0;

        void OnEnable()
        {
            #region
            // Setup the SerializedProperties.
            m_modelsRefList = serializedObject.FindProperty("modelsRefList");
            SeeInspector = serializedObject.FindProperty("SeeInspector");
            moreOptions = serializedObject.FindProperty("moreOptions");
            helpBox = serializedObject.FindProperty("helpBox");
            m_CarController = serializedObject.FindProperty("carController");
            m_isCarSetupEnabled = serializedObject.FindProperty("isCarSetupEnabled");

            m_frontWheelDistanceZ = serializedObject.FindProperty("frontWheelDistanceZ");
            m_frontWheelDistanceX = serializedObject.FindProperty("frontWheelDistanceX");
            m_rearWheelDistanceZ = serializedObject.FindProperty("rearWheelDistanceZ");
            m_rearWheelDistanceX = serializedObject.FindProperty("rearWheelDistanceX");
            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region
            if (SeeInspector.boolValue)                         // If true Default Inspector is drawn on screen
                DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("See Inspector:", GUILayout.Width(85));
            EditorGUILayout.PropertyField(SeeInspector, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.LabelField("HelpBox:", GUILayout.Width(60));
            EditorGUILayout.PropertyField(helpBox, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.LabelField("CarSetupEnabled:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_isCarSetupEnabled, new GUIContent(""), GUILayout.Width(30));


            if (EditorPrefs.GetBool("MoreOptions") == true)
            {
                EditorGUILayout.LabelField("More Options:", GUILayout.Width(85));
                EditorGUILayout.PropertyField(moreOptions, new GUIContent(""), GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.LabelField("");

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                 "It is possible to setup the car only when the game is not playing.", MessageType.Info);

                EditRuntimeSuspensionLength();
            }
            else
            {
                SerializedProperty m_tab = serializedObject.FindProperty("tab");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vehicle Parts (Easy Access):", GUILayout.Width(170));
                EditorGUILayout.PropertyField(m_tab, new GUIContent(""), GUILayout.Width(30));
                EditorGUILayout.EndHorizontal();

                if (m_tab.boolValue)
                {
                    Section3DModels();
                }

                if (m_isCarSetupEnabled.boolValue)
                {
                    EditorGUILayout.LabelField("");
                    SectionPerformance();
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField("");
            #endregion
        }

        void Section3DModels()
        {
            #region
            for (var i = 0; i < m_modelsRefList.arraySize; i++)
                EditorGUILayout.PropertyField(m_modelsRefList.GetArrayElementAtIndex(i), new GUIContent(""));
            #endregion
        }

        void SectionPerformance()
        {
            #region
            EditorGUILayout.LabelField("|-> CAR BODY <-|", EditorStyles.boldLabel);
            refSize = RefSize();

            refPos = RefPos();

            // Wheels Positions
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("|-> WHEELS <-|", EditorStyles.boldLabel);

            EditWheelPosXZ();
            UpdateWheelsXZPos(refSize, "Front");
            UpdateWheelsXZPos(refSize, "Rear");

            // Car Controller parameters
            EditWheelRadius();
            EditWheelRadiusFront("Front");
            EditWheelRadiusFront("Rear");

            EditWheelWidth();
            EditWheelWidthFront("Front");
            EditWheelWidthFront("Rear");

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("|-> SUSPENSION <-|", EditorStyles.boldLabel);
            EditSuspensionLength();

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("|-> OTHER <-|", EditorStyles.boldLabel);
            MoreParameters();

            // Trackers and Body Positions
            UpdateTrackerAndBodyPos(refSize);
            UpdateColliderListScale(refSize);
            #endregion
        }

        void UpdateSuspension()
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
            SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");

            for (var i = 0; i < m_WheelsList.arraySize; i++)
            {

                SerializedProperty m_SupensionLengthOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("suspensionLength");

                SerializedProperty m_wheelRadius = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelRadius");

                // Update the position of the spring.
                SerializedObject serializedObject5 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i].parent.parent.parent.parent);
                SerializedProperty m_springPosition = serializedObject5.FindProperty("m_LocalPosition");

                serializedObject5.Update();

                float newYPos = m_SupensionLengthOther.floatValue + m_wheelRadius.floatValue;

                m_springPosition.vector3Value = new Vector3(
                    m_springPosition.vector3Value.x,
                    newYPos,
                    m_springPosition.vector3Value.z);

                serializedObject5.ApplyModifiedProperties();


                // Update the position of the car body.
                SerializedObject serializedObject6 = new UnityEditor.SerializedObject(CarSetup.carBody);

                SerializedProperty m_carBodyPosition = serializedObject6.FindProperty("m_LocalPosition");
                serializedObject6.Update();
                m_carBodyPosition.vector3Value = new Vector3(0, m_springPosition.vector3Value.y, 0);
                serializedObject6.ApplyModifiedProperties();


                // Update the position of the car collider group.
                SerializedObject serializedObject7 = new UnityEditor.SerializedObject(CarSetup.mainCollider);

                SerializedProperty m_mainColliderPosition = serializedObject7.FindProperty("m_LocalPosition");

                serializedObject7.Update();

                m_mainColliderPosition.vector3Value = new Vector3(
                    m_mainColliderPosition.vector3Value.x,
                    m_springPosition.vector3Value.y,
                    m_mainColliderPosition.vector3Value.z);

                serializedObject7.ApplyModifiedProperties();

                SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i].parent);
                SerializedProperty m_objWheelModelPosList = serializedObject3.FindProperty("m_LocalPosition");

                serializedObject3.Update();

                m_objWheelModelPosList.vector3Value = new Vector3(
                    m_objWheelModelPosList.vector3Value.x,
                    -m_SupensionLengthOther.floatValue + m_wheelRadius.floatValue,
                    m_objWheelModelPosList.vector3Value.z);

                serializedObject3.ApplyModifiedProperties();
            }
            #endregion
        }

        void EditSuspensionLength()
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");

                SerializedProperty m_SupensionLength = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("suspensionLength");
                SerializedProperty m_SuspensionRuntimeOffset = serializedObject2.FindProperty("suspensionRuntimeOffset");

                EditorGUI.BeginChangeCheck();

                serializedObject2.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Length:", GUILayout.Width(120));
                m_SupensionLength.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_SupensionLength.floatValue, 0, 4);
                EditorGUILayout.EndHorizontal();

                if (helpBox.boolValue) EditorGUILayout.HelpBox(
                  "[Length] change the length of the car suspension", MessageType.Info);


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Runtime offset:", GUILayout.Width(120));
                m_SuspensionRuntimeOffset.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_SuspensionRuntimeOffset.floatValue, -.2f, .2f);
                EditorGUILayout.EndHorizontal();


                if (EditorGUI.EndChangeCheck())
                {
                    for (var i = 0; i < m_WheelsList.arraySize; i++)
                    {
                        SerializedProperty m_SupensionLengthOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("suspensionLength");
                        m_SupensionLengthOther.floatValue = m_SupensionLength.floatValue;
                    }
                }

                refSuspensionLength = m_SupensionLength.floatValue;

                SpringStiffness(serializedObject2);

                serializedObject2.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck())
                    UpdateSuspension();
            }
            #endregion
        }

        void EditWheelRadius()
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");
                SerializedProperty m_wheelRadius = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("wheelRadius");

                serializedObject2.Update();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Wheel Radius:", GUILayout.Width(120));

                m_wheelRadius.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_wheelRadius.floatValue, .1f, 2);
                EditorGUILayout.EndHorizontal();

                if (helpBox.boolValue)
                {
                    //EditorGUILayout.LabelField(""); 
                    EditorGUILayout.HelpBox(
                    "[Wheel Radius] to change all wheels radius at once." + "\n" +
                    "[Front] to change only the front wheel radius." + "\n" +
                    "[Rear] to change only the rear wheel radius.", MessageType.Info);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (var i = 0; i < m_WheelsList.arraySize; i++)
                    {
                        SerializedProperty m_wheelRadiusOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelRadius");

                        m_wheelRadiusOther.floatValue = m_wheelRadius.floatValue;

                        SerializedProperty m_FrontOnly = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelOffsetRadius");

                        // Update the position of the Wheel 3D Model.
                        SerializedObject serializedObject4 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i].parent.parent.parent);
                        //Debug.Log(CarSetup.objWheelModelList[i].parent.parent.parent.name);
                        SerializedProperty m_objWheelPosition = serializedObject4.FindProperty("m_LocalPosition");
                        serializedObject4.Update();
                        float newYPosition = -1 * (m_wheelRadiusOther.floatValue - m_FrontOnly.floatValue);
                        m_objWheelPosition.vector3Value = new Vector3(0, newYPosition, 0);
                        serializedObject4.ApplyModifiedProperties();

                        // Debug.Log(m_FrontOnly.floatValue);
                        // Update the scale of the Wheel 3D Model.
                        SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i]);
                        SerializedProperty m_objWheelModelList = serializedObject3.FindProperty("m_LocalScale");

                        serializedObject3.Update();

                        float newScaleYZ = (m_wheelRadius.floatValue + m_FrontOnly.floatValue) / .3f;
                        newScaleYZ = 1 * newScaleYZ;

                        m_objWheelModelList.vector3Value = new Vector3(m_objWheelModelList.vector3Value.x, newScaleYZ, newScaleYZ);
                        serializedObject3.ApplyModifiedProperties();


                        SerializedObject serializedObject7 = new UnityEditor.SerializedObject(CarSetup.rbSweepTestList[i]);
                        SerializedProperty m_wheelColliderScale = serializedObject7.FindProperty("m_LocalScale");
                        SerializedProperty m_wheelColliderPos = serializedObject7.FindProperty("m_LocalPosition");

                        serializedObject7.Update();
                        newScaleYZ = .9f * newScaleYZ;

                        m_wheelColliderScale.vector3Value = new Vector3(m_objWheelModelList.vector3Value.x, newScaleYZ, newScaleYZ);

                        // Debug.Log(m_wheelColliderScale.vector3Value.y*.5f * .7f + m_FrontOnly.floatValue);
                        m_wheelColliderPos.vector3Value = new Vector3(0, m_wheelColliderScale.vector3Value.y * .5f * .7f + m_FrontOnly.floatValue, 0);

                        serializedObject7.ApplyModifiedProperties();
                    }
                }

                refWheelRadius = m_wheelRadius.floatValue;
                serializedObject2.ApplyModifiedProperties();
            }
            #endregion
        }

        void EditWheelRadiusFront(string frontOrRear)
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");
                SerializedProperty m_wheelRadius = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("wheelRadius");

                int id = 0;
                if (frontOrRear == "Rear")
                    id = 2;

                SerializedProperty m_wheelRadiusFrontOnly = m_WheelsList.GetArrayElementAtIndex(id).FindPropertyRelative("wheelOffsetRadius");

                serializedObject2.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("             " + frontOrRear + ":", GUILayout.Width(120));
                m_wheelRadiusFrontOnly.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_wheelRadiusFrontOnly.floatValue, 0f, 3);
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    if (CarSetup.wheelsList.Count > 0)
                    {
                        int firtID = 0;
                        int lastId = 2;
                        if (frontOrRear == "Rear")
                        {
                            firtID = 2;
                            lastId = 4;
                        }

                        for (var i = firtID; i < lastId; i++)
                        {
                            if (CarSetup.wheelsList[i].refTransform)
                            {
                                SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.carController);
                                SerializedProperty m_FrontOnly = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelOffsetRadius");

                                serializedObject3.Update();

                                m_FrontOnly.floatValue = m_wheelRadiusFrontOnly.floatValue;

                                serializedObject3.ApplyModifiedProperties();
                            }
                        }
                    }

                    for (var i = 0; i < m_WheelsList.arraySize; i++)
                    {
                        SerializedProperty m_wheelRadiusOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelRadius");
                        m_wheelRadiusOther.floatValue = m_wheelRadius.floatValue;

                        SerializedProperty m_FrontOnly = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelOffsetRadius");

                        // Update the scale of the Wheel 3D Model.
                        SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i]);
                        SerializedProperty m_objWheelModelList = serializedObject3.FindProperty("m_LocalScale");
                        serializedObject3.Update();

                        float newScaleYZ = (m_wheelRadius.floatValue + m_FrontOnly.floatValue) / .3f;
                        newScaleYZ = 1 * newScaleYZ;
                        //if (newScaleYZ == .3f) newScaleYZ += 0.0001f;

                        m_objWheelModelList.vector3Value = new Vector3(m_objWheelModelList.vector3Value.x, newScaleYZ, newScaleYZ);
                        serializedObject3.ApplyModifiedProperties();

                        // Update the position of the Wheel 3D Model.
                        SerializedObject serializedObject4 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i].parent.parent.parent);
                        SerializedProperty m_objWheelPosition = serializedObject4.FindProperty("m_LocalPosition");
                        serializedObject4.Update();

                        float newYPosition = -1 * CarSetup.carController.wheelsList[i].wheelRadius + m_FrontOnly.floatValue;

                        m_objWheelPosition.vector3Value = new Vector3(0, newYPosition, 0);
                        serializedObject4.ApplyModifiedProperties();

                        SerializedObject serializedObject7 = new UnityEditor.SerializedObject(CarSetup.rbSweepTestList[i]);
                        SerializedProperty m_wheelColliderScale = serializedObject7.FindProperty("m_LocalScale");
                        SerializedProperty m_wheelColliderPos = serializedObject7.FindProperty("m_LocalPosition");

                        serializedObject7.Update();
                        newScaleYZ = .9f * newScaleYZ;

                        m_wheelColliderScale.vector3Value = new Vector3(m_objWheelModelList.vector3Value.x, newScaleYZ, newScaleYZ);

                        // Debug.Log(distance);
                        m_wheelColliderPos.vector3Value = new Vector3(0, m_wheelColliderScale.vector3Value.y * .5f * .7f + m_FrontOnly.floatValue, 0);

                        serializedObject7.ApplyModifiedProperties();

                    }
                }
                serializedObject2.ApplyModifiedProperties();
            }
            #endregion
        }

        void EditWheelWidth()
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");
                SerializedProperty m_wheelWidth = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("WheelWidth");

                serializedObject2.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Wheel Width:", GUILayout.Width(120));
                m_wheelWidth.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_wheelWidth.floatValue, .1f, 2);
                EditorGUILayout.EndHorizontal();

                if (helpBox.boolValue)
                {
                    EditorGUILayout.HelpBox(
                   "[Wheel Width] to change all wheels width at once." + "\n" +
                   "[Front] to change only the front wheel width." + "\n" +
                   "[Rear] to change only the rear wheel width.", MessageType.Info);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (var i = 0; i < m_WheelsList.arraySize; i++)
                    {
                        SerializedProperty m_wheelRadiusOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("WheelWidth");
                        m_wheelRadiusOther.floatValue = m_wheelWidth.floatValue;

                        SerializedProperty m_wheelExtraWidth = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelExtraWidth");

                        // Update the scale of the Wheel 3D Model.
                        SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i]);
                        SerializedProperty m_objWheelModelList = serializedObject3.FindProperty("m_LocalScale");
                        serializedObject3.Update();

                        float newScaleX = (m_wheelWidth.floatValue + m_wheelExtraWidth.floatValue) / .11f;
                        newScaleX = 1 * newScaleX;

                        m_objWheelModelList.vector3Value = new Vector3(newScaleX, m_objWheelModelList.vector3Value.y, m_objWheelModelList.vector3Value.z);
                        serializedObject3.ApplyModifiedProperties();

                        SerializedObject serializedObject7 = new UnityEditor.SerializedObject(CarSetup.rbSweepTestList[i]);
                        SerializedProperty m_wheelColliderScale = serializedObject7.FindProperty("m_LocalScale");

                        serializedObject7.Update();

                        m_wheelColliderScale.vector3Value = new Vector3(newScaleX, m_objWheelModelList.vector3Value.y, m_objWheelModelList.vector3Value.z);

                        serializedObject7.ApplyModifiedProperties();
                    }
                }
                serializedObject2.ApplyModifiedProperties();
            }
            #endregion
        }

        void EditWheelWidthFront(string frontOrRear)
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedProperty m_WheelsList = serializedObject2.FindProperty("wheelsList");
                SerializedProperty m_wheelRadius = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("wheelRadius");

                int id = 0;
                if (frontOrRear == "Rear")
                    id = 2;

                SerializedProperty m_wheelExtraWidthFrontOrRearOnly = m_WheelsList.GetArrayElementAtIndex(id).FindPropertyRelative("wheelExtraWidth");

                serializedObject2.Update();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("             " + frontOrRear + ":", GUILayout.Width(120));
                m_wheelExtraWidthFrontOrRearOnly.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_wheelExtraWidthFrontOrRearOnly.floatValue, 0f, 3);
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    if (CarSetup.wheelsList.Count > 0)
                    {
                        int firtID = 0;
                        int lastId = 2;
                        if (frontOrRear == "Rear")
                        {
                            firtID = 2;
                            lastId = 4;
                        }

                        for (var i = firtID; i < lastId; i++)
                        {
                            if (CarSetup.wheelsList[i].refTransform)
                            {
                                SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.carController);
                                SerializedProperty m_FrontOnly = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelExtraWidth");


                                serializedObject3.Update();

                                m_FrontOnly.floatValue = m_wheelExtraWidthFrontOrRearOnly.floatValue;

                                serializedObject3.ApplyModifiedProperties();
                            }

                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        for (var i = 0; i < m_WheelsList.arraySize; i++)
                        {
                            SerializedProperty m_wheelWidth = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("WheelWidth");
                            SerializedProperty m_wheelRadiusOther = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("WheelWidth");
                            m_wheelRadiusOther.floatValue = m_wheelWidth.floatValue;

                            SerializedProperty m_wheelExtraWidth = m_WheelsList.GetArrayElementAtIndex(i).FindPropertyRelative("wheelExtraWidth");

                            // Update the scale of the Wheel 3D Model.
                            SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.objWheelModelList[i]);
                            SerializedProperty m_objWheelModelList = serializedObject3.FindProperty("m_LocalScale");
                            serializedObject3.Update();

                            float newScaleX = (m_wheelWidth.floatValue + m_wheelExtraWidth.floatValue) / .11f;

                            m_objWheelModelList.vector3Value = new Vector3(newScaleX, m_objWheelModelList.vector3Value.y, m_objWheelModelList.vector3Value.z);
                            serializedObject3.ApplyModifiedProperties();

                            SerializedObject serializedObject7 = new UnityEditor.SerializedObject(CarSetup.rbSweepTestList[i]);
                            SerializedProperty m_wheelColliderScale = serializedObject7.FindProperty("m_LocalScale");

                            serializedObject7.Update();
                            m_wheelColliderScale.vector3Value = new Vector3(newScaleX, m_objWheelModelList.vector3Value.y, m_objWheelModelList.vector3Value.z);

                            serializedObject7.ApplyModifiedProperties();
                        }
                    }
                }
                serializedObject2.ApplyModifiedProperties();
            }
            #endregion
        }

        Vector3 RefSize()
        {
            #region
            //GenerateSizeList();
            CarSetup CarSetup = (CarSetup)target;
            if (CarSetup.refSize)
            {
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.refSize);
                SerializedProperty m_localScale = serializedObject2.FindProperty("m_LocalScale");
                serializedObject2.Update();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Scale:", GUILayout.Width(120));
                EditorGUILayout.PropertyField(m_localScale, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (helpBox.boolValue) EditorGUILayout.HelpBox(
                "Change [Scale] to fit car colliders and car detectors to the car body. " +
                 "Use only [X] and [Z] Axis", MessageType.Info);

                serializedObject2.ApplyModifiedProperties();
                return m_localScale.vector3Value;
            }
            return Vector3.zero;
            #endregion
        }

        float counter = 0;
        void GenerateSizeList()
        {
            if (GUILayout.Button("GenerateSizeList", GUILayout.Height(30)))
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.refSize);
                SerializedProperty m_localScale = serializedObject2.FindProperty("m_LocalScale");

                SerializedProperty m_detectorScaleList = serializedObject.FindProperty("detectorScaleList");
               // m_detectorScaleList.ClearArray();
           
               

                serializedObject2.Update();

                Vector3 front = CarSetup.objList[1].refTransform.position;
                Vector3 back = CarSetup.objList[4].refTransform.position;
                Vector3 left = CarSetup.objList[2].refTransform.position;
                Vector3 right = CarSetup.objList[3].refTransform.position;

                m_localScale.vector3Value = new Vector3(counter, 1, counter);

                m_detectorScaleList.InsertArrayElementAtIndex(0);
                float frontBack = Vector3.Distance(front, back);
                float leftRight = Vector3.Distance(left, right);

                m_detectorScaleList.GetArrayElementAtIndex(0).FindPropertyRelative("refDistance").floatValue = counter;
                m_detectorScaleList.GetArrayElementAtIndex(0).FindPropertyRelative("scaleFrontBack").floatValue = frontBack;
                m_detectorScaleList.GetArrayElementAtIndex(0).FindPropertyRelative("scaleLeftRight").floatValue = leftRight;

                serializedObject2.ApplyModifiedProperties();
                counter += .1f;


            }
        }

        Vector3 RefPos()
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            if (CarSetup.refSize)
            {
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.refSize);
                SerializedProperty m_localpos = serializedObject2.FindProperty("m_LocalPosition");
                serializedObject2.Update();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Position:", GUILayout.Width(120));
                EditorGUILayout.PropertyField(m_localpos, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                if (helpBox.boolValue) EditorGUILayout.HelpBox(
         "Change [Position] to move car colliders, car detectors and the car body.", MessageType.Info);

                serializedObject2.ApplyModifiedProperties();
                return m_localpos.vector3Value;
            }
            return Vector3.zero;
            #endregion
        }

        void UpdateColliderListScale(Vector3 refSize)
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            if (CarSetup.objColliderList.Count > 0)
            {
                for (var i = 0; i < CarSetup.objColliderList.Count; i++)
                {
                    if (CarSetup.objColliderList[i].refTransform)
                    {
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.objColliderList[i].refTransform);
                        SerializedProperty m_localScale = serializedObject2.FindProperty("m_LocalScale");

                        Vector3 refObjScale = CarSetup.objColliderList[i].refScale;

                        serializedObject2.Update();


                        float vehicleLength = Vector3.Distance(CarSetup.objList[1].refTransform.position, CarSetup.objList[4].refTransform.position);
                        float vehicleWidth = Vector3.Distance(CarSetup.objList[2].refTransform.position, CarSetup.objList[3].refTransform.position);


                        float newXPos = m_localScale.vector3Value.x;
                        if (CarSetup.objColliderList[i].editScaleX) newXPos = vehicleWidth;

                        float newYPos = m_localScale.vector3Value.y;

                        float newZPos = m_localScale.vector3Value.z;
                        if (CarSetup.objColliderList[i].editScaleZ) newZPos = vehicleLength;

                        m_localScale.vector3Value = new Vector3(newXPos, newYPos, newZPos);

                        serializedObject2.ApplyModifiedProperties();
                    }
                }
            }
            #endregion
        }

        void UpdateTrackerAndBodyPos(Vector3 refSize)
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            if (CarSetup.objList.Count > 0)
            {
                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(CarSetup.refSize);
                SerializedProperty m_localRefPos = serializedObject1.FindProperty("m_LocalPosition");
                for (var i = 0; i < CarSetup.objList.Count; i++)
                {
                    if (CarSetup.objList[i].refTransform)
                    {
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.objList[i].refTransform);
                        SerializedProperty m_localPosition = serializedObject2.FindProperty("m_LocalPosition");

                        Vector3 refObjPos = CarSetup.objList[i].refPos;

                        serializedObject2.Update();

                        float newXPos = refSize.x * .5f + (refObjPos.x - .5f);
                        if (refObjPos.x < 0) newXPos = -refSize.x * .5f + (refObjPos.x + .5f);
                        if (refObjPos.x == 0) newXPos = refObjPos.x;
                        newXPos += m_localRefPos.vector3Value.x;

                        float newYPos = refSize.y * .5f + (refObjPos.y - .5f);
                        if (refObjPos.y < 0) newYPos = -refSize.y * .5f + (refObjPos.y + .5f);
                        if (refObjPos.y == 0) newYPos = refObjPos.y;
                        newYPos += m_localRefPos.vector3Value.y;
                        newYPos += refSuspensionLength - .1f;


                        newYPos += refWheelRadius * 2 - .8f;

                        float newZPos = refSize.z * .5f + (refObjPos.z - .5f);
                        if (refObjPos.z < 0) newZPos = -refSize.z * .5f + (refObjPos.z + .5f);
                        if (refObjPos.z == 0) newZPos = refObjPos.z;
                        newZPos += m_localRefPos.vector3Value.z;
                        //Debug.Log(newZPos);

                        m_localPosition.vector3Value = new Vector3(newXPos, newYPos, newZPos);

                        serializedObject2.ApplyModifiedProperties();
                    }
                }
            }
            #endregion
        }

        void EditWheelPosXZ()
        {
            #region
            EditorGUILayout.LabelField("Front Position:");

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
            "Change [Z Axis] and/or [X Axis] to move the position of the front wheels.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("             " + "Z Axis:", GUILayout.Width(120));
            m_frontWheelDistanceZ.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_frontWheelDistanceZ.floatValue, .01f, 5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("             " + "X Axis:", GUILayout.Width(120));
            m_frontWheelDistanceX.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_frontWheelDistanceX.floatValue, .01f, 5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Rear Position:");

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
            "Change [Z Axis] and/or [X Axis] to move the position of the rear wheels.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("             " + "Z Axis:", GUILayout.Width(120));
            m_rearWheelDistanceZ.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_rearWheelDistanceZ.floatValue, .01f, 5);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("             " + "X Axis:", GUILayout.Width(120));
            m_rearWheelDistanceX.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_rearWheelDistanceX.floatValue, .01f, 5);
            EditorGUILayout.EndHorizontal();
            #endregion
        }

        void UpdateWheelsXZPos(Vector3 refSize, string frontOrRear)
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            if (CarSetup.wheelsList.Count > 0)
            {
                int firtID = 0;
                int lastId = 2;
                if (frontOrRear == "Rear")
                {
                    firtID = 2;
                    lastId = 4;
                }
                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(CarSetup.refSize);
                SerializedProperty m_localRefPos = serializedObject1.FindProperty("m_LocalPosition");

                for (var i = firtID; i < lastId; i++)
                {
                    if (CarSetup.wheelsList[i].refTransform)
                    {
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.wheelsList[i].refTransform);
                        SerializedProperty m_localPosition = serializedObject2.FindProperty("m_LocalPosition");

                        Vector3 refObjPos = CarSetup.wheelsList[i].refPos;

                        serializedObject2.Update();

                        float newXPos = m_frontWheelDistanceX.floatValue;
                        if (frontOrRear == "Rear")
                            newXPos = -m_rearWheelDistanceX.floatValue;

                        if (i == firtID + 1) newXPos *= -1;

                        float newZPos = m_frontWheelDistanceZ.floatValue;
                        if (frontOrRear == "Rear")
                            newZPos = -m_rearWheelDistanceZ.floatValue;

                        m_localPosition.vector3Value = new Vector3(newXPos, m_localPosition.vector3Value.y, newZPos);

                        serializedObject2.ApplyModifiedProperties();
                    }
                }
            }
            #endregion
        }

        void EditCenterOfMass()
        {
            #region
            CarSetup CarSetup = (CarSetup)target;

            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController.centerOfMass);
            SerializedProperty m_localPosition = serializedObject2.FindProperty("m_LocalPosition");

            serializedObject2.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Center Of Mass:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_localPosition, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
               "[Center Of Mass] Small changes can make big difference. Default value x=0 y=0 z=0."
               , MessageType.Info);

            serializedObject2.ApplyModifiedProperties();
            #endregion
        }

        void MoreParameters()
        {
            #region
            CarSetup CarSetup = (CarSetup)target;
            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);

            serializedObject2.Update();

            MaxSpeed(serializedObject2);
            BackwardSpeed(serializedObject2);

            EditorGUILayout.Space();
            BreakForce(serializedObject2);

            EditorGUILayout.Space();

            SteeringSpeed(serializedObject2);
            EditorGUILayout.Space();
            ForwardForceApplied(serializedObject2);
            EditorGUILayout.Space();
            JumpDownForceApplied(serializedObject2);

            EditorGUILayout.Space();

            EditCenterOfMass();

            serializedObject2.ApplyModifiedProperties();
            #endregion
        }

        void SteeringSpeed(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_speedRotation = serializedObject2.FindProperty("speedRotation");
            SerializedProperty m_rotationSpeedCurve = serializedObject2.FindProperty("rotationSpeedCurve");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Steering:", GUILayout.Width(120));
            EditorGUILayout.LabelField("| Speed:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(m_speedRotation, new GUIContent(""), GUILayout.Width(50));
            EditorGUILayout.LabelField("| Curve:", GUILayout.Width(60));
            EditorGUILayout.PropertyField(m_rotationSpeedCurve, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
               "[Speed] modify the max car steering. Default value: 25." + "\n" +
               "[Curve] modify steering depending car speed. By default there is more steering applied when the car is slow. "
               , MessageType.Info);

            #endregion
        }
        void JumpDownForceApplied(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_jumpDownForceApplied = serializedObject2.FindProperty("jumpDownForceApplied");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Jump Force (down):", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_jumpDownForceApplied, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
               "[Jump Force (down)] when the car is not touching the ground an extra force is applied downward. Default value 3500. " +
               "Jumps are bigger when the value decrease. A value too small create unrealistic effect.", MessageType.Info);

            #endregion
        }
        void ForwardForceApplied(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_forwardForceApplied = serializedObject2.FindProperty("forwardForceApplied");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Forward Force:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_forwardForceApplied, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            SerializedProperty m_forwardForceAppliedSpeedCurve = serializedObject2.FindProperty("forwardForceAppliedSpeedCurve");
            SerializedProperty m_famountOfForceCurve = serializedObject2.FindProperty("amountOfForceCurve");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(120));
            EditorGUILayout.LabelField("| Start:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(m_forwardForceAppliedSpeedCurve, new GUIContent(""), GUILayout.Width(50));
            EditorGUILayout.LabelField("| Applied:", GUILayout.Width(60));
            EditorGUILayout.PropertyField(m_famountOfForceCurve, new GUIContent(""));

            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
               "[Forward Force] is the force applied to move the car. Default value: 6000." + "\n" +
               "[Start] modify the force applied to the car during start phase. By default there is less force applied during the start phase. " +
               "Flat curve means  that the car starts instantly with the max force applied." + "\n" +
               "[Applied] modify force applied to the car depending the car speed. By default there is less force applied when the car is moving slowy than when the car is moving fast. " +
               "Flat curve means same force is applied to the car whatever its speed.", MessageType.Info);

            #endregion
        }

        void SpringStiffness(SerializedObject serializedObject2)
        {
            #region
            // spring Stiffness
            EditorGUI.BeginChangeCheck();
            SerializedProperty m_springStiffnessConstant = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(0).FindPropertyRelative("springStiffness");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spring Stiffness:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_springStiffnessConstant, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                 "[Spring Stiffness] 40000 is the default value. Smaller value make the suspension softer. " +
                 "Bigger value make the suspension stiffer", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {

                for (var i = 0; i < serializedObject2.FindProperty("wheelsList").arraySize; i++)
                {
                    SerializedProperty m_springStiffnessConstantTmp = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(i).FindPropertyRelative("springStiffness");
                    m_springStiffnessConstantTmp.floatValue = m_springStiffnessConstant.floatValue;
                }
            }

            // spring Max Compression Ratio
            EditorGUI.BeginChangeCheck();
            SerializedProperty m_springMaxCompressionRatio = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(0).FindPropertyRelative("springMaxCompressionRatio");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Damping Threshold:", GUILayout.Width(120));
            //EditorGUILayout.PropertyField(m_springStiffnessDependingCompression, new GUIContent(""));
            m_springMaxCompressionRatio.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_springMaxCompressionRatio.floatValue, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                 "[Damping Threshold] corresponds to a percentage of the full spring length. " +
                 "Lower the value if the wheels enter into the car body. " +
                 "1 means the full spring length. 0 means spring length = 0.", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                for (var i = 0; i < serializedObject2.FindProperty("wheelsList").arraySize; i++)
                {
                    SerializedProperty m_springMaxCompressionRatioTmp = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(i).FindPropertyRelative("springMaxCompressionRatio");
                    m_springMaxCompressionRatioTmp.floatValue = m_springMaxCompressionRatio.floatValue;
                }
            }

            // damper Stiffeness Min Max
            EditorGUI.BeginChangeCheck();
            SerializedProperty m_damperStiffenessMin = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(0).FindPropertyRelative("damperStiffenessMin");
            SerializedProperty m_damperStiffenessMax = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(0).FindPropertyRelative("damperStiffenessMax");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Damping:", GUILayout.Width(120));
            EditorGUILayout.LabelField("| Min:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(m_damperStiffenessMin, new GUIContent(""), GUILayout.MinWidth(50));
            EditorGUILayout.LabelField("| Max:", GUILayout.Width(50));
            EditorGUILayout.PropertyField(m_damperStiffenessMax, new GUIContent(""), GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                  "[Min] is used if the spring lenght is under the [Damping Theshold] value." + "\n" +
                  "[Max] is used if the spring lenght is higher the [Damping Theshold] value.", MessageType.Info);


            if (EditorGUI.EndChangeCheck())
            {
                for (var i = 0; i < serializedObject2.FindProperty("wheelsList").arraySize; i++)
                {
                    SerializedProperty m_damperStiffenessMinTmp = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(i).FindPropertyRelative("damperStiffenessMin");
                    m_damperStiffenessMinTmp.floatValue = m_damperStiffenessMin.floatValue;
                }

                for (var i = 0; i < serializedObject2.FindProperty("wheelsList").arraySize; i++)
                {
                    SerializedProperty m_damperStiffenessMaxTmp = serializedObject2.FindProperty("wheelsList").GetArrayElementAtIndex(i).FindPropertyRelative("damperStiffenessMax");
                    m_damperStiffenessMaxTmp.floatValue = m_damperStiffenessMax.floatValue;
                }
            }
            #endregion
        }

        void BackwardSpeed(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_maxBackwardSpeedMag = serializedObject2.FindProperty("maxBackwardSpeedMag");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Backward Speed:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_maxBackwardSpeedMag, new GUIContent(""));

            EditorGUILayout.LabelField("(" + Mathf.Round(m_maxBackwardSpeedMag.floatValue) * 3.6f + " km/h | " + Mathf.Round(m_maxBackwardSpeedMag.floatValue * 2.237f) + " mph)", GUILayout.MinWidth(120));

            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                "[Backward Speed] is the max speed when the car goes backward. A higher value increases the max car speed.", MessageType.Info);
            #endregion
        }

        void BreakForce(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_BrakeForce = serializedObject2.FindProperty("BrakeForce");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Brake Force:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_BrakeForce, new GUIContent(""));
            EditorGUILayout.EndHorizontal();
            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                "[Brake Force] is the brake force applied to the car. A higher value increases brake force.", MessageType.Info);
            #endregion
        }

        void MaxSpeed(SerializedObject serializedObject2)
        {
            #region
            SerializedProperty m_maxSpeed = serializedObject2.FindProperty("maxSpeed");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Speed:", GUILayout.Width(120));
            EditorGUILayout.PropertyField(m_maxSpeed, new GUIContent(""));

            EditorGUILayout.LabelField("(" + Mathf.Round(m_maxSpeed.floatValue) * 3.6f + " km/h | " + Mathf.Round(m_maxSpeed.floatValue * 2.237f) + " mph)", GUILayout.MinWidth(120));
            EditorGUILayout.EndHorizontal();

            if (helpBox.boolValue) EditorGUILayout.HelpBox(
                "[Speed] is the max car speed. A higher value increases the max car speed.", MessageType.Info);
            #endregion
        }

        void EditRuntimeSuspensionLength()
        {
            #region
            if (m_CarController.objectReferenceValue)
            {
                CarSetup CarSetup = (CarSetup)target;
                SerializedObject serializedObject2 = new UnityEditor.SerializedObject(CarSetup.carController);
                SerializedObject serializedObject3 = new UnityEditor.SerializedObject(CarSetup.carController.centerOfMass);

                SerializedProperty m_SuspensionRuntimeOffset = serializedObject2.FindProperty("suspensionRuntimeOffset");

                SerializedProperty m_editParametersAtRuntime = serializedObject2.FindProperty("editParametersAtRuntime");
                SerializedProperty m_centerOfMass = serializedObject3.FindProperty("m_LocalPosition");

                serializedObject2.Update();
                serializedObject3.Update();

                //EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Runtime offset:", GUILayout.Width(120));
                m_SuspensionRuntimeOffset.floatValue = EditorGUILayout.Slider(new GUIContent(""), m_SuspensionRuntimeOffset.floatValue, -.2f, .2f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Edit Center Of Mass:", GUILayout.Width(120));
                EditorGUILayout.PropertyField(m_editParametersAtRuntime, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(m_centerOfMass, new GUIContent(""));

                serializedObject3.ApplyModifiedProperties();
                serializedObject2.ApplyModifiedProperties();

            }
            #endregion
        }

        void OnSceneGUI()
        {
        }
    }
}


#endif
