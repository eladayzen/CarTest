// Description: TSOptiGrid: Optimize the number of objects activated in the scene depending the player position.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TS.Generics
{
    public class TSOptiGrid : MonoBehaviour, IValidateAction<bool>
    {
        public static TSOptiGrid                instance;
        public bool                             isInitializedWhenSceneStarts = true;
        //[HideInInspector]
        public bool                             isInitDone = false;
        private bool                            b_InitInProgress;

        [Header("Set Terrain Size")]
        public int                              terrainX = 2100;
        public int                              terrainZ = 2100;

        [Header("Set Grid Size")]
        public int                              row = 5;
        public int                              column = 10;

        [System.Serializable]
        public class TargetParam
        {
            public Transform    target;
            public int          objGridPosRow = 0;
            public int          objGridPosColumn = 0;
            public int          lastPlayerIndex;

            public TargetParam(Transform trans)
            {
                target = trans;
            }
        }

        public List<TargetParam> targetsList = new List<TargetParam>();

        [HideInInspector]
        public List<int>                        activeZoneList = new List<int>();
        [HideInInspector]
        public List<int>                        lastActiveZoneList = new List<int>();

        [System.Serializable]
        public class GrpStreamParam
        {
            public bool             isEnable = false;
            public List<GameObject> objList = new List<GameObject>();
        }

        public List<GrpStreamParam>             steamList = new List<GrpStreamParam>();

        [HideInInspector]
        public bool                             isUpdateZoneProcessDone = true;

        [Space]
        public int                              howManyObjectUpdatedByFrame = 20;

        [Header("Actions During Initialization process")]
        public List<UnityEvent>                 ActionWhenProcessStart = new List<UnityEvent>();
        public List<UnityEvent>                 ActionWhenProcessEnded = new List<UnityEvent>();

        [HideInInspector]
        public bool                             isActionProcessDone = false;

        public bool                             showGizmo = false;
        public float                            gizmoSphereSize = 10;


        public IOptiGridCondition<GameObject>   optiGridCondition;


        public List<ObjDistanceParams>          objsDistanceList = new List<ObjDistanceParams>();

        void Awake()
        {
            #region Create only one instance of the gameObject in the Hierarchy
            if (instance == null)
                instance = this;
            #endregion
        }

        void Start()
        {
            #region
            if (isInitializedWhenSceneStarts)
                StartCoroutine(InitRoutine());
            #endregion
        }

        public void Init()
        {
            #region 
            StartCoroutine(InitRoutine()); 
            #endregion
        }

        public IEnumerator InitRoutine()
        {
            #region
            isInitDone = false;

            optiGridCondition = GetComponent<IOptiGridCondition<GameObject>>();
           
            // Do something before initialization
            for (var i = 0; i < ActionWhenProcessStart.Count; i++)
            {
                isActionProcessDone = false;
                ActionWhenProcessStart[i].Invoke();
                yield return new WaitUntil(() => isActionProcessDone);
            }

            // Find Player
            TSCharacterTag[] targets = FindObjectsByType<TSCharacterTag>(FindObjectsSortMode.None);

            targetsList.Clear();

            int howManyTarget = 0;

            foreach (TSCharacterTag target in targets)
            {
                if (optiGridCondition == null ||
                    (optiGridCondition != null && optiGridCondition.IsThisObjectATarget(target.gameObject)))
                {
                    howManyTarget++;
                    targetsList.Add(new TargetParam(target.transform));
                }
            }
               
            yield return new WaitUntil(() => howManyTarget == targetsList.Count);

            // Create the grid
            steamList.Clear();
            for (var i = 0; i <= column; i++)
                for (var j = 0; j <= row; j++)
                    steamList.Add(new GrpStreamParam());

            TSStreamGridTag[] all = FindObjectsByType<TSStreamGridTag>(FindObjectsSortMode.None);

            foreach (TSStreamGridTag obj in all)
            {
                int objPosRow = Mathf.FloorToInt((obj.transform.position.x- transform.position.x) / (terrainZ / column));
                int objPosColumn = Mathf.FloorToInt((obj.transform.position.z - transform.position.z) / (terrainX / row));

                int index = objPosColumn * (column + 1) + objPosRow;

                if (index >= 0 && index < (column + 1) * (row + 1))
                {
                    steamList[index].objList.Add(obj.gameObject);
                    for (var k = 0; k < obj.transform.childCount; k++)
                    {
                        obj.transform.GetChild(k).gameObject.SetActive(false);
                    }
                }
            }

            // Activate zones close to the player
            activeZoneList = ReturnTargetPositionAndActiveZones();
            StartCoroutine(UpdateActiveZoneOnMapRoutine());
            yield return new WaitUntil(() => isUpdateZoneProcessDone);

            // Init TSStreamDistanceTag
            StartCoroutine(ChangeObjectStateRoutine());

            // Do something after initialization
            for (var i = 0; i < ActionWhenProcessEnded.Count; i++)
            {
                isActionProcessDone = false;
                ActionWhenProcessEnded[i].Invoke();

                yield return new WaitUntil(() => isActionProcessDone);
            }

            isInitDone = true;

            yield return null;
            #endregion
        }

        void Update()
        {
            #region
            if (isInitDone)
            {
                UpdateActiveZone();     
            }
            #endregion
        }

        void UpdateActiveZone()
        {
            #region
            if (isUpdateZoneProcessDone)
                activeZoneList = ReturnTargetPositionAndActiveZones();


            for (var k = 0; k < targetsList.Count; k++)
            {
                if (isUpdateZoneProcessDone)
                {
                    if (targetsList[k].lastPlayerIndex != CurrentPlayerGridIndex(k))
                        StartCoroutine(UpdateActiveZoneOnMapRoutine());

                    targetsList[k].lastPlayerIndex = CurrentPlayerGridIndex(k);
                }
            }
            #endregion
        }

        List<int> ReturnTargetPositionAndActiveZones()
        {
            #region
            activeZoneList.Clear();

            // Find zones close to the player
            for (var k = 0; k < targetsList.Count; k++)
            {
                targetsList[k].objGridPosRow = Mathf.FloorToInt((targetsList[k].target.position.x - transform.position.x) / (terrainZ / column));
                targetsList[k].objGridPosColumn = Mathf.FloorToInt((targetsList[k].target.position.z - transform.position.z) / (terrainX / row));

                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        int index = (targetsList[k].objGridPosColumn * (column + 1) + targetsList[k].objGridPosRow) - (column + 2) + j + (column + 1) * i;
                        if (index >= 0 && index < (column + 1) * (row + 1))
                        {
                            activeZoneList.Add(index);
                        }
                    }
                }
            }

            // Remove duplicate zone
            for (var i = activeZoneList.Count - 1; i >= 0; i--)
            {
                for (var j = 0; j < activeZoneList.Count; j++)
                {
                    if (i != j && activeZoneList[i] == activeZoneList[j])
                    {
                        activeZoneList.RemoveAt(i);
                        break;
                    }
                }
            }


            if (lastActiveZoneList.Count == 0)
            {
                for (var i = 0; i < activeZoneList.Count; i++)
                    lastActiveZoneList.Add(activeZoneList[i]);
            }

            return activeZoneList;
            #endregion
        }

        int CurrentPlayerGridIndex(int iD)
        {
            #region
            return targetsList[iD].objGridPosColumn * (column + 1) + targetsList[iD].objGridPosRow;
            #endregion
        }

        IEnumerator UpdateActiveZoneOnMapRoutine()
        {
            #region
            isUpdateZoneProcessDone = false;
            for (var i = lastActiveZoneList.Count - 1; i >= 0; i--)
            {
                bool needToBeDisabled = true;
                for (var j = 0; j < activeZoneList.Count; j++)
                {
                    if (lastActiveZoneList[i] == activeZoneList[j])
                    {
                        needToBeDisabled = false;
                        break;
                    }
                }
                if (!needToBeDisabled)
                    lastActiveZoneList.RemoveAt(i);
            }


            // Disable zones
            int counter = 0;
            for (var i = 0; i < lastActiveZoneList.Count; i++)
            {
                for (var j = 0; j < steamList[lastActiveZoneList[i]].objList.Count; j++)
                {
                    for (var k = 0; k < steamList[lastActiveZoneList[i]].objList[j].transform.childCount; k++)
                    {
                        steamList[lastActiveZoneList[i]].objList[j].transform.GetChild(k).gameObject.SetActive(false);

                        counter++;
                        if (counter % howManyObjectUpdatedByFrame == howManyObjectUpdatedByFrame - 1)
                            yield return new WaitForEndOfFrame();

                        while(objsDistanceList.Count != 0)
                        {
                            yield return null;
                        }
                    }
                }
                steamList[lastActiveZoneList[i]].isEnable = false;
            }
            lastActiveZoneList.Clear();


            // Enable Zones
            for (var i = 0; i < activeZoneList.Count; i++)
            {
                if (!steamList[activeZoneList[i]].isEnable)
                {
                    for (var j = 0; j < steamList[activeZoneList[i]].objList.Count; j++)
                    {
                        for (var k = 0; k < steamList[activeZoneList[i]].objList[j].transform.childCount; k++)
                        {
                            steamList[activeZoneList[i]].objList[j].transform.GetChild(k).gameObject.SetActive(true);

                            counter++;
                            if (counter % howManyObjectUpdatedByFrame == howManyObjectUpdatedByFrame - 1)
                                yield return new WaitForEndOfFrame();

                            while (objsDistanceList.Count != 0)
                            {
                                yield return null;
                            }
                        }
                    }
                    steamList[activeZoneList[i]].isEnable = true;
                }
            }

            isUpdateZoneProcessDone = true;
            yield return null;
            #endregion
        }

        public void ValidateAction(bool actionState)
        {
            #region
            isActionProcessDone = true;
            #endregion
        }

        void OnDrawGizmosSelected()
        {
            #region
            if (showGizmo)
            {
                // Display Grid
                for (var i = 0; i <= column; i++)
                {
                    for (var j = 0; j <= row; j++)
                    {
                        float ZPos = transform.position.z + ( terrainZ / column * i);
                        float XPos = transform.position.x + (terrainX / row * j);

                        Vector3 point = new Vector3(XPos, transform.position.y, ZPos);
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(point, gizmoSphereSize);
                    }
                }

                //Display Player position
                for (var i = 0; i < targetsList.Count; i++)
                {
                    if (targetsList[i].target)
                    {
                        Vector3 targetPos = targetsList[i].target.position;
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(targetPos, gizmoSphereSize);
                    }
                }
            }
            #endregion
        }

        public bool bInitOptimizationGrid()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                isInitDone = false;
                StartCoroutine(InitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (isInitDone)
                b_InitInProgress = false;

            return isInitDone;
            #endregion
        }

        public void AddObjToList(GameObject obj,bool state)
        {
            objsDistanceList.Add(new ObjDistanceParams(obj, state));
        }

        IEnumerator ChangeObjectStateRoutine()
        {
            #region
            yield return new WaitUntil(() => isInitDone);

            int counter = 0;
            while (objsDistanceList.Count > 0)
            {
                objsDistanceList[objsDistanceList.Count -1].obj.SetActive(objsDistanceList[objsDistanceList.Count - 1].state);
                 objsDistanceList.RemoveAt(objsDistanceList.Count -1);
                counter++;

                if (counter % howManyObjectUpdatedByFrame == howManyObjectUpdatedByFrame - 1)
                    yield return new WaitForEndOfFrame();
            }


            while (objsDistanceList.Count == 0)
                yield return null;

            StartCoroutine(ChangeObjectStateRoutine());
            yield return null;
            #endregion
        }
    }

    [System.Serializable]
    public class ObjDistanceParams
    {
        public GameObject obj;
        public bool state;

        public ObjDistanceParams(GameObject _obj, bool _state)
        {
            obj = _obj;
            state = _state;
        }
    }
}
