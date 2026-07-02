#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using TS.Generics;

namespace TS
{
    public class TSMenuCommand : MonoBehaviour
    {
       /* [MenuItem("Tools/TS/Command/Focus on Scene view U Command _u")]
        static void FocusCommandWhenKeyUIsPressed()
        {
            #region
            if (EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
            {
                if (Selection.activeGameObject &&
                    (Selection.activeGameObject.GetComponent<Bezier>() 
                    ||
                    Selection.activeGameObject.GetComponent<RoadCross>()))
                {
                    SceneView.lastActiveSceneView.Focus();
                }
            }
            #endregion
        }

        [MenuItem("Tools/TS/Command/Focus on Scene view N Command _n")]
        static void FocusCommandWhenKeyNIsPressed()
        {
            #region
            if (EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
            {
                if (Selection.activeGameObject &&
                    (Selection.activeGameObject.GetComponent<Bezier>()   ||
                    Selection.activeGameObject.GetComponent<RoadCross>() ||
                    Selection.activeGameObject.GetComponent<FocusTagCommand>()))
                {
                    SceneView.lastActiveSceneView.Focus();
                }
            }
            #endregion
        }

        [MenuItem("Tools/TS/Command/Focus on Scene view Shift+S Command _#s")]
        static void FocusCommandWhenKeyShiftSIsPressed()
        {
            #region
            if (EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
            {
                if (Selection.activeGameObject &&
                    Selection.activeGameObject.GetComponent<Bezier>())
                {
                    SceneView.lastActiveSceneView.Focus();
                }
            }
            #endregion
        }*/

        [MenuItem("GameObject/TS/Move To Root")]
        static void MoveToRoot(MenuCommand menuCommand)
        {
            #region
            if (Selection.objects.Length > 1)
                if (menuCommand.context != Selection.objects[0])
                    return;

            GameObject[] objs = Selection.gameObjects;

            foreach (GameObject obj in objs)
            {
                Undo.SetTransformParent(obj.transform, null, obj.name);
                Undo.RegisterFullObjectHierarchyUndo(obj, obj.name);
                obj.transform.SetAsLastSibling();
            }
            #endregion
        }

        [MenuItem("GameObject/TS/Create Group")]
        static void CreateGroup(MenuCommand menuCommand)
        {
            #region
            if (Selection.objects.Length > 1)
            {
                if (menuCommand.context == Selection.objects[0])
                    GroupObjects();
            }
            else if (menuCommand.context == Selection.objects[0])
                if (EditorUtility.DisplayDialog("This action is not possible", "Select at least 2 objects to create a group", "Continue")) { }

            #endregion
        }

        [MenuItem("GameObject/TS/Group + Move To Root")]
        static void GroupPlusMoveToRoot(MenuCommand menuCommand)
        {
            #region
            if (Selection.objects.Length > 1)
            {
                if (menuCommand.context == Selection.objects[0])
                    GroupObjects(true);
            }
            else if (menuCommand.context == Selection.objects[0])
                if (EditorUtility.DisplayDialog("This action is not possible", "Select at least 2 objects to create a group", "Continue")) { }
            #endregion
        }

        static void GroupObjects(bool isObjectsGrouped = false)
        {
            #region
            GameObject[] objs = Selection.gameObjects;

            // Create folder
            GameObject grpFolder = new GameObject();

            Undo.RegisterCreatedObjectUndo(grpFolder, grpFolder.name);

            Undo.SetTransformParent(grpFolder.transform, objs[0].transform.parent, grpFolder.name);

            Vector3 centerPos = FindCenter(objs);

            grpFolder.transform.position = centerPos;
            grpFolder.name = "Grp_";

            // Move Object to folder
            foreach (GameObject obj in objs)
            {
                Undo.SetTransformParent(obj.transform, grpFolder.transform, obj.name);
                Undo.RegisterFullObjectHierarchyUndo(obj, obj.name);
                obj.transform.SetAsLastSibling();
            }

            if (isObjectsGrouped)
            {
                Undo.SetTransformParent(grpFolder.transform, null, grpFolder.name);
                Undo.RegisterFullObjectHierarchyUndo(grpFolder, grpFolder.name);
                grpFolder.transform.SetAsLastSibling();
            }

                Selection.activeObject = grpFolder;
            #endregion
        }

