// Description: CarAudio. Attached to GrpAudio -> other object inside the vehicle.
// Manage Audio sfx except motor engine
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


namespace TS.Generics
{
    public class CarAudio : MonoBehaviour
    {
        public CarController    carController;
        public CarSide          carSide;
        public CarState         carState;
        public Rigidbody        rb;

        public AudioSource      Skid;
        public float            defaultSkidVol = .15f;
        public float            maxSkidVolume = .7f;
        public AudioSource      impact;
        public AudioImpactSfx   audioImpactSfx;
        public float            impactVolumeMin = .4f;
        public float            impactVolumeMax = .6f;
        public float            impactPitchRange = .1f;

        public AudioSource      surface;

        public AudioSource      sparks;

        public AudioSource      brake;
        public AudioSource      Inside;
        
        float                   refInsideVolume;

        public AudioSource      spring;

        public AudioSource      curb;
        public float            curbMinVolume = .5f;

        public List<Transform>  sparksParticleList = new List<Transform>();
        List<bool>              sparksParticleStateList = new List<bool>();

        public bool             AutoInit = true;

        public bool             isInitDone = false;
        public bool             Once = false;
        public Countdown        countdown;

        public List<float>      SpatialBend = new List<float>();
        public VehicleInfo      VInfo;
        public float            AIOffsetVolume = -.5f;

        public AnimationCurve   surfaceVolumeCurve;
        public AnimationCurve   surfacePitchCurve;

        public AudioMixer       AudioMix;
        public List<AudioLowPassFilter> LowPassFilterList = new List<AudioLowPassFilter>();
        public float            CutOffFrequencyMin = 3000;
        public float            CutOffFrequencyMax = 22000;

        int                     howManyWheels = 0;

        public float            audioFadeOffset = 1; // Use to fade Out or Fade In
        bool                    isFadeProcessDone = true;

        public VehicleDamage    vehicleDamage;
        float                   brakeVol = 0;
        public float            brakeVolMultiplier = 1.25f;

        RoadType                lastAverageSurface = RoadType.Default;

        OnTriggerFeedback       carOnTriggerFeedback;

        public List<float>      aSourceMaxDistanceDependingCamStyle = new List<float>();

        public void OnDestroy()
        {
            #region
            if (vehicleDamage)
            {
                vehicleDamage.VehicleRespawnPart1 -= VehicleOutOfLimitZone;
                vehicleDamage.VehicleRespawnPart2 -= VehicleRespawn;
            }

            if (carOnTriggerFeedback)
                carOnTriggerFeedback.CarOnCollisionEnterAction -= CarCollisionEnterAction;

            #endregion
        }
        private void Start()
        {
           #region 
            howManyWheels = carSide.wheelsList.Count;

            for (var i = 0; i < sparksParticleList.Count; i++)
                sparksParticleStateList.Add(false);

            if (AutoInit)
                StartCoroutine(startRoutine()); 
            #endregion
        }

        void Update()
        {
            #region
            if (isInitDone)
            {
                Skidding();
                CarBrake();

                //if (sparks) CarSparks();

                if (carState.carPlayerType == CarState.CarPlayerType.Human ||
                    !isFadeProcessDone)
                {
                    UpdateInsideSoundVolume();
                    CarSurface();
                    CarCurb();
                }

                if(carController)
                    lastAverageSurface = carController.averageSurface;
            } 
            #endregion
        }

        void CarBrake()
        {
            #region 
            if (lastAverageSurface != carController.averageSurface)
                brake.clip = carController.surfaceData.surfaceList[(int)carController.averageSurface].aBrake;


            float aiMultiplier = 1;
            if (carState.carPlayerType == CarState.CarPlayerType.AI)
                aiMultiplier = .6f;

            if (carState.carForceApply == CarState.CarForceApply.Brake ||
                    carState.carDrift == CarState.CarDrift.Drift)
            {
                if (!brake.isPlaying)
                {
                    brakeVol = .15f;
                    brake.volume = brakeVol * audioFadeOffset * brakeVolMultiplier * aiMultiplier;
                    brake.Play();
                }
                else
                {
                    brakeVol = Mathf.MoveTowards(brakeVol, 25f, Time.deltaTime * 40);
                    brake.volume = Mathf.Clamp(brakeVol / 25 * .2f, .15f, .25f) * audioFadeOffset * brakeVolMultiplier * aiMultiplier;
                }
            }
            else
            {
                brakeVol = Mathf.MoveTowards(brakeVol, 0 * audioFadeOffset, Time.deltaTime * 50);

                if (brake.isPlaying) brake.volume = (brakeVol / 25 * .25f) * audioFadeOffset * brakeVolMultiplier * aiMultiplier;
                else if (brake.isPlaying && brake.volume == 0) brake.Stop();
            } 
            #endregion
        }

