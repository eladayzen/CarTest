// Description: AudioSourceFollowPathEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    [CustomEditor(typeof(AudioSourceFollowPath))]
    public class AudioSourceFollowPathEditor : Editor
    {
        SerializedProperty m_spotPosList;

        public bool showFreeMoveHandle = false;
        void OnEnable()
        {
            #region
            m_spotPosList = serializedObject.FindProperty("spotPosList");
            #endregion
        }
        public override void OnInspectorGUI()
        {
            #region
            DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.LabelField("");

            serializedObject.ApplyModifiedProperties();
            #endregion
        }
        void OnSceneGUI()
        {
            #region
            serializedObject.Update();
            CheckInput();
            DisplayHandlesPositions();

            serializedObject.ApplyModifiedProperties();

            if (EditorWindow.mouseOverWindow && EditorWindow.mouseOverWindow is UnityEditor.SceneView && !Application.isPlaying)
                EditorWindow.mouseOverWindow.Focus();
            #endregion
        }
        void DisplayHandlesPositions()
        {
            #region
            AudioSourceFollowPath aSourceFollowPath = (AudioSourceFollowPath)target;
            for (var i = 0; i < m_spotPosList.arraySize; i++)
            {
                Handles.color = Color.blue;
                Vector3 pointPos = m_spotPosList.GetArrayElementAtIndex(i).vector3Value + aSourceFollowPath.transform.position;
                // Debug.Log(pointPos);
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

                        m_spotPosList.GetArrayElementAtIndex(i).vector3Value += dir * dist;
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

            // Show Gizmo 
            showFreeMoveHandle = Event.current.shift ? false : true;
            #endregion
        }

        void AddPoint()
        {
            #region
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            AudioSourceFollowPath aSourceFollowPath = (AudioSourceFollowPath)target;
            Undo.RegisterFullObjectHierarchyUndo(aSourceFollowPath.gameObject, aSourceFollowPath.name);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 newPos = hit.point + Vector3.up * aSourceFollowPath.groundOffset - aSourceFollowPath.transform.position;
                aSourceFollowPath.spotPosList.Add(newPos);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(aSourceFollowPath);

            if (aSourceFollowPath.transform.parent.GetComponent<TSStreamDistanceTag>())
            {
                TSStreamDistanceTag streamDist = aSourceFollowPath.transform.parent.GetComponent<TSStreamDistanceTag>();
                Undo.RegisterFullObjectHierarchyUndo(aSourceFollowPath.transform.parent, aSourceFollowPath.transform.parent.name);
                streamDist.positionToCheckList.Clear();



                for (var i = 0; i < aSourceFollowPath.spotPosList.Count; i++)
                {
                    streamDist.positionToCheckList.Add(aSourceFollowPath.spotPosList[i]);
                }
            }
            #endregion
        }
    }
}


#endif