        public static Vector3 FindCenter(GameObject[] objs)
        {
            #region
            var bound = new Bounds(objs[0].transform.position, Vector3.zero);
            for (int i = 1; i < objs.Length; i++)
                bound.Encapsulate(objs[i].transform.position);
            return bound.center;
            #endregion
        }

        [MenuItem("GameObject/TS/OptiGrid + Group")]
        static void CreateGroupOptiGrid(MenuCommand menuCommand)
        {
            #region

            if (menuCommand.context == Selection.objects[0])
                GroupObjectsOptiGrid();

          /*  if (Selection.objects.Length > 1)
            {
                if (menuCommand.context == Selection.objects[0])
                    GroupObjectsOptiGrid();
            }
            else if (menuCommand.context == Selection.objects[0])
                if (EditorUtility.DisplayDialog("This action is not possible", "Select at least 2 objects to create a group", "Continue")) { }
          */
          
            #endregion
        }

        [MenuItem("GameObject/TS/OptiGrid + Move To Root")]
        static void GroupPlusMoveToRootOptiGrid(MenuCommand menuCommand)
        {
            #region
           // if (Selection.objects.Length > 1)
            //{
                if (menuCommand.context == Selection.objects[0])
                    GroupObjectsOptiGrid(true);
           // }
           // else if (menuCommand.context == Selection.objects[0])
          //      if (EditorUtility.DisplayDialog("This action is not possible", "Select at least 2 objects to create a group", "Continue")) { }
            #endregion
        }

        static void GroupObjectsOptiGrid(bool isObjectsGrouped = false, string OptiGridType = "StreamGrid")
        {
            #region
            GameObject[] objs = Selection.gameObjects;

            // Create folder
            GameObject grpFolder = new GameObject();

            Undo.RegisterCreatedObjectUndo(grpFolder, grpFolder.name);

            Undo.SetTransformParent(grpFolder.transform, objs[0].transform.parent, grpFolder.name);

            Vector3 centerPos = FindCenter(objs);

            grpFolder.transform.position = centerPos;
            grpFolder.name = "Grp_Opti";

            if(OptiGridType == "StreamGrid")
                grpFolder.AddComponent<TSStreamGridTag>();
            if (OptiGridType == "StreamDistance")
                grpFolder.AddComponent<TSStreamDistanceTag>();

            // Move Object to folder
            foreach (GameObject obj in objs)
            {
                Undo.SetTransformParent(obj.transform, grpFolder.transform, obj.name);
                Undo.RegisterFullObjectHierarchyUndo(obj, obj.name);
                obj.transform.SetAsLastSibling();
            }

            if (isObjectsGrouped)
            {
                Undo.SetTransformParent(grpFolder.transform, null, grpFolder.name);
                Undo.RegisterFullObjectHierarchyUndo(grpFolder, grpFolder.name);
                grpFolder.transform.SetAsLastSibling();
            }


            Selection.activeObject = grpFolder;

            #endregion
        }

        [MenuItem("GameObject/TS/Pivot to Center")]
        static void CenterParent(MenuCommand menuCommand)
        {
            #region
            // Create folder
            GameObject grpFolder = new GameObject();

            Undo.RegisterCreatedObjectUndo(grpFolder, grpFolder.name);

            if (Selection.activeTransform.GetComponent<Renderer>())
            {
                var bounds = Selection.activeTransform.GetComponent<Renderer>().bounds;
                grpFolder.transform.position = bounds.center;
            }
            else
                grpFolder.transform.position = Selection.activeTransform.position;


            Undo.SetTransformParent(grpFolder.transform, Selection.activeTransform.parent, grpFolder.name);

            grpFolder.transform.localScale = Vector3.one;


            grpFolder.name = "CP_" + Selection.activeTransform.name;

            Undo.SetTransformParent(Selection.activeTransform, grpFolder.transform, Selection.activeTransform.name);
            Undo.RegisterFullObjectHierarchyUndo(Selection.activeTransform, Selection.activeTransform.name);
            Selection.activeTransform.transform.SetAsLastSibling();

            Selection.activeObject = grpFolder;
            #endregion
        }

        [MenuItem("GameObject/TS/OptiDistance + Group")]
        static void CreateGroupOptiDistance(MenuCommand menuCommand)
        {
            #region

            if (menuCommand.context == Selection.objects[0])
                GroupObjectsOptiGrid(false,"StreamDistance");


            #endregion
        }
    }
}
#endif