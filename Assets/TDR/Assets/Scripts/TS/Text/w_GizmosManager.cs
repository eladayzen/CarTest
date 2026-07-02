// Description: window to set up Gizmo global parameters in the project 
#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

namespace TS.Generics
{
    public class w_GizmosManager : EditorWindow
    {
        private Vector2 scrollPosAll;

        Path path;


        [MenuItem("Tools/TS/Other/w_GizmosManager")]
        public static void ShowWindow()
        {
            #region 
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(w_GizmosManager));
            #endregion
        }

     

        void OnEnable()
        {
            
        }

        void OnGUI()
        {
            #region
            //--> Scrollview
            scrollPosAll = EditorGUILayout.BeginScrollView(scrollPosAll);
            UpdateInfo();
            EditorGUILayout.EndScrollView();
            #endregion
        }

        void UpdateInfo()
        {
            #region 
            if (path == null)
            {
                path = FindFirstObjectByType<Path>();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Main Path:", GUILayout.Width(60));

                if (path.gizmoShowPath)
                {
                    if (GUILayout.Button("Enable"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(path, "path");
                        path.gizmoShowPath = !path.gizmoShowPath;
                    }
                }
                else
                {
                    if (GUILayout.Button("Disable"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(path, "path");
                        path.gizmoShowPath = !path.gizmoShowPath;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            #endregion
        }
    }
}

#endif