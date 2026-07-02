//Description: GarageInitCategoryButtonEditor: Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace TS.Generics
{
    [CustomEditor(typeof(GarageInitCategoryButton))]
    public class GarageInitCategoryButtonEditor : Editor
    {
        SerializedProperty m_currentText;
        SerializedProperty m_category;


        public TextVariousMethods txtVariousMethods;
        void OnEnable()
        {
            m_currentText = serializedObject.FindProperty("currentText");
            m_category = serializedObject.FindProperty("category");
            txtVariousMethods = new TextVariousMethods();
        }

        public override void OnInspectorGUI()
        {
            #region
        
                DrawDefaultInspector();

            if (GUILayout.Button("Select text"))
            {
                GarageInitCategoryButton g = (GarageInitCategoryButton)target;

               

                int listID = g.vehicleData.VehicleCategoryParamsList[m_category.intValue].ListID;
                int entryID = g.vehicleData.VehicleCategoryParamsList[m_category.intValue].EntryID;

                CurrentText currentText = g.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent < CurrentText>();
                Undo.RegisterCompleteObjectUndo(currentText.gameObject, currentText.gameObject.name);
                currentText.tab = listID;
                currentText._Entry = entryID;
               // string txt = LanguageManager.instance.String_ReturnText(listID, entryID);
               // currentText.DisplayTextComponent(currentText.gameObject, txt);


                Selection.activeGameObject = g.transform.GetChild(0).GetChild(1).GetChild(1).gameObject;
            }


            #endregion
        }

        void OnSceneGUI()
        {
        }
    }
}

#endif
