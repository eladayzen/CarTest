//Description: tSGridSortObjectsEditor: Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;


namespace TS.Generics
{
    [CustomEditor(typeof(TSGridSortObjects))]
    public class tSGridSortObjectsEditor : Editor
    {
        SerializedProperty m_seeInspector;                                            // use to draw default Inspector
        SerializedProperty m_howItWorks;
        SerializedProperty m_terrainX;
        SerializedProperty m_terrainZ;
        SerializedProperty m_row;
        SerializedProperty m_column;

        TSOptiGrid tSOptiGrid;

        void OnEnable()
        {
            #region
            // Setup the SerializedProperties.
            m_seeInspector = serializedObject.FindProperty("seeInspector");
            m_howItWorks = serializedObject.FindProperty("howItWorks");
            m_terrainX = serializedObject.FindProperty("terrainX");
            m_terrainZ = serializedObject.FindProperty("terrainZ");
            m_row = serializedObject.FindProperty("row");
            m_column = serializedObject.FindProperty("column");
            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region
            serializedObject.Update();
            if (m_seeInspector.boolValue)
                DrawDefaultInspector();

            if (!m_seeInspector.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Show Inspector: ", GUILayout.Width(100));
                EditorGUILayout.PropertyField(m_seeInspector, new GUIContent(""), GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }

            TSOptiGrid tSOptiGrid = FindFirstObjectByType<TSOptiGrid>();

            if (!tSOptiGrid)
            {
                EditorGUILayout.HelpBox("GridOptimization object is missing in this scene (TSOptiGrid.cs). " +
                    "Add GridOptimization object in your scene before continuing the setup.", MessageType.None);
            }
            else
            {
                HowItWorksSection();
                InitSection();
                DefineTerrainSizeSection();
                DefineGridSizeSection();

                ClearMasterFolderAndMoveObjectToSortFolderSection();
                SortObjectsSection();
            }



            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField("");
            #endregion
        }

        void HowItWorksSection()
        {
            #region
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("How  it works: ", GUILayout.Width(100));
            EditorGUILayout.PropertyField(m_howItWorks, new GUIContent(""), GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            if (m_howItWorks.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "This object is used to sort objects in a grid according to their positions in the scene." + "\n\n" +

                    "When the game is launched, the GridOptimization object in the Hierarchy uses the grid to enable " +
                    "or disable objects depending on the player's position in the scene." + "\n\n" +

                    "When the system detects that the player is close to a part of the grid, " +
                    "the system enables all the objects in the scene found in this area." + "\n\n" +

                    "When the system detects that the player is too far from a part of the grid, " +
                    "the system disables all the objects in the scene found in this area.", MessageType.None);
                EditorGUILayout.LabelField("");
            }
            #endregion
        }
        void InitSection()
        {
            #region
            EditorGUILayout.HelpBox("1-Press 'Init' button the first time you use this object.", MessageType.None);

            if (GUILayout.Button("Init"))
                CreateGridFolders();
            #endregion
        }

        void DefineTerrainSizeSection()
        {
            #region
            EditorGUILayout.HelpBox("2-Define the terrain size.", MessageType.None);


            if (!tSOptiGrid)
                tSOptiGrid = FindFirstObjectByType<TSOptiGrid>();

            if (tSOptiGrid && (tSOptiGrid.terrainX != m_terrainX.intValue || tSOptiGrid.terrainZ != m_terrainZ.intValue))
            {
                string sText = "";
                if (tSOptiGrid) sText = "Actually GridOptimization terrain size is " + tSOptiGrid.terrainX + " x " + tSOptiGrid.terrainZ;
                EditorGUILayout.HelpBox("Use must use the same terrain size as GridOptimization object. " + sText, MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Terrain size| ", GUILayout.Width(70));
            EditorGUILayout.LabelField("X: ", GUILayout.Width(40));
            EditorGUILayout.PropertyField(m_terrainX, new GUIContent(""), GUILayout.MinWidth(40));
            EditorGUILayout.LabelField("Z: ", GUILayout.Width(20));
            EditorGUILayout.PropertyField(m_terrainZ, new GUIContent(""), GUILayout.MinWidth(40));
            EditorGUILayout.EndHorizontal();
            #endregion
        }

        void DefineGridSizeSection()
        {
            #region
            EditorGUILayout.HelpBox("2-Define the grid.", MessageType.None);

            if (!tSOptiGrid)
                tSOptiGrid = FindFirstObjectByType<TSOptiGrid>();

            if (tSOptiGrid && (tSOptiGrid.row != m_row.intValue || tSOptiGrid.column != m_column.intValue))
            {
                string sText = "";
                if (tSOptiGrid) sText = "Actually GridOptimization is " + tSOptiGrid.row + " rows x " + tSOptiGrid.column + " column.";
                EditorGUILayout.HelpBox("Use must use the same row and column as GridOptimization object. " + sText, MessageType.Error);
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid| ", GUILayout.Width(70));
            EditorGUILayout.LabelField("Row: ", GUILayout.Width(40));
            EditorGUILayout.PropertyField(m_row, new GUIContent(""), GUILayout.MinWidth(40));
            EditorGUILayout.LabelField("Column: ", GUILayout.Width(60));
            EditorGUILayout.PropertyField(m_column, new GUIContent(""), GUILayout.MinWidth(40));
            EditorGUILayout.EndHorizontal();
            #endregion
        }

        void ClearMasterFolderAndMoveObjectToSortFolderSection()
        {
            #region
            EditorGUILayout.HelpBox("3-Press 'Clear Master folder' button to update the parameters.", MessageType.None);

            if (GUILayout.Button("Clear Master Folders"))
                ClearMasterFolderAndMoveObjectsToSortFolder();
            #endregion
        }

        void SortObjectsSection()
        {
            #region
            EditorGUILayout.HelpBox("4-In the Hierarchy, put the objects you want to sort inside 'SortFolder'.", MessageType.None);
            EditorGUILayout.HelpBox("5-Press 'Sort Objects button'.", MessageType.None);

            if (GUILayout.Button("Sort Objects", GUILayout.Height(30)))
                SortObjects();
            #endregion
        }

        void CreateGridFolders()
        {
            TSGridSortObjects tSGridSortObjects = (TSGridSortObjects)target;
            #region
            if (PrefabUtility.IsPartOfAnyPrefab(tSGridSortObjects.gameObject))
                PrefabUtility.UnpackPrefabInstance(tSGridSortObjects.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);

            if (tSGridSortObjects.masterFolder && tSGridSortObjects.masterFolder.childCount == 0)
            {
                tSGridSortObjects.zonesList.Clear();
                int counter = 0;
                for (var i = 0; i <= tSGridSortObjects.column; i++)
                {
                    GameObject colFolder = new GameObject();
                    colFolder.transform.SetParent(tSGridSortObjects.masterFolder);

                    float columnSize = tSGridSortObjects.terrainZ / tSGridSortObjects.column;
                    Vector3 newPos = new Vector3(0, 0, columnSize * i);

                    colFolder.transform.localPosition = newPos;
                    colFolder.name = "col_" + i;

                    for (var j = 0; j <= tSGridSortObjects.row; j++)
                    {
                        GameObject subFolder = new GameObject();
                        subFolder.transform.SetParent(colFolder.transform);

                        float rowSize = tSGridSortObjects.terrainX / tSGridSortObjects.row;

                        newPos = new Vector3(rowSize * j + rowSize / 2, 0, 0 + columnSize / 2);

                        subFolder.transform.localPosition = newPos;
                        subFolder.name = counter + "_col_" + i + " row_" + j;
                        subFolder.transform.SetSiblingIndex(counter);
                        subFolder.AddComponent(typeof(TSStreamGridTag));
                        tSGridSortObjects.zonesList.Add(subFolder.transform);
                        counter++;
                    }
                }
            }
            #endregion
        }

        void SortObjects()
        {
            #region
            TSGridSortObjects tSGridSortObjects = (TSGridSortObjects)target;

            Undo.RegisterCompleteObjectUndo(tSGridSortObjects.gameObject, tSGridSortObjects.name);

            if (tSGridSortObjects.masterFolder && tSGridSortObjects.masterFolder.childCount > 0 &&
                tSGridSortObjects.sortFolder)
            {
                int howManyObjectSorted = tSGridSortObjects.sortFolder.childCount;
                for (var i = tSGridSortObjects.sortFolder.childCount - 1; i >= 0; i--)
                {
                    GameObject obj = tSGridSortObjects.sortFolder.GetChild(i).gameObject;
                    int objPosRow = Mathf.FloorToInt(obj.transform.position.x / (tSGridSortObjects.terrainZ / tSGridSortObjects.column));
                    int objPosColumn = Mathf.FloorToInt(obj.transform.position.z / (tSGridSortObjects.terrainX / tSGridSortObjects.row));

                    int index = objPosColumn * (tSGridSortObjects.column + 1) + objPosRow;

                    if (index >= 0 && index < (tSGridSortObjects.column + 1) * (tSGridSortObjects.row + 1))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(obj.gameObject, obj.name);
                        obj.transform.SetParent(tSGridSortObjects.zonesList[index]);
                        while (obj.transform.parent != tSGridSortObjects.zonesList[index]) ;
                    }
                }

                if (EditorUtility.DisplayDialog("Process Done", howManyObjectSorted + " objects have been sorted.", "Continue")) { }
            }
            #endregion
        }

        void ClearMasterFolderAndMoveObjectsToSortFolder()
        {
            #region
            TSGridSortObjects tSGridSortObjects = (TSGridSortObjects)target;

            if (tSGridSortObjects.masterFolder && tSGridSortObjects.masterFolder.childCount > 0 &&
                tSGridSortObjects.sortFolder)
            {
                // Move all the objects contained in the MasterFolder to the SortFolder
                for (var j = tSGridSortObjects.zonesList.Count - 1; j >= 0; j--)
                {
                    Transform subFolder = tSGridSortObjects.zonesList[j];

                    for (var i = subFolder.childCount - 1; i >= 0; i--)
                    {
                        GameObject obj = subFolder.GetChild(i).gameObject;
                        obj.transform.SetParent(tSGridSortObjects.sortFolder);
                        while (obj.transform.parent != tSGridSortObjects.sortFolder) ;
                    }
                }

                for (var j = tSGridSortObjects.masterFolder.childCount - 1; j >= 0; j--)
                    DestroyImmediate(tSGridSortObjects.masterFolder.GetChild(j).gameObject);

                CreateGridFolders();
            }
            #endregion
        }


    }
}

#endif

