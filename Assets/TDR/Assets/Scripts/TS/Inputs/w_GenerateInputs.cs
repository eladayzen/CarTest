// Description : w_PuzzlesCreator_Pc.cs :  Allow to create puzzles and access some puzzles parameters
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
    public class w_GenerateInputs : EditorWindow
    {
        private Vector2         scrollPosAll;
      
        [MenuItem("Tools/TS/Other/w_GenerateInputs")]
        public static void ShowWindow()
        {
            #region 
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(w_GenerateInputs)); 
            #endregion
        }

   

        public string[]         listItemType = new string[] { };

        public List<string>     _test = new List<string>();
        public int              page = 0;
        public int              numberOfIndexInAPage = 50;
        public int              seachSpecificID = 0;

        public Color            _cGreen = new Color(1f, .8f, .4f, 1);
        public Color            _cGray = new Color(.9f, .9f, .9f, 1);


        public Texture2D        eye;
        public Texture2D        currentItemDisplay;
        public int              intcurrentItemDisplay = 0;
        public bool             b_UpdateProcessDone = false;
        public bool             b_AllowUpdateScene = false;



        void OnEnable()
        {
            #region
          
            #endregion
        }

        void OnGUI()
        {
            #region
            //--> Scrollview
            scrollPosAll = EditorGUILayout.BeginScrollView(scrollPosAll);
            //--> Window description
            //GUI.backgroundColor = _cGreen;
           

            EditorGUILayout.BeginVertical();
            ARC_CreateInputs();
            ARC_CreateJoysticInputs();
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("");

            EditorGUILayout.EndScrollView();
            #endregion
        }

        void OnInspectorUpdate()
        {
            #region 
            Repaint(); 
            #endregion
        }

      
        private static void CreateInput(inputParams newInput)
        {
            #region
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);

            serializedObject.Update();
            SerializedProperty m_Axes = serializedObject.FindProperty("m_Axes");

            m_Axes.InsertArrayElementAtIndex(m_Axes.arraySize - 1);

            SerializedProperty m_Name = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("m_Name");
            SerializedProperty descriptiveName = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("descriptiveName");
            SerializedProperty descriptiveNegativeName = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("descriptiveNegativeName");
            SerializedProperty negativeButton = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("negativeButton");
            SerializedProperty positiveButton = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("positiveButton");
            SerializedProperty altNegativeButton = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("altNegativeButton");
            SerializedProperty altPositiveButton = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("altPositiveButton");
            SerializedProperty gravity = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("gravity");
            SerializedProperty dead = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("dead");
            SerializedProperty sensitivity = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("sensitivity");
            SerializedProperty snap = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("snap");
            SerializedProperty invert = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("invert");
            SerializedProperty type = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("type");
            SerializedProperty axis = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("axis");
            SerializedProperty joyNum = m_Axes.GetArrayElementAtIndex(m_Axes.arraySize - 1).FindPropertyRelative("joyNum");

            m_Name.stringValue = newInput.name;
            descriptiveName.stringValue = newInput.descriptiveName;
            descriptiveNegativeName.stringValue = newInput.descriptiveNegativeName;
            negativeButton.stringValue = newInput.negativeButton;
            positiveButton.stringValue = newInput.positiveButton;
            altNegativeButton.stringValue = newInput.altNegativeButton;
            altPositiveButton.stringValue = newInput.altPositiveButton;
            gravity.floatValue = newInput.gravity;
            dead.floatValue = newInput.dead;
            sensitivity.floatValue = newInput.sensitivity;
            snap.boolValue = newInput.snap;
            invert.boolValue = newInput.invert;
            type.intValue = newInput.type;
            axis.intValue = newInput.axis;
            joyNum.intValue = newInput.joyNum;

            serializedObject.ApplyModifiedProperties();
            #endregion
        }

        public class inputParams
        {
            #region
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;

            public float gravity;
            public float dead;
            public float sensitivity;

            public bool snap = false;
            public bool invert = false;

            public int type;

            public int axis;
            public int joyNum;
            #endregion
        }

        public static bool checkIfAlreadyExist(string nameToCheck)
        {
            #region
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty m_Axes = serializedObject.FindProperty("m_Axes");
            serializedObject.Update();
            Debug.Log(m_Axes.arraySize);
            for (var i = 0; i < m_Axes.arraySize; i++)
            {
                if (m_Axes.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == nameToCheck)
                    return true;
            }

            return false;
            #endregion
        }

        public void ARC_CreateInputs()
        {
            #region 
            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Create Event System Inputs"))
            {
                #region 
                if (checkIfAlreadyExist("TSOrbitalHorPad"))
                    Debug.Log("Already Exist");
                else
                {
                    CreateInput(new inputParams()
                    {
                        name = "TSOrbitalHorPad",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 2,
                        axis = 3,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSOrbitalVertPad",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 2,
                        axis = 4,
                        joyNum = 0
                    });
                }




                if (checkIfAlreadyExist("TSSubmit"))
                    Debug.Log("Already Exist");
                else
                {
                    CreateInput(new inputParams()
                    {
                        name = "TSSubmit",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1000,
                        dead = .001f,
                        sensitivity = 1000,
                        snap = true,
                        invert = false,
                        type = 0,
                        axis = 0,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSHorizontal",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 2,
                        axis = 0,
                        joyNum = 1
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSVertical",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = true,
                        type = 2,
                        axis = 1,
                        joyNum = 1
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSHorizontalRemap",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 2,
                        axis = 0,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSVerticalRemap",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "",
                        positiveButton = "",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = true,
                        type = 2,
                        axis = 1,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSHorizontal",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "left",
                        positiveButton = "right",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 0,
                        axis = 0,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSVertical",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "down",
                        positiveButton = "up",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = false,
                        invert = false,
                        type = 0,
                        axis = 1,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSHorizontalRemap",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "left",
                        positiveButton = "right",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = true,
                        invert = false,
                        type = 0,
                        axis = 0,
                        joyNum = 0
                    });

                    CreateInput(new inputParams()
                    {
                        name = "TSVerticalRemap",
                        descriptiveName = "",
                        descriptiveNegativeName = "",
                        negativeButton = "down",
                        positiveButton = "up",
                        altNegativeButton = "",
                        gravity = 1,
                        dead = .001f,
                        sensitivity = 1,
                        snap = false,
                        invert = false,
                        type = 0,
                        axis = 1,
                        joyNum = 0
                    });
                }
                #endregion
            }
            EditorGUILayout.LabelField(""); 
            #endregion
        }

        public void ARC_CreateJoysticInputs()
        {
            #region 
            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Create Joystick Inputs for player 1 and 2"))
            {
                for (var k = 0; k < 2; k++)
                {
                    int whichPlayer = k + 1;
                    //-> Create Joystick Axis
                    for (var i = 0; i < 10; i++)
                    {
                        #region 
                        if (checkIfAlreadyExist("Joystick" + whichPlayer + "Axis" + (i + 1).ToString()))
                            Debug.Log("Already Exist");
                        else
                        {
                            CreateInput(new inputParams()
                            {
                                name = "Joystick" + whichPlayer + "Axis" + (i + 1).ToString(),
                                descriptiveName = "",
                                descriptiveNegativeName = "",
                                negativeButton = "",
                                positiveButton = "",
                                altNegativeButton = "",
                                gravity = 0,
                                dead = .19f,
                                sensitivity = 1.0f,
                                snap = true,
                                invert = false,
                                type = 2,
                                axis = i,
                                joyNum = whichPlayer
                            });
                        }
                        #endregion
                    }

                    //-> Create Joystick Axis
                    for (var i = 0; i < 20; i++)
                    {
                        #region 
                        if (checkIfAlreadyExist("Joystick" + whichPlayer + "Button" + (i).ToString()))
                            Debug.Log("Already Exist");
                        else
                        {
                            CreateInput(new inputParams()
                            {
                                name = "Joystick" + whichPlayer + "Button" + (i).ToString(),
                                descriptiveName = "",
                                descriptiveNegativeName = "",
                                negativeButton = "",
                                positiveButton = "joystick " + whichPlayer + " button " + i,
                                altNegativeButton = "",
                                gravity = 1,
                                dead = .001f,
                                sensitivity = 1.0f,
                                snap = true,
                                invert = false,
                                type = 0,
                                axis = 0,
                                joyNum = whichPlayer
                            });
                        }
                        #endregion
                    }
                }

            }
            EditorGUILayout.LabelField(""); 
            #endregion
        }
    }
}
#endif

