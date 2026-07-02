// Description: ManualPathLimitEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    [CustomEditor(typeof(ManualPathLimit))]
    public class ManualPathLimitEditor : Editor
    {
        SerializedProperty m_spotInfoLimitList;
        SerializedProperty m_currentSelectedSpot;

        public bool showFreeMoveHandle = false;
        void OnEnable()
        {
            #region
            m_spotInfoLimitList = serializedObject.FindProperty("spotInfoLimitList");
            m_currentSelectedSpot = serializedObject.FindProperty("currentSelectedSpot");
            #endregion
        }
        public override void OnInspectorGUI()
        {
            #region
            serializedObject.Update();

            if (GUILayout.Button("Reset"))
            {
                ResetCollider();
            }

            if (GUILayout.Button("Reverse"))
            {
                SectionReverse();
            }

            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Generate Limit", GUILayout.Height(40)))
            {
                SectionGenerateMesh();
            }

            EditorGUILayout.LabelField("");

            /* if (GUILayout.Button("Generate Collider", GUILayout.Height(40)))
             {
                 GenerateCollider();
             }*/

            if (GUILayout.Button("DeleteSelectedPoint"))
            {
                DeleteSelectedPoint();
            }

            if (GUILayout.Button("Insert Point"))
            {
                InsertPoint();
            }
            

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField("");
            DrawDefaultInspector();
            #endregion
        }
        void OnSceneGUI()
        {
            #region
            serializedObject.Update();
            CheckInput();
            DisplayHandlesPositions();

           

            serializedObject.ApplyModifiedProperties();

           // if (EditorWindow.mouseOverWindow && EditorWindow.mouseOverWindow is UnityEditor.SceneView && !Application.isPlaying)
           //     EditorWindow.mouseOverWindow.Focus();
            #endregion
        }
        void DisplayHandlesPositions()
        {
            #region
            ManualPathLimit newPoint = (ManualPathLimit)target;
            for (var i = 0; i < m_spotInfoLimitList.arraySize; i++)
            {
                Handles.color = Color.blue;
                Vector3 pointPos = m_spotInfoLimitList.GetArrayElementAtIndex(i).FindPropertyRelative("posGround").vector3Value + newPoint.transform.position;
                // Debug.Log(pointPos);
                
                
                // Ground
                float size = HandleUtility.GetHandleSize(pointPos);
                Vector3 newTargetPosition = Vector3.zero;
                EditorGUI.BeginChangeCheck();

                if (showFreeMoveHandle)
                    newTargetPosition = Handles.FreeMoveHandle(pointPos, size * .15f, Vector3.zero, Handles.SphereHandleCap);
                else
                    newTargetPosition = Handles.DoPositionHandle(pointPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newTargetPosition != pointPos)
                    {
                        Vector3 dir = (newTargetPosition - pointPos).normalized;
                        float dist = Vector3.Distance(newTargetPosition, pointPos);

                        //m_spotInfoLimitList.GetArrayElementAtIndex(i).FindPropertyRelative("posUp").vector3Value += dir * dist;
                        m_spotInfoLimitList.GetArrayElementAtIndex(i).FindPropertyRelative("posGround").vector3Value += dir * dist;
                    }
                }


                // Wall height
                Handles.color = Color.red;
               Vector3 pointPosUp = m_spotInfoLimitList.GetArrayElementAtIndex(i).FindPropertyRelative("posUp").vector3Value + newPoint.transform.position;
                float sizeUp = HandleUtility.GetHandleSize(pointPosUp);
              Vector3 newTargetPositionUp = Vector3.zero;
                EditorGUI.BeginChangeCheck();

                if (showFreeMoveHandle)
                    newTargetPositionUp = Handles.FreeMoveHandle(pointPosUp, sizeUp * .15f, Vector3.zero, Handles.SphereHandleCap);
                else
                    newTargetPositionUp = Handles.DoPositionHandle(pointPosUp, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newTargetPositionUp != pointPosUp)
                    {
                        Vector3 dir = (newTargetPositionUp - pointPosUp).normalized;
                        float dist = Vector3.Distance(newTargetPositionUp, pointPosUp);

                        m_spotInfoLimitList.GetArrayElementAtIndex(i).FindPropertyRelative("posUp").vector3Value += dir * dist;
                    }
                }

            }
            #endregion
        }
        void CheckInput()
        {
            #region
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.N)
                AddPoint();

            if (EditorGUIUtility.hotControl != 0 && Event.current.type == EventType.MouseUp)
            {
                m_currentSelectedSpot.intValue = FindClosestPoint();
            }
               

            // Show Gizmo 
            showFreeMoveHandle = Event.current.shift ? false : true;
            #endregion
        }

        void AddPoint()
        {
            #region
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            ManualPathLimit newPoint = (ManualPathLimit)target;
            Undo.RegisterFullObjectHierarchyUndo(newPoint.gameObject, newPoint.name);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 newPos = hit.point + Vector3.up * newPoint.groundOffset - newPoint.transform.position;
                newPoint.spotInfoLimitList.Add(new SpotInfoLimit(newPos,newPos+Vector3.up * newPoint.wallHeight));
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(newPoint);
            #endregion
        }

        void SectionReverse()
        {
            #region 
            ManualPathLimit pathLimit = (ManualPathLimit)target;
            pathLimit.spotInfoLimitList.Reverse();

            if (EditorUtility.DisplayDialog("WARNING", "Press ''Generate Limit'' button to refresh the collider.", "Continue"))
            { }
            #endregion
        }

        void SectionGenerateMesh()
        {
            #region 
            GenerateMesh(); 
            #endregion
        }

        public void GenerateMesh()
        {
            #region 
            ManualPathLimit pathLimit = (ManualPathLimit)target;

            // Create the mesh
            Mesh mesh;
            pathLimit.objMesh.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Limit";

            int howManySpots = m_spotInfoLimitList.arraySize;
           // Debug.Log(howManySpots);

            if ( m_spotInfoLimitList.arraySize > 1)
            {
                pathLimit.vertices = new Vector3[howManySpots * 2];

                int counter = 0;
                for (var k = 0; k < howManySpots; k++)
                {
                    //Debug.Log(k + " | " + (howManySpots - 2) * 2) ;
                    Vector3 posGround = m_spotInfoLimitList.GetArrayElementAtIndex(k).FindPropertyRelative("posGround").vector3Value;
                    Vector3 posUp = m_spotInfoLimitList.GetArrayElementAtIndex(k).FindPropertyRelative("posUp").vector3Value;

                    pathLimit.vertices[counter] = posGround + -Vector3.up * pathLimit.extraGround;
                    counter++;
                    pathLimit.vertices[counter] = posUp;
                    counter++;
                }

                mesh.vertices = pathLimit.vertices;

                //-> Create Triangles
                int[] triangles = new int[(howManySpots-1) * 6 * 2];

                //Debug.Log("triangles: " + triangles.Length);
                
                for (var i = 0; i < howManySpots-1; i++)
                {
                    int offset = 2 * i;
                   // Debug.Log("spots: " + i + " -> " + (i * 6).ToString());
                    //First Triangle
                    triangles[0 + i * 6] = offset;
                    triangles[1 + i * 6] = 1 + offset;
                    triangles[2 + i * 6] = 2 + offset;
                    //Second Triangle
                    triangles[3 + i * 6] = 2 + offset;
                    triangles[4 + i * 6] = 1 + offset;
                    triangles[5 + i * 6] = 3 + offset;
                   // Debug.Log("i " + (5 + i * 6));
                }
           
                mesh.triangles = triangles;

                //if (EditorUtility.DisplayDialog("Generate Mesh", "Process Done", "Continue"))
                //{ }

                GenerateCollider();
            }
            #endregion
        }
       
        public void GenerateCollider()
        {
            #region 
            ManualPathLimit pathLimit = (ManualPathLimit)target;
            MeshCollider meshCol = pathLimit.objMesh.GetComponent<MeshCollider>();
            meshCol.sharedMesh = pathLimit.objMesh.GetComponent<MeshFilter>().sharedMesh;

            if (EditorUtility.DisplayDialog("Generate Limit", "Process Done", "Continue"))
            { }
            #endregion
        }

        public void ResetCollider()
        {
            #region 
            m_spotInfoLimitList.ClearArray();
            ManualPathLimit pathLimit = (ManualPathLimit)target;
            MeshCollider meshCol = pathLimit.objMesh.GetComponent<MeshCollider>();
            meshCol.sharedMesh = null;

            pathLimit.objMesh.GetComponent<MeshFilter>().sharedMesh = null;

            if (EditorUtility.DisplayDialog("Reset Collider", "Process Done", "Continue"))
            { }
            #endregion
        }

        public void DeleteSelectedPoint()
        {
            #region 
            int currentSelectedSpot = m_currentSelectedSpot.intValue;

            if (m_spotInfoLimitList.arraySize > currentSelectedSpot && currentSelectedSpot != -1)
            {
                m_currentSelectedSpot.intValue = -1;
                m_spotInfoLimitList.DeleteArrayElementAtIndex(currentSelectedSpot);
               // if (EditorUtility.DisplayDialog("Delete Selected Point", "Process Done", "Continue"))
               // { }

                GenerateMesh();
            }
            else
            {
                if (EditorUtility.DisplayDialog("Error", "Selected point doesn't exist. Select a point and try again.", "Continue"))
                { }
            }
            #endregion
        }

        int FindClosestPoint()
        {
            #region
            int closestIndex = -1;
            float currentMinDist = 1000;

            Vector2 screenPos = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);

            for (var i = 0; i < m_spotInfoLimitList.arraySize; i++)
            {
                Vector3 pos = SceneView.currentDrawingSceneView.camera.WorldToViewportPoint(SListPoint(i));
                pos = new Vector2(pos.x, 1 - pos.y);

                float dist = Vector3.Distance(screenPos, pos);

                if (dist < currentMinDist && dist < .02f)
                {
                    currentMinDist = dist;
                    closestIndex = i;
                }
            }
            return closestIndex;
            #endregion
        }

        public Vector3 SListPoint(int index)
        {
            #region
            ManualPathLimit pathLimit = (ManualPathLimit)target;
            return m_spotInfoLimitList.GetArrayElementAtIndex(index).FindPropertyRelative("posGround").vector3Value + pathLimit.transform.position;
            #endregion
        }


        public void InsertPoint()
        {
            #region 
            int currentSelectedSpot = m_currentSelectedSpot.intValue;

            if (m_spotInfoLimitList.arraySize - 1 > currentSelectedSpot && currentSelectedSpot != -1)
            {
                m_currentSelectedSpot.intValue = -1;
                Vector3 pos01 = m_spotInfoLimitList.GetArrayElementAtIndex(currentSelectedSpot).FindPropertyRelative("posGround").vector3Value;
                Vector3 pos02 = m_spotInfoLimitList.GetArrayElementAtIndex(currentSelectedSpot + 1).FindPropertyRelative("posGround").vector3Value;

                Vector3 dir = (pos02 - pos01).normalized;
                float dist = Vector3.Distance(pos02, pos01);

                Vector3 posGround = pos01 + dir * dist * .5f;

                m_spotInfoLimitList.InsertArrayElementAtIndex(currentSelectedSpot);

                m_spotInfoLimitList.GetArrayElementAtIndex(currentSelectedSpot + 1).FindPropertyRelative("posGround").vector3Value = posGround;
                ManualPathLimit pathLimit = (ManualPathLimit)target;
                m_spotInfoLimitList.GetArrayElementAtIndex(currentSelectedSpot + 1).FindPropertyRelative("posUp").vector3Value = posGround + Vector3.up * pathLimit.wallHeight;

                //if (EditorUtility.DisplayDialog("New Point Added", "Process Done", "Continue"))
                //{ }

                GenerateMesh();
            }
            else
            {
                if (EditorUtility.DisplayDialog("Error", "Selected point doesn't exist. Select a point and try again.", "Continue"))
                { }
            }
            #endregion
        }
    }
}
#endif
