//Description: VehicleCustomizationEditor
#if (UNITY_EDITOR)
using UnityEngine;

using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
    [CustomEditor(typeof(CustomizationManager))]
    public class CustomizationManagerEditor : Editor
    {
        SerializedProperty m_seeInspector;

        SerializedProperty m_category;
        SerializedProperty m_categoryRef;
        SerializedProperty m_grpCategory;

        SerializedProperty m_categoryName;


        SerializedProperty m_categoryButtonList;

        void OnEnable()
        {
            #region
            m_seeInspector = serializedObject.FindProperty("seeInspector");
            m_category = serializedObject.FindProperty("category");
            m_categoryRef = serializedObject.FindProperty("categoryRef");
            m_grpCategory = serializedObject.FindProperty("grpCategory");

            m_categoryName = serializedObject.FindProperty("categoryName");
        
            m_categoryButtonList = serializedObject.FindProperty("CategoryButtonList");
            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region
            serializedObject.Update();
          /*  EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("See Inspector:", GUILayout.Width(85));
            EditorGUILayout.PropertyField(m_seeInspector, new GUIContent(""), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
          */
           
            CreateNewCategorySection();
            serializedObject.ApplyModifiedProperties();

           // if (m_seeInspector.boolValue)                         // If true Default Inspector is drawn on screen
                DrawDefaultInspector();
            #endregion
        }

        void CreateNewCategorySection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type:", GUILayout.Width(100));
            EditorGUILayout.PropertyField(m_category, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(100));
            EditorGUILayout.PropertyField(m_categoryName, new GUIContent(""));
            EditorGUILayout.EndHorizontal();


            #region
            if (GUILayout.Button("Create New Category", GUILayout.Height(30)))
            {
                bool nameAlreadyExist = false;

                for (var i = 0; i < m_categoryButtonList.arraySize; i++)
                {
                    if (m_categoryButtonList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == m_categoryName.stringValue)
                        nameAlreadyExist = true;
                }

                if (nameAlreadyExist &&
           EditorUtility.DisplayDialog("Error",
               "The name already exist. Each category must have a unique name." , "Continue"))
                {

                }
                else
                {
                    GameObject parent = (GameObject)m_grpCategory.objectReferenceValue;
                    GameObject obj = (GameObject)Instantiate(m_categoryRef.GetArrayElementAtIndex(m_category.enumValueIndex).objectReferenceValue, parent.transform);


                    obj.name = m_categoryName.stringValue;

                    m_categoryButtonList.InsertArrayElementAtIndex(0);

                    m_categoryButtonList.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue = m_categoryName.stringValue;
                    m_categoryButtonList.GetArrayElementAtIndex(0).FindPropertyRelative("objButton").objectReferenceValue = obj;

                    m_categoryButtonList.MoveArrayElement(0, m_categoryButtonList.arraySize - 1);

                    obj.SetActive(true);

                    //UpdateSprites(obj);


                    Undo.RegisterCreatedObjectUndo(obj, obj.name);
                }   
            }
            EditorGUILayout.LabelField("");
            #endregion
        }
    }
}

#endif

