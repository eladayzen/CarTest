//Description: VehicleCustomizationResetEditor: Custom Editor
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using TS.Generics;
using System.Collections.Generic;

namespace TS.Generics
{
    [CustomEditor(typeof(VehicleCustomizationReset))]
    public class VehicleCustomizationResetEditor : Editor
    {

        void OnEnable()
        {
            #region

            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region
            VehicleCustomizationReset customization = (VehicleCustomizationReset)target;

            if (customization.GetComponent<VehicleCustomValue>())
                VehicleCustomValue();

            else if (customization.GetComponent<VehicleCustomModels>())
                VehicleCustomModels();

            else if (customization.GetComponent<VehicleCustomColor>())
                VehicleCustomColor();

            else if (customization.GetComponent<VehicleCustomLivery>())
                VehicleCustomLivery();

           
            #endregion
        }

        void VehicleCustomValue() 
        {
            #region
            if (GUILayout.Button("Reset"))
            {
                VehicleCustomizationReset customization = (VehicleCustomizationReset)target;

                VehicleCustomValue customValue = customization.GetComponent<VehicleCustomValue>();

                SerializedObject serializedObject = new UnityEditor.SerializedObject(customValue);

                serializedObject.Update();

                SerializedProperty m_CurrentSelection = serializedObject.FindProperty("CurrentSelection");
                SerializedProperty m_ValueList = serializedObject.FindProperty("ValueList");

                m_CurrentSelection.intValue = 0;
                m_ValueList.ClearArray();

                serializedObject.ApplyModifiedProperties();
            } 
            #endregion
        }
        void VehicleCustomModels()
        {
            #region
            if (GUILayout.Button("Reset"))
            {
                VehicleCustomizationReset customization = (VehicleCustomizationReset)target;

                VehicleCustomModels customModels = customization.GetComponent<VehicleCustomModels>();

                SerializedObject serializedObject = new UnityEditor.SerializedObject(customModels);

                serializedObject.Update();

                SerializedProperty m_CurrentSelection = serializedObject.FindProperty("CurrentSelection");
                SerializedProperty m_ModelsList = serializedObject.FindProperty("ModelsList");
                SerializedProperty m_PivotsList = serializedObject.FindProperty("PivotsList");
                SerializedProperty m_aiCustomList = serializedObject.FindProperty("aiCustomList");

                m_CurrentSelection.intValue = 0;
                m_ModelsList.ClearArray();
                m_aiCustomList.ClearArray();
                m_PivotsList.ClearArray();

                for (var i = 0; i < 4; i++)
                    m_PivotsList.InsertArrayElementAtIndex(0);

                serializedObject.ApplyModifiedProperties();
            }
            #endregion
        }

        void VehicleCustomColor()
        {
            #region
            if (GUILayout.Button("Reset"))
            {
                VehicleCustomizationReset customization = (VehicleCustomizationReset)target;

                VehicleCustomColor customColor = customization.GetComponent<VehicleCustomColor>();

                SerializedObject serializedObject = new UnityEditor.SerializedObject(customColor);

                serializedObject.Update();

                SerializedProperty m_CurrentSelection = serializedObject.FindProperty("CurrentSelection");
                SerializedProperty m_ColorList = serializedObject.FindProperty("ColorList");
                SerializedProperty m_ObjList = serializedObject.FindProperty("ObjList");
                SerializedProperty m_aiCustomList = serializedObject.FindProperty("aiCustomList");
                

                m_CurrentSelection.intValue = 0;
                m_ColorList.ClearArray();
                m_aiCustomList.ClearArray();
                m_ObjList.ClearArray();

                m_ObjList.InsertArrayElementAtIndex(0);

                serializedObject.ApplyModifiedProperties();
            }
            #endregion
        }

        void VehicleCustomLivery()
        {
            #region
            if (GUILayout.Button("Reset"))
            {
                VehicleCustomizationReset customization = (VehicleCustomizationReset)target;

                VehicleCustomLivery customLivery = customization.GetComponent<VehicleCustomLivery>();

                SerializedObject serializedObject = new UnityEditor.SerializedObject(customLivery);

                serializedObject.Update();

                SerializedProperty m_CurrentSelection = serializedObject.FindProperty("CurrentSelection");
                SerializedProperty m_liveryList = serializedObject.FindProperty("liveryList");
                SerializedProperty m_ObjList = serializedObject.FindProperty("ObjList");
                SerializedProperty m_aiCustomList = serializedObject.FindProperty("aiCustomList");

                m_CurrentSelection.intValue = 0;
                m_liveryList.ClearArray();
                m_aiCustomList.ClearArray();
                m_ObjList.ClearArray();

                m_ObjList.InsertArrayElementAtIndex(0);

                serializedObject.ApplyModifiedProperties();
            }
            #endregion
        }
    }
}

#endif

