// Description: AudioAssistant: Used by the SceneInitManager in the Main Menu to start the Main Menu music
using UnityEngine;
using System.Collections;

namespace TS.Generics
{
    public class AudioAssistant : MonoBehaviour
    {
        public bool PlayMenuMusic()
        {
            #region 
            //Debug.Log("PlayMusic");
            MusicManager.instance.MCrossFade(1, MusicList.instance.ListAudioClip[1]);
            return true; 
            #endregion
        }

        public bool AudioFadeOutAmbiance()
        {
            StartCoroutine(AudioFadeOutAmbianceRoutine());
            return true;
        }


        IEnumerator AudioFadeOutAmbianceRoutine()
        {
            #region 
            float value;
            string ambianceVol = SoundManager.instance.listAudioGroupParams[2].exposedParameterName;
            bool result = SoundManager.instance._AudioMixer.GetFloat(ambianceVol, out value);
            float t = value;

            while (t > -80)
            {
                t = Mathf.MoveTowards(t, -80f, Time.deltaTime * 100);
                SoundManager.instance._AudioMixer.SetFloat(ambianceVol, t);
                yield return null;
            }
            Debug.Log("---> Disable AMbiance 2");
            yield return null;
            #endregion
        }

        public bool AudioFadeInAmbiance()
        {
            StartCoroutine(AudioFadeInAmbianceRoutine());
            return true;
        }

         IEnumerator AudioFadeInAmbianceRoutine()
         {
             #region 
             float value;
            string ambianceVol = SoundManager.instance.listAudioGroupParams[2].exposedParameterName;
            float defaultVolume = SoundManager.instance.listGroupVolume[2];
            bool result = SoundManager.instance._AudioMixer.GetFloat(ambianceVol, out value);
             float t = value;
             while (t < 0)
             {
                 t = Mathf.MoveTowards(t, defaultVolume, Time.deltaTime * 200);
                 SoundManager.instance._AudioMixer.SetFloat(ambianceVol, t);
                 yield return null;
             }

             yield return null;
             #endregion
         }

        public bool AudioDisableCrowdAndSpeaker()
        {
            StartCoroutine(AudioDisableCrowdAndSpeakerRoutine());
            return true;
        }


        IEnumerator AudioDisableCrowdAndSpeakerRoutine()
        {
            #region 
            float value;
            string ambianceVol = SoundManager.instance.listAudioGroupParams[2].exposedParameterName;
            //float defaultVolume = SoundManager.instance.listAudioGroupParams[2].volume;

            bool result = SoundManager.instance._AudioMixer.GetFloat(ambianceVol, out value);
            float t = value;

            while (t > -80)
            {
                t = Mathf.MoveTowards(t, -80f, Time.deltaTime * 100);

                yield return null;
            }


            SpeakerCommentary[] speaker = FindObjectsByType<SpeakerCommentary>(FindObjectsSortMode.None);
            for (var i = 0; i < speaker.Length; i++)
                speaker[i].gameObject.SetActive(false);

            AudioSourceFollowPath[] crowd= FindObjectsByType<AudioSourceFollowPath>(FindObjectsSortMode.None);
            for (var i = 0; i < crowd.Length; i++)
                crowd[i].gameObject.SetActive(false);


            yield return null;
            #endregion
        }

        public void AudioFadeInAmbianceWhenResultPageOpened()
        {
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;
            StartCoroutine(AudioFadeInAmbianceWhenResultPageOpenedRoutine());
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
        }

        IEnumerator AudioFadeInAmbianceWhenResultPageOpenedRoutine()
        {
            #region 
            float value;
            string ambianceVol = SoundManager.instance.listAudioGroupParams[2].exposedParameterName;
            float defaultVolume = SoundManager.instance.listGroupVolume[2];
            bool result = SoundManager.instance._AudioMixer.GetFloat(ambianceVol, out value);
            float t = value;
            while (t < 0)
            {
                t = Mathf.MoveTowards(t, defaultVolume, Time.deltaTime * 200);
                SoundManager.instance._AudioMixer.SetFloat(ambianceVol, t);
                yield return null;
            }

            yield return null;
            #endregion
        }
    }
}
