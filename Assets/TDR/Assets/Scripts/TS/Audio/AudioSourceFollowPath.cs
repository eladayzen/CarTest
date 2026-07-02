// Description: AudioSourceFollowPath. Andiosource is following the vehicle.
// Used for crowd and sea sound.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class AudioSourceFollowPath : MonoBehaviour
    {
        public bool                 isInitDone = false;
        public List<Vector3>        spotPosList = new List<Vector3>();

        public List<SpotInfo>       spotInfoList = new List<SpotInfo>();

        public Transform            aSourceRefPos;
        AudioSource                 aSource;

        public List<Transform>      targetList = new List<Transform>();
        public List<Rigidbody>      targetRbList = new List<Rigidbody>();

        int                         closestTargetID = 0;

        [HideInInspector]
        public float                dist = 0;

        public float                distanceBetweenTwoSpots = 2;

        [HideInInspector]
        public int                  closestPoint = 0;
        [HideInInspector]
        public float                closestSqrDist = 1000000;

        [SerializeField]
        int howManyPositionChecked = 10;


        [HideInInspector]
        public float                groundOffset = .1f;

        public Vector3              gizmoASourceRefPosSize = Vector3.one;

        public float                spatialBlend2P = .25f;

        void OnEnable()
        {
            if (aSource)
            {
                 float clipDuration = aSource.clip.length;
                aSource.time = UnityEngine.Random.Range(0, clipDuration);
                aSource.Play();
            }
        }
        void Start()
        {
            #region
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            aSource = aSourceRefPos.GetComponent<AudioSource>();

            targetRbList.Clear();
            targetList.Clear();

            if (VehiclesRef.instance && CheckIfCurrentGameModeUseVehicleRefInit())
                yield return new WaitUntil(() => VehiclesRef.instance.b_InitDone);

            yield return new WaitUntil(() => FindTargets());

            yield return new WaitUntil(() => GenerateSpotPosList());

            dist = PathLength();

            isInitDone = true;
            yield return null;
            #endregion
        }

        bool CheckIfCurrentGameModeUseVehicleRefInit()
        {
            #region
            List<int> gameModeList = VehiclesRef.instance.vehicleGlobalData.gameModeThatWaitVehicleRefInitList;
            for (var i = 0;i< gameModeList.Count; i++){
                if (gameModeList[i] == InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode)
                    return true;
            }

            return false;
            #endregion
        }

        bool FindTargets()
        {
            #region
            if (VehiclesRef.instance && CheckIfCurrentGameModeUseVehicleRefInit())
            {
                targetRbList.Add(VehiclesRef.instance.listVehicles[0].GetComponent<Rigidbody>());
                targetList.Add(VehiclesRef.instance.listVehicles[0].transform);

                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    targetRbList.Add(VehiclesRef.instance.listVehicles[1].GetComponent<Rigidbody>());
                    targetList.Add(VehiclesRef.instance.listVehicles[1].transform);
                }
            }
            else
            {
                TSCharacterTag[] cars = FindObjectsByType<TSCharacterTag>(FindObjectsSortMode.None);

                foreach (var car in cars)
                {
                    targetRbList.Add(car.GetComponent<Rigidbody>());
                    targetList.Add(car.transform);
                }
            }
           
            return true;
            #endregion
        }

        float PathLength()
        {
            #region
            for (var i = 0; i < spotPosList.Count - 1; i++)
            {
                Vector3 posA = spotPosList[i] + transform.position;
                Vector3 posB = spotPosList[i + 1] + transform.position;
                dist += Vector3.Distance(posA, posB);
            }
            return dist;
            #endregion
        }

        bool GenerateSpotPosList()
        {
            #region
            spotInfoList.Clear();

            for (var i = 0; i < spotPosList.Count - 1; i++)
            {
                Vector3 posA = spotPosList[i] + transform.position;
                Vector3 posB = spotPosList[i + 1] + transform.position;
                float distance = Vector3.Distance(posA, posB);
                Vector3 direction = (posB - posA).normalized;

                float counter = 0;
                while (counter < distance)
                {
                    Vector3 newPos = posA + direction * counter;
                    Vector3 newDir = (posB - posA).normalized;
                    spotInfoList.Add(new SpotInfo(newPos, newDir));
                    counter += distanceBetweenTwoSpots;
                }
            }

            return true;
            #endregion
        }

        void Update()
        {
            #region
            if (spotPosList.Count > 1 && targetList.Count > 0)
            {
                bool isSpotFound = FindClosestPoint();
                
                if (!isSpotFound)
                {
                   // ResetClosestPosition();
                }  
                else
                {
                    MoveTheAudioSourceDependingPlayerPosition();
                    SpatialBlendingDependingPlayers();
                }
            }
            #endregion
        }

        void ResetClosestPosition()
        {
            #region
            closestSqrDist = 100000;
            #endregion
        }


      

        bool FindClosestPoint()
        {
            #region
            bool isSpotFound = false;
            closestSqrDist = 100000;

            var counter = 0;
            var start = 0;
            List<int> pointsCheckedList = new List<int>();


            if (spotInfoList.Count > 0)
            {
                start = (closestPoint - howManyPositionChecked + spotInfoList.Count) % spotInfoList.Count;




                for (var i = 0; i < howManyPositionChecked*2; i++)
                    pointsCheckedList.Add((start + i) % spotInfoList.Count);
            }





            for (var j = 0; j < targetList.Count; j++)
            {
                for (var i = 0; i < pointsCheckedList.Count; i++)
                {
                    Vector3 pos01 = spotInfoList[pointsCheckedList[i]].pos;

                    Vector3 offset = targetList[j].position - pos01;
                    float sqrLen = offset.sqrMagnitude;
                    //Debug.Log(i + ": " + sqrLen);

                    if (sqrLen <= closestSqrDist && sqrLen < 3000)
                    {
                        closestSqrDist = sqrLen;
                        closestPoint = pointsCheckedList[i];
                        closestTargetID = j;
                        isSpotFound = true;
                        //Debug.Log("C");
                    }
                    /* else
                     {
                         break;
                     }*/

                    counter++;
                }
            }

            if (!isSpotFound)
            {
                closestPoint = 0;
                closestSqrDist = 100000;
                for (var j = 0; j < targetList.Count; j++)
                {
                    for (var i = 0; i < spotInfoList.Count; i++)
                    {
                        Vector3 pos01 = spotInfoList[i].pos;

                        Vector3 offset = targetList[j].position - pos01;
                        float sqrLen = offset.sqrMagnitude;

                        if (sqrLen <= closestSqrDist)
                        {
                            closestSqrDist = sqrLen;
                            closestPoint = i;
                            closestTargetID = j;
                            isSpotFound = true;
                            // Debug.Log("A");
                        }


                        counter++;
                    }
                }
            }


            //Debug.Log("Counter: " + counter);

            return isSpotFound;
            #endregion
        }

        void MoveTheAudioSourceDependingPlayerPosition()
        {
            #region
            if (closestTargetID == 0 && targetRbList.Count > 0 && targetRbList[0] != null)
            {
                Vector3 closetSpot = spotInfoList[closestPoint].pos;

                float speed = 15 * (targetRbList[0].linearVelocity.magnitude / 50);
                speed = Mathf.Clamp(speed, 3, 15);

                aSourceRefPos.position = Vector3.Lerp(aSourceRefPos.position, closetSpot, Time.deltaTime * speed);
            }
            #endregion
        }

        void SpatialBlendingDependingPlayers()
        {
            #region
            float spatialBlendTarget = 0;
            if (closestTargetID != 0)
            {
                float distanceToPath = Vector3.Distance(spotInfoList[closestPoint].pos, targetRbList[closestTargetID].position);
                distanceToPath = Mathf.Clamp(distanceToPath, 0, aSource.maxDistance);
                float ratio = distanceToPath / aSource.maxDistance;

                spatialBlendTarget =  1  * ratio;
                spatialBlendTarget = Mathf.Clamp(spatialBlendTarget, spatialBlend2P, 1);
            }
            else
                spatialBlendTarget = 1;


            aSource.spatialBlend = Mathf.MoveTowards(aSource.spatialBlend, spatialBlendTarget, Time.deltaTime);
            #endregion
        }

        void OnDrawGizmosSelected()
        {
            #region
            if (spotPosList.Count > 1)
            {
                for (var i = 0; i < spotPosList.Count - 1; i++)
                {
                    Gizmos.color = Color.blue;
                    Vector3 pos01 = spotPosList[i] + transform.position;
                    Vector3 pos02 = spotPosList[i + 1] + transform.position;
                    Gizmos.DrawLine(pos01, pos02);
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(aSourceRefPos.position, gizmoASourceRefPosSize);
            #endregion
        }

        public void OnDisableSetSpatialBlend(float value)
        {
            #region
            aSource.spatialBlend = value;
            #endregion
        }
    }

    [System.Serializable]
    public class SpotInfo
    {
        public Vector3 pos;
        public Vector3 dir;

        public SpotInfo(Vector3 _pos, Vector3 _dir)
        {
            pos = _pos;
            dir = _dir;
        }
    }

}
