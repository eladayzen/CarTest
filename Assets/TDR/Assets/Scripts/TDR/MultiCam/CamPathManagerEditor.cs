// Description: CamPathManagerEditor. Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
    [CustomEditor(typeof(CamPathManager))]
    public class CamPathManagerEditor : Editor
    {
        SerializedProperty m_seeInspector;
        SerializedProperty m_spotPosList;
        SerializedProperty m_targetHeight;
        SerializedProperty m_speed;
        SerializedProperty m_defaultTangentLength;

        SerializedProperty m_editorCurrentSelectedPoint;
        public bool showFreeMoveHandle = false;
        public bool showDoPositionHandlePivot = false;

        public int whichHandleToDisplay = 0;
        public bool lastShift = false;

        void OnEnable()
        {
            #region
            m_seeInspector = serializedObject.FindProperty("seeInspector");
            m_spotPosList = serializedObject.FindProperty("spotPosList");

            m_targetHeight = serializedObject.FindProperty("targetHeight");
            m_speed = serializedObject.FindProperty("speed");

            m_defaultTangentLength = serializedObject.FindProperty("defaultTangentLength");
            
            m_editorCurrentSelectedPoint = serializedObject.FindProperty("editorCurrentSelectedPoint");
            m_editorCurrentSelectedPoint.intValue = -1;

            #endregion
        }
        public override void OnInspectorGUI()
        {
            #region
            if(m_seeInspector.boolValue)
            DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("See Inspector:", GUILayout.Width(180));
            EditorGUILayout.PropertyField(m_seeInspector, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");


            GlobalParams();
            EditorGUILayout.LabelField("");
            ResetPath();
            UpdatePath();
            EditorGUILayout.LabelField("");
            AddPoint();
            RemovePoint();

            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        void GlobalParams()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Height:", GUILayout.Width(180));
            m_targetHeight.floatValue = EditorGUILayout.Slider(m_targetHeight.floatValue, 0, 150);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Speed:", GUILayout.Width(180));
            m_speed.floatValue = EditorGUILayout.Slider(m_speed.floatValue, 0, 50);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Editor default Tangent Length:", GUILayout.Width(180));
            EditorGUILayout.PropertyField(m_defaultTangentLength, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI()
        {
            #region
            CheckInput();
            serializedObject.Update();
            DisplayHandlesPositions();

            if (m_spotPosList.arraySize > 3)
            {
                DisplayBezierPath();
                CamPathManager camPathManager = (CamPathManager)target;
                ReturnTotalCurveDistance(camPathManager);
            }

            serializedObject.ApplyModifiedProperties();
            #endregion
        }



        void CheckInput()
        {
            #region

            // Show Gizmo 
            showFreeMoveHandle = Event.current.shift ? true : false;

            showDoPositionHandlePivot = Event.current.control ? true : false;

            if (EditorGUIUtility.hotControl != 0 && Event.current.type == EventType.MouseUp)
            {
                m_editorCurrentSelectedPoint.intValue = FindClosestPoint();
                serializedObject.ApplyModifiedProperties();
            }
            #endregion
        }

        int FindClosestPoint()
        {
            #region
            CamPathManager camPathManager = (CamPathManager)target;
            int closestIndex = -1;
            float currentMinDist = 1000;

            Vector2 screenPos = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);

            for (var i = 0; i < m_spotPosList.arraySize; i++)
            {
                if (i % 3 == 0)
                {
                    Vector3 pointPos = m_spotPosList.GetArrayElementAtIndex(i).vector3Value + camPathManager.transform.position;
                    Vector3 pos = SceneView.currentDrawingSceneView.camera.WorldToViewportPoint(pointPos);
                    pos = new Vector2(pos.x, 1 - pos.y);

                    float dist = Vector3.Distance(screenPos, pos);

                    if (dist < currentMinDist && dist < .02f)
                    {
                        currentMinDist = dist;
                        closestIndex = i;
                    }
                }
            }

            return closestIndex;
            #endregion
        }


        void DisplayHandlesPositions()
        {
            #region
            if (showFreeMoveHandle)
            {
                whichHandleToDisplay = 1;
            }
            else if (showDoPositionHandlePivot)
            {
                whichHandleToDisplay = 2;
            }
            else
            {
                whichHandleToDisplay = 0;
            }

            CamPathManager camPathManager = (CamPathManager)target;
            for (var i = 0; i < m_spotPosList.arraySize; i++)
            {
                Handles.color = new Color(0, .7f, 1f);
                Vector3 pointPos = m_spotPosList.GetArrayElementAtIndex(i).vector3Value + camPathManager.transform.position;
                // Debug.Log(pointPos);
                float size = HandleUtility.GetHandleSize(pointPos) * .15f;
                if((i%3) == 1 || (i % 3) == 2)
                    size = HandleUtility.GetHandleSize(pointPos) * .075f;

                Vector3 newTargetPosition = Vector3.zero;
                EditorGUI.BeginChangeCheck();

                if(i%3 == 0)
                {
                    if (i > 0)
                        Handles.DrawLine(m_spotPosList.GetArrayElementAtIndex(i - 1).vector3Value + camPathManager.transform.position, m_spotPosList.GetArrayElementAtIndex(i).vector3Value + camPathManager.transform.position);
                    if (i < m_spotPosList.arraySize - 2)
                        Handles.DrawLine(m_spotPosList.GetArrayElementAtIndex(i + 1).vector3Value + camPathManager.transform.position, m_spotPosList.GetArrayElementAtIndex(i).vector3Value + camPathManager.transform.position);
                }

                if (whichHandleToDisplay == 0)
                {
                    if (i % 3 == 1 || i % 3 == 2)
                    {
                        Handles.color = new Color(0, .7f, 1f);
                        newTargetPosition = Handles.FreeMoveHandle(pointPos, size, Vector3.zero, Handles.CubeHandleCap);
                    }
                    else
                    {
                        Handles.color = Color.blue;
                        if(m_editorCurrentSelectedPoint.intValue == i || (m_editorCurrentSelectedPoint.intValue == 0 && i == m_spotPosList.arraySize-1))
                            Handles.color = Color.yellow;

                        newTargetPosition = Handles.FreeMoveHandle(pointPos, size, Vector3.zero, Handles.SphereHandleCap);
                    }
                }
                else if (whichHandleToDisplay == 1)
                {
                    Vector3 relativePos = Vector3.zero;
                    Quaternion rotation = Quaternion.identity;

                    if (i % 3 == 1)
                        relativePos = m_spotPosList.GetArrayElementAtIndex(i - 1).vector3Value - m_spotPosList.GetArrayElementAtIndex(i).vector3Value;
                    if (i % 3 == 2)
                        relativePos = m_spotPosList.GetArrayElementAtIndex(i + 1).vector3Value - m_spotPosList.GetArrayElementAtIndex(i).vector3Value;

                    if ((i % 3 == 1 || i % 3 == 2))
                    {
                        rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                        newTargetPosition = Handles.DoPositionHandle(pointPos, rotation);
                    }
                    else
                    {
                        newTargetPosition = Handles.FreeMoveHandle(pointPos, size, Vector3.zero, Handles.SphereHandleCap);
                    }
                }
                else if (whichHandleToDisplay == 2)
                {
                    Vector3 relativePos = Vector3.zero;
                    Quaternion rotation = Quaternion.identity;

                    if (i % 3 == 1)
                        relativePos = m_spotPosList.GetArrayElementAtIndex(i - 1).vector3Value - m_spotPosList.GetArrayElementAtIndex(i).vector3Value;
                    if (i % 3 == 2)
                        relativePos = m_spotPosList.GetArrayElementAtIndex(i + 1).vector3Value - m_spotPosList.GetArrayElementAtIndex(i).vector3Value;

                    if ((i % 3 == 0))
                    {
                        newTargetPosition = Handles.DoPositionHandle(pointPos, rotation);
                    }
                    else
                    {
                        newTargetPosition = Handles.FreeMoveHandle(pointPos, size, Vector3.zero, Handles.SphereHandleCap);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (newTargetPosition != pointPos)
                    {
                        Vector3 dir = (newTargetPosition - pointPos).normalized;
                        float dist = Vector3.Distance(newTargetPosition, pointPos);

                        m_spotPosList.GetArrayElementAtIndex(i).vector3Value += dir * dist;
                        // First Point or last Point
                        if (i == 0 || i == m_spotPosList.arraySize -1)
                        {
                            if(i == 0)
                                m_spotPosList.GetArrayElementAtIndex(m_spotPosList.arraySize - 1).vector3Value += dir * dist;
                            if(i == m_spotPosList.arraySize - 1)
                                m_spotPosList.GetArrayElementAtIndex(0).vector3Value += dir * dist;

                            var idPlusOne = 1;
                            var idMinusOne = m_spotPosList.arraySize -2;
                            m_spotPosList.GetArrayElementAtIndex(idPlusOne).vector3Value = m_spotPosList.GetArrayElementAtIndex(idPlusOne).vector3Value + dir * dist;
                            m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value = m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value + dir * dist;
                        }
                        // First Tangent
                       else if (i == 1)
                        {
                            var idMinusOne = m_spotPosList.arraySize - 2;
                            m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value = m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value - dir * dist;
                        }
                        // last Tangent
                            else if (i == m_spotPosList.arraySize - 2)
                            {
                                m_spotPosList.GetArrayElementAtIndex(1).vector3Value -= dir * dist;
                            }
                          else if ((i % 3) == 0)
                          {
                              var idPlusOne = (i + 1 + m_spotPosList.arraySize) % m_spotPosList.arraySize;
                              var idMinusOne = (i - 1 + m_spotPosList.arraySize) % m_spotPosList.arraySize;
                              m_spotPosList.GetArrayElementAtIndex(idPlusOne).vector3Value = m_spotPosList.GetArrayElementAtIndex(idPlusOne).vector3Value + dir * dist;
                              m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value = m_spotPosList.GetArrayElementAtIndex(idMinusOne).vector3Value + dir * dist;
                          }
                          else if ((i % 3) == 1)
                          {
                              var idMinusTwo = (i -2 + m_spotPosList.arraySize) % m_spotPosList.arraySize;
                              m_spotPosList.GetArrayElementAtIndex(idMinusTwo).vector3Value -= dir * dist;
                          }
                          else if ((i % 3) == 2)
                          {
                              var idPlusTwo = (i + 2 + m_spotPosList.arraySize) % m_spotPosList.arraySize;
                              m_spotPosList.GetArrayElementAtIndex(idPlusTwo).vector3Value -= dir * dist;
                          }
                    }
                }
            }
            #endregion
        }
   
        void DisplayBezierPath()
        {
            #region
            CamPathManager camPathManager = (CamPathManager)target;

            for (var i = 0; i < m_spotPosList.arraySize - 1; i += 3)
            {
                Handles.color = Color.red;
                Vector3 startPos = camPathManager.spotPosList[i] + camPathManager.transform.position;
                Vector3 startTangent = camPathManager.spotPosList[i + 1] + camPathManager.transform.position;
                Vector3 endTangent = camPathManager.spotPosList[i + 2] + camPathManager.transform.position;
                Vector3 endtPos = camPathManager.spotPosList[i + 3] + camPathManager.transform.position;

                Handles.DrawBezier(startPos, endtPos, startTangent, endTangent, Color.red, null, 1);
            }
            #endregion
        }

        public static Vector3 GetPointPosition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            #region
            return (1f - t) * (1f - t) * (1f - t) * p0 + 3f * (1f - t) * (1f - t) * t * p1 + 3f * (1f - t) * t * t * p2 + t * t * t * p3;
            #endregion
        }

        public void ReturnTotalCurveDistance(CamPathManager myScript = null, float distanceBetweenTwoPoints = 1.5f)
        {
            #region
            if (myScript.spotPosList.Count > 0)
            {
                float dist = 0.0f;
                Vector3 lastPos = myScript.spotPosList[0];

                float multiplier = 1;

                for (int j = 0; j < myScript.spotPosList.Count - 1; j++)
                {
                    if (j % 3 == 0)
                    {
                        float currentPosOnCurve = 0;
                        while (currentPosOnCurve < 1)
                        {
                            Vector3 startPos = myScript.spotPosList[j] + myScript.transform.position;
                            Vector3 startTangent = myScript.spotPosList[j + 1] + myScript.transform.position;
                            Vector3 endTangent = myScript.spotPosList[j + 2] + myScript.transform.position;
                            Vector3 endtPos = myScript.spotPosList[j + 3] + myScript.transform.position;

                            Vector3 subPoint = GetPointPosition(startPos, startTangent, endTangent, endtPos, currentPosOnCurve);

                            dist += Vector3.Distance(lastPos, subPoint);

                            if (dist >= distanceBetweenTwoPoints * multiplier)
                            {
                                multiplier++;
                            }

                            lastPos = subPoint;
                            currentPosOnCurve += .005f;
                        }
                    }
                }
            }
            #endregion
        }

        void ResetPath()
        {
            #region 
            if (GUILayout.Button("Reset Path"))
                m_spotPosList.ClearArray(); 
            #endregion
        }

        void UpdatePath()
        {
            #region 
            if (GUILayout.Button("Create Default Path"))
            {
                CamPathManager camPathManager = (CamPathManager)target;
                Undo.RegisterFullObjectHierarchyUndo(camPathManager.gameObject, camPathManager.name);

                CreateDefaultPath();
            } 
            #endregion
        }

        void CreateDefaultPath()
        {
            #region 
            CamPathManager camPathManager = (CamPathManager)target;
            camPathManager.spotPosList.Clear();
            PathRef pathRef = FindFirstObjectByType<PathRef>();
            if (pathRef)
            {
                List<Transform> checkpointsList = new List<Transform>(pathRef.Track.checkpoints);
                for (var i = 0; i < checkpointsList.Count; i++)
                {
                    if (i == 0)
                    {
                        Vector3 dir = (checkpointsList[i + 1].position - checkpointsList[i].position).normalized;
                        Vector3 tangent01Pos = checkpointsList[i].position + dir * m_defaultTangentLength.floatValue;
                        camPathManager.spotPosList.Add(checkpointsList[i].position - camPathManager.transform.position);
                        camPathManager.spotPosList.Add(tangent01Pos - camPathManager.transform.position);
                    }
                    else if (i < checkpointsList.Count - 1)
                    {
                        Vector3 dir = (checkpointsList[i + 1].position - checkpointsList[i].position).normalized;

                        Vector3 tangent01Pos = checkpointsList[i].position - dir * m_defaultTangentLength.floatValue;

                        Vector3 tangent02Pos = checkpointsList[i].position + dir * m_defaultTangentLength.floatValue;

                        camPathManager.spotPosList.Add(tangent01Pos - camPathManager.transform.position);
                        camPathManager.spotPosList.Add(checkpointsList[i].position - camPathManager.transform.position);
                        camPathManager.spotPosList.Add(tangent02Pos - camPathManager.transform.position);
                    }
                    else
                    {
                        Vector3 dir = (checkpointsList[i - 1].position - checkpointsList[i].position).normalized;
                        Vector3 tangent01Pos = checkpointsList[i].position + dir * m_defaultTangentLength.floatValue;
                        Vector3 tangent02Pos = checkpointsList[i].position - dir * m_defaultTangentLength.floatValue;
                        camPathManager.spotPosList.Add(tangent01Pos - camPathManager.transform.position);
                        camPathManager.spotPosList.Add(checkpointsList[i].position - camPathManager.transform.position);

                        Path path = FindFirstObjectByType<Path>();

                        if (path.TrackIsLooped)
                        {
                            camPathManager.spotPosList.Add(tangent02Pos - camPathManager.transform.position);

                            dir = (checkpointsList[1].position - checkpointsList[0].position).normalized;
                            tangent01Pos = checkpointsList[0].position - dir * m_defaultTangentLength.floatValue;
                            camPathManager.spotPosList.Add(tangent01Pos - camPathManager.transform.position);
                            camPathManager.spotPosList.Add(checkpointsList[0].position - camPathManager.transform.position);
                        }
                    }
                }
            }
            #endregion
        }

        void AddPoint()
        {
            #region 
            if (GUILayout.Button("Add Point"))
            {
                int spotID = m_editorCurrentSelectedPoint.intValue;
                if (spotID!=-1 && spotID != m_spotPosList.arraySize - 1)
                {
                    CamPathManager camPathManager = (CamPathManager)target;

                    Vector3 pos1 = camPathManager.spotPosList[spotID + 1];
                    Vector3 pos2 = camPathManager.spotPosList[spotID + 2];

                    Vector3 dir = (pos2 - pos1).normalized;
                    float dist = Vector3.Distance(pos2, pos1);
                    Vector3 pivot = pos1 + dir * dist * .5f;

                    Vector3 tangent1 = pivot - dir * m_defaultTangentLength.floatValue;
                    Vector3 tangent2 = pivot + dir * m_defaultTangentLength.floatValue;

                    m_spotPosList.InsertArrayElementAtIndex(m_spotPosList.arraySize-1);
                    m_spotPosList.GetArrayElementAtIndex(m_spotPosList.arraySize - 1).vector3Value = tangent1;

                    m_spotPosList.InsertArrayElementAtIndex(m_spotPosList.arraySize - 1);
                    m_spotPosList.GetArrayElementAtIndex(m_spotPosList.arraySize - 1).vector3Value = pivot;

                    m_spotPosList.InsertArrayElementAtIndex(m_spotPosList.arraySize - 1);
                    m_spotPosList.GetArrayElementAtIndex(m_spotPosList.arraySize - 1).vector3Value = tangent2;

                    m_spotPosList.MoveArrayElement(m_spotPosList.arraySize - 1, spotID+2);
                    m_spotPosList.MoveArrayElement(m_spotPosList.arraySize - 1, spotID+2);
                    m_spotPosList.MoveArrayElement(m_spotPosList.arraySize - 1, spotID+2);
                }
            }
            #endregion
        }

        void RemovePoint()
        {
            #region 
            if (GUILayout.Button("Remove Point"))
            {
                int spotID = m_editorCurrentSelectedPoint.intValue;
                if (spotID != -1 && spotID != m_spotPosList.arraySize - 1 && spotID != 0)
                {
                    m_spotPosList.DeleteArrayElementAtIndex(spotID);
                    m_spotPosList.DeleteArrayElementAtIndex(spotID);
                    m_spotPosList.DeleteArrayElementAtIndex(spotID-1);
                }
            }
            #endregion
        }

    }
}
#endif
