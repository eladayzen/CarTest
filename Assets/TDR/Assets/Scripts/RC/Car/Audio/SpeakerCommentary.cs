using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class SpeakerCommentary : MonoBehaviour
    {
        public bool OnEnabledPlayAudio = true;
        public bool randomStartPosition;

        AudioSource aSource;

        void Awake()
        {
            aSource = GetComponent<AudioSource>();
        }

        void Start()
        {
           
            Countdown.instance.countdownStartAction += InitSPeakerCommentary;
        }

        void OnDestroy()
        {
            Countdown.instance.countdownStartAction -= InitSPeakerCommentary;
        }

        private void OnEnable()
        {
            if(OnEnabledPlayAudio)
                InitSPeakerCommentary();
        }

        void InitSPeakerCommentary()
        {
            if (!aSource.isPlaying)
            {
                if (randomStartPosition)
                {
                    float startPos = UnityEngine.Random.Range(0, aSource.clip.length);
                    aSource.time = startPos;
                }

                aSource.Play();
            }
           
        }
    }

}
