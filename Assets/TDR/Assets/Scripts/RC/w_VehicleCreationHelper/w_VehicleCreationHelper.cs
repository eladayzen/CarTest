// Description:  w_VehicleCreationHelper
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    public class w_VehicleCreationHelper : EditorWindow
    {
        private Vector2 scrollPosAll;
        public InitPosVehicleSetup vehicleContainer;

        [MenuItem("Tools/TS/w_VehicleCreationHelper")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(w_VehicleCreationHelper));
        }

        void OnEnable()
        {
            #region 
            Init();
            #endregion
        }

        void Init()
        {
            #region Init Inspector Color
            //listColor.Clear();


            InitPosVehicleSetup vContainer = FindFirstObjectByType<InitPosVehicleSetup>();

            InitVehicleSetupSceneButton();
            InitVehicleTemplateButton();
            InitVehicleBodyExampleButton();
            InitVehicleWheelExampleButton();

            if (vContainer)
            {
                vehicleContainer = vContainer;
                Undo.RegisterFullObjectHierarchyUndo(vContainer.gameObject, "n");
            }

         
            #endregion
        }

        void InitButton()
        {
            if (GUILayout.Button("Refresh Window", GUILayout.Height(30)))
            {
                Init();

                if (vehicleContainer.transform.childCount > 0 && vehicleContainer.transform.GetChild(0).GetComponent<VehiclePrefabInit>())
                {
                    Selection.activeGameObject = vehicleContainer.gameObject;
                }
                else if(EditorPrefs.GetInt("VehicleCreationStep") >= 3)
                {
                    if (EditorUtility.DisplayDialog("Error ",
                                "No Vehicle available in the Hierarchy." +
                                "\nIn the Hierarchy add a vehicle inside InitPos object."
                                 , "Continue"))
                    {
                    }
                }   
            }
        }


        void OnGUI()
        {
            #region
            //--> Scrollview
            scrollPosAll = EditorGUILayout.BeginScrollView(scrollPosAll);

            EditorGUILayout.LabelField("");
            InitButton();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 0", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 0); }
            EditorGUILayout.HelpBox("Read First", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 1", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 1); }
            EditorGUILayout.HelpBox("Open the scene designed to setup vehicles.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 2", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 2); }
            EditorGUILayout.HelpBox("Create the new vehicle prefab.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 3a", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 3); }
            EditorGUILayout.HelpBox("Prepare 3D models (Vehicle body).", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 3b", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 31); }
            EditorGUILayout.HelpBox("Prepare 3D models (wheels).", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 4", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 4); }
            EditorGUILayout.HelpBox("Setup the position and the size of the wheels.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 5", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 5); }
            EditorGUILayout.HelpBox("Setup the vehicles colliders.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 6", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 6); }
            EditorGUILayout.HelpBox("Setup center of mass.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 7", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 7); }
            EditorGUILayout.HelpBox("Override the prefab to save the modification.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Step 8", GUILayout.Width(60))) { EditorPrefs.SetInt("VehicleCreationStep", 8); }
            EditorGUILayout.HelpBox("Add the prefab to the w_VehicleManager.", MessageType.None);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");


            if (EditorPrefs.GetInt("VehicleCreationStep") == 0 || !EditorPrefs.HasKey("VehicleCreationStep"))
            {
                EditorGUILayout.LabelField("");
                EditorGUILayout.LabelField("STEP 0: ", EditorStyles.boldLabel, GUILayout.Width(60));


                EditorGUILayout.LabelField("This window is designed to be used with TUTO 2: SETUP A VEHICLE in the documentation." +
                    "\n\nEach step gives you access to elements that make car creation easier. " +
                    "\n\nAt the start of each step you will find a help box which will give you a summary of the step.", EditorStyles.wordWrappedLabel);
            }


            if (EditorPrefs.GetInt("VehicleCreationStep") == 1)
            {
                EditorGUILayout.LabelField("");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("STEP 1: ", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Open the scene named VehicleSetup.");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("This scene is designed to make the vehicle creation process easier.", MessageType.Info);

                VehicleSetupSceneButton();

            }

            if (EditorPrefs.GetInt("VehicleCreationStep") == 2)
            {

                EditorGUILayout.LabelField("");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("STEP 2: ", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Duplicate the vehicle template.");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Use the duplicated object as a starting point for the new vehicle.", MessageType.Info);
                EditorGUILayout.HelpBox("In the Hierarchy, drag and drop the prefab inside InitPos object.", MessageType.None);

                VehicleTemplateButton();
            }

            if (vehicleContainer)
            {
                if (vehicleContainer.transform.childCount > 0 && vehicleContainer.transform.GetChild(0).GetComponent<VehiclePrefabInit>())
                {
                    if (EditorPrefs.GetInt("VehicleCreationStep") == 3)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 3a: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Prepare 3D models (Vehicle body).");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.HelpBox("Prepare vehicle body 3D model.", MessageType.Info);
                        EditorGUILayout.HelpBox("In the Hierarchy, drag and drop the prefab inside PrepareBody object.", MessageType.None);
                   
                        VehicleBodyExampleButton();

                        if (GUILayout.Button("Copy Body"))
                            UpdateBodyPosition();

                    
                        EditorGUILayout.LabelField("");
                    }
                    if (EditorPrefs.GetInt("VehicleCreationStep") == 31)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 3b: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Prepare 3D models (wheels).");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.HelpBox("Prepare vehicle wheel 3D model.", MessageType.Info);
                        EditorGUILayout.HelpBox("In the Hierarchy, drag and drop the prefab inside PrepareWheel object.", MessageType.None);
                        VehicleWheelExampleButton();
                        if (GUILayout.Button("Copy Wheels"))
                            CloneWheels();

                        EditorGUILayout.LabelField("");
                    }

                    if (EditorPrefs.GetInt("VehicleCreationStep") == 4)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 4: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Setup the position and the size of the wheels.");
                        EditorGUILayout.EndHorizontal();

                        CarSetupObjectButton();

                        EditorGUILayout.LabelField("");
                    }

                    if (EditorPrefs.GetInt("VehicleCreationStep") == 5)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 5: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Setup the vehicles colliders.");
                        EditorGUILayout.EndHorizontal();

                        ShowColliders(true, "Show Colliders");

                        EditorGUILayout.LabelField("");

                        EditorGUILayout.BeginHorizontal();
                        SelectColliders(9, "Body",20);
                        SelectColliders(10, "Floor",20);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        SelectColliders(11, "Windshield");
                        SelectColliders(12, "Interior");
                        SelectColliders(13, "Rear Window");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.LabelField("");

                        ShowColliders(false, "Hide Colliders");
                        EditorGUILayout.LabelField("");
                    }

                    if (EditorPrefs.GetInt("VehicleCreationStep") == 6)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 6: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Setup center of mass.");
                        EditorGUILayout.EndHorizontal();

                        ModeFiveSection();

                        EditorGUILayout.LabelField("");
                    }

                    if (EditorPrefs.GetInt("VehicleCreationStep") == 7)
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("STEP 7: ", EditorStyles.boldLabel, GUILayout.Width(60));
                        EditorGUILayout.LabelField("Override the prefab to save the modification.");
                        EditorGUILayout.EndHorizontal();
                        CarPrefabParentButton();

                        EditorGUILayout.LabelField("");
                    }

                   
                }
                else
                {
                    if (EditorPrefs.GetInt("VehicleCreationStep") >= 3 && EditorPrefs.GetInt("VehicleCreationStep") != 8)
                    {
                        EditorGUILayout.HelpBox("Press the button below to refresh the window.", MessageType.Warning);
                        InitButton();
                    }
                }
            }
            else
            {
                if (EditorPrefs.GetInt("VehicleCreationStep") >= 3 && EditorPrefs.GetInt("VehicleCreationStep") != 8)
                {
                    EditorGUILayout.HelpBox("Press the button below to refresh the window.", MessageType.Warning);
                    InitButton();
                }
                   
            }

            if (EditorPrefs.GetInt("VehicleCreationStep") == 8)
            {
                EditorGUILayout.LabelField("");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("STEP 8: ", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Add the prefab to the w_VehicleManager.");
                EditorGUILayout.EndHorizontal();
                OpenWindowVehicleManager();
                EditorGUILayout.LabelField("");

            }


            EditorGUILayout.EndScrollView();
            #endregion
        }

        void CarSetupObjectButton()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("", GUILayout.Width(20)))
            {
                CarSetup carSetup = ReturnCarSetup();
                // GameObject carSetup = vehicleContainer.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

                Selection.activeGameObject = carSetup.gameObject;

            }
            EditorGUILayout.LabelField("Car Setup");
            EditorGUILayout.EndHorizontal();
        }

        void CarPrefabParentButton()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("", GUILayout.Width(20)))
            {
                GameObject carSetup = vehicleContainer.transform.GetChild(0).gameObject;
                Selection.activeGameObject = carSetup;
            }
            EditorGUILayout.LabelField("Vehicle Prefab");
            EditorGUILayout.EndHorizontal();
        }

        void CloneWheels()
        {
            #region
            string name = "No selection";

            if (Selection.activeGameObject != null)
                name = Selection.activeGameObject.name;
            if (vehicleContainer.quad && name != "No selection")
            {

                if (EditorUtility.DisplayDialog("Copy the wheel inside the vehicle template",
                "Are you sure you want to move:" + "\n" + name
                , "Yes", "No"))
                {
                    CarSetup carSetup = ReturnCarSetup();

                    for (var j = 0; j < 5; j++)
                    {
                        if (j == 1)
                            continue;

                        int id = 2;

                        GameObject wheelPIVOT = new GameObject();
                        wheelPIVOT.transform.SetParent(Selection.activeGameObject.transform.parent);

                        wheelPIVOT.transform.localRotation = Quaternion.identity;
                        wheelPIVOT.transform.localPosition = Vector3.zero;

                        wheelPIVOT.name = "Grp_Wheel";
                        Undo.RegisterCreatedObjectUndo(wheelPIVOT, "grp_Model");

                        GameObject Grp_Wheel_FL = Instantiate(
                            Selection.activeGameObject, 
                            Selection.activeGameObject.transform.position, 
                            Selection.activeGameObject.transform.rotation);
                        id += j;

                        int wheelChild = carSetup.modelsRefList[id].transform.childCount;

                        for (var i = 0; i < wheelChild; i++)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(carSetup.modelsRefList[id].transform.GetChild(i).gameObject, carSetup.modelsRefList[id].name + "child_i");
                            carSetup.modelsRefList[id].transform.GetChild(i).gameObject.SetActive(false);
                        }

                        Grp_Wheel_FL.transform.SetParent(wheelPIVOT.transform);


                        wheelPIVOT.transform.SetParent(carSetup.modelsRefList[id].transform);
                        wheelPIVOT.transform.localPosition = Vector3.zero;
                        wheelPIVOT.transform.localEulerAngles = Vector3.zero;

                        Undo.RegisterCreatedObjectUndo(Grp_Wheel_FL, "Wheel_" + j);
                    }
                    Undo.DestroyObjectImmediate(Selection.activeGameObject);
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Error",
                               "No object selected"
                                , "Continue"))
                {
                }
            }

            #endregion
        }


        Object _vehicleSetupScene;
        void InitVehicleSetupSceneButton()
        {
            #region 
            string objectPath = "Assets/TDR/Assets/Scenes/Tracks/VehicleSetup/VehicleSetup.unity";
            _vehicleSetupScene = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as Object;
            #endregion
        }
        void VehicleSetupSceneButton()
        {
            #region 
            if (_vehicleSetupScene)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("", GUILayout.Width(20)))
                {
                    Selection.activeObject = _vehicleSetupScene;
                    EditorGUIUtility.PingObject(_vehicleSetupScene);
                }
                EditorGUILayout.LabelField("Scene: " + _vehicleSetupScene.name);
                EditorGUILayout.EndHorizontal();
            }
            #endregion
        }

        Object _vehicleTemplate;
        void InitVehicleTemplateButton()
        {
            #region 
            string objectPath = "Assets/TDR/Assets/Prefabs/AS/Vehicle/Prefabs/Vehicle/00_Vehicle_Base_sc.prefab";
            _vehicleTemplate = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as Object;
            #endregion
        }
        void VehicleTemplateButton()
        {
            #region 
            if (_vehicleTemplate)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("", GUILayout.Width(20)))
                {
                    Selection.activeObject = _vehicleTemplate;
                    EditorGUIUtility.PingObject(_vehicleTemplate);
                }
                EditorGUILayout.LabelField("Vehicle Template: " + _vehicleTemplate.name);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Press the button below to refresh the window.", MessageType.Warning);
                InitButton();
            }
            #endregion
        }

        Object _vehicleBodyExample;

        void InitVehicleBodyExampleButton()
        {
            #region 
            string objectPath = "Assets/TDR/Assets/Prefabs/AS/Vehicle/Prefabs/Vehicle/Example/PickupBodyExample.prefab";
            _vehicleBodyExample = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as Object;
            #endregion
        }

        void VehicleBodyExampleButton()
        {
            #region 
            if (_vehicleBodyExample)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("", GUILayout.Width(20)))
                {
                    Selection.activeObject = _vehicleBodyExample;
                    EditorGUIUtility.PingObject(_vehicleBodyExample);
                }
                EditorGUILayout.LabelField(_vehicleBodyExample.name, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Press the button below to refresh the window.", MessageType.Warning) ;
                InitButton();
            }
            #endregion
        }

        void UpdateBodyPosition()
        {
            #region

            string name = "No selection";

            if (Selection.activeGameObject != null)
                name = Selection.activeGameObject.name;

            if (vehicleContainer.quad && name != "No selection")
            {

                if (EditorUtility.DisplayDialog("Move the body inside the vehicle template",
                "Are you sure you want to move:" + "\n" + name
                , "Yes", "No"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject.gameObject, Selection.activeGameObject.name);

                    UpdateDetectorPosition(Selection.activeGameObject.transform.parent.GetComponent<PrepareBody>().quadRef.localScale);

                    GameObject grp_Model = new GameObject();
                    grp_Model.transform.SetParent(Selection.activeGameObject.transform.parent);

                    grp_Model.transform.localRotation = Quaternion.identity;
                    grp_Model.transform.localPosition = vehicleContainer.quad.transform.localPosition;

                    grp_Model.name = "Grp_Model";
                    Undo.RegisterCreatedObjectUndo(grp_Model, "grp_Model");

                    Selection.activeGameObject.transform.SetParent(grp_Model.transform);

                    // Move the body in the vehicle template
                    CarSetup carSetup = ReturnCarSetup();

                    int howManyChild = carSetup.modelsRefList[0].transform.childCount;
                    for (var i = 0; i < howManyChild; i++)
                    {
                        GameObject child = carSetup.modelsRefList[0].transform.GetChild(i).gameObject;
                        Undo.RegisterCompleteObjectUndo(child, child.name);
                        child.SetActive(false);
                    }


                    grp_Model.transform.SetParent(carSetup.modelsRefList[0].transform);

                    grp_Model.transform.localRotation = Quaternion.identity;
                    grp_Model.transform.localPosition = Vector3.zero + new Vector3(0, 0.32f, 0);


                    UpdateTrackerAndBodyPos();
                    UpdateColliderListScale();
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Error",
                               "No object selected"
                                , "Continue"))
                {
                }
            }
            #endregion
        }

        void UpdateTrackerAndBodyPos()
        {
            #region
            CarSetup carSetup = ReturnCarSetup();

            if (carSetup.objList.Count > 0)
            {
                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(carSetup.refSize);
                SerializedProperty m_localRefPos = serializedObject1.FindProperty("m_LocalPosition");
                SerializedProperty m_localRefScale = serializedObject1.FindProperty("m_LocalScale");
                Vector3 refSize = m_localRefScale.vector3Value;

                for (var i = 0; i < carSetup.objList.Count; i++)
                {
                    if (carSetup.objList[i].refTransform)
                    {
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(carSetup.objList[i].refTransform);
                        SerializedProperty m_localPosition = serializedObject2.FindProperty("m_LocalPosition");

                        Vector3 refObjPos = carSetup.objList[i].refPos;

                        serializedObject2.Update();

                        float newXPos = refSize.x * .5f + (refObjPos.x - .5f);
                        if (refObjPos.x < 0) newXPos = -refSize.x * .5f + (refObjPos.x + .5f);
                        if (refObjPos.x == 0) newXPos = refObjPos.x;
                        newXPos += m_localRefPos.vector3Value.x;

                        float newYPos = refSize.y * .5f + (refObjPos.y - .5f);
                        if (refObjPos.y < 0) newYPos = -refSize.y * .5f + (refObjPos.y + .5f);
                        if (refObjPos.y == 0) newYPos = refObjPos.y;
                        newYPos += m_localRefPos.vector3Value.y;

                        SerializedObject serializedObject3 = new UnityEditor.SerializedObject(carSetup.carController);
                        SerializedProperty m_WheelsList = serializedObject3.FindProperty("wheelsList");
                       
                        SerializedProperty m_SupensionLength = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("suspensionLength");
                        SerializedProperty m_wheelRadius = m_WheelsList.GetArrayElementAtIndex(0).FindPropertyRelative("wheelRadius");

                        newYPos += m_SupensionLength.floatValue - .1f;

                        newYPos += m_wheelRadius.floatValue * 2 - .8f;

                        float newZPos = refSize.z * .5f + (refObjPos.z - .5f);
                        if (refObjPos.z < 0) newZPos = -refSize.z * .5f + (refObjPos.z + .5f);
                        if (refObjPos.z == 0) newZPos = refObjPos.z;
                        newZPos += m_localRefPos.vector3Value.z;

                        m_localPosition.vector3Value = new Vector3(newXPos, newYPos, newZPos);

                        serializedObject2.ApplyModifiedProperties();
                    }
                }
            }
            #endregion
        }

        void UpdateColliderListScale()
        {
            #region
            CarSetup carSetup = ReturnCarSetup();
            if (carSetup.objColliderList.Count > 0)
            {
                for (var i = 0; i < carSetup.objColliderList.Count; i++)
                {
                    if (carSetup.objColliderList[i].refTransform)
                    {
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(carSetup.objColliderList[i].refTransform);
                        SerializedProperty m_localScale = serializedObject2.FindProperty("m_LocalScale");

                        serializedObject2.Update();

                        float vehicleLength = Vector3.Distance(carSetup.objList[1].refTransform.position, carSetup.objList[4].refTransform.position);
                        float vehicleWidth = Vector3.Distance(carSetup.objList[2].refTransform.position, carSetup.objList[3].refTransform.position);


                        float newXPos = m_localScale.vector3Value.x;
                        if (carSetup.objColliderList[i].editScaleX) newXPos = vehicleWidth;

                        float newYPos = m_localScale.vector3Value.y;

                        float newZPos = m_localScale.vector3Value.z;
                        if (carSetup.objColliderList[i].editScaleZ) newZPos = vehicleLength;

                        m_localScale.vector3Value = new Vector3(newXPos, newYPos, newZPos);

                        serializedObject2.ApplyModifiedProperties();
                    }
                }
            }
            #endregion
        }

        void UpdateDetectorPosition(Vector3 quadScale)
        {
            #region 
            CarSetup carSetup = ReturnCarSetup();

            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(carSetup.refSize);
            SerializedProperty m_localScale = serializedObject2.FindProperty("m_LocalScale");
            serializedObject2.Update();
            // x -> Z
            // x -> Y
            float xScale = 0;
            for (var i = 0; i < carSetup.detectorScaleList.Count; i++)
            {
                if (carSetup.detectorScaleList[i].scaleFrontBack < quadScale.y)
                {
                    xScale = carSetup.detectorScaleList[i].refDistance;
                    break;
                }
            }

            float zScale = 0;
            for (var i = 0; i < carSetup.detectorScaleList.Count; i++)
            {
                if (carSetup.detectorScaleList[i].scaleLeftRight < quadScale.x)
                {
                    zScale = carSetup.detectorScaleList[i].refDistance;
                    break;
                }
            }

            m_localScale.vector3Value = new Vector3(zScale, m_localScale.vector3Value.y, xScale);

            serializedObject2.ApplyModifiedProperties(); 
            #endregion
        }

        Object _vehicleWheelExample;

        void InitVehicleWheelExampleButton()
        {
            #region 
            string objectPath = "Assets/TDR/Assets/Prefabs/AS/Vehicle/Prefabs/Vehicle/Example/WheelTuto.prefab";
            _vehicleWheelExample = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as Object;
            #endregion
        }

        void VehicleWheelExampleButton()
        {
            #region 
            if (_vehicleWheelExample)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("", GUILayout.Width(20)))
                {
                    Selection.activeObject = _vehicleWheelExample;
                    EditorGUIUtility.PingObject(_vehicleWheelExample);
                }
                EditorGUILayout.LabelField(_vehicleWheelExample.name, GUILayout.Width(70));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
               EditorGUILayout.HelpBox("Press the button below to refresh the window.", MessageType.Warning);
                InitButton();
            }
            #endregion
        }

        void OpenWindowVehicleManager()
        {
            #region 
            if (GUILayout.Button("Open window w_VehicleManager", GUILayout.Height(30)))
            {
                var window = EditorWindow.GetWindow(typeof(w_VehicleManager));
                window.Show();
            } 
            #endregion
        }
      
        void ShowColliders(bool isShown,string label)
        {
            #region 
            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                CarSetup carSetup = ReturnCarSetup();

                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(carSetup);
                serializedObject1.Update();
                SerializedProperty m_modelsRefList = serializedObject1.FindProperty("modelsRefList");

                for (var i = 0; i < m_modelsRefList.arraySize; i++)
                {
                    if (i == 9 || i == 10)
                    {
                        GameObject objRef = (GameObject)m_modelsRefList.GetArrayElementAtIndex(i).objectReferenceValue;
                        SerializedObject serializedObject2 = new UnityEditor.SerializedObject(objRef.GetComponent<MeshRenderer>());
                        serializedObject2.Update();
                        SerializedProperty m_IsActive = serializedObject2.FindProperty("m_Enabled");
                        m_IsActive.boolValue = isShown;
                        serializedObject2.ApplyModifiedProperties();
                    }

                    if (i >= 11 && i <= 13)
                    {
                        GameObject objRef = (GameObject)m_modelsRefList.GetArrayElementAtIndex(i).objectReferenceValue;
                        MeshRenderer[] activeCollider = objRef.GetComponentsInChildren<MeshRenderer>();

                        for (var j = 0; j < activeCollider.Length; j++)
                        {
                            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(activeCollider[j].GetComponent<MeshRenderer>());
                            serializedObject2.Update();
                            SerializedProperty m_IsActive = serializedObject2.FindProperty("m_Enabled");
                            m_IsActive.boolValue = isShown;
                            serializedObject2.ApplyModifiedProperties();
                        }

                    }
                }

                serializedObject1.ApplyModifiedProperties();
            }  
            #endregion
        }

        void SelectColliders(int id, string label,float height = 30)
        {
            #region 
            if (GUILayout.Button(label, GUILayout.Height(height)))
            {
                CarSetup carSetup = ReturnCarSetup();

                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(carSetup);
                serializedObject1.Update();
                SerializedProperty m_modelsRefList = serializedObject1.FindProperty("modelsRefList");

                for (var i = 0; i < m_modelsRefList.arraySize; i++)
                {
                    if (i == id)
                    {
                        GameObject objRef = (GameObject)m_modelsRefList.GetArrayElementAtIndex(i).objectReferenceValue;
                        Selection.activeGameObject = objRef;
                    }

                    if (i >= 11 && i <= 13 && i == id)
                    {
                        GameObject objRef = (GameObject)m_modelsRefList.GetArrayElementAtIndex(i).objectReferenceValue;
                        //Debug.Log("i: " + i + " | " + objRef.name);
                        MeshRenderer[] activeCollider = objRef.GetComponentsInChildren<MeshRenderer>();
                        for (var j = 0; j < activeCollider.Length; j++)
                        {
                            Selection.activeGameObject = activeCollider[j].transform.parent.gameObject;
                        }

                    }
                }

                serializedObject1.ApplyModifiedProperties();
            } 
            #endregion
        }

        void ModeFiveSection()
        {
            #region 
            VehiclePrefabInit carSetup = vehicleContainer.transform.GetChild(0).GetComponent<VehiclePrefabInit>();
            EditorGUILayout.BeginHorizontal();
            if (carSetup.carModeFive == 0)
            {
                EditorGUILayout.LabelField("Mode 5: ", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Currently Mode 5 is NOT enabled: ");
            }
            else
            {
                EditorGUILayout.LabelField("Mode 5: ", EditorStyles.boldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("Currently Mode 5 is enabled: ");
            }
            EditorGUILayout.EndHorizontal();

            ModeFive(true, "Enable");
            EditorGUILayout.LabelField("");
            CarSetupObjectButton();
            EditorGUILayout.LabelField("");
            ModeFive(false, "Disable"); 
            #endregion
        }
      

        void ModeFive(bool isModeFiveEnable, string label)
        {
            #region 
            if (GUILayout.Button(label))
            {
                VehiclePrefabInit carSetup = vehicleContainer.transform.GetChild(0).GetComponent<VehiclePrefabInit>();
                SerializedObject serializedObject1 = new UnityEditor.SerializedObject(carSetup);
                SerializedProperty m_carModeFive = serializedObject1.FindProperty("carModeFive");
                SerializedProperty m_b_AutoInit = serializedObject1.FindProperty("b_AutoInit");
                SerializedProperty m_currentSelectedList = serializedObject1.FindProperty("currentSelectedList");

                serializedObject1.Update();
                InfoRememberMainMenuSelection infoRemember = GameObject.FindFirstObjectByType<InfoRememberMainMenuSelection>();

                if (!isModeFiveEnable)
                {
                    m_carModeFive.intValue = 0;
                    if (infoRemember)
                    {
                        infoRemember.playerMainMenuSelection.HowManyPlayer = 1;
                        infoRemember.playerMainMenuSelection.currentGameMode = 3;
                    }
                    m_b_AutoInit.boolValue = false;
                    m_currentSelectedList.intValue = 0;
                    if (EditorUtility.DisplayDialog("Car is ready for Game",
                        "The car is ready for game. " +
                        "The Current Game Mode is set to 3. ", "Done"))
                    { }
                }
                if (isModeFiveEnable)
                {
                    m_carModeFive.intValue = 1;
                    if (infoRemember)
                    {
                        infoRemember.playerMainMenuSelection.HowManyPlayer = 1;
                        infoRemember.playerMainMenuSelection.currentGameMode = 5;
                    }
                    m_b_AutoInit.boolValue = true;
                    m_currentSelectedList.intValue = 2;

                    if (EditorUtility.DisplayDialog("Car is ready for test",
                      "The car is ready for mode 5. " +
                      "Don't forget to switch to Ready For Game after the modifications.", "Done"))
                    { }
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(carSetup);

                serializedObject1.ApplyModifiedProperties();
            } 
            #endregion
        }


        CarSetup ReturnCarSetup()
        {
            CarSetup[] carSetup = vehicleContainer.transform.GetChild(0).GetChild(0).GetComponentsInChildren<CarSetup>();

            for (var i = 0; i < carSetup.Length; i++)
            {
                if (carSetup[i].useNewVersion)
                {
                    return carSetup[i];
                }
            }

            return null;
        }

    }
}

#endif