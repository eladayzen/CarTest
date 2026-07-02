// Description : w_VehicleManager.cs :  window to manage vehicle
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace TS.Generics
{
    public class w_VehicleManager : EditorWindow
    {
        private Vector2 scrollPosAll;

        VehicleGlobalData _VehicleData;
        SerializedObject serializedObject;
        SerializedProperty HelpBox;
        SerializedProperty MoreOptions;
        SerializedProperty carParametersList;

        globalTextDatas _globalTextDatas;
        SerializedObject serializedObjectTxtRef;

        //-> Vehicle UI Colors
        SerializedObject serializedObjectUICOlor;
        SerializedProperty listVehicleUIColorsDatas;
        VehicleUIColorsDatas vehicleUIColorsDatas;

        //-> Difficulty Manager
        SerializedObject serializedObjectDiffManager;
        SerializedProperty difficultyParamsList;
        SerializedProperty m_VehicleCategoryParamsList;
        DifficultyManagerData difficultyManagerData;
        public List<String> difficultyNamesList = new List<string>();


        [MenuItem("Tools/TS/w_VehicleManager")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(w_VehicleManager));
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


        void OnEnable()
        {
            #region 
            //-> Access Track data
            string objectPath = "Assets/TDR/Assets/Datas/Ref/VehicleGlobalData.asset";
            _VehicleData = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as VehicleGlobalData;

            if (_VehicleData)
            {
                serializedObject = new UnityEditor.SerializedObject(_VehicleData);
                HelpBox = serializedObject.FindProperty("HelpBox");
                MoreOptions = serializedObject.FindProperty("MoreOptions");
                carParametersList = serializedObject.FindProperty("carParametersList");
                m_VehicleCategoryParamsList = serializedObject.FindProperty("VehicleCategoryParamsList");
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
            }

            //-> Access Multi Language texts
            string objectPathTxtData = "Assets/TDR/Assets/Datas/Ref/globalTextDatas.asset";
            _globalTextDatas = AssetDatabase.LoadAssetAtPath(objectPathTxtData, typeof(UnityEngine.Object)) as globalTextDatas;

            if (_globalTextDatas)
            {
                serializedObjectTxtRef = new UnityEditor.SerializedObject(_globalTextDatas);
                serializedObjectTxtRef.Update();
                serializedObjectTxtRef.ApplyModifiedProperties();
            }

            objectPath = "Assets/TDR/Assets/Datas/Ref/VehicleUIColorsDatas.asset";
            vehicleUIColorsDatas = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as VehicleUIColorsDatas;

            if (vehicleUIColorsDatas)
            {
                serializedObjectUICOlor = new UnityEditor.SerializedObject(vehicleUIColorsDatas);

                listVehicleUIColorsDatas = serializedObjectUICOlor.FindProperty("listVehicleUIColorsDatas");
                serializedObjectUICOlor.Update();
                serializedObjectUICOlor.ApplyModifiedProperties();
            }

            objectPath = "Assets/TDR/Assets/Datas/Ref/DifficultyManagerData.asset";
            difficultyManagerData = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as DifficultyManagerData;

            if (difficultyManagerData)
            {
                serializedObjectDiffManager = new UnityEditor.SerializedObject(difficultyManagerData);

                difficultyParamsList = serializedObjectDiffManager.FindProperty("difficultyParamsList");

                serializedObjectDiffManager.Update();
                difficultyNamesList = GenerateDifficultyNamesListList();
                serializedObjectDiffManager.ApplyModifiedProperties();
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

            serializedObject.Update();
            if (_VehicleData && _globalTextDatas)
            {
                if (listTex.Count == 0 ||
                    listTex.Count > 0 && listTex[0] == null)
                {
                    InitColorStyle();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("HelpBox:", GUILayout.Width(85));
                EditorGUILayout.PropertyField(HelpBox, new GUIContent(""), GUILayout.Width(30));

                if (EditorPrefs.GetBool("MoreOptions") == true)
                {
                    EditorGUILayout.LabelField("More Options:", GUILayout.Width(85));
                    EditorGUILayout.PropertyField(MoreOptions, new GUIContent(""), GUILayout.Width(30));
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("");

                SerializedProperty m_tab = serializedObject.FindProperty("tab");

                m_tab.intValue = GUILayout.SelectionGrid(m_tab.intValue, new string[] { "Vehicles Setup", "Player | AI Names", "UI Color", "Difficulty Manager", "Category" }, 2);

                EditorGUILayout.LabelField("");

                switch (m_tab.intValue)
                {
                    case 0:
                        ShowItemsList();
                        AddVehicleToList();
                        break;
                    case 1:
                        ShowPlayerAndAiNamesList();
                        break;
                    case 2:
                        GlobalVehiclesUIColor();
                        break;
                    case 3:
                        DifficultyManager();
                        break;
                    case 4:
                        VehicleCategory();
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            #endregion
        }

        public void ShowDataAndReset()
        {
            #region 
            //-> Reset Zone
            if (GUILayout.Button("Delete Tracks and Leaderboards saved data"))
            {
                if (EditorUtility.DisplayDialog("Delete all the saved files",
                                            "Are you sure you delete " + "TL" + ".Dat", "Yes", "No"))
                {
                    DeleteData();
                }
            }
            if (GUILayout.Button("Show .Dat In Explorer"))
            {
                ShowDataInExplorer();
            }
            #endregion
        }

        public void DeleteData()
        {
            #region 
            string itemPath = Application.persistentDataPath;
            itemPath = itemPath.TrimEnd(new[] { '\\', '/' });
            FileUtil.DeleteFileOrDirectory(itemPath + "/" + "TL" + ".dat");
            #endregion
        }

        //-> Create a new track at the end of the list
        public void AddVehicleToList()
        {
            #region 
            EditorGUILayout.BeginVertical(listGUIStyle[3]);
            if (GUILayout.Button("Add a new vehicle to the List", GUILayout.Height(50)))
            {
                carParametersList.InsertArrayElementAtIndex(carParametersList.arraySize);
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("bShowInEditor").boolValue = true;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("name").stringValue = "New Name";
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("bShow").boolValue = true;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("Prefab").objectReferenceValue = null;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("speed").floatValue = 50;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("vehicleCategory").intValue = 0;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("damageResistance").intValue = 10;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterPower").floatValue = 70;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterDuration").floatValue = 2;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterCooldown").floatValue = 4;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("coinMultiplier").floatValue = 5;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("cost").intValue = 0;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("isUnlocked").boolValue = true;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("speedAI").floatValue = 50;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("damageResistanceAI").intValue = 10;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterPowerAI").floatValue = 60;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterDurationAI").floatValue = 2;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("boosterCooldownAI").floatValue = .1f;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("prefabScaleMultiplierInMenu").floatValue = 1;
                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("offsetselectionCamP1RotationZInMenu").floatValue = 1;

                carParametersList.GetArrayElementAtIndex(carParametersList.arraySize - 1).FindPropertyRelative("vehicleCategory").intValue = 0;
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        void ShowItemsList()
        {
            #region
            for (var i = 0; i < carParametersList.arraySize; i++)
            {
                SerializedProperty m_bShowInEditor = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("bShowInEditor");
                SerializedProperty m_name = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("name");

                SerializedProperty m_bShow = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("bShow");
                SerializedProperty m_Prefab = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab");

                SerializedProperty m_Img = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("img");

                SerializedProperty m_vehicleCategory = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("vehicleCategory");

                SerializedProperty m_speed = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("speed");
                SerializedProperty m_damageResistance = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("damageResistance");

                SerializedProperty m_boosterPower = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterPower");
                SerializedProperty m_boosterDuration = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterDuration");
                SerializedProperty m_boosterCooldown = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterCooldown");
                SerializedProperty m_cost = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("cost");
                SerializedProperty m_isUnlocked = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("isUnlocked");

                SerializedProperty m_speedAI = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("speedAI");
                SerializedProperty m_damageResistanceAI = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("damageResistanceAI");

                SerializedProperty m_boosterPowerAI = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterPowerAI");
                SerializedProperty m_boosterDurationAI = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterDurationAI");
                SerializedProperty m_boosterCooldownAI = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("boosterCooldownAI");
                SerializedProperty m_prefabScaleMultiplierInMenu = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("prefabScaleMultiplierInMenu");
                SerializedProperty m_offsetselectionCamP1RotationZInMenu = carParametersList.GetArrayElementAtIndex(i).FindPropertyRelative("offsetselectionCamP1RotationZInMenu");


                EditorGUILayout.BeginVertical(listGUIStyle[0]);
                EditorGUILayout.BeginHorizontal(listGUIStyle[3]);
                EditorGUILayout.LabelField(i + ":", GUILayout.Width(15));
                EditorGUILayout.PropertyField(m_bShowInEditor, new GUIContent(""), GUILayout.Width(20));
                //-> Select the name of the track using the multilanguage system
                EditorGUILayout.LabelField(m_name.stringValue, EditorStyles.boldLabel);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    carParametersList.DeleteArrayElementAtIndex(i);
                    DeleteData();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (m_bShowInEditor.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_name, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    DisplayCategory(m_vehicleCategory);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Is Unlocked: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_isUnlocked, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Prefab: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_Prefab, new GUIContent(""), GUILayout.MinWidth(90));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(110));

                    if (GUILayout.Button("Update Vehicle Customization ID"))
                    {
                        GameObject prefab = (GameObject)m_Prefab.objectReferenceValue;
                        string assetPath = AssetDatabase.GetAssetPath(prefab);
                        var myPrefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(assetPath);
                        VehicleCustomization vCustom = myPrefab.GetComponentInChildren<VehicleCustomization>();
                        vCustom.VehicleID = i;
                        EditorUtility.SetDirty(vCustom);
                        PrefabUtility.SavePrefabAsset(myPrefab);

                        if (EditorUtility.DisplayDialog("Update Vehicle Customization ID",
                       "ID = " + i, "Done"))
                        { }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.HelpBox("Don't Forget to press the button after replacing the prefab.", MessageType.Warning);

                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Sprite: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_Img, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                    /*EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Menu Cam: ", GUILayout.Width(110));
                    EditorGUILayout.LabelField("Z pos: ", GUILayout.Width(35));
                    EditorGUILayout.PropertyField(m_prefabScaleMultiplierInMenu, new GUIContent(""), GUILayout.MinWidth(20));

                    EditorGUILayout.LabelField("X Rot: ", GUILayout.Width(35));
                    EditorGUILayout.PropertyField(m_offsetselectionCamP1RotationZInMenu, new GUIContent(""), GUILayout.MinWidth(20));
                    EditorGUILayout.EndHorizontal();
                    */
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Show In Game: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_bShow, new GUIContent(""), GUILayout.MinWidth(20));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Price: ", GUILayout.Width(110));
                    EditorGUILayout.PropertyField(m_cost, new GUIContent(""), GUILayout.MinWidth(20));
                    EditorGUILayout.EndHorizontal();

                    /*  EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.BeginVertical();
                      EditorGUILayout.BeginVertical(listGUIStyle[1]);
                      EditorGUILayout.LabelField("Player:", EditorStyles.boldLabel);
                      EditorGUILayout.EndVertical();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("Speed: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_speed, new GUIContent(""), GUILayout.MinWidth(90));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("Damage Resitance: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_damageResistance, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("Booster Power: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterPower, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("Booster Duration: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterDuration, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("Booster Cooldown: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterCooldown, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.EndVertical();

                      EditorGUILayout.BeginVertical();
                      EditorGUILayout.BeginVertical(listGUIStyle[1]);
                      EditorGUILayout.LabelField("| AI:", EditorStyles.boldLabel);
                      EditorGUILayout.EndVertical();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("| Speed: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_speedAI, new GUIContent(""), GUILayout.MinWidth(90));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("| Damage Resitance: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_damageResistanceAI, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();

                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("| Booster Power: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterPowerAI, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("| Booster Duration: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterDurationAI, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField("| Booster Cooldown: ", GUILayout.Width(120));
                      EditorGUILayout.PropertyField(m_boosterCooldownAI, new GUIContent(""), GUILayout.MinWidth(20));
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.EndVertical();
                      EditorGUILayout.EndHorizontal();
                    */
                }
                EditorGUILayout.EndVertical();

                if (m_bShowInEditor.boolValue)
                    EditorGUILayout.LabelField("");
            }
            #endregion
        }

        public void ShowPlayerAndAiNamesList()
        {
            #region
            //-> Display Player Names
            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.BeginHorizontal(listGUIStyle[3]);
            EditorGUILayout.LabelField("Player names: ", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            SerializedProperty m_playerNamesList = serializedObject.FindProperty("playerNamesList");

            for (var i = 0; i < m_playerNamesList.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ": ", GUILayout.Width(20));

                SerializedProperty m_listID = m_playerNamesList.GetArrayElementAtIndex(i).FindPropertyRelative("listID");
                SerializedProperty m_EntryID = m_playerNamesList.GetArrayElementAtIndex(i).FindPropertyRelative("EntryID");

                EditorGUILayout.LabelField("listID: ", GUILayout.Width(40));
                EditorGUILayout.PropertyField(m_listID, new GUIContent(""), GUILayout.Width(40));
                EditorGUILayout.LabelField("ID: ", GUILayout.Width(20));
                EditorGUILayout.PropertyField(m_EntryID, new GUIContent(""), GUILayout.Width(40));

                //-> Player Name
                string sName = ReturnTrackName(
                    m_listID.intValue,
                    m_EntryID.intValue);

                EditorGUILayout.LabelField(" -> " + sName, GUILayout.MinWidth(60));

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("");

            //-> Display AI Names
            SerializedProperty m_aiNamesList = serializedObject.FindProperty("aiNamesList");
            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.BeginHorizontal(listGUIStyle[3]);
            EditorGUILayout.LabelField("AI names: ", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("Random Order: ", GUILayout.Width(90));
            SerializedProperty m_aiNameRandom = serializedObject.FindProperty("aiNameRandom");
            EditorGUILayout.PropertyField(m_aiNameRandom, new GUIContent(""), GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < m_aiNamesList.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i + ": ", GUILayout.Width(20));

                SerializedProperty m_listID = m_aiNamesList.GetArrayElementAtIndex(i).FindPropertyRelative("listID");
                SerializedProperty m_EntryID = m_aiNamesList.GetArrayElementAtIndex(i).FindPropertyRelative("EntryID");

                EditorGUILayout.LabelField("listID: ", GUILayout.Width(40));
                EditorGUILayout.PropertyField(m_listID, new GUIContent(""), GUILayout.Width(40));
                EditorGUILayout.LabelField("ID: ", GUILayout.Width(20));
                EditorGUILayout.PropertyField(m_EntryID, new GUIContent(""), GUILayout.Width(40));

                //-> Player Name
                string sName = ReturnTrackName(
                    m_listID.intValue,
                    m_EntryID.intValue);

                EditorGUILayout.LabelField(" -> " + sName, GUILayout.MinWidth(60));

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    m_aiNamesList.DeleteArrayElementAtIndex(i);
                    break;
                }
                if (GUILayout.Button("^", GUILayout.Width(20)))
                {
                    m_aiNamesList.MoveArrayElement(i, Mathf.Clamp(i - 1, 0, m_aiNamesList.arraySize));
                }
                if (GUILayout.Button("v", GUILayout.Width(20)))
                {
                    m_aiNamesList.MoveArrayElement(i, Mathf.Clamp(i + 1, 0, m_aiNamesList.arraySize));
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add new AI name"))
            {
                m_aiNamesList.InsertArrayElementAtIndex(0);
                m_aiNamesList.GetArrayElementAtIndex(0).FindPropertyRelative("EntryID").intValue = 141;
                m_aiNamesList.MoveArrayElement(0, Mathf.Clamp(m_aiNamesList.arraySize - 1, 0, m_aiNamesList.arraySize));
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        void GlobalVehiclesUIColor()
        {
            #region
            serializedObjectUICOlor.Update();
            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Vehicle UI Colors:", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            if (HelpBox.boolValue) HelpZone(0);

            EditorGUILayout.LabelField("");

            for (var i = 0; i < listVehicleUIColorsDatas.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Vehicle " + i + " :", EditorStyles.boldLabel, GUILayout.Width(70));
                EditorGUILayout.PropertyField(listVehicleUIColorsDatas.GetArrayElementAtIndex(i), new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            serializedObjectUICOlor.ApplyModifiedProperties();
            #endregion
        }

        void DifficultyManager()
        {
            #region
            serializedObjectDiffManager.Update();

            SerializedProperty howManyCarAI = serializedObjectDiffManager.FindProperty("howManyCarAI");
            SerializedProperty difficultyName = serializedObjectDiffManager.FindProperty("difficultyName");
            SerializedProperty currentDifficulty = serializedObjectDiffManager.FindProperty("currentEditorDifficulty");

            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.LabelField("Difficulty Manager:", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndVertical();

            SerializedProperty m_tabDifficulty = serializedObjectDiffManager.FindProperty("tabDifficulty");
            m_tabDifficulty.intValue = GUILayout.Toolbar(m_tabDifficulty.intValue, new string[] { "Vehicle AI Parameters", "Other" }, GUILayout.MinWidth(30));

            EditorGUILayout.LabelField("");

            switch (m_tabDifficulty.intValue)
            {
                case 0:
                    CarAIParameters(currentDifficulty);
                    break;
                case 1:
                    Other(difficultyName, howManyCarAI, currentDifficulty);
                    break;
            }
            serializedObjectDiffManager.ApplyModifiedProperties();
            #endregion
        }

        void CarAIParameters(SerializedProperty currentDifficulty)
        {
            #region
            //-> Display selected difficulty
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Difficulty: ", GUILayout.Width(80));
            currentDifficulty.intValue = EditorGUILayout.Popup(currentDifficulty.intValue, difficultyNamesList.ToArray());              // --> Display all methods
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");

            for (var i = 0; i < difficultyParamsList.arraySize; i++)
            {
                if (currentDifficulty.intValue == i)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Force Applied Offset:", EditorStyles.boldLabel, GUILayout.Width(140));
                    EditorGUILayout.PropertyField(difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("forceAppliedOffset"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Global Speed Offset:", EditorStyles.boldLabel, GUILayout.Width(140));
                    EditorGUILayout.PropertyField(difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("globalSpeedOffset"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Chase Force Applied Offset:", EditorStyles.boldLabel, GUILayout.Width(140));
                    EditorGUILayout.PropertyField(difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("chaseForceAppliedOffset"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Chase Speed Offset:", EditorStyles.boldLabel, GUILayout.Width(140));
                    SerializedProperty _chaseSpeedOffset = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("chaseSpeedOffset");
                    _chaseSpeedOffset.floatValue = EditorGUILayout.Slider(_chaseSpeedOffset.floatValue, 0, 9);

                    //EditorGUILayout.PropertyField(difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("chaseSpeedOffset"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    for (var j = 0; j < difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").arraySize; j++)
                    {
                        EditorGUILayout.BeginVertical(listGUIStyle[0]);
                        EditorGUILayout.BeginVertical(listGUIStyle[2]);
                        EditorGUILayout.LabelField("Vehicle AI " + j + " :", EditorStyles.boldLabel, GUILayout.Width(90));
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("ChooseBestAltPath:", EditorStyles.boldLabel, GUILayout.Width(140));
                        SerializedProperty chooseBestPath = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").GetArrayElementAtIndex(j).FindPropertyRelative("chooseBestAltPath");
                        chooseBestPath.floatValue = EditorGUILayout.Slider(chooseBestPath.floatValue, 0, 100);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space();
                    }
                }
            }
            #endregion
        }

        void Other(SerializedProperty difficultyName, SerializedProperty howManyCarAI, SerializedProperty currentDifficulty)
        {
            #region 
            //-> Creation options
            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.LabelField("Add new difficulty Mode", EditorStyles.boldLabel);
            if (HelpBox.boolValue) HelpZone(2);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Difficulty -> Name: ", GUILayout.Width(140));
            EditorGUILayout.PropertyField(difficultyName, new GUIContent(""), GUILayout.MinWidth(100));
            if (GUILayout.Button("Add", GUILayout.MinWidth(50)))
            {
                //-> Check if difficultyName already exist
                bool bExist = false;
                for (var i = 0; i < difficultyParamsList.arraySize; i++)
                {
                    if (difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == difficultyName.stringValue)
                    {
                        bExist = true;
                        break;
                    }
                }

                if (!bExist)
                {
                    difficultyParamsList.InsertArrayElementAtIndex(0);
                    difficultyParamsList.GetArrayElementAtIndex(0).FindPropertyRelative("aICarParams").ClearArray();

                    difficultyParamsList.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue = difficultyName.stringValue;

                    for (var i = 0; i < howManyCarAI.intValue; i++)
                    {
                        difficultyParamsList.GetArrayElementAtIndex(0).FindPropertyRelative("aICarParams").InsertArrayElementAtIndex(0);
                    }
                    difficultyParamsList.MoveArrayElement(0, difficultyParamsList.arraySize - 1);
                    difficultyNamesList = GenerateDifficultyNamesListList();
                    currentDifficulty.intValue = difficultyParamsList.arraySize - 1;
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Name already exist",
                    "Each Difficulty must have a unique name.", "Continue", ""))
                    {
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");

            //-> Display all the difficulty parameters
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID ", GUILayout.Width(20));
            EditorGUILayout.LabelField("| Name: ", GUILayout.MinWidth(70));
            EditorGUILayout.LabelField("| Text Folder:", GUILayout.MinWidth(80));
            EditorGUILayout.LabelField("| ID:", GUILayout.MinWidth(30));
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < difficultyParamsList.arraySize; i++)
            {
                SerializedProperty name = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                SerializedProperty folderTxt = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("folderTxt");
                SerializedProperty idTxt = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("idTxt");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ":", GUILayout.Width(20));
                EditorGUILayout.PropertyField(name, new GUIContent(""), GUILayout.MinWidth(70));
                EditorGUILayout.PropertyField(folderTxt, new GUIContent(""), GUILayout.MinWidth(80));
                EditorGUILayout.PropertyField(idTxt, new GUIContent(""), GUILayout.MinWidth(70));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("");

            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.LabelField("Add car in each difficulty mode", EditorStyles.boldLabel);
            //-> Section Add new car at the end of AI Difficulty list
            if (HelpBox.boolValue) HelpZone(1);
            if (GUILayout.Button("Add new car at the end of AI Difficulty list", GUILayout.Height(30)))
            {
                howManyCarAI.intValue += 1;

                //-> Add car at the end of each difficulty list
                for (var i = 0; i < difficultyParamsList.arraySize; i++)
                {
                    difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").InsertArrayElementAtIndex(0);
                    difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").GetArrayElementAtIndex(0).FindPropertyRelative("speedOffset").floatValue = 0;
                    difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").GetArrayElementAtIndex(0).FindPropertyRelative("aggressiveness").floatValue = 0;
                    difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").MoveArrayElement(0, difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("aICarParams").arraySize - 1);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("");
            #endregion
        }

        void VehicleCategory()
        {
            #region 
            EditorGUILayout.BeginVertical(listGUIStyle[0]);
            EditorGUILayout.LabelField("Vehicle Category:", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndVertical();

            if (m_VehicleCategoryParamsList.arraySize == 0)
            {
                if (GUILayout.Button("Create First Category"))
                {
                    m_VehicleCategoryParamsList.InsertArrayElementAtIndex(0);
                }
            }


            for (var i = 0; i < m_VehicleCategoryParamsList.arraySize; i++)
            {
                // SerializedProperty m_Name = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("Name");
                SerializedProperty m_ListID = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("ListID");
                SerializedProperty m_EntryID = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("EntryID");
                SerializedProperty m_sprite = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("sprite");

                SerializedObject serializedObjectTxtList = new UnityEditor.SerializedObject(_globalTextDatas.textDatasList[0]);
                SerializedProperty m_TextsList = serializedObjectTxtList.FindProperty("TextsList");


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));

                if (m_EntryID.intValue < m_TextsList.arraySize &&
                    m_ListID.intValue < m_TextsList.GetArrayElementAtIndex(m_EntryID.intValue).FindPropertyRelative("multiLanguage").arraySize)
                {
                    SerializedProperty m_Txt = m_TextsList.GetArrayElementAtIndex(m_EntryID.intValue).FindPropertyRelative("multiLanguage").GetArrayElementAtIndex(m_ListID.intValue);
                    EditorGUILayout.LabelField("| " + m_Txt.stringValue, GUILayout.MinWidth(120));
                }
                else
                    EditorGUILayout.LabelField("| " + "Text Doesn't Exit", GUILayout.MinWidth(120));

                EditorGUILayout.LabelField("| List: ", GUILayout.Width(50));
                EditorGUILayout.PropertyField(m_ListID, new GUIContent(""), GUILayout.MinWidth(20));
                EditorGUILayout.LabelField("| ID: ", GUILayout.Width(40));
                EditorGUILayout.PropertyField(m_EntryID, new GUIContent(""), GUILayout.MinWidth(20));
                EditorGUILayout.LabelField("| Sprite: ", GUILayout.Width(50));
                EditorGUILayout.PropertyField(m_sprite, new GUIContent(""), GUILayout.MinWidth(50));
                EditorGUILayout.EndHorizontal();
            }

            if (m_VehicleCategoryParamsList.arraySize > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Category"))
                    m_VehicleCategoryParamsList.InsertArrayElementAtIndex(m_VehicleCategoryParamsList.arraySize - 1);

                if (GUILayout.Button("-", GUILayout.Width(40)))
                    m_VehicleCategoryParamsList.DeleteArrayElementAtIndex(m_VehicleCategoryParamsList.arraySize - 1);

                EditorGUILayout.EndHorizontal();
            }
            #endregion
        }

        void DisplayCategory(SerializedProperty m_VehicleCat)
        {
            #region 
            List<string> categoryName = new List<string>();
            for (var i = 0; i < m_VehicleCategoryParamsList.arraySize; i++)
            {
                SerializedProperty m_ListID = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("ListID");
                SerializedProperty m_EntryID = m_VehicleCategoryParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("EntryID");

                SerializedObject serializedObjectTxtList = new UnityEditor.SerializedObject(_globalTextDatas.textDatasList[0]);
                SerializedProperty m_TextsList = serializedObjectTxtList.FindProperty("TextsList");

                if (m_EntryID.intValue < m_TextsList.arraySize &&
                      m_ListID.intValue < m_TextsList.GetArrayElementAtIndex(m_EntryID.intValue).FindPropertyRelative("multiLanguage").arraySize)
                {
                    SerializedProperty m_Txt = m_TextsList.GetArrayElementAtIndex(m_EntryID.intValue).FindPropertyRelative("multiLanguage").GetArrayElementAtIndex(m_ListID.intValue);
                    categoryName.Add(m_Txt.stringValue);
                }
                else
                    categoryName.Add("Problem with the name of the category");
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category: ", GUILayout.Width(110));
            m_VehicleCat.intValue = EditorGUILayout.Popup(m_VehicleCat.intValue, categoryName.ToArray());

            EditorGUILayout.EndHorizontal();
            #endregion
        }

        List<string> GenerateDifficultyNamesListList()
        {
            #region 
            List<string> newList = new List<string>();
            difficultyNamesList.Clear();

            for (var i = 0; i < difficultyParamsList.arraySize; i++)
            {
                SerializedProperty m_Name = difficultyParamsList.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                newList.Add(m_Name.stringValue);
            }

            return newList;
            #endregion
        }

        void OnInspectorUpdate()
        {
            #region 
            Repaint();
            #endregion
        }

        public void ShowDataInExplorer()
        {
            #region
            string itemPath = Application.persistentDataPath;
            itemPath = itemPath.TrimEnd(new[] { '\\', '/' });
            System.Diagnostics.Process.Start(itemPath);
            #endregion
        }

        string ReturnTrackName(int _ListNumber, int _ID)
        {
            #region
            if (_globalTextDatas.textDatasList.Count > _ListNumber &&
                _globalTextDatas.textDatasList[_ListNumber].TextsList.Count > _ID)
            {
                return _globalTextDatas.textDatasList[_ListNumber].TextsList[_ID].multiLanguage[0];
            }
            else
            {
                return "Wrong Name (Text doesn't exist).";
            }
            #endregion
        }

        private void HelpZone(int value)
        {
            #region

            switch (value)
            {
                case 0:
                    EditorGUILayout.HelpBox(
                    "This section allows to choose the UI color that represent Vehicles on UI", MessageType.Info);
                    break;

                case 1:
                    EditorGUILayout.HelpBox(
                    "A car is added at the end of each Difficulty. (Data -> DifficultyManagerData)", MessageType.Info);
                    break;

                case 2:
                    EditorGUILayout.HelpBox(
                    "This section allows to create new difficulty. (Data -> DifficultyManagerData)", MessageType.Info);
                    break;
            }
            #endregion
        }
    }
}

#endif