using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DestructibleAssistant : MonoBehaviour
    {
        public List<AudioClip> audioClipList = new List<AudioClip>();
        public float volumeMultiplier = 1;
        public List<GameObject> switchCollider = new List<GameObject>();

        [HideInInspector]
        DestructiblePropsTag destructibleProps;

        void Start()
        {
            destructibleProps = GetComponent<DestructiblePropsTag>();
        }

        public void DisableWireUtilityPole()
       {
            int siblingIndex = transform.parent.parent.GetSiblingIndex();

            // Disable the wire of the previous Utility pole in the list
            if (siblingIndex > 0)
            {
                transform.parent.parent.parent.GetChild(siblingIndex - 1).transform.GetChild(0).gameObject.SetActive(false);
            }

            // Disable the wire of the Utility pole in the list
            transform.parent.parent.GetChild(0).gameObject.SetActive(false);
        }

        public void PlayAudioClipFromListRandomly(AudioSource audioSource)
        {
            int listID = UnityEngine.Random.Range(0, audioClipList.Count);

            if (listID < audioClipList.Count && audioClipList[listID])
            {
                float ratio = destructibleProps.carImpactMag / 40;
                ratio = Mathf.Clamp(ratio, .5f, 1);
                audioSource.volume = ratio * volumeMultiplier;
                audioSource.clip = audioClipList[listID];
                audioSource.Play();
            }
        }

        public void StopFlagOscillation(MeshRenderer meshRenderer)
        {
            for(var i = 0;i< meshRenderer.materials.Length; i++)
            { 
                meshRenderer.materials[i].SetFloat("SinusSpeed", 0);
               meshRenderer.materials[i].SetFloat("SinusFrequency", 0);
            }
        }


        public void DisabledObjectAfterOneSecond()
        {
            StartCoroutine(DisabledObjectAfterOneSecondRoutine());
        }

        IEnumerator DisabledObjectAfterOneSecondRoutine()
        {
            var t = 0f;
            while(t < 1)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime;
                }

                yield return null;
            }

            switchCollider[0].SetActive(false);
            switchCollider[1].SetActive(true);

            yield return null;
        }
    }

}
