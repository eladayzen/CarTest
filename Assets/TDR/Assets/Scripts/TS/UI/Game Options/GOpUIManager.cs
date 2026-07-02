// Description: GOpUIManager: Script used in the Game Options Menu.
// Methods to change the selected language.
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class GOpUIManager : MonoBehaviour
    {
        public static GOpUIManager  instance = null;

        public bool                 SeeInspector;
        public bool                 helpBox = true;

        public GameObject           btnPreviousLanguage;
        public GameObject           btnNextLanguage;
        public CurrentText          currentText;

        [System.Serializable]
        public class singleLanguage {
            public List<string> languageList = new List<string>();
        }

        public List<singleLanguage> singleLanguageList = new List<singleLanguage>();

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        public bool B_InitLanguage()
        {
            #region 
            int currentLanguage = LanguageManager.instance.currentLanguage;
            string newString = singleLanguageList[currentLanguage].languageList[currentLanguage];
            currentText.NewTextManageByScript(new List<TextEntry>() { new TextEntry(newString) });
            return true; 
            #endregion
        }

        public void NextLanguage()
        {
            #region 
            LanguageManager.instance.currentLanguage = (LanguageManager.instance.currentLanguage + 1) % singleLanguageList.Count;
            int currentLanguage = LanguageManager.instance.currentLanguage;
            string newString = singleLanguageList[currentLanguage].languageList[currentLanguage];
            currentText.NewTextManageByScript(new List<TextEntry>() { new TextEntry(newString) });
            LanguageManager.instance.Bool_UpdateSelectedLanguage(currentLanguage, true); 
            #endregion
        }

        public void PreviousLanguage()
        {
            #region 
            LanguageManager.instance.currentLanguage--;
            if (LanguageManager.instance.currentLanguage < 0) LanguageManager.instance.currentLanguage = singleLanguageList.Count - 1;
            int currentLanguage = LanguageManager.instance.currentLanguage;
            string newString = singleLanguageList[currentLanguage].languageList[currentLanguage];
            currentText.NewTextManageByScript(new List<TextEntry>() { new TextEntry(newString) });
            LanguageManager.instance.Bool_UpdateSelectedLanguage(currentLanguage, true); 
            #endregion
        }
    }
}