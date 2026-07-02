// Desciption: DisplayPauseMenu. Start/Stop the pause
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TS.Generics
{
    public class DisplayPauseMenu : MonoBehaviour
    {

        public int          escapeButtonID = 0;      // refers to Input ID in object InfoInputs
        public int          pauseButtonID = 2;       // refers to Input ID in object InfoInputs
        public List<int>    pagesThatAllowsPause = new List<int>();

        public bool         b_EnablePauseModule = true; // Pause Manager is enabled only in gameplay scenes not in the Main Menu Scene

        AudioClip           rememberMusicAudioClip;
        float               clipPosition;

        public void Start()
        {
            #region 
            if (b_EnablePauseModule)
            {
                for (var i = 0; i < InfoInputs.instance.ListOfInputsForEachPlayer.Count; i++)
                {
                    InfoInputs.instance.ListOfInputsForEachPlayer[i].listOfButtons[escapeButtonID].OnGetKeyDownReceived += OnPauseAction;
                    InfoInputs.instance.ListOfInputsForEachPlayer[i].listOfButtons[pauseButtonID].OnGetKeyDownReceived += OnPauseAction;
                }
            } 
            #endregion
        }

        public void OnDestroy()
        {
            #region 
            if (b_EnablePauseModule)
            {
                for (var i = 0; i < InfoInputs.instance.ListOfInputsForEachPlayer.Count; i++)
                {
                    InfoInputs.instance.ListOfInputsForEachPlayer[i].listOfButtons[escapeButtonID].OnGetKeyDownReceived -= OnPauseAction;
                    InfoInputs.instance.ListOfInputsForEachPlayer[i].listOfButtons[pauseButtonID].OnGetKeyDownReceived -= OnPauseAction;
                }
            } 
            #endregion
        }

        //-> Enable the menu page in gameplay scene
        public void EnableMenuPage()
        {
            #region 
            Debug.Log("Pause Page Displayed");
            PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[CanvasMainMenuManager.instance.menuPageInGameplayScene].transform.parent.GetComponent<PageIn>();
            currentMenu.DisplayNewPage(CanvasMainMenuManager.instance.menuPageInGameplayScene); 
            #endregion
        }

        //-> Disable the menu page in gameplay scene
        public void DisableMenuPage()
        {
            #region 
            //Debug.Log("Stop Pause Menu Page");
            PageIn currentMenu = CanvasMainMenuManager.instance.listMenu[CanvasMainMenuManager.instance.gamePageInGameplayScene].transform.parent.GetComponent<PageIn>();
            currentMenu.DisplayNewPage(CanvasMainMenuManager.instance.gamePageInGameplayScene); 
            #endregion
        }

        public void OnPauseAction()
        {
            #region
            //Debug.Log("Pause Starts -1");
            if (IsPauseAllowed())
            {
                //Debug.Log("Pause 0");
                PauseManager.instance.Bool_IsGamePaused = !PauseManager.instance.Bool_IsGamePaused;
                //-> Pause the game
                if (PauseManager.instance.Bool_IsGamePaused)// Check if the game is paused
                { // Check if Game page is displayed in gamplay scene
                    PauseManager.instance.PauseGame(0);
                }
                //-> Unpause the game
                else
                { // Check if Menu page is displayed in gamplay scene
                    PauseManager.instance.UnpauseGame(0);
                }
            } 
            #endregion
        }


        bool IsPauseAllowed()
        {
            #region 
            if (MusicManager.instance.b_IsFading)
                return false;

            if (!PauseManager.instance.isPauseModeEnable)
                return false;

            if (!b_EnablePauseModule)
                return false;

            if (CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed)
                return false;

            //-> 2 Players. Allows a player to enable pause even if the other player has finished the race.
            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            if (howManyPlayer > 1)
            {
                if (!InfoPlayerTS.instance.returnCheckState(0) ||
                    (LapCounterAndPosition.instance.posList[0].IsRaceComplete &&
                    LapCounterAndPosition.instance.posList[1].IsRaceComplete))
                {
                    //Debug.Log("Pause Starts 1");
                    return false;
                }
                  

            }
            else
            {
                if (!InfoPlayerTS.instance.returnCheckState(0) ||
                    LapCounterAndPosition.instance.posList[0].IsRaceComplete)
                    return false;
            }


            foreach (int value in pagesThatAllowsPause)
                if (value == CanvasMainMenuManager.instance.currentSelectedPage)
                {
                   //Debug.Log("Pause Starts 2");
                    return true;
                }

            return false; 
            #endregion
        }

        public void OnPauseAction2()
        {
            #region 
            Debug.Log("--> Pause <--"); 
            #endregion
        }

        public void OnUnPauseAction()
        {
            #region
            Debug.Log("--> UnPause <--"); 
            #endregion
        }

        public void OnPauseTimeScale()
        {
            #region 
            Time.timeScale = 0;
            Debug.Log("--> Pause <--"); 
            #endregion
        }

        public void OnUnPauseTimeScale()
        {
            #region 
            Time.timeScale = 1;
            Debug.Log("--> UnPause <--"); 
            #endregion
        }

        //-> Cursor Visibility
        public void CusrorVisibility(bool state)
        {
            #region
            Debug.Log("Cursor " + state);
            Cursor.visible = state; 
            #endregion
        }

        //-> Music Pause Menu
        public void MusicMenuPause(int musicID)
        {
            #region
            rememberMusicAudioClip = MusicManager.instance.currentAudioClip;
            int currentAudioSource = MusicManager.instance.currentAudioSource;
            clipPosition = MusicManager.instance.ListAudioSource[currentAudioSource].time;
            if (musicID == -1)
                MusicManager.instance.MFadeOut(1);
            else
            {
                if (MusicList.instance.ListAudioClip.Count > musicID)
                {
                    MusicManager.instance.MCrossFade(1, MusicList.instance.ListAudioClip[musicID]);
                }
            } 
            #endregion
        }

        public void MusicMenuUnPause()
        {
            #region 
            if (rememberMusicAudioClip != null)
            {
                MusicManager.instance.MCrossFade(1, rememberMusicAudioClip, 0, clipPosition);
            }
            else
            {
                MusicManager.instance.MFadeOut(1);
            } 
            #endregion
        }

        public void DisableAudioMixerCarEngine()
        {
            #region 
            StopAllCoroutines();
            StartCoroutine(DisableAudioMixerCarEngineRoutine()); 
            #endregion
        }

        public void EnableAudioMixerCarEngine()
        {
            #region 
            StartCoroutine(EnableAudioMixerCarEngineRoutine()); 
            #endregion
        }

        IEnumerator DisableAudioMixerCarEngineRoutine()
        {
            #region 
            float value;
            bool result = SoundManager.instance._AudioMixer.GetFloat("CarEngineVol", out value);
            float t = value;
            while (t > -80)
            {
                t = Mathf.MoveTowards(t, -80, Time.deltaTime * 200);
                SoundManager.instance._AudioMixer.SetFloat("CarEngineVol", t);
                yield return null;
            }

            yield return null; 
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
                t = Mathf.MoveTowards(t, 0, Time.deltaTime * 200);
                SoundManager.instance._AudioMixer.SetFloat("CarEngineVol", t);
                yield return null;
            }

            yield return null; 
            #endregion
        }
    }

}
