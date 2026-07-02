// Description:  SceneStepsManagerAssistant: Attached to SceneStepsManager. Methods used in SceneStepsManager script
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class SceneStepsManagerAssistant : MonoBehaviour
    {
        public bool Step1()
        {
            #region 
            StartCoroutine(GoStep2());
            return true; 
            #endregion
        }

        IEnumerator GoStep2()
        {
            #region 
            yield return new WaitUntil(() => SceneStepsManager.instance.b_IsSceneStepManagerRunning == false);
            SceneStepsManager.instance.NextStep();
            yield return null; 
            #endregion
        }

        public bool Step2()
        {
            #region 
            StartCoroutine(GoStep3());
            return true; 
            #endregion
        }

        IEnumerator GoStep3()
        {
            #region 
            yield return new WaitUntil(() => SceneStepsManager.instance.b_IsSceneStepManagerRunning == false);
            SceneStepsManager.instance.NextStep();
            yield return null; 
            #endregion
        }

        public bool Step3()
        {
            #region 
            StartCoroutine(GoEnd());
            return true; 
            #endregion
        }

        IEnumerator GoEnd()
        {
            #region 
            //Debug.Log("Wait for End");
            yield return new WaitUntil(() => SceneStepsManager.instance.b_IsSceneStepManagerRunning == false);
            // Debug.Log("It is the End");
            yield return null; 
            #endregion
        }

        public bool DisplayNewPage(int PageNumber)
        {
            #region
            if (CanvasMainMenuManager.instance.currentSelectedPage != PageNumber)
            {
                //Debug.Log("returnCheckState: " + InfoPlayerTS.instance.returnCheckState(0));
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[PageNumber].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(PageNumber);
            }
            return true; 
            #endregion
        }

        public bool DisplayFirstMenuScreen()
        {
            #region 
            int pageToOpen = GameModeGlobal.instance.lastSelectedMenuPage;

            if (CanvasMainMenuManager.instance.currentSelectedPage != pageToOpen)
            {
                PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[pageToOpen].transform.parent.GetComponent<PageIn>();
                currentMenu.DisplayNewPage(pageToOpen);
            }
            //Debug.Log("First PAge");
            return true; 
            #endregion
        }

        public bool PlayMusic(int musicID = 0)
        {
            #region 
            if (musicID == -1)
                MusicManager.instance.MFadeOut(1);
            else
            {
                if (MusicList.instance.ListAudioClip.Count > musicID)
                    MusicManager.instance.MCrossFade(1, MusicList.instance.ListAudioClip[musicID]);
            }
            return true; 
            #endregion
        }

        public bool FadeOutVehiclesAudioSources()
        {
            #region 
            for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
            {
                VehiclesRef.instance.listVehicles[i].GetComponent<VehicleInitAudio>().FadeOut();
            }
            return true; 
            #endregion
        }

        public bool DisableAudioMixerCarEngine()
        {
            #region
            SoundManager.instance._AudioMixer.SetFloat("CarEngineVol", -80);
            return true; 
            #endregion
        }

        public bool EnableAudioMixerCarEngine()
        {
            #region 
            StartCoroutine(EnableAudioMixerCarEngineRoutine());
            return true; 
            #endregion
        }

        IEnumerator EnableAudioMixerCarEngineRoutine()
        {
            #region 
            float value;
            bool result = SoundManager.instance._AudioMixer.GetFloat("CarEngineVol", out value);
            float t = value;
            while (t < 0)
            {
                t = Mathf.MoveTowards(t, 0f, Time.deltaTime * 200);
                SoundManager.instance._AudioMixer.SetFloat("CarEngineVol", t);
                yield return null;
            }

            yield return null; 
            #endregion
        }

        public bool BInitOptimizationGrid()
        {
            #region 
            StartCoroutine(BInitOptimizationGridRoutine());
            return true; 
            #endregion
        }

        IEnumerator BInitOptimizationGridRoutine()
        {
            #region
            if (TSOptiGrid.instance)
                yield return new WaitUntil(() => TSOptiGrid.instance.bInitOptimizationGrid() == true);

            yield return null; 
            #endregion
        }

        public bool FadeOutAudioMixerCarEngine()
        {
            #region 
            StartCoroutine(FadeOutAudioMixerCarEngineRoutine());
            return true; 
            #endregion
        }

        IEnumerator FadeOutAudioMixerCarEngineRoutine()
        {
            #region 
            float value;
            bool result = SoundManager.instance._AudioMixer.GetFloat("CarEngineVol", out value);
            float t = value;
            while (t > -80)
            {
                t = Mathf.MoveTowards(t, -80, Time.deltaTime * 20);
                SoundManager.instance._AudioMixer.SetFloat("CarEngineVol", t);
                yield return null;
            }

            yield return null; 
            #endregion
        }

        public bool ModeFive()
        {
            #region
            ModeFiveTag modeFiveTag = FindFirstObjectByType<ModeFiveTag>( FindObjectsInactive.Include);

            if (modeFiveTag) modeFiveTag.gameObject.SetActive(true);

            return true; 
            #endregion
        }

        public bool ModeFiveDisableTheCountdown() 
        {
            #region
            CanvasInGameTag canvasInGameTag = CanvasInGameTag.instance;
            // Disable the UI Countdown P1 and P2
            canvasInGameTag.objList[0].transform.parent.gameObject.SetActive(false);
            canvasInGameTag.objList[2].transform.parent.gameObject.SetActive(false);

            return true;
            #endregion
        }
    }
}