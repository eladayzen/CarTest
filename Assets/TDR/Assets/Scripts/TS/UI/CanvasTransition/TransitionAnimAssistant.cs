//Decription: TransitionAnimAssistant.cs. The script is used in association with a transition using animation.
using UnityEngine;

namespace TS.Generics
{
    public class TransitionAnimAssistant : MonoBehaviour
    {
        public float sfxTransitionVolume = .75f;

        //-> This method is used with transition that use Animation.
        // The method is called when the current page is disabled and the new page is enable
        public void TransitionPart1Ended()
        {
            #region 
            TransitionManager.instance.TransitionPart1Ended();
            Debug.Log("Transition Part 2"); 
            #endregion
        }

        //-> This method is used with transition that use Animation.
        // The method is called when the animation ended.
        public void TransitionPart2Ended()
        {
            #region 
            Debug.Log("Transition Part 1");
            TransitionManager.instance.TransitionPart2Ended(); 
            #endregion
        }

        public bool PlaySfx(int value)
        {
            #region 
            SoundFxManager.instance.Play(SfxList.instance.listAudioClip[value], sfxTransitionVolume);
            return true; 
            #endregion
        }
    }
}