        void CarSparks()
        {
            #region 
            bool playSparks = false;

            for (var i = 0; i < howManyWheels; i++)
            {
                if (carSide.wheelsList[i].isObstacle && carSide.wheelsList[i].lastDistance < 1.2f & rb.linearVelocity.magnitude > 1)
                {
                    playSparks = true;
                    break;
                }
            }

            for (var i = 0; i < sparksParticleList.Count; i++)
            {
                if (carSide.wheelsList[i].isObstacle && carSide.wheelsList[i].lastDistance < 1.2f && !sparksParticleStateList[i])
                {
                    sparksParticleList[i].gameObject.SetActive(true);
                    sparksParticleStateList[i] = true;
                }
                else if (!carSide.wheelsList[i].isObstacle && sparksParticleStateList[i])
                {
                    sparksParticleList[i].gameObject.SetActive(false);
                    sparksParticleStateList[i] = false;
                }
            }

            float sparkScale = Mathf.Clamp01(carController.rb.linearVelocity.magnitude / 60);
            for (var i = 0; i < sparksParticleList.Count; i++)
            {
                if (sparksParticleStateList[i])
                {
                    sparksParticleList[i].transform.localScale = Vector3.one * sparkScale * 3f;
                }
            }

            bool isSparksPlaying = sparks.isPlaying;

            if (playSparks && !isSparksPlaying)
            {
                sparks.Play();
            }
            else if (!playSparks && isSparksPlaying)
            {
                sparks.Stop();
            } 
            #endregion
        }

        void Skidding()
        {
            #region
            if(lastAverageSurface != carController.averageSurface)
            {
                Skid.clip = carController.surfaceData.surfaceList[(int)carController.averageSurface].aSkidmark;
                Skid.pitch = carController.surfaceData.surfaceList[(int)carController.averageSurface].skidmarkPitch;
            }
         

            if (carState.carSkid == CarState.CarSkid.Skid && !Skid.isPlaying &&
                carState.carTouchedSurface == CarState.CarTouchedSurface.IsGround)
                Skid.Play();
            else if (carState.carSkid == CarState.CarSkid.NoSkid && Skid.isPlaying)
                Skid.Stop();
            else if (carState.carTouchedSurface == CarState.CarTouchedSurface.InAir && Skid.isPlaying)
                Skid.Stop();

            if (carState.carSkid == CarState.CarSkid.Skid && Skid.isPlaying)
            {
                float sidewayVel = Vector3.Dot(rb.transform.right, rb.linearVelocity);
                float scaledValue = (Mathf.Abs(sidewayVel) - 7) / (40 - 7);

                scaledValue = Mathf.Clamp01(scaledValue) * maxSkidVolume;

                float aiMultiplier = 1;
                if (carState.carPlayerType == CarState.CarPlayerType.AI)
                    aiMultiplier = .6f;


                Skid.volume = (defaultSkidVol + scaledValue) * audioFadeOffset * aiMultiplier;
            }
            #endregion
        }

        public bool IsImpactAudioAllowed()
        {
            #region 
            if (impact && impact.gameObject.activeInHierarchy && !impact.isPlaying && !carController.vehicleInfo.b_IsRespawn)
                return true;
            else
                return false; 
            #endregion
        } 
        public void CarImpact(float volume = 1)
        {
            #region 
            impact.volume = volume * audioFadeOffset;
            impact.Play(); 
            #endregion
        }

        void CarSurface()
        {
            #region 
            if (carController.surfaceData.surfaceList[(int)carController.averageSurface].aSurface
                    != surface.clip ||
                   !surface.isPlaying)
            {
                surface.clip = carController.surfaceData.surfaceList[(int)carController.averageSurface].aSurface;
                surface.Play();
            }

            float magnitude = carController.rb.linearVelocity.magnitude;
            float maxMag = carController.maxSpeed;

            float scaledValue = (magnitude - 0) / (maxMag - 0);

            surface.volume = surfaceVolumeCurve.Evaluate(scaledValue) * audioFadeOffset;
            surface.pitch = surfacePitchCurve.Evaluate(scaledValue); 
            #endregion
        }

        public bool BVehicleInitAudioCarOther()
        {
            #region 
            StartCoroutine(startRoutine());
            return true; 
            #endregion
        }

        IEnumerator startRoutine()
        {
            #region 
            if (Countdown.instance)
                countdown = Countdown.instance;

            carOnTriggerFeedback = carController.GetComponent<OnTriggerFeedback>();
            if(carOnTriggerFeedback)
                carOnTriggerFeedback.CarOnCollisionEnterAction += CarCollisionEnterAction;

            if (vehicleDamage)
            {
                vehicleDamage.VehicleRespawnPart1 += VehicleOutOfLimitZone;
                vehicleDamage.VehicleRespawnPart2 += VehicleRespawn;
            }

            Skid.mute = true;
            impact.mute = true;
            surface.mute = true;
            sparks.mute = true;
            brake.mute = true;
            Inside.mute = true;
            refInsideVolume = Inside.volume;

            // Change Spacial blend if it is car 0 or 1
            if ((VInfo && VInfo.playerNumber == 0)
                ||
                (VInfo && VInfo.playerNumber == 1 && InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2))
            {
                // Solo mode
                float spacialBlend = SpatialBend[0];
                // Versus mode
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    if (VInfo.playerNumber == 0)
                        spacialBlend = SpatialBend[1];
                    if (VInfo.playerNumber == 1)
                        spacialBlend = SpatialBend[2];
                }

                Skid.spatialBlend = spacialBlend;
                impact.spatialBlend = spacialBlend;
                surface.spatialBlend = spacialBlend;
                sparks.spatialBlend = spacialBlend;
                brake.spatialBlend = spacialBlend;
                Inside.spatialBlend = spacialBlend;
                spring.spatialBlend = spacialBlend;
            }


