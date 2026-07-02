//Description: CreateMinimapEditor: Custom Editor
#if (UNITY_EDITOR)
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace TS.Generics
{
    [CustomEditor(typeof(MinimapSaver))]
    public class MinimapSaverEditor : Editor
    {
        SerializedProperty _minimapName;
        SerializedProperty _path;
        SerializedProperty _imgMinimapP1;
        SerializedProperty _imgMinimapP2;
        void OnEnable()
        {
            #region
            // Setup the SerializedProperties.
            /*    SeeInspector = serializedObject.FindProperty("SeeInspector");
                helpBox = serializedObject.FindProperty("helpBox");
                moreOptions = serializedObject.FindProperty("moreOptions");
                grpPath = serializedObject.FindProperty("grpPath");
            */

            _minimapName = serializedObject.FindProperty("minimapName");
            _path = serializedObject.FindProperty("path");
            _imgMinimapP1 = serializedObject.FindProperty("imgMinimapP1");
            _imgMinimapP2 = serializedObject.FindProperty("imgMinimapP2");
            #endregion
        }

        public override void OnInspectorGUI()
        {
            #region
            //if (SeeInspector.boolValue)                         // If true Default Inspector is drawn on screen
            DrawDefaultInspector();

            serializedObject.Update();

            GenerateMinimap();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("");
            #endregion
        }

        private void GenerateMinimap()
        {
            #region 
            if (GUILayout.Button("Init"))
            {
                Init();
            }
            if (GUILayout.Button("Generate Sprite"))
            {
                GenerateImageAsset();
            }
            #endregion
        }

        void GenerateImageAsset()
        {
            #region 
            MinimapSaver myScript = (MinimapSaver)target;
            Camera Cam = myScript.GetComponent<Camera>();

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = Cam.targetTexture;

            Cam.Render();

            Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
            Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);

            for (var i = 0; i < Cam.targetTexture.width; i++)
                for (var j = 0; j < Cam.targetTexture.height; j++)
                {
                    if (Image.GetPixel(i, j).r > .9f)
                        Image.SetPixel(i, j, new Color(0, 0, 0, 0));
                }
            // if (Image.GetPixel(i, j) == new Color(0, 0, 0))
            //    Image.SetPixel(i, j, new Color(0, 0, 0, 0));

            Image.Apply();
            RenderTexture.active = currentRT;

            if (!IsFileAlreadyExist())
            {
                GenerateTexture(Image);
            }
            else
            {
                if (EditorUtility.DisplayDialog("Name already exist",
                               "Are you sur you want to replace this file?", "Yes", "No"))
                {
                    GenerateTexture(Image);
                }
            }
            #endregion
        }

        bool IsFileAlreadyExist()
        {
            #region

            return File.Exists(Application.dataPath + "/" + _path.stringValue + "/" + _minimapName.stringValue + ".png");
            #endregion
        }

        void GenerateTexture(Texture2D Image)
        {
            #region 
            string path = Application.dataPath + "/" + _path.stringValue + "/" + _minimapName.stringValue + ".png";

            var Bytes = Image.EncodeToPNG();
            DestroyImmediate(Image);
            File.WriteAllBytes(path, Bytes);
            AssetDatabase.Refresh();

            UpdateTextureImportParameters();
        }
        #endregion

        void UpdateTextureImportParameters()
        {
            #region 
            string path = "Assets" + "/" + _path.stringValue + "/" + _minimapName.stringValue + ".png";
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);

            while (textureImporter == null) { }

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.sRGBTexture = true;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.alphaIsTransparency = true;
            textureImporter.SaveAndReimport();

            ApplyImagesInCanvasInGameMenu();
            #endregion
        }

        void ApplyImagesInCanvasInGameMenu()
        {
            #region 
            string path = "Assets" + "/" + _path.stringValue + "/" + _minimapName.stringValue + ".png";
            var obj = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            MinimapSaver myScript = (MinimapSaver)target;

            SerializedObject serializedObject2 = new UnityEditor.SerializedObject(myScript.imgMinimapP1);
            SerializedProperty m_Sprite = serializedObject2.FindProperty("m_Sprite");
            serializedObject2.Update();

            m_Sprite.objectReferenceValue = (Sprite)obj;
            serializedObject2.ApplyModifiedProperties();

            SerializedObject serializedObject3 = new UnityEditor.SerializedObject(myScript.imgMinimapP2);
            SerializedProperty m_SpriteP2 = serializedObject3.FindProperty("m_Sprite");
            serializedObject3.Update();

            m_SpriteP2.objectReferenceValue = (Sprite)obj;
            serializedObject3.ApplyModifiedProperties();
            #endregion
        }

        void Init()
        {
            #region
            ImageMinimapTag[] allImages = FindObjectsByType<ImageMinimapTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _imgMinimapP1.objectReferenceValue = allImages[0].GetComponent<Image>();
            _imgMinimapP2.objectReferenceValue = allImages[1].GetComponent<Image>();
            #endregion
        }

        void OnSceneGUI()
        {
        }
    }
}

#endif
