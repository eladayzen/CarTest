// Description: VehicleDamage.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class VehicleDamage : MonoBehaviour
    {
        public bool b_InitDone;
        private bool b_InitInProgress;
        private VehiclePrefabInit vehiclePrefabInit;

        private VehicleInfo vehicleInfo;
        public Action VehicleExplosionAction;               // Vehicle is destroyed. Life = 0
        public Action VehicleRespawnPart1;                  // Transition between the position where vehicle is destroyed and the position where the vehicle is respawned
        public Action VehicleRespawnPart2;                  // Actions when the vehicle is respawned (init) 

        public Action<int> VehicleLoseLife;
        public Action<int> VehicleWinLife;

        public UnityEvent CustomInit;

        [HideInInspector]
        public Vector3 respawnPoint;
        [HideInInspector]
        public Quaternion respawnRotation;
        [HideInInspector]
        public int checkPointID = 0;


        [Header("Damage and Explosion")]
        public bool IsLifePointInitialized = true;
        public int lifePoints = 10;
        [HideInInspector]
        public int refLifePoints;

        public ParticleSystem damageParticle;
        //private ParticleSystem.EmissionModule damageParticleEmission;

        public GameObject objExplosion;
        public AudioSource aSourceExplosion;                       // AudioSource used to play vehicle explosion sound
        public AudioSource aSourceHit;                       // AudioSource used to play vehicle hit sound

        // Detect wall collisions
        public float sphereRadius = .5f;

        public List<Transform> impactPosList = new List<Transform>();
        public List<Vector3> lastPosList = new List<Vector3>();

        private bool b_LastPosNull = true;
        public Vector3 ImpactPosition = Vector3.zero;

        public List<int> listLayersUsedBylayerMask = new List<int>();
        public LayerMask layerMask;

        public float offsetDistanceMax = 50;
        public float offsetDistanceMin = -200;

        public bool b_Invincibility = false;
        public float invincibilityDuration = 2;

        public UnityEvent invisibilityEvent;

        public CarState carState;

        private void Start()
        {
            #region
            //-> Init LayerMask
            string[] layerUsed = new string[listLayersUsedBylayerMask.Count];
            for (var i = 0; i < listLayersUsedBylayerMask.Count; i++)
                layerUsed[i] = LayerMask.LayerToName(LayersRef.instance.layersListData.listLayerInfo[listLayersUsedBylayerMask[i]].layerID);
            layerMask = LayerMask.GetMask(layerUsed);

            vehicleInfo = GetComponent<VehicleInfo>();

            carState = GetComponent<CarState>();

            for (var i = 0; i < impactPosList.Count; i++)
                lastPosList[i] = impactPosList[i].position;

            refLifePoints = lifePoints;

            UpdateDamageParticleFx(0);
            #endregion
        }

        //-> Initialisation
        public bool bInitVehicleDamage()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(InitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;

            return b_InitDone;
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            vehiclePrefabInit = transform.parent.GetComponent<VehiclePrefabInit>();

            //-> Special Init done with methods connected in the Inspector
            CustomInit?.Invoke();

            b_InitDone = true;
            yield return null;
            #endregion
        }

        void Update()
        {
            #region
            if (b_InitDone && vehiclePrefabInit && vehiclePrefabInit.b_InitDone)
            {
                if (vehicleInfo.b_IsVehicleAvailableToMove && 
                    IsItAPlayer() &&
                    LapCounterAndPosition.instance && !LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].IsRaceComplete)
                    CheckCollisionWithLimitZone();
            }
            #endregion
        }

        void CheckCollisionWithLimitZone()
        {
            #region
            // Init the detection after the vehicle has respawned
            if (b_LastPosNull)
            {
                for (var i = 0; i < impactPosList.Count; i++)
                    lastPosList[i] = impactPosList[i].position;

                b_LastPosNull = false;
            }

            //-> Check collision with a part of the stage (Limit Zone...)
            for (var i = 0; i < impactPosList.Count; i++)
            {
                RaycastHit hit;
                if (!b_LastPosNull && impactPosList[i].gameObject.activeInHierarchy && lastPosList[i] != impactPosList[i].position)
                {
                    if (Physics.Linecast(lastPosList[i], impactPosList[i].position, out hit, layerMask))
                    {
                        //Debug.Log("hit: " + hit.transform.name);
                        b_LastPosNull = true;
                        ImpactPosition = hit.point;
                        VehicleExplosionAction.Invoke();
                        //Debug.Log("Here");
                        break;
                    }
                }
            }

            for (var i = 0; i < impactPosList.Count; i++)
                lastPosList[i] = impactPosList[i].position;
            #endregion
        }

        void OnDrawGizmos()
        {
        }

        //-> Update Vehicle Damage
        public void LifeUpdate(int value, int whatTypeOfDammage = -1, AudioClip hitSound = null, float hitVolume = 1)
        {
            #region

            #endregion
        }

        public void UpdateDamageParticleFx(int value = 0)
        {
            #region
            #endregion
        }

        public IEnumerator InvincibiltyRoutine(GameObject Grp_EnemyDetector)
        {
            #region
            yield return null;
            #endregion

        }

        //-> Update the P1 P2 UI Life Gauge
        public void UILifeGaugeUpdate(int lifePoints)
        {
            #region
            #endregion
        }

        public void InitBloodFx()
        {
            #region
            #endregion
        }

        public void InitUIBulletDelegate()
        {
            #region
            #endregion
        }

        public void SetInvisibility(bool state)
        {
            #region
            #endregion
        }

        public void Invisibility()
        {
            #region
            if (!b_Invincibility)
            {
                b_Invincibility = true;
                invisibilityEvent?.Invoke();
            }
            #endregion
        }

        bool IsItAPlayer()
        {
            #region
            if (carState.carPlayerType == CarState.CarPlayerType.Human ||
               carState.carPlayerType == CarState.CarPlayerType.AI && vehicleInfo.playerNumber == 0)
            {
                return true;
            }


            return false;
            #endregion
        }
    }

}
