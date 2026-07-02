// Description:  w_PathGenerator: Window to create the track path
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
    public class w_PathGenerator : EditorWindow
    {
        private Vector2 scrollPosAll;
        public Path trackPath;

        [MenuItem("Tools/TS/w_PathGenerator")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(w_PathGenerator));
        }

        #region Init Inspector Color
        private Texture2D MakeTex(int width, int height, Color col)
        {                       // use to change the GUIStyle
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private List<Texture2D> listTex = new List<Texture2D>();
        public List<GUIStyle> listGUIStyle = new List<GUIStyle>();
        private List<Color> listColor = new List<Color>();
        #endregion

        public string[] listItemType = new string[] { };

        public List<string> _test = new List<string>();
        public int page = 0;
        public int numberOfIndexInAPage = 50;
        public int seachSpecificID = 0;

        public Color _cGreen = new Color(1f, .8f, .4f, 1);
        public Color _cGray = new Color(.9f, .9f, .9f, 1);


        public Texture2D eye;
        public Texture2D currentItemDisplay;
        public int intcurrentItemDisplay = 0;
        public bool b_UpdateProcessDone = false;
        public bool b_AllowUpdateScene = false;



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


            Path allPath = FindFirstObjectByType<Path>();

            if (allPath)
            {
                trackPath = allPath;
                Undo.RegisterFullObjectHierarchyUndo(trackPath.gameObject, "n");
            }
            #endregion
        }

        void InitColorStyle()
        {
            #region Init Inspector Color
            listGUIStyle.Clear();
            listTex.Clear();
            for (var i = 0; i < inspectorColor.listColor.Length; i++)
            {
                listTex.Add(MakeTex(2, 2, inspectorColor.ReturnColor(i)));
                listGUIStyle.Add(new GUIStyle());
                listGUIStyle[i] = new GUIStyle(); listGUIStyle[i].normal.background = listTex[i];
            }
            #endregion
        }

        void OnGUI()
        {
            #region
            //--> Scrollview
            scrollPosAll = EditorGUILayout.BeginScrollView(scrollPosAll);

            if (listTex.Count == 0 || listTex.Count > 0 && listTex[0] == null)
            {
                InitColorStyle();
            }


            UpdateInfo();

            GlobalOptions();

            CreateANewCheckpoint();

            EditorGUILayout.LabelField("");

            ResetPath();

            ListOfAllPaths();



            EditorGUILayout.EndScrollView();
            #endregion
        }

        void GlobalOptions()
        {
            #region
            if (trackPath)
            {
                EditorGUILayout.LabelField("");
                SerializedObject serializedObject0 = new UnityEditor.SerializedObject(trackPath);
                serializedObject0.Update();
                SerializedProperty m_TrackIsLooped = serializedObject0.FindProperty("TrackIsLooped");
                SerializedProperty checkpoints = serializedObject0.FindProperty("checkpoints");
                SerializedProperty prefabCheckpoint = serializedObject0.FindProperty("prefabCheckpoint");

                EditorGUILayout.HelpBox("Select if the track is a loop or if vehicle goes from a point A to a point B. " +
                    "Press the toggle to switch between the Two modes.", MessageType.Info);
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_TrackIsLooped, new GUIContent(""), GUILayout.Width(20));
                if (m_TrackIsLooped.boolValue) EditorGUILayout.LabelField("Track is looped", GUILayout.Width(120));
                else EditorGUILayout.LabelField("Track is not looped", GUILayout.Width(120));

                EditorGUILayout.EndHorizontal();

                // End the code block and update the label if a change occurred
                if (EditorGUI.EndChangeCheck() && checkpoints.arraySize > 0)
                {
                    if (!m_TrackIsLooped.boolValue)
                    {
                        // Create Start Ref Position
                        Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                        GameObject newCheckpoint = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(newCheckpoint, "NoLoopStart");
                        newCheckpoint.name = "NoLoopStart";

                        checkpoints.InsertArrayElementAtIndex(0);
                        checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newCheckpoint;

                        // Create End Ref Position
                        parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 1).objectReferenceValue;
                        newCheckpoint = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(newCheckpoint, "NoLoopEnd");
                        newCheckpoint.name = "NoLoopEnd";

                        // Update Checkpoint List
                        checkpoints.InsertArrayElementAtIndex(0);
                        checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newCheckpoint;
                        checkpoints.MoveArrayElement(0, checkpoints.arraySize - 1);


                        // Update number of lap
                        LapCounterAndPosition lapCounterAndPosition = FindFirstObjectByType<LapCounterAndPosition>();
                        if (lapCounterAndPosition)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(lapCounterAndPosition, lapCounterAndPosition.name);
                            lapCounterAndPosition.howManyLapsInTheCurrentRace = 1;
                        }

                        // Update End Line 
                        StartLine startLine = FindFirstObjectByType<StartLine>();
                        if (startLine)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(startLine.EndLineTrackIsLooped, startLine.EndLineTrackIsLooped.name);
                            startLine.EndLineTrackIsLooped.gameObject.SetActive(true);

                            Transform newPosForEndLine = (Transform)checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 1).objectReferenceValue;
                            startLine.EndLineTrackIsLooped.transform.position = newPosForEndLine.position;
                            startLine.EndLineTrackIsLooped.transform.rotation = newPosForEndLine.rotation;
                        }
                    }
                    else
                    {
                        Path myScript = trackPath;
                        Undo.RegisterFullObjectHierarchyUndo(myScript, myScript.name);

                        // Destroy End Ref Position
                        if (checkpoints.GetArrayElementAtIndex(myScript.checkpoints.Count - 1).objectReferenceValue != null)
                            Undo.DestroyObjectImmediate(myScript.checkpoints[myScript.checkpoints.Count - 1].gameObject);

                        // Destroy Start Ref Position
                        checkpoints.DeleteArrayElementAtIndex(myScript.checkpoints.Count - 1);

                        if (checkpoints.GetArrayElementAtIndex(0).objectReferenceValue != null)
                            Undo.DestroyObjectImmediate(myScript.checkpoints[0].gameObject);

                        // Update Checkpoint List
                        checkpoints.DeleteArrayElementAtIndex(0);

                        // Update number of lap
                        LapCounterAndPosition lapCounterAndPosition = FindFirstObjectByType<LapCounterAndPosition>();
                        if (lapCounterAndPosition)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(lapCounterAndPosition, lapCounterAndPosition.name);
                            lapCounterAndPosition.howManyLapsInTheCurrentRace = 3;
                        }

                        // Update End Line 
                        StartLine startLine = FindFirstObjectByType<StartLine>();
                        if (startLine)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(startLine.EndLineTrackIsLooped, startLine.EndLineTrackIsLooped.name);
                            startLine.EndLineTrackIsLooped.gameObject.SetActive(false);
                        }
                    }
                }
                serializedObject0.ApplyModifiedProperties();
                EditorGUILayout.LabelField("");
            }
            #endregion
        }

        void UpdateInfo()
        {
            #region
            if (!trackPath && !Application.isPlaying)
            {
                Path allPath = FindFirstObjectByType<Path>();

                if (allPath)
                {
                    trackPath = allPath;
                }
            }
            #endregion
        }

        void CreateANewCheckpoint()
        {
            #region
            if (trackPath)
            {
                SerializedObject serializedObject0 = new UnityEditor.SerializedObject(trackPath);
                serializedObject0.Update();
                SerializedProperty checkpoints = serializedObject0.FindProperty("checkpoints");
                SerializedProperty prefabCheckpoint = serializedObject0.FindProperty("prefabCheckpoint");
                SerializedProperty m_TrackIsLooped = serializedObject0.FindProperty("TrackIsLooped");

                //-> Add a new checkpoint to the main path
                string sCreateCheckpoint = "New checkpoint at the end of the list";
                if (checkpoints.arraySize == 0)
                    sCreateCheckpoint = "Create First Checkpoint: Position -> Vector3(0,0,0) ";

                if (GUILayout.Button(sCreateCheckpoint, GUILayout.Height(50)))
                {
                    AddNewSpotAtTheEndOftheList(checkpoints, m_TrackIsLooped, prefabCheckpoint);
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Update Checkpoint Direction"))
                {
                    UpdateCheckpointDirection(checkpoints);

                    if (EditorUtility.DisplayDialog("Process Done.",
                    "Update Checkpoint Direction Done.", "Continue")) { }
                }

                if (GUILayout.Button("Rename Checkpoints"))
                {
                    for (var i = 0; i < checkpoints.arraySize; i++)
                    {
                        Transform trans01 = checkpoints.GetArrayElementAtIndex(i).objectReferenceValue as Transform;

                        Undo.RegisterFullObjectHierarchyUndo(trans01.gameObject, trans01.name);
                        string number = "0";
                        if (i > 9) number = "";
                        trans01.name = "cp_" + number + i;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(20));
                EditorGUILayout.LabelField("Checkpoints list:", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                for (var i = 0; i < checkpoints.arraySize; i++)
                {
                    if (!m_TrackIsLooped.boolValue &&
                      (checkpoints.arraySize > 3 && i == (checkpoints.arraySize - 1)
                      ||
                      checkpoints.arraySize > 1 && i == 0))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("--", GUILayout.Width(20));
                        EditorGUILayout.LabelField(checkpoints.GetArrayElementAtIndex(i).objectReferenceValue.name);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (Selection.activeGameObject && Selection.activeGameObject.transform == checkpoints.GetArrayElementAtIndex(i).objectReferenceValue)
                            EditorGUILayout.LabelField(i + ": ", listGUIStyle[5], GUILayout.Width(20));
                        else
                            EditorGUILayout.LabelField(i + ": ", GUILayout.Width(20));

                        if (GUILayout.Button("*", GUILayout.Width(20)))
                        {
                            Path myScript = trackPath;
                            if (myScript.checkpoints[i] != Selection.activeObject)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(myScript.checkpoints[i].gameObject, myScript.name);
                                myScript.checkpoints[i].transform.position = Selection.activeTransform.position;
                                myScript.checkpoints[i].transform.rotation = Selection.activeTransform.rotation;
                            }
                        }

                        if (GUILayout.Button("+", GUILayout.Width(20)))
                        {
                            Path myScript = trackPath;
                            GameObject newCheckpoint = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, myScript.transform) as GameObject;
                            Undo.RegisterCreatedObjectUndo(newCheckpoint, "newAltPath");

                            newCheckpoint.transform.position = myScript.checkpoints[i].transform.position;
                            newCheckpoint.transform.rotation = myScript.checkpoints[i].transform.rotation;

                            newCheckpoint.name = myScript.checkpoints[i].name + "b";

                            int childPosition = myScript.checkpoints[i].transform.GetSiblingIndex();
                            newCheckpoint.transform.SetSiblingIndex(childPosition + 1);


                            checkpoints.InsertArrayElementAtIndex(0);
                            checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newCheckpoint;

                            // Debug.Log(i + " | " + checkpoints.arraySize);
                            // Case: Track is not looped
                            if (!m_TrackIsLooped.boolValue)
                            {
                                // Create End Ref Position
                                if (i == checkpoints.arraySize - 3)
                                {
                                    Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                                    GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                                    Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopEnd");
                                    newEndRef.name = "NoLoopEnd";

                                    parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 2).objectReferenceValue;
                                    if (parentTransform.childCount >= 2)
                                        Undo.DestroyObjectImmediate(parentTransform.GetChild(1).gameObject);

                                    checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 1).objectReferenceValue = newEndRef;
                                }

                                // Case Second Spot created
                                if (checkpoints.arraySize == 3)
                                {
                                    Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                                    GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                                    Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopEnd");
                                    newEndRef.name = "NoLoopEnd";

                                    checkpoints.InsertArrayElementAtIndex(0);
                                    checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newEndRef;
                                    checkpoints.MoveArrayElement(0, checkpoints.arraySize - 1);

                                    UpdateCheckpointDirection(checkpoints);
                                }
                            }

                            checkpoints.MoveArrayElement(0, i + 1);
                            Selection.activeGameObject = newCheckpoint;
                        }

                        if ((!m_TrackIsLooped.boolValue && i > 1 && i < checkpoints.arraySize -2) || m_TrackIsLooped.boolValue)
                        {
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                Path myScript = trackPath;
                                Selection.activeGameObject = null;
                                Undo.RegisterFullObjectHierarchyUndo(myScript, myScript.name);
                                if (checkpoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                                {
                                    Undo.DestroyObjectImmediate(myScript.checkpoints[i].gameObject);
                                }

                               


                                // Case: Track is not looped
                                if (!m_TrackIsLooped.boolValue)
                                {
                                    // Create End Ref Position
                                    if (checkpoints.arraySize > 4 && i == checkpoints.arraySize - 2)
                                    {
                                        Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 3).objectReferenceValue;
                                        GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                                        Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopEnd");
                                        newEndRef.name = "NoLoopEnd";

                                        myScript.checkpoints[myScript.checkpoints.Count - 1] = newEndRef.transform;
                                    }
                                    else
                                    {
                                        if (myScript.checkpoints.Count == 1)
                                            myScript.checkpoints.RemoveAt(0);
                                        else
                                            myScript.checkpoints.RemoveAt(i);
                                    }
                                }
                                else
                                {
                                    myScript.checkpoints.RemoveAt(i);
                                }

                                //break;
                            }
                        }

                        if ((!m_TrackIsLooped.boolValue && i == 1) || (!m_TrackIsLooped.boolValue && i == checkpoints.arraySize - 2))
                            EditorGUILayout.LabelField("", GUILayout.Width(20));

                        EditorGUILayout.PropertyField(checkpoints.GetArrayElementAtIndex(i), new GUIContent(""));
                        EditorGUILayout.LabelField("Alt Path: ", GUILayout.Width(50));

                        Transform refTrans = (Transform)checkpoints.GetArrayElementAtIndex(i).objectReferenceValue;

                        if (refTrans && refTrans.GetChild(0).childCount > 0)
                        {
                            GameObject AltPath = refTrans.GetChild(0).GetChild(0).gameObject;
                            SerializedObject serializedObject1 = new UnityEditor.SerializedObject(AltPath);
                            serializedObject1.Update();
                            SerializedProperty m_IsActive = serializedObject1.FindProperty("m_IsActive");

                            if (m_IsActive.boolValue && GUILayout.Button("", listGUIStyle[5], GUILayout.Width(20)))
                            {
                                m_IsActive.boolValue = !m_IsActive.boolValue;
                            }
                            else if (!m_IsActive.boolValue && GUILayout.Button("", listGUIStyle[0], GUILayout.Width(20)))
                            {
                                m_IsActive.boolValue = !m_IsActive.boolValue;
                            }

                            serializedObject1.ApplyModifiedProperties();

                            if (m_IsActive.boolValue)
                            {
                                if (GUILayout.Button("Select", GUILayout.Width(50)))
                                {
                                    Selection.activeGameObject = refTrans.GetChild(0).GetChild(0).gameObject;
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("", GUILayout.Width(50));
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                }
                serializedObject0.ApplyModifiedProperties();
            }
            #endregion
        }

        void OnInspectorUpdate()
        {
            #region
            /*   if (EditorWindow.mouseOverWindow && EditorWindow.mouseOverWindow is UnityEditor.SceneView && !Application.isPlaying)
               {
                   EditorWindow.mouseOverWindow.Focus();
               }

               Repaint(); */
            #endregion
        }

        private void HelpZone(int value)
        {
            #region

            switch (value)
            {
                case 0:
                    EditorGUILayout.HelpBox(
                    "This section allows to create checkpoints on the main Path", MessageType.Info);
                    break;
            }
            #endregion
        }

        void OnFocus()
        {
            #region 
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            #endregion
        }

        void OnDestroy()
        {
            #region 
            SceneView.duringSceneGui -= OnSceneGUI;
            #endregion
        }

        void OnSceneGUI(SceneView sceneview)
        {
            #region
            CheckInput();
            #endregion
        }

        void CheckInput()
        {

            #region
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.N)
            {
                if (trackPath)
                {
                    SerializedObject serializedObject0 = new UnityEditor.SerializedObject(trackPath);
                    serializedObject0.Update();
                    SerializedProperty checkpoints = serializedObject0.FindProperty("checkpoints");
                    SerializedProperty prefabCheckpoint = serializedObject0.FindProperty("prefabCheckpoint");
                    SerializedProperty m_TrackIsLooped = serializedObject0.FindProperty("TrackIsLooped");

                    AddNewSpotAtTheEndOftheList(checkpoints, m_TrackIsLooped, prefabCheckpoint, true);

                    serializedObject0.ApplyModifiedProperties();
                }

            }
            #endregion
        }

        Vector3 ReturnMousePosition()
        {
            #region 
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                return hit.point;
            }
            else
                return Vector3.zero;
            #endregion
        }

        void AddNewSpotAtTheEndOftheList(SerializedProperty checkpoints, SerializedProperty m_TrackIsLooped, SerializedProperty prefabCheckpoint, bool useMousePosition = false)
        {
            #region
            Path myScript = trackPath;
            GameObject newCheckpoint = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, myScript.transform) as GameObject;
            Undo.RegisterCreatedObjectUndo(newCheckpoint, "newAltPath");

            int pos = Mathf.Clamp(myScript.checkpoints.Count - 1, 0, myScript.checkpoints.Count - 1);

            //Debug.Log("pos: " + pos);
            if (myScript.checkpoints.Count > 0)
            {
                if (useMousePosition && ReturnMousePosition() != Vector3.zero)
                {
                    newCheckpoint.transform.position = ReturnMousePosition();

                    Vector3 dir = (myScript.checkpoints[pos].transform.position - newCheckpoint.transform.position);
                    newCheckpoint.transform.LookAt(newCheckpoint.transform.position + dir * 5);
                }
                else
                {
                    newCheckpoint.transform.position = myScript.checkpoints[pos].transform.position;
                    newCheckpoint.transform.rotation = myScript.checkpoints[pos].transform.rotation;
                }

                string _name = (myScript.checkpoints.Count).ToString();
                if (myScript.checkpoints.Count < 10) _name = "0" + _name;
                newCheckpoint.name = "cp_" + _name;
                checkpoints.InsertArrayElementAtIndex(0);
                checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newCheckpoint;

                // Case: Track is not looped
                if (!m_TrackIsLooped.boolValue)
                {
                    // Create End Ref Position
                    if (checkpoints.arraySize > 3)
                    {
                        Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                        GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopEnd");
                        newEndRef.name = "NoLoopEnd";

                        parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 2).objectReferenceValue;
                        if (parentTransform.childCount >= 2)
                            Undo.DestroyObjectImmediate(parentTransform.GetChild(1).gameObject);

                        checkpoints.GetArrayElementAtIndex(checkpoints.arraySize - 1).objectReferenceValue = newEndRef;
                        checkpoints.MoveArrayElement(0, checkpoints.arraySize - 2);
                    }

                    //Debug.Log(checkpoints.arraySize);
                    // Case Second Spot created
                    if (checkpoints.arraySize == 3)
                    {
                        Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                        GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopEnd");
                        newEndRef.name = "NoLoopEnd";

                        checkpoints.MoveArrayElement(0, checkpoints.arraySize - 1);

                        checkpoints.InsertArrayElementAtIndex(0);
                        checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newEndRef;
                        checkpoints.MoveArrayElement(0, checkpoints.arraySize - 1);
                        UpdateCheckpointDirection(checkpoints);
                    }
                }
                else
                {
                    checkpoints.MoveArrayElement(0, checkpoints.arraySize - 1);
                }
            }
            else
            {
                if (useMousePosition && ReturnMousePosition() != Vector3.zero)
                {
                    newCheckpoint.transform.position = ReturnMousePosition();
                }
                else
                {
                    if (myScript.checkpoints.Count == 0)
                    {
                        newCheckpoint.transform.position = Vector3.zero; ;
                        newCheckpoint.transform.rotation = Quaternion.identity;
                    }
                    else
                    {
                        newCheckpoint.transform.position = myScript.checkpoints[pos].transform.position;
                        newCheckpoint.transform.rotation = myScript.checkpoints[pos].transform.rotation;
                    }
                }

                checkpoints.InsertArrayElementAtIndex(0);
                checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newCheckpoint;
                newCheckpoint.name = "cp_00";

                // Case First Spot created
                if (!m_TrackIsLooped.boolValue && checkpoints.arraySize == 1)
                {
                    Transform parentTransform = (Transform)checkpoints.GetArrayElementAtIndex(0).objectReferenceValue;
                    GameObject newEndRef = PrefabUtility.InstantiatePrefab((GameObject)prefabCheckpoint.objectReferenceValue, parentTransform) as GameObject;
                    Undo.RegisterCreatedObjectUndo(newEndRef, "NoLoopStart");
                    newEndRef.name = "NoLoopStart";

                    checkpoints.InsertArrayElementAtIndex(0);
                    checkpoints.GetArrayElementAtIndex(0).objectReferenceValue = newEndRef;
                }
            }

            Selection.activeGameObject = newCheckpoint;
            #endregion
        }

        void UpdateCheckpointDirection(SerializedProperty checkpoints)
        {
            #region 
            for (var i = 0; i < checkpoints.arraySize; i++)
            {
                Transform trans01 = checkpoints.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                Transform trans02 = checkpoints.GetArrayElementAtIndex((i + 1) % checkpoints.arraySize).objectReferenceValue as Transform;

                Undo.RegisterFullObjectHierarchyUndo(trans01.gameObject, trans01.name);
                trans01.LookAt(trans02);
                trans01.localEulerAngles = new Vector3(0, trans01.localEulerAngles.y + 180, 0);
            }
            #endregion
        }

        void ListOfAllPaths()
        {
            #region 
            if (trackPath)
            {
                SerializedObject serializedObject0 = new UnityEditor.SerializedObject(trackPath);

                SerializedProperty m_TrackIsLooped = serializedObject0.FindProperty("TrackIsLooped");
                SerializedProperty checkpoints = serializedObject0.FindProperty("checkpoints");
                SerializedProperty prefabCheckpoint = serializedObject0.FindProperty("prefabCheckpoint");

                // Default path
                string checkpointsList = "Checkpoint 0 -> ";
                for (var i = 0; i < checkpoints.arraySize; i++)
                {
                    checkpointsList += checkpoints.GetArrayElementAtIndex(i).objectReferenceValue.name;
                    checkpointsList += ", ";
                }

                EditorGUILayout.LabelField(checkpointsList);

                // Alt paths
                checkpointsList = "";
                for (var j = 0; j < checkpoints.arraySize; j++)
                {
                    if (IsAltPathEnabled((Transform)checkpoints.GetArrayElementAtIndex(j).objectReferenceValue))
                    {
                        Transform refTrans = (Transform)checkpoints.GetArrayElementAtIndex(j).objectReferenceValue;
                        int HowManyAltPath = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList.Count;
                        //Debug.Log(j + " -> HowManyAltPath: " + HowManyAltPath);
                        for (var l = 0; l < HowManyAltPath; l++)
                        {
                            checkpointsList = "Checkpoint " + j + " -> " + "Alt Path: " + l + " ::: ";
                            for (var i = 0; i < checkpoints.arraySize; i++)
                            {
                                checkpointsList += checkpoints.GetArrayElementAtIndex(i).objectReferenceValue.name;
                                checkpointsList += ", ";


                                if (i == j)
                                {
                                    //Transform refTrans = (Transform)checkpoints.GetArrayElementAtIndex(j).objectReferenceValue;
                                    int howManySubPoints = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList[l].tmpCheckpoints.Count;
                                    for (var k = 0; k < howManySubPoints; k++)
                                    {
                                        string AltPath = refTrans.GetChild(0).GetChild(0).gameObject.GetComponent<TriggerAltPath>().AltPathList[l].tmpCheckpoints[k].name;
                                        checkpointsList += AltPath;
                                        checkpointsList += ", ";
                                    }
                                }
                            }
                            EditorGUILayout.LabelField(checkpointsList);
                        }
                    }
                }
            }
            #endregion
        }

        bool IsAltPathEnabled(Transform checkpoint)
        {
            #region
            GameObject AltPath = checkpoint.GetChild(0).GetChild(0).gameObject;

            if (AltPath && AltPath.activeSelf)
                return true;

            return false;
            #endregion
        }

        void ResetPath()
        {
            #region 
            if (trackPath)
            {
                SerializedObject serializedObject0 = new UnityEditor.SerializedObject(trackPath);
                serializedObject0.Update();
                SerializedProperty m_checkpoints = serializedObject0.FindProperty("checkpoints");
                SerializedProperty m_checkpointsDistanceFromPathStart = serializedObject0.FindProperty("checkpointsDistanceFromPathStart");
                SerializedProperty m_AltPathList = serializedObject0.FindProperty("AltPathList");
                SerializedProperty m_difficultyPathList = serializedObject0.FindProperty("difficultyPathList");
                SerializedProperty m_difficultyReversPathList = serializedObject0.FindProperty("difficultyReversPathList");
                SerializedProperty m_speedRatioDependingGripList = serializedObject0.FindProperty("speedRatioDependingGripList");
                SerializedProperty m_allPathList = serializedObject0.FindProperty("allPathList");
                SerializedProperty m_difficultyOffset = serializedObject0.FindProperty("difficultyOffset");
                SerializedProperty m_pathLength = serializedObject0.FindProperty("pathLength");
                SerializedProperty m_TrackIsLooped = serializedObject0.FindProperty("TrackIsLooped");

                //-> Add a new checkpoint to the main path
                if (GUILayout.Button("Reset Path"))
                {
                    for (var i = 0; i < m_checkpoints.arraySize; i++)
                    {
                        Transform trans01 = m_checkpoints.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                        if (m_TrackIsLooped.boolValue)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(trans01.gameObject, trans01.name);
                            DestroyImmediate(trans01.gameObject);
                        }
                        else
                        {
                            if (i != 0 && i != m_checkpoints.arraySize - 1)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(trans01.gameObject, trans01.name);
                                DestroyImmediate(trans01.gameObject);
                            }
                        }
                    }

                    m_checkpoints.ClearArray();
                    m_checkpointsDistanceFromPathStart.ClearArray();
                    m_AltPathList.ClearArray();
                    m_difficultyPathList.ClearArray();
                    m_difficultyReversPathList.ClearArray();
                    m_speedRatioDependingGripList.ClearArray();
                    m_allPathList.ClearArray();
                    m_difficultyOffset.ClearArray();

                    AltPath[] altPaths = trackPath.GetComponentsInChildren<AltPath>();

                    for (var i = 0; i < altPaths.Length; i++)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(altPaths[i].gameObject, altPaths[i].name);
                        DestroyImmediate(altPaths[i].gameObject);
                    }

                    m_pathLength.floatValue = 0;
                }

                serializedObject0.ApplyModifiedProperties();
            }
            #endregion
        }
    }
}

#endif