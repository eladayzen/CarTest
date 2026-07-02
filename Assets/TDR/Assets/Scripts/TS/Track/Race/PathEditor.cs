//Description : PathEditor.cs : Works in association with Path.cs . Allows to create a car path
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TS.Generics
{
	[CustomEditor(typeof(Path))]
	public class PathEditor : Editor
	{
		SerializedProperty SeeInspector;                                            // use to draw default Inspector
		SerializedProperty moreOptions;
		SerializedProperty helpBox;

		SerializedProperty additionalPathsList;
		SerializedProperty checkpoints;
		SerializedProperty AltPathList;
		SerializedProperty currentSelectedCheckpoint;
		SerializedProperty prefabCheckpoint;
		SerializedProperty showCheckpoints;

		SerializedProperty difficultyOffset;

		SerializedProperty minDifficultyOffset;
		SerializedProperty maxDifficultyOffset;

		SerializedProperty selectedIDOffsetDifficulty;

		SerializedProperty improveAIPathLoopMode;
		SerializedProperty improveAIPathStartID;
		SerializedProperty improveAIPathEndID;
		SerializedProperty improveLastTwoLaps;

		SerializedProperty pathLength;
		SerializedProperty spotDifficultyDistance;

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


		public Color _cRed2 = new Color(1f, .35f, 0f, 1f);
		public Color _cRed = new Color(1f, .5f, 0f, .5f);
		public Color _cGray = new Color(.9f, .9f, .9f, 1);
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
			moreOptions = serializedObject.FindProperty("moreOptions");
			helpBox = serializedObject.FindProperty("helpBox");
			additionalPathsList = serializedObject.FindProperty("additionalPathsList");
			checkpoints = serializedObject.FindProperty("checkpoints");
			AltPathList = serializedObject.FindProperty("AltPathList");
			currentSelectedCheckpoint = serializedObject.FindProperty("currentSelectedCheckpoint");
			prefabCheckpoint = serializedObject.FindProperty("prefabCheckpoint");
			showCheckpoints = serializedObject.FindProperty("showCheckpoints");
			difficultyOffset = serializedObject.FindProperty("difficultyOffset");

			minDifficultyOffset = serializedObject.FindProperty("minDifficultyOffset");
			maxDifficultyOffset = serializedObject.FindProperty("maxDifficultyOffset");

			selectedIDOffsetDifficulty = serializedObject.FindProperty("selectedIDOffsetDifficulty");

			improveAIPathLoopMode = serializedObject.FindProperty("improveAIPathLoopMode");
			improveAIPathStartID = serializedObject.FindProperty("improveAIPathStartID");
			improveAIPathEndID = serializedObject.FindProperty("improveAIPathEndID");
			improveLastTwoLaps = serializedObject.FindProperty("improveLastTwoLaps");

			pathLength = serializedObject.FindProperty("pathLength");
			spotDifficultyDistance = serializedObject.FindProperty("spotDifficultyDistance");
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

			EditorGUILayout.LabelField("");

			DisplayAltPath();

			if (EditorPrefs.GetBool("MoreOptions") == true && moreOptions.boolValue)
				DisplayAdditionalPaths();

			DisplayDifficultyOffset();

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.LabelField("");
			#endregion
		}

		void DisplayAdditionalPaths()
		{
			#region
			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Show All"))
			{
				for (var i = 0; i < additionalPathsList.arraySize; i++)
				{
					SerializedProperty b_Show = additionalPathsList.GetArrayElementAtIndex(i).FindPropertyRelative("b_Show");
					b_Show.boolValue = true;
				}
			}
			if (GUILayout.Button("Hide All"))
			{
				for (var i = 0; i < additionalPathsList.arraySize; i++)
				{
					SerializedProperty b_Show = additionalPathsList.GetArrayElementAtIndex(i).FindPropertyRelative("b_Show");
					b_Show.boolValue = false;
				}
			}
			EditorGUILayout.EndHorizontal();

			for (var i = 0; i < additionalPathsList.arraySize; i++)
			{
				SerializedProperty b_Show = additionalPathsList.GetArrayElementAtIndex(i).FindPropertyRelative("b_Show");
				SerializedProperty offset = additionalPathsList.GetArrayElementAtIndex(i).FindPropertyRelative("offset");
				SerializedProperty color = additionalPathsList.GetArrayElementAtIndex(i).FindPropertyRelative("color");


				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(20)))
				{

				}
				EditorGUILayout.LabelField(i + ":", GUILayout.Width(20));

				b_Show.boolValue = EditorGUILayout.Toggle(b_Show.boolValue, GUILayout.Width(20));

				if (b_Show.boolValue)
				{
					EditorGUILayout.PropertyField(offset, new GUIContent(""));
					EditorGUILayout.PropertyField(color, new GUIContent(""));
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.LabelField("");
			if (GUILayout.Button("Add New Alternative Path"))
			{

			}
			#endregion
		}
		void DisplayDifficultyOffset()
		{
			#region
			EditorGUILayout.LabelField("", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("AI Speed Offset:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(difficultyOffset, new GUIContent("Right Click -> Copy"));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.HelpBox(
				"Offset > 0 increase max speed allowed." +
				"\n" +
				"Offset < 0 reduce max speed allowed.", MessageType.Info);


			ImproveAIPathLoop();


			if (difficultyOffset.arraySize > 0 &&
				difficultyOffset.GetArrayElementAtIndex(0).FindPropertyRelative("spotID").intValue > 1)
			{
				if (GUILayout.Button("Create Spot ID = 1"))
				{
					difficultyOffset.InsertArrayElementAtIndex(0);
					difficultyOffset.GetArrayElementAtIndex(0).FindPropertyRelative("spotID").intValue = 1;
					difficultyOffset.GetArrayElementAtIndex(0).FindPropertyRelative("difficultyOffset").floatValue = 0;
				}
			}
			else
			{
				if (GUILayout.Button(""))
				{

				}
			}

			int howManyEntries = difficultyOffset.arraySize;
			for (var i = 0; i < howManyEntries; i++)
			{

				EditorGUILayout.BeginHorizontal();

				string sBtn = "";
				if (selectedIDOffsetDifficulty.intValue == i)
				{
					sBtn = "x";
				}

				if (GUILayout.Button(sBtn, GUILayout.Width(20)))
				{
					if (selectedIDOffsetDifficulty.intValue == i)
						selectedIDOffsetDifficulty.intValue = -1;
					else
						selectedIDOffsetDifficulty.intValue = i;
				}

				EditorGUILayout.LabelField(i + ": ", GUILayout.Width(20));
				SerializedProperty _spotID = difficultyOffset.GetArrayElementAtIndex(i).FindPropertyRelative("spotID");
				SerializedProperty _difficultyOffset = difficultyOffset.GetArrayElementAtIndex(i).FindPropertyRelative("difficultyOffset");

				EditorGUILayout.PropertyField(_spotID);


				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.Width(40));
				EditorGUILayout.LabelField("|Offset", GUILayout.Width(70));
				_difficultyOffset.floatValue = EditorGUILayout.Slider(
					_difficultyOffset.floatValue,
					minDifficultyOffset.floatValue,
					maxDifficultyOffset.floatValue, GUILayout.MinWidth(100));

				_spotID.intValue = Mathf.Clamp(_spotID.intValue, 1, _spotID.intValue);


				if (GUILayout.Button("+", GUILayout.Width(20)))
				{

					SerializedProperty _spotIDNext;
					int diff = -1;
					if (i + 1 < howManyEntries)
					{
						_spotIDNext = difficultyOffset.GetArrayElementAtIndex(i + 1).FindPropertyRelative("spotID");
						diff = _spotIDNext.intValue - _spotID.intValue;
					}




					if (diff == -1 || diff > 1)
					{

						difficultyOffset.InsertArrayElementAtIndex(i);

						SerializedProperty _spotIDNew = difficultyOffset.GetArrayElementAtIndex(i + 1).FindPropertyRelative("spotID");
						SerializedProperty _difficultyOffsetNew = difficultyOffset.GetArrayElementAtIndex(i + 1).FindPropertyRelative("difficultyOffset");
						_spotIDNew.intValue = _spotID.intValue + 1;
						_difficultyOffsetNew.floatValue = 0;

						selectedIDOffsetDifficulty.intValue = i + 1;
					}
					else if (diff == 1)
					{
						if (EditorUtility.DisplayDialog("This action is not possible.",
							"It is not possible to insert a new entry. Difference between the two ID must be greater than 1.", "Continue"))
						{

						}
					}

					break;
				}
				if (GUILayout.Button("-", GUILayout.Width(20)))
				{
					difficultyOffset.DeleteArrayElementAtIndex(i);
					break;
				}

				EditorGUILayout.EndHorizontal();

				// Special case
				if (i > 0)
				{
					SerializedProperty _spotIDLast = difficultyOffset.GetArrayElementAtIndex(i - 1).FindPropertyRelative("spotID");
					if (_spotIDLast.intValue >= _spotID.intValue)
						_spotID.intValue = _spotIDLast.intValue + 1;

				}

				EditorGUILayout.Space();
			}

			if (difficultyOffset.arraySize == 0)
			{
				if (GUILayout.Button("Create First Speed offset value"))
				{
					difficultyOffset.InsertArrayElementAtIndex(0);
					difficultyOffset.GetArrayElementAtIndex(0).FindPropertyRelative("spotID").intValue = 1;
					difficultyOffset.GetArrayElementAtIndex(0).FindPropertyRelative("difficultyOffset").floatValue = 0;
				}
			}



			#endregion
		}

		void ImproveAIPathLoop()
		{
			#region
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Loop A Section:", GUILayout.Width(90));
			EditorGUILayout.PropertyField(improveAIPathLoopMode, new GUIContent(""), GUILayout.Width(20));
			if (GUILayout.Button("Go", GUILayout.Width(30)))
			{
				VehiclesRef.instance.listVehicles[0].GetComponent<Rigidbody>().isKinematic = true;
				VehiclesRef.instance.listVehicles[0].GetComponent<VehiclePathFollow>().UpdateImproveAIMode(true);
			}
			EditorGUILayout.EndHorizontal();

			/*			improveAITimer = 0;

					public float lastLap = 0;
					public string improveLastTwoLaps = "Last Lap: "\n" +  Penultimate Lap:";
			*/
			EditorGUILayout.HelpBox(improveLastTwoLaps.stringValue, MessageType.Info);

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Start:", GUILayout.Width(30));
			EditorGUILayout.PropertyField(improveAIPathStartID, new GUIContent(" "));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("End:", GUILayout.Width(30));
			EditorGUILayout.PropertyField(improveAIPathEndID, new GUIContent(" "));
			EditorGUILayout.EndHorizontal();

			int howManySpots = Mathf.RoundToInt(pathLength.floatValue / spotDifficultyDistance.floatValue);

			improveAIPathStartID.intValue = Mathf.Clamp(improveAIPathStartID.intValue, 0, howManySpots - 2);
			improveAIPathEndID.intValue = Mathf.Clamp(improveAIPathEndID.intValue, improveAIPathStartID.intValue + 1, howManySpots - 1);
			#endregion
		}

		void DisplayAltPath()
		{
			#region
			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Alt Path List:", EditorStyles.boldLabel);
				for (var i = 0; i < AltPathList.arraySize; i++)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(i + ": ", GUILayout.Width(20));
					EditorGUILayout.PropertyField(AltPathList.GetArrayElementAtIndex(i), new GUIContent("")/*,  GUILayout.Width(30)*/);
					if (GUILayout.Button("-", GUILayout.Width(20)))
					{
						Path myScript = (Path)target;
						Undo.RegisterFullObjectHierarchyUndo(myScript, myScript.name);
						if (AltPathList.GetArrayElementAtIndex(i).objectReferenceValue != null)
						{
							Undo.DestroyObjectImmediate(myScript.AltPathList[i].gameObject);
						}

						myScript.AltPathList.RemoveAt(i);
						break;
					}
					EditorGUILayout.EndHorizontal();
				}

				CreateNewAltPath();
			}
			#endregion
		}

		void CreateNewAltPath()
		{
			#region
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create a new Alt Path using the checkpoint: "))
			{
				if (currentSelectedCheckpoint.intValue < checkpoints.arraySize)
				{
					//-> Instantiate anew ALt Path Prefab
					AltPathList.InsertArrayElementAtIndex(0);
					Path myScript = (Path)target;

					PathRef pathRef = myScript.transform.parent.GetComponent<PathRef>();
					GameObject newAltPath = PrefabUtility.InstantiatePrefab((GameObject)pathRef.prefabAltPath_Ref, pathRef.transform) as GameObject;
					Undo.RegisterCreatedObjectUndo(newAltPath, "newAltPath");

					AltPathList.GetArrayElementAtIndex(0).objectReferenceValue = newAltPath;
					AltPathList.MoveArrayElement(0, AltPathList.arraySize - 1);

					newAltPath.name = "Checkpoint_" + currentSelectedCheckpoint.intValue + "_AltPath_" + (AltPathList.arraySize - 1);

					//-> Activate the Alt Path Trigger
					Transform refTrans = (Transform)checkpoints.GetArrayElementAtIndex(currentSelectedCheckpoint.intValue).objectReferenceValue;

					if (refTrans.GetChild(0).childCount > 0)
					{
						GameObject AltPathTrigger = refTrans.GetChild(0).GetChild(0).gameObject;
						SerializedObject serializedObject0 = new UnityEditor.SerializedObject(AltPathTrigger);
						serializedObject0.Update();
						SerializedProperty m_IsActive = serializedObject0.FindProperty("m_IsActive");

						m_IsActive.boolValue = true;

						serializedObject0.ApplyModifiedProperties();
					}

					//-> Connect the new Alt Path to the Trigger ALt Path object
					if (refTrans.GetChild(0).childCount > 0)
					{
						GameObject AltPathTrigger = refTrans.GetChild(0).GetChild(0).gameObject;
						SerializedObject serializedObject0 = new UnityEditor.SerializedObject(AltPathTrigger.GetComponent<TriggerAltPath>());
						serializedObject0.Update();
						SerializedProperty m_AltPathList = serializedObject0.FindProperty("AltPathList");

						m_AltPathList.InsertArrayElementAtIndex(0);
						m_AltPathList.GetArrayElementAtIndex(0).objectReferenceValue = newAltPath;
						m_AltPathList.MoveArrayElement(0, m_AltPathList.arraySize - 1);

						serializedObject0.ApplyModifiedProperties();
					}


					//-> Auto-Connect chepointStart
					newAltPath.GetComponent<AltPath>().checkpointStart = (Transform)checkpoints.GetArrayElementAtIndex(currentSelectedCheckpoint.intValue).objectReferenceValue;
					newAltPath.transform.position = myScript.checkpoints[currentSelectedCheckpoint.intValue].transform.position;

					//-> Auto-Connect altPathTrigger object
					if (refTrans.GetChild(0).childCount > 0)
					{
						GameObject AltPathTrigger = refTrans.GetChild(0).GetChild(0).gameObject;
						newAltPath.GetComponent<AltPath>().triggerAltPath = AltPathTrigger.GetComponent<TriggerAltPath>();
					}

					Transform PlayerTrigger = newAltPath.transform.GetChild(0);
					PlayerTrigger.rotation = newAltPath.GetComponent<AltPath>().checkpointStart.rotation;

					Selection.activeGameObject = newAltPath;
				}
			}
			EditorGUILayout.PropertyField(currentSelectedCheckpoint, new GUIContent(""), GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();
			#endregion
		}

	}
}


#endif