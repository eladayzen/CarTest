// Description: PressAnyKey: Wait until the player press a key to load the next track in championship mode
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class PressAnyKey : MonoBehaviour
    {
        public bool        bOnce;
        public UnityEvent   loadNextTrack;

        void Update()
        {
            #region 
            if (Input.anyKeyDown && 
                !bOnce && 
                !InfoPlayerTS.instance.b_IsPageCustomPartInProcess &&
                !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed &&
                !MusicManager.instance.b_IsFading)
            {
                bOnce = true;
                loadNextTrack.Invoke();
            } 
            #endregion
        }

        //-> Load New Scene
        public void LoadSceneUsingCurrentSelectedTrackName()
        {
            #region 
            string trackName = GameModeGlobal.instance.currentSelectedTrack;
            LoadScene.instance.LoadSceneWithSceneNameAndSpecificCustomMethodList(trackName); 
            #endregion
        }
    }

}
