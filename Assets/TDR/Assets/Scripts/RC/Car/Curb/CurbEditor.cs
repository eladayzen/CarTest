//Description: CurbEditor: Custom editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    [CustomEditor(typeof(Curb))]
    public class CurbEditor : Editor
    {
        SerializedProperty SeeInspector;                                            // use to draw default Inspector
        SerializedProperty moreOptions;
        SerializedProperty helpBox;
        SerializedProperty m_roadRefForCurb;
        SerializedProperty m_selectStart;
        SerializedProperty m_selectStop;
        SerializedProperty m_direction;


        void OnEnable()
        {

            #region
            // Setup the SerializedProperties.
            SeeInspector = serializedObject.FindProperty("SeeInspector");
            moreOptions = serializedObject.FindProperty("moreOptions");
            helpBox = serializedObject.FindProperty("helpBox");
            m_roadRefForCurb = serializedObject.FindProperty("roadRefForCurb");
            m_selectStart = serializedObject.FindProperty("selectStart");
            m_selectStop = serializedObject.FindProperty("selectStop");
            m_direction = serializedObject.FindProperty("direction");
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


            if (EditorPrefs.GetBool("MoreOptions") == true)
            {
                EditorGUILayout.LabelField("More Options:", GUILayout.Width(85));
                EditorGUILayout.PropertyField(moreOptions, new GUIContent(""), GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();



            if (m_roadRefForCurb.objectReferenceValue)
            {
                EditorGUILayout.LabelField("");
                Direction();
                EditorGUILayout.LabelField("");
                SelectionSection();

                if (GUILayout.Button("Copy Path From Ref Road", GUILayout.MinWidth(100), GUILayout.Height(40)))
                    CopyPath();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("");
            #endregion
        }

        void Direction()
        {
            EditorGUILayout.LabelField("Border:", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.PropertyField(m_direction, new GUIContent(""));
        }

        void SelectionSection()
        {
            #region
            Curb curb = (Curb)target;

            // DisplaySelectionBorderSize();


            int minRef = 0;
            int maxref = curb.roadRefForCurb.distVecList.Count - 1;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selection:", EditorStyles.boldLabel, GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", GUILayout.MinWidth(100)))
            {
                m_selectStart.intValue = minRef;
                m_selectStop.intValue = maxref;
            }
            if (GUILayout.Button("No Selection", GUILayout.MinWidth(100)))
            {
                m_selectStart.intValue = minRef;
                m_selectStop.intValue = minRef;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min: ", GUILayout.Width(50));
            m_selectStart.intValue = EditorGUILayout.IntSlider(m_selectStart.intValue, minRef, maxref);


            if (GUILayout.Button("-", GUILayout.Width(20)))
                m_selectStart.intValue--;

            if (GUILayout.Button("+", GUILayout.Width(20)))
                m_selectStart.intValue++;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max: ", GUILayout.Width(50));
            m_selectStop.intValue = EditorGUILayout.IntSlider(m_selectStop.intValue, minRef, maxref);

            if (GUILayout.Button("-", GUILayout.Width(20)))
                m_selectStop.intValue--;

            if (GUILayout.Button("+", GUILayout.Width(20)))
                m_selectStop.intValue++;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (m_selectStart.intValue > m_selectStop.intValue)
                    m_selectStart.intValue = m_selectStop.intValue;
                if (m_selectStop.intValue < m_selectStart.intValue)
                    m_selectStop.intValue = m_selectStart.intValue;
            }

            EditorGUILayout.LabelField("");
            #endregion
        }

        void CopyPath()
        {

            Curb curb = (Curb)target;

            Bezier bezier = curb.GetComponent<Bezier>();

            Undo.RegisterFullObjectHierarchyUndo(bezier.gameObject, bezier.name);

            bezier.pointsList.Clear();

            var start = curb.selectStart;
            start = Mathf.Clamp(start, 0, curb.selectStop);

            var stop = curb.selectStop;
            stop = Mathf.Clamp(stop, curb.selectStart, curb.roadRefForCurb.distVecList.Count - curb.accuracy);

            var multiplier = 1;
            if (curb.direction == Curb.Direction.Left)
                multiplier = -1;

            var distanceToRoad = multiplier * curb.roadRefForCurb.roadSize * .5f;
            if (curb.direction == Curb.Direction.Right)
                distanceToRoad += curb.curbOffsetRight;

            for (var i = start; i < stop; i++)
            {
                if (i % curb.accuracy == 0)
                {
                    var dir = (curb.roadRefForCurb.distVecList[i].spotPos - curb.roadRefForCurb.distVecList[i + 1].spotPos).normalized;

                    var left = distanceToRoad * Vector3.Cross(dir, Vector3.up).normalized;

                    bezier.pointsList.Add(new PointDescription(curb.roadRefForCurb.distVecList[i].spotPos + left, Quaternion.identity));
                }

            }
            // if (curb.direction == Curb.Direction.Right)
            //   bezier.pointsList.Reverse();

            PrefabUtility.RecordPrefabInstancePropertyModifications(bezier);
        }
    }
}


#endif
