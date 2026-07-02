// Desciption: DetectVelocityVariation. Audio andCam feedback
// when the are spring variations and velocity variations
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class DetectVelocityVariation : MonoBehaviour
    {
        public Rigidbody        rb;

        float                   lastMag = 0;
       

        public float                   diff = 0;

        public CarAudio         carAudio;

        public CarController    carController;
        LapCounterBadge         lapCounter;
        VehicleDamage           vehicleDamage;
        CarState                carState;

        float                   lastLength = 0;
        float                   diffSpring = 0;

        // TODO: Remove at the end if not needed
        public BoostShakeCam    boostShake;

        public bool             isInitDone = false;

        void Start()
        {
            #region
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            if(InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode != 5)
                yield return new WaitUntil(() => GetComponent<VehicleInfo>().b_InitDone);


            lapCounter = GetComponentInChildren<LapCounterBadge>();
            vehicleDamage = GetComponent<VehicleDamage>();
            carState = GetComponent<CarState>();

           /* if (!boostShake && CamRef.instance)
            {
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 1 && GetComponent<VehicleInfo>().playerNumber == 0)
                    boostShake = CamRef.instance.listCameras[0].transform.parent.GetComponent<BoostShakeCam>();
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2 && GetComponent<VehicleInfo>().playerNumber == 1)
                    boostShake = CamRef.instance.listCameras[1].transform.parent.GetComponent<BoostShakeCam>();

                if(boostShake)
                    vehicleDamage.VehicleRespawnPart2 += boostShake.ShakeForceStop;
            }*/
            isInitDone = true;
            yield return null; 
            #endregion
        }
        void FixedUpdate()
        {
            #region
            if (isInitDone)
            {
                CarMagnitudeComparaison();
                CarSpringCompressionComparaison();
            } 
            #endregion
        }

        void CarSpringCompressionComparaison()
        {
            #region
            diffSpring = carController.wheelsList[0].lastLength - lastLength ;

            if (diffSpring > .01f)
                PlayAudioSpring();

            lastLength = carController.wheelsList[0].lastLength;
            #endregion
        }

        void CarMagnitudeComparaison()
        {
            #region
            diff = lastMag - rb.linearVelocity.magnitude;

            if ((diff > .5f && 
                !IsTheEndOfRace() 
                && !vehicleDamage.b_Invincibility && 
                carState.carRespawn == CarState.CarRespawn.Nothing)
                ||
                (diff > .5f &&
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 5)
                )
            {
                float amount = diff / 15;
                amount = Mathf.Clamp01(amount);

               // Debug.Log("Difference: " + diff + " | Volume: " + amount);
                PlayShakeImpact(amount);
            }
               

            lastMag = rb.linearVelocity.magnitude;
            #endregion
        }

        void PlayShakeImpact(float amount)
        {
            #region
            if (boostShake) boostShake.ShakeWithParams(.55f * amount, .1f * amount, 2);
            #endregion
        }

        void PlayAudioSpring()
        {
            #region
            if (carAudio && 
                carAudio.spring &&
                carAudio.spring.gameObject.activeInHierarchy && 
                !carAudio.spring.isPlaying &&
                !vehicleDamage.b_Invincibility
                )
            {
                float vol = diffSpring * 15;
                vol = Mathf.Clamp(vol,0,.2f);

                carAudio.CarSpring(vol);

                if (boostShake)boostShake.ShakeWithParams(.35f * vol, .3f * vol,1);
            }
            #endregion
        }

        bool IsTheEndOfRace()
        {
            #region 
            if (lapCounter.Lock && LapCounterAndPosition.instance.posList[carController.vehicleInfo.playerNumber].IsRaceComplete)
                return true;
            else
                return false; 
            #endregion
        }

        void OnDestroy()
        {
            #region 
           /* if (boostShake)
                vehicleDamage.VehicleRespawnPart2 -= boostShake.ShakeForceStop; 
           */
            #endregion
        }
    }

}
