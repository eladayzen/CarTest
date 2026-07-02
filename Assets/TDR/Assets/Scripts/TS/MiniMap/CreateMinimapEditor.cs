//Description: CreateMinimapEditor: Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
    [CustomEditor(typeof(CreateMinimap))]
    public class CreateMinimapEditor : Editor
    {
        SerializedProperty SeeInspector;                                            // use to draw default Inspector
        SerializedProperty helpBox;
        SerializedProperty moreOptions;
        SerializedProperty grpPath;

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

        void OnEnable()
        {
            #region Init Inspector Color
            listColor.Clear();
            listGUIStyle.Clear();
            for (var i = 0; i < inspectorColor.listColor.Length; i++)
            {
                listTex.Add(MakeTex(2, 2, inspectorColor.ReturnColor(i)));
                listGUIStyle.Add(new GUIStyle());
                listGUIStyle[i] = new GUIStyle(); listGUIStyle[i].normal.background = listTex[i];
            }
            #endregion

            #region
            // Setup the SerializedProperties.
            SeeInspector = serializedObject.FindProperty("SeeInspector");
            helpBox = serializedObject.FindProperty("helpBox");
            moreOptions = serializedObject.FindProperty("moreOptions");
            grpPath = serializedObject.FindProperty("grpPath");

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
            EditorGUILayout.LabelField("HelpBox:", GUILayout.Width(85));
            EditorGUILayout.PropertyField(helpBox, new GUIContent(""), GUILayout.Width(30));

            if (EditorPrefs.GetBool("MoreOptions") == true)
            {
                EditorGUILayout.LabelField("More Options:", GUILayout.Width(85));
                EditorGUILayout.PropertyField(moreOptions, new GUIContent(""), GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();

            if (grpPath.objectReferenceValue == null)
            {
                Path path = FindFirstObjectByType<Path>();
                if (path) grpPath.objectReferenceValue = path.gameObject;
                else
                    EditorGUILayout.HelpBox(
                    "The scene doesn't contain a track path." + "\n" +
                    "You must have a path in the Hierachy to be able to create the Minimap.", MessageType.Warning);
            }
            else
            {
                GeneratePath();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("");
            #endregion
        }

        private void GeneratePath()
        {
            #region 
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path Container:", GUILayout.Width(1));
            EditorGUILayout.PropertyField(grpPath, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (grpPath.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
               "You must select a path in the Hierachy to be able to create the Minimap.", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Generate Minimap"))
                {

                    CreateMinimap myScript = (CreateMinimap)target;

                    SerializedObject serializedObject2 = new SerializedObject(myScript.lineRenderer.GetComponent<LineRenderer>());

                    serializedObject2.Update();
                    SerializedProperty m_Positions = serializedObject2.FindProperty("m_Positions");
                    SerializedProperty m_Loop = serializedObject2.FindProperty("m_Loop");

                    //Debug.Log(m_Loop.boolValue);

                    if (myScript.grpPath.GetComponent<Path>().TrackIsLooped)
                        m_Loop.boolValue = true;
                    else
                        m_Loop.boolValue = false;

                    m_Positions.ClearArray();

                    int howManyPathSpots = myScript.grpPath.GetComponent<Path>().checkpoints.Count;

                    for (var i = howManyPathSpots - 1; i >= 0; i--)
                    {
                        m_Positions.InsertArrayElementAtIndex(0);
                        Vector3 newPos = new Vector3(
                            myScript.grpPath.GetComponent<Path>().checkpoints[i].transform.position.x,
                            myScript.lineRenderer.transform.position.y,
                            myScript.grpPath.GetComponent<Path>().checkpoints[i].transform.position.z);
                        m_Positions.GetArrayElementAtIndex(0).vector3Value = newPos;
                    }

                    UpdateCameraPosition(m_Positions);

                    serializedObject2.ApplyModifiedProperties();

                    Selection.activeGameObject = myScript.cam.gameObject;
                }
            }
            #endregion
        }

        void UpdateCameraPosition(SerializedProperty m_Positions)
        {
            #region 
            CreateMinimap myScript = (CreateMinimap)target;
            SerializedObject serializedObject3 = new SerializedObject(myScript.cam.GetComponent<Transform>());
            serializedObject3.Update();
            SerializedProperty m_CamPositions = serializedObject3.FindProperty("m_LocalPosition");

            int howManyPositions = m_Positions.arraySize;
            Vector3[] posList = new Vector3[howManyPositions];

            for (var i = 0; i < posList.Length; i++)
            {
                posList[i] = m_Positions.GetArrayElementAtIndex(i).vector3Value;
            }

            Vector3 center = FindCenter(posList);

            m_CamPositions.vector3Value = new Vector3(center.x, 3000, center.z);

            serializedObject3.ApplyModifiedProperties();
            #endregion
        }

        public static Vector3 FindCenter(Vector3[] objs)
        {
            #region
            var bound = new Bounds(objs[0], Vector3.zero);
            for (int i = 1; i < objs.Length; i++)
                bound.Encapsulate(objs[i]);
            return bound.center;
            #endregion
        }


        void OnSceneGUI()
        {
        }
    }
}


#endif
