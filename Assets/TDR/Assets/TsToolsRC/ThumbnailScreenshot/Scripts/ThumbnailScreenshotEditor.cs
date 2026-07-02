#if (UNITY_EDITOR)
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace TS.Generics
{
    public class ThumbnailScreenshotEditor : EditorWindow
    {

        [MenuItem("Tools/TS/Thumbnail Screenshot")]
        static void CreateMenu()
        {
            var window = GetWindow<ThumbnailScreenshotEditor>();
            window.titleContent = new GUIContent("Thumbnail Screenshot");
        }


        public ThumbnailData source;
        SerializedObject serializedObject;

        private VisualElement m_rootVisualElement;


        public void CreateGUI()
        {
            #region 
            string objectPath = "Assets/TDR/Assets/TsToolsRC/ThumbnailScreenshot/Data/dataThumbnail.asset";
            source = AssetDatabase.LoadAssetAtPath(objectPath, typeof(UnityEngine.Object)) as ThumbnailData;

            m_rootVisualElement = new ScrollView(ScrollViewMode.Vertical);

            if (source)
            {
                serializedObject = new UnityEditor.SerializedObject(source);
                serializedObject.Update();

                m_rootVisualElement.Add(VEText("STEP 1: Open the scene Thumbnail " +
                    "\n(Assets -> Scenes -> Thumbnail -> thumbnail)."));

                m_rootVisualElement.Add(VEText("STEP 2: In the Game view choose Thumbnail (604x277) preset."));

                m_rootVisualElement.Add(VEText("STEP 3: Put your vehicle inside Pivot object and reset position and rotation to zero."));

                m_rootVisualElement.Add(VEText("STEP 4: Move the camera where you want."));

                //m_rootVisualElement.Add(VEText(""));
                m_rootVisualElement.Add(VEText("STEP 5: Choose the path to save the screenshot.")); 


                m_rootVisualElement.Add(VEPath());
                m_rootVisualElement.Add(VEText("STEP 6: Choose the name of your screenshot."));
                m_rootVisualElement.Add(VEScreenshotName());

                //m_rootVisualElement.Add(VEText(""));
                m_rootVisualElement.Add(VEText("STEP 7: Generate the screenshot."));
                m_rootVisualElement.Add(VENewTaskButton());


                //m_rootVisualElement.Add(VEText(""));
                m_rootVisualElement.Add(VEText("STEP 8: Exit and go back to Unity. " +
                    "(Check if the screenshot was created in the Project folder)."));
                //m_rootVisualElement.Add(VECheckIfTheTextureisCreated());

                //m_rootVisualElement.Add(VEText(""));
                m_rootVisualElement.Add(VEText("STEP 9: Convert screenshot to Sprite."));
                m_rootVisualElement.Add(VEUpdateTextureImportParameters());


                m_rootVisualElement.Add(VEText("The process is finished." +
                    "\nDon't forget to change your Game view resolution. Choose for example the default preset Full HD (1920x1080)."));

                rootVisualElement.Add(m_rootVisualElement);
                serializedObject.ApplyModifiedProperties();
            } 
            #endregion
        }

        VisualElement VENewTaskButton()
        {
            #region 
            Button btn = new Button();
            btn.text = "Create";
            btn.style.height = 50;
            btn.clicked += CreateNewTask;

            return btn;
            #endregion
        }

        void CreateNewTask()
        {
            #region 
            SerializedProperty m_path = serializedObject.FindProperty("path");
            SerializedProperty m_screenshotName = serializedObject.FindProperty("screenshotName");
            ScreenCapture.CaptureScreenshot(m_path.stringValue + "/" + m_screenshotName.stringValue + ".png");
            
            //UpdateTextureImportParameters();
            #endregion

        }

        VisualElement VEPath()
        {
            #region 
            TextField txt = new TextField();
            SerializedProperty m_path = serializedObject.FindProperty("path");

            txt.BindProperty(m_path);

            return txt;
            #endregion
        }
        VisualElement VEScreenshotName()
        {
            #region 
            TextField txt = new TextField();
            SerializedProperty m_screenshotName = serializedObject.FindProperty("screenshotName");

            txt.BindProperty(m_screenshotName);

            return txt;
            #endregion
        }

        VisualElement VEUpdateTextureImportParameters()
        {
            #region 
            Button btn = new Button();
            btn.text = "Convert to Sprite";
            btn.style.height = 50;
            btn.clicked += UpdateTextureImportParameters;

            return btn;
            #endregion
        }
        void UpdateTextureImportParameters()
        {
            #region 
             SerializedProperty m_path = serializedObject.FindProperty("path");
             SerializedProperty m_screenshotName = serializedObject.FindProperty("screenshotName");

             string path = m_path.stringValue + "/" + m_screenshotName.stringValue + ".png";
             TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);

             textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.sRGBTexture = true;
            // textureImporter.alphaIsTransparency = true;
             textureImporter.SaveAndReimport();

             if (textureImporter != null && EditorUtility.DisplayDialog("Process Done",
                 "Screenshot saved in folder: " + m_path.stringValue, "Continue"))
             {}
            
            #endregion
        }

        VisualElement VECheckIfTheTextureisCreated()
        {
            #region 
            Button btn = new Button();
            btn.text = "Step 2: Exit and go back to unity.";
            btn.style.height = 50;
           // btn.clicked += CheckIfTheTextureisCreated;

            return btn;
            #endregion
        }


        VisualElement VEText(string txt)
        {
            #region 
            HelpBox helpBox = new HelpBox(txt, HelpBoxMessageType.Info);
            //Label label = new Label();
           // label.text = txt;
           // label.style. =
            //label.style.height = 50;
            // btn.clicked += CheckIfTheTextureisCreated;

            return helpBox;
            #endregion
        }
    }



}

#endif

