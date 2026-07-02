using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics{
    public class AudioCarEngine : MonoBehaviour
    {
        public bool                     AutoInit = true;
        public bool                     IsInitDone = false;
        public AudioSource              aSourceIdle;
       
        public AudioSource              aSourceMaxRpm;
        public float                    refMaxRpmVolume = 1;

        public List<AudioSource>        aSourceList = new List<AudioSource>();

        public List<AnimationCurve>     aSourceCuveList = new List<AnimationCurve>();

        public List<AnimationCurve>     aSourcePitchCuveList = new List<AnimationCurve>();

        [Range(0.0f, 1.0f)]
        public float                    rpm = 0f;

        public CarController            carController;
        public CarState                 carState;
        public Rigidbody                rb;

        [HideInInspector]
        public int                      currentGear = 0;

        public float                    lastVelocityMag = 0;
        public int                      dir = 1;

        public List<float>              gearSpeed = new List<float>();

        [Header ("More Options")]   
        public bool                     isIdleUsed = true;
        public bool                     isMaxRpmUsed = true;
        public float                    engineVolDuringMaxRpm = .25f;

        public bool                     isTestModeEnabled = false;

        public float                    SpeedStopState = 1.5f;

        public Countdown                countdown;

        public List<float>              SpatialBend = new List<float>();

        public VehicleInfo              VInfo;

        public float                    AIOffsetVolume = -.5f;

        public List<AudioLowPassFilter> LowPassFilterList = new List<AudioLowPassFilter>();
        public float                    CutOffFrequencyMin = 3000;
        public float                    CutOffFrequencyMax = 22000;

        public AudioSource              aExhaustPipe;
        public List<AudioClip>          sExhaustPipeList = new List<AudioClip>();

        int                             lastGear = 0;

        public float                    audioFadeOffset = 0; // Use to fade Out or Fade In

        CarPlayerInputs                 playerInputs;
        public bool                     muteAudio = false;

        public bool                     useExternalAudioEngine = false;

        public List<float>              aSourceMaxDistanceDependingCamStyle = new List<float>();

        void Start()
        {
            #region 
            if (AutoInit)
                BVehicleInitAudioCarEngine(); 
            #endregion
        }

        public bool BVehicleInitAudioCarEngine()
        {
            #region 
            StartCoroutine(InitAudioCarEngineRoutine());
            return true; 
            #endregion
        }

        IEnumerator InitAudioCarEngineRoutine()
        {
            #region 
            if (Countdown.instance)
                countdown = Countdown.instance;

            aSourceIdle.volume = 0;
            aSourceMaxRpm.volume = 0;

            InitAudioMaxDistance();

            for (var i = 0; i < aSourceList.Count; i++)
            {
                aSourceList[i].volume = 0;
            }

            // Change Spacial blend if it is car 0 or 1
            if ((VInfo && VInfo.playerNumber == 0)
                ||
                (VInfo && VInfo.playerNumber == 1 && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2))
            {
                // Solo mode
                float spacialBlend = SpatialBend[0];

                AIOffsetVolume = 0;

                // Versus mode
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    if (VInfo.playerNumber == 0)
                        spacialBlend = SpatialBend[1];
                    if (VInfo.playerNumber == 1)
                        spacialBlend = SpatialBend[2];

                    // if (aSourceIdle)
                    //   aSourceIdle.pitch += .02f;

                    AIOffsetVolume = -.25f;
                }

                for (var i = 0; i < aSourceList.Count; i++)
                    aSourceList[i].spatialBlend = spacialBlend;

                aSourceIdle.spatialBlend = spacialBlend;
                aSourceMaxRpm.spatialBlend = spacialBlend;
                aExhaustPipe.spatialBlend = spacialBlend;


                if (VInfo.playerNumber == 1)
                    aSourceIdle.pitch += .05f;

               
            }

            playerInputs = carController.gameObject.GetComponent<CarPlayerInputs>();

            if (CanvasLoadingManager.instance)
            {
                yield return new WaitUntil(() => !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed);
            }

            muteAudio = false;

            IsInitDone = true;

            yield return null; 
            #endregion
        }

        void Update()
        {
            #region
            if (IsInitDone && !muteAudio)
            {
                if (IsPlayer())
                    rpm = RPM();
                else if (Time.frameCount % 4 == VInfo.playerNumber % 4)
                    rpm = RPM();

                if (countdown && !countdown.b_IsCountdownEnded)
                {
                    if (IsPlayer() && playerInputs.ReturnUpInputValue() > 0)
                    {
                        rpm = Mathf.MoveTowards(rpm, 1, Time.deltaTime * SpeedStopState);
                        CarIsMoving();
                    }
                    else
                    {
                        UpdateVolumeAndPitch();
                        rpm = Mathf.MoveTowards(rpm, 0, Time.deltaTime * SpeedStopState);
                    }


                    if (!useExternalAudioEngine)
                    {
                        CurrentGear();
                    }
                }
                else
                {
                    if (IsPlayer())
                    {
                        if (!useExternalAudioEngine)
                            UpdateVolumeAndPitch();

                        if (!isTestModeEnabled)
                        {
                            CurrentGear();
                            MuteSound();
                        }
                    }
                    else if (Time.frameCount % 4 == VInfo.playerNumber % 4)
                    {
                        if (!useExternalAudioEngine)
                            UpdateVolumeAndPitch();

                        if (!isTestModeEnabled)
                        {
                            CurrentGear();
                            MuteSound();
                        }
                    }     
                }

            }
            #endregion
        }

        void CurrentGear()
        {
            #region
            if (rb)
            {
                float carSpeed = rb.linearVelocity.magnitude * 3.6f;

                bool find = false;
                for (var i = gearSpeed.Count - 1; i >= 0; i--)
                {
                    if (carSpeed > gearSpeed[i])
                    {
                        currentGear = i + 2;
                        currentGear = Mathf.Clamp(currentGear, 0, gearSpeed.Count);
                        find = true;
                        break;
                    }
                }

                if (carState.carDirection == CarState.CarDirection.Backward)
                    currentGear = 1;
                else if (!find && carState.carDirection == CarState.CarDirection.Forward)
                    currentGear = 1;
                else if (!find && carState.carDirection == CarState.CarDirection.Stop)
                    currentGear = 1;

                if (lastGear != currentGear && !useExternalAudioEngine)
                    PlayExhaustPipeSound();


                lastGear = currentGear;
            }
            #endregion
        }

        public float randomMin = 1;
        float RPM()
        {
            #region
            if (countdown && !countdown.b_IsCountdownEnded)
            {
            }
            else if (rb && !isTestModeEnabled)
            {
                float randomRPM = 1;
                if (carState.carTouchedSurface == CarState.CarTouchedSurface.InAir)
                    randomRPM = UnityEngine.Random.Range(.75f, 1f);

                rpm = (rb.linearVelocity.magnitude * 3.6f) / gearSpeed[(int)Mathf.Clamp(currentGear - 1, 0, gearSpeed.Count - 1)];
                rpm *= randomRPM;
            }

            return rpm;
            #endregion
        }

     

        void UpdateVolumeAndPitch()
        {
            #region
            // The car is stopped
            if(isIdleUsed && carState.carDirection == CarState.CarDirection.Stop && !isTestModeEnabled 
                ||
               isIdleUsed && rpm == 0 && isTestModeEnabled)
            {
               
                // Fade out maxRpm sound
                if (aSourceMaxRpm.isPlaying)
                {
                    aSourceMaxRpm.volume = Mathf.MoveTowards(aSourceMaxRpm.volume, 0f, Time.deltaTime * 2);
                    if ( aSourceMaxRpm.volume == 0)
                    aSourceMaxRpm.Stop();
                }

                // Play Idle sound
                if (!aSourceIdle.isPlaying) 
                    aSourceIdle.Play();

                aSourceIdle.volume = Mathf.MoveTowards(aSourceIdle.volume, (.8f + AIOffsetVolume) * audioFadeOffset, Time.deltaTime * 10);

                // Fade out engine sounds.
                for (var i = 0; i < aSourceList.Count; i++)
                {
                    aSourceList[i].volume = Mathf.MoveTowards(aSourceList[i].volume, 0 * audioFadeOffset, Time.deltaTime * 5);
                    aSourceList[i].pitch = aSourcePitchCuveList[i].Evaluate(rpm);
                }
            }
            // The car is moving
            else
            {
              
                CarIsMoving();
            }
            #endregion
        }

        void CarIsMoving()
        {
            #region 
           
            // Init engine sounds
            if (!aSourceList[0].isPlaying)
                for (var i = 0; i < aSourceList.Count; i++)
                    aSourceList[i].Play();

            // Fade out Idle sound
            if (isIdleUsed) aSourceIdle.volume = Mathf.MoveTowards(aSourceIdle.volume, 0, Time.deltaTime * 3);
            if (aSourceIdle.isPlaying && aSourceIdle.volume == 0)
                aSourceIdle.Stop();


          if (rpm == 1 && isMaxRpmUsed)
            {
                // Play MaxRpm sound
                if (!aSourceMaxRpm.isPlaying)
                    aSourceMaxRpm.Play();

                aSourceMaxRpm.volume = Mathf.MoveTowards(aSourceMaxRpm.volume, refMaxRpmVolume + AIOffsetVolume * audioFadeOffset, Time.deltaTime * 10);

                // Modify engine volume
                int lastAsource = aSourceList.Count - 1;
                aSourceList[lastAsource].volume = Mathf.MoveTowards(aSourceList[lastAsource].volume, (engineVolDuringMaxRpm + AIOffsetVolume) * audioFadeOffset, Time.deltaTime * 10);
            }
            // rpm < 1
            else
            {
                // Fade out maxRpm sound
                aSourceMaxRpm.volume = Mathf.MoveTowards(aSourceMaxRpm.volume, 0f, Time.deltaTime * 2);
                if (aSourceMaxRpm.isPlaying && aSourceMaxRpm.volume == 0)
                    aSourceMaxRpm.Stop();

                // Update engine volume and pitch
                for (var i = 0; i < aSourceList.Count; i++)
                {
                    float randomRPM = UnityEngine.Random.Range(randomMin, 1f) * rpm;
                    aSourceList[i].volume = Mathf.MoveTowards(aSourceList[i].volume, (aSourceCuveList[i].Evaluate(randomRPM) + AIOffsetVolume) * audioFadeOffset, Time.deltaTime * 20);
                    aSourceList[i].pitch = aSourcePitchCuveList[i].Evaluate(randomRPM) + UnityEngine.Random.Range(-.02f, .02f);
                }
            } 
            #endregion
        }

        void MuteSound()
        {
            #region 
            #endregion
        }

        public void LowPassFilter(bool isEnable)
        {
            #region
            StartCoroutine(LowPassFilterRoutine(isEnable));
            #endregion
        }

        IEnumerator LowPassFilterRoutine(bool isEnable)
        {
            #region
            float t = 0;
            float duration = .1f;

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                if (!isEnable)
                    for (var i = 0; i < LowPassFilterList.Count; i++)
                    {
                        LowPassFilterList[i].cutoffFrequency = Mathf.Lerp(CutOffFrequencyMin, CutOffFrequencyMax, t);
                        LowPassFilterList[i].lowpassResonanceQ = 0;
                    }
                       

                if (isEnable)
                    for (var i = 0; i < LowPassFilterList.Count; i++)
                    {
                        LowPassFilterList[i].cutoffFrequency = Mathf.Lerp(CutOffFrequencyMax, CutOffFrequencyMin, t);
                        LowPassFilterList[i].lowpassResonanceQ = 0;
                    }
                       
                yield return null;
            }


            yield return null;
            #endregion
        }

        void PlayExhaustPipeSound()
        {
            #region
            if(sExhaustPipeList.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, sExhaustPipeList.Count);
                aExhaustPipe.clip = sExhaustPipeList[rand];
                aExhaustPipe.Play();
            }
            #endregion
        }

        public void RaceEndedFadeOutAudioEngine()
        {
            #region 
            bool isTrackLooped = VehiclesRef.instance.listVehicles[0].GetComponent<VehiclePathFollow>().Track.TrackIsLooped;
            if (!isTrackLooped)
                StartCoroutine(FadeAudioEngineRoutine(0)); 
            #endregion
        }

        public void RaceEndedFadeInAudioEngine()
        {
            #region 
            bool isTrackLooped = VehiclesRef.instance.listVehicles[0].GetComponent<VehiclePathFollow>().Track.TrackIsLooped;
            if (!isTrackLooped)
                StartCoroutine(FadeAudioEngineRoutine(1)); 
            #endregion
        }
        IEnumerator FadeAudioEngineRoutine(float newValue)
        {
            #region 
            float t = 0;
            float duration = .5f;
            float currentOffset = audioFadeOffset;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                audioFadeOffset = Mathf.Lerp(currentOffset, newValue, t);
                yield return null;
            }

            yield return null; 
            #endregion
        }

        bool IsPlayer()
        {
            #region 
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 1 && carController.vehicleInfo.playerNumber == 0)
                return true;

            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2 && carController.vehicleInfo.playerNumber == 0)
                return true;

            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2 && carController.vehicleInfo.playerNumber == 1)
                return true;

            return false; 
            #endregion
        }

        public float GetRPM()
        {
            #region
            return rpm;
            #endregion
        }

        public int GetGear()
        {
            #region
            return currentGear;
            #endregion
        }

        void InitAudioMaxDistance()
        {
            int maxDistanceRef = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentCamStyle;

            aExhaustPipe.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            aSourceIdle.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            aSourceMaxRpm.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];

            for(var i = 0;i< aSourceList.Count; i++)
                aSourceList[i].maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
        }
    }

}
