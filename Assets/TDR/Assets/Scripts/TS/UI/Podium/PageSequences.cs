// PageSequences: Call a sequence of methods inside a UI page
// When podium is displayed. This script is called when the player presses continue button.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics{
    public class PageSequences : MonoBehaviour
    {
        [System.Serializable]
        public class SeqParam
        {
            public List<UnityEvent> eventsSeq;
        }
        public List<SeqParam>       seqParamList = new List<SeqParam>();

        bool                        isSequenceDone = true;

        public void Sequence(int sequenceID)
        {
            #region
            Debug.Log("Seq");
            if(!InfoPlayerTS.instance.b_IsPageCustomPartInProcess && isSequenceDone)
                StartCoroutine(SequenceRoutine(sequenceID));
            #endregion
        }

        IEnumerator SequenceRoutine(int sequenceID)
        {
            #region
            isSequenceDone = false;
            for (var i =0;i< seqParamList[sequenceID].eventsSeq.Count; i++)
            {
                InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
                yield return new WaitUntil(() => InfoPlayerTS.instance.b_IsPageCustomPartInProcess);
                seqParamList[sequenceID].eventsSeq[i]?.Invoke();
                yield return new WaitUntil(() => !InfoPlayerTS.instance.b_IsPageCustomPartInProcess);
            }

            isSequenceDone = true;

            yield return null;
            #endregion
        }

        public void WaitOneSecond(float duration)
        {
            #region
            StartCoroutine(WaitOneSecondRoutine(duration));
            #endregion
        }
        IEnumerator WaitOneSecondRoutine(float duration)
        {
            #region
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            yield return new WaitForSeconds(duration);
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null;
            #endregion
        }
    }

}
