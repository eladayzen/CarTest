using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class YouDontHaveCarBlinkingText : MonoBehaviour
    {
        public void BlinkTextYouDontHaveCarForThisRace(GameObject obj)
        {
            StopAllCoroutines();
            StartCoroutine(BlinkTextYouDontHaveCarForThisRaceRoutine(obj));
        }

        IEnumerator BlinkTextYouDontHaveCarForThisRaceRoutine(GameObject obj = null)
        {
            for (var i = 0; i < 3; i++)
            {
                float t = 0;
                obj.SetActive(true);
                while (t < .5f)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                obj.SetActive(false);
                t = 0;
                while (t < .5f)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }



            yield return null;
        }
    }

}
