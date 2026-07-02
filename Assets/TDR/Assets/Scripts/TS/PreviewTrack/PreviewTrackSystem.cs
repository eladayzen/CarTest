// Description: PreviewTrackSystem: Manage the Preview track system. Attached to Grp_PreviewTrack prefab
// Work in association with PreviewTrackAssistant
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class PreviewTrackSystem : MonoBehaviour
    {
        public List<UnityEvent>     eventsList = new List<UnityEvent>();
        public bool                 isPreviewTrackEnabled = true;

        public void StartEventList(int listID)
        {
            #region
            eventsList[listID]?.Invoke(); 
            #endregion
        }

        public void CanvasInGameState(bool state)
        {
            #region 
            CanvasInGameTag.instance.gameObject.SetActive(state); 
            #endregion
        }

        public void PlayATransition(int id)
        {
            #region
            StartCoroutine(TransitionRoutine(id)); 
            #endregion
        }

        IEnumerator TransitionRoutine(int id)
        {
            #region
            TransitionManager.instance.isTransitionPart1Progress = true;
            StartCoroutine(TransitionManager.instance.Transition(id, true));

            yield return new WaitUntil(() => TransitionManager.instance.isTransitionPart1Progress == false);

            TransitionManager.instance.listMultiMethods[id]._animator.speed = 1;
            SceneStepsManager.instance.NextStep();

            yield return null; 
            #endregion
        }
    }

}