            if (CanvasLoadingManager.instance)
            {
                yield return new WaitUntil(() => !CanvasLoadingManager.instance.b_CanvasLoadingIsDisplayed);
            }

            Skid.mute = false;
            impact.mute = false;
            surface.mute = false;
            sparks.mute = false;
            brake.mute = false;
            Inside.mute = false;

            InitAudioMaxDistance();


            if (carState.carPlayerType == CarState.CarPlayerType.AI)
            {
                surface.gameObject.SetActive(false);
                Inside.gameObject.SetActive(false);
            }

            for (var i = 0; i < sparksParticleList.Count; i++)
            {
                sparksParticleList[i].gameObject.SetActive(false);
                sparksParticleStateList[i] = false;
            }

            isInitDone = true;
            yield return null; 
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

        void UpdateInsideSoundVolume()
        {
            #region 
            float speedRatio = carController.rb.linearVelocity.magnitude / 60;
            Inside.volume = speedRatio;
            Inside.volume = Mathf.Clamp(Inside.volume, 0, refInsideVolume) * audioFadeOffset; 
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
            isFadeProcessDone = false;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                audioFadeOffset = Mathf.Lerp(currentOffset, newValue, t);
                yield return null;
            }

            isFadeProcessDone = true;
            yield return null; 
            #endregion
        }
        public void CarSpring(float volume = 1)
        {
            #region 
            if (!carController.vehicleInfo.b_IsRespawn)
            {
                spring.volume = volume * audioFadeOffset;
                spring.Play();
            } 
            #endregion
        }
        void VehicleOutOfLimitZone()
        {
            #region
            StartCoroutine(FadeAudioEngineRoutine(0));
            #endregion
        }

        void VehicleRespawn()
        {
            #region
            StartCoroutine(FadeAudioEngineRoutine(1));
            #endregion
        }

        void CarCollisionEnterAction(Collision collision)
        {
            #region
            if (carState.carPlayerType == CarState.CarPlayerType.Human)
            {
                if (!impact.isPlaying &&
                    IsAllowedToPlayImpactSFX(collision.gameObject)
                    )
                {
                   // Debug.Log(collision.gameObject.name + " | " + collision.gameObject.layer);

                    float amount = Mathf.Clamp01(collision.relativeVelocity.magnitude / 60);

                    amount += impactVolumeMin;

                    int sel = UnityEngine.Random.Range(0, audioImpactSfx.acImpactList.Count);

                    impact.clip = audioImpactSfx.acImpactList[sel];
                    impact.pitch = UnityEngine.Random.Range(1- impactPitchRange, 1 + impactPitchRange);

                    impact.volume = Mathf.Clamp(amount, 0, impactVolumeMax);

                    impact.Play();

                    //Debug.Log(" | Volume: " + amount);
                }
            }
            #endregion
        }

        bool IsAllowedToPlayImpactSFX(GameObject obj)
        {
            #region 
            if (obj.GetComponent<TagDontUseCarImpactSfx>())
                return false;

            TagDontUseCarImpactSfx[] all = obj.GetComponentsInChildren<TagDontUseCarImpactSfx>(true);
            if (all.Length > 0)
                return false;

            return true; 
            #endregion
        }

        void CarCurb()
        {
            #region 
            if (carController.isCurbDetected && !curb.isPlaying)
            {
                curb.Play();
            }
            else if (!carController.isCurbDetected && curb.isPlaying)
            {
                curb.Stop();
            }

            float magnitude = carController.rb.linearVelocity.magnitude;
            float maxMag = carController.maxSpeed;

            float scaledValue = (magnitude - 0) / (maxMag - 0);

            curb.volume = curbMinVolume + surfaceVolumeCurve.Evaluate(scaledValue) * audioFadeOffset;
            //curb.pitch = surfacePitchCurve.Evaluate(scaledValue);
            #endregion
        }

        void InitAudioMaxDistance()
        {
            int maxDistanceRef = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentCamStyle;
            Skid.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            impact.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            surface.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            sparks.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            brake.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            Inside.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            spring.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
            curb.maxDistance = aSourceMaxDistanceDependingCamStyle[maxDistanceRef];
        }
    }

}
