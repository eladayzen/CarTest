// Description: TSLightOptiInit. Init all the objects using TSLightOpti script.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class TSLightOptiInit : MonoBehaviour
    {
        public UnityEvent   WaitForPlayerEvent;

        bool                wait = true;

        void Start()
        {
            #region 
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            while (wait)
            {
                WaitForPlayerEvent?.Invoke();
                yield return null;
            }

            TSLightOpti[] targets = FindObjectsByType<TSLightOpti>(FindObjectsSortMode.None);
            Debug.Log("TSLightOpti: " + targets.Length);
            foreach (TSLightOpti target in targets)
                target.StartCoroutine(target.InitRoutine());

            yield return null; 
            #endregion
        }

        public void WaitForPlayer()
        {
            #region 
            wait = false; 
            #endregion
        }

        public void WaitForTwoPlayers()
        {
            #region
            TSCharacterTag[] targets = FindObjectsByType<TSCharacterTag>(FindObjectsSortMode.None);
            if (targets.Length == 2)
                wait = false; 
            #endregion
        }
       
    }

}
