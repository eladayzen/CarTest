// Description: LapCounterAndPosition: Allows any script to access info about vehcle progression during race (TIme, race position, ...)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace TS.Generics
{
    public class LapCounterAndPosition : MonoBehaviour
    {
        public static LapCounterAndPosition instance = null;
        public Text                         txtVehiclesPosition;
        public bool                         b_InitDone;
        private bool                        b_InitInProgress;
        public List<VehiclePathFollow>      listVehicles = new List<VehiclePathFollow>();
        public List<LapCounterBadge>        lapCounterBadgeList = new List<LapCounterBadge>();

        public List<CurrentText>            txtLapList = new List<CurrentText>();
        public List<CurrentText>            txtPositionList = new List<CurrentText>();
        public List<CurrentText>            txtGlobalTimeList = new List<CurrentText>();
        public List<CurrentText>            txtLapTimeList = new List<CurrentText>();
        
        public List<int>                    updateLapValueList = new List<int>();
        private int                         lastLastValueUpdate = 0;
        bool                                updateAllowed = true;

        private Path                        Track;                                   // A reference to the waypoint-based route we should follow

        public float                        howManyPosChecked = 50;                // How many position on path are checked to know the player position on the track.
        public bool                         bAllowPositionToBeUpdated;

        public int                          howManyLapsInTheCurrentRace = 2;

        public Action<int>                  AVechicleFinishTheRace;
        int                                 interval = 3;
        private float                       delay = 0;

        public List<PosInfo>                posList = new List<PosInfo>();

        public float                        extraForceAppliedToAiInThisRace = 0;


        public float                        overrideStartPart_Duration = 2f;
        public float                        overrideStartPart_Speed = 2.5f;


        [System.Serializable]
        public class PosInfo
        {
            #region
            public float _progression;
            public int ID;
            public int howLapDone = 1;

            public bool IsRaceComplete;

            //-> variable used for find the race position
            public int RacePos;
            public int lastRacePos;                         // Use to change the player position displayed on UI only if his position has changed.

            public float lastPathDistance = 0;

            public float globalTime;
            public List<float> lapsTime = new List<float>();

            public PosInfo(float _progression, int _ID)
            {
                this._progression = _progression;
                this.ID = _ID;
            }
            #endregion
        }

        void Awake()
        {
            #region 
            //-> Check if instance already exists
            if (instance == null)
                instance = this; 
            #endregion
        }

        //-> Init Lap counter
        public bool bInitLapCounter()
        {
            #region
            //-> Play the coroutine Once
            if (!b_InitInProgress)
            {
                b_InitInProgress = true;
                b_InitDone = false;
                StartCoroutine(bInitRoutine());
            }
            //-> Check if the coroutine is finished
            else if (b_InitDone)
                b_InitInProgress = false;


            return b_InitDone;
            #endregion
        }

        IEnumerator bInitRoutine()
        {
            #region
            b_InitDone = false;

            List<VehicleInfo> refList = VehiclesRef.instance.listVehicles;
            for(var i = 0;i< refList.Count; i++)
            {
                listVehicles.Add(refList[i].GetComponent<VehiclePathFollow>());
                posList.Add(new PosInfo(0, i));
            }

            if (txtLapList.Count == 0)
            {
                if (CanvasInGameUIRef.instance)
                {
                    int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
                    for (var i = 0;i< howManyPlayer; i++)
                    {
                        txtLapList.Add(CanvasInGameUIRef.instance.listPlayerUIElements[i].listTexts[0]);
                        txtPositionList.Add(CanvasInGameUIRef.instance.listPlayerUIElements[i].listTexts[1]);

                        txtGlobalTimeList.Add(CanvasInGameUIRef.instance.listPlayerUIElements[i].listTexts[2]);

                        ReturnTextNewLap(i,true);
                        ReturnTextPlayerPosition(i,true);
                        ReturnGlobalTimer(i, true);
                        ReturnLapTimer(i, true);
                    }
                }
            }

            for (var i = 0; i < refList.Count; i++)
            {
                LapCounterBadge lapCounterBadge = refList[i].GetComponentInChildren<LapCounterBadge>();
                if (lapCounterBadge)
                {
                    lapCounterBadge.vehicleID = i;
                    lapCounterBadgeList.Add(lapCounterBadge);
                }
                else
                {
                    lapCounterBadgeList.Add(null);
                }
            }

            Track = PathRef.instance.Track;

            b_InitDone = true;
            yield return null;
            #endregion
        }


        //-> return Player Lap Text;
        string ReturnTextNewLap(int playerID, bool bStart = false)
        {
            #region 
            if (playerID >= posList.Count)
                Debug.Log("TS: ERROR: Check if the number of player < Total vehicle in the race");


            int howManyLap = posList[playerID].howLapDone - 1;
            if (bStart) howManyLap += 1;
            string newLap = howManyLap + "/" + howManyLapsInTheCurrentRace;

            txtLapList[playerID].DisplayTextComponent(
            txtLapList[playerID].gameObject,
            newLap);

            return newLap; 
            #endregion
        }

        //-> return Player Lap Text;
        string ReturnTextPlayerPosition(int playerID, bool bStart = false)
        {
            #region 
            string newPos = (posList[playerID].RacePos + 1).ToString() + "/" + VehiclesRef.instance.listVehicles.Count;
            if (bStart) newPos = VehiclesRef.instance.listVehicles.Count - playerID
                     + "/" + VehiclesRef.instance.listVehicles.Count;

            txtPositionList[playerID].DisplayTextComponent(
             txtPositionList[playerID].gameObject,
            newPos);

            return newPos; 
            #endregion
        }

        //-> return Player Global Timer Text;
        string ReturnGlobalTimer(int playerID, bool bStart = false)
        {
            #region 
            string newPos = FormatTimer(posList[playerID].globalTime);
            txtGlobalTimeList[playerID].DisplayTextComponent(
             txtGlobalTimeList[playerID].gameObject,
            newPos);

            return newPos; 
            #endregion
        }

        void AddNewLapTime(int playerID)
        {
            #region 
            float TotalLap = 0;
            for (var i = 0; i < posList[playerID].lapsTime.Count; i++)
            {
                TotalLap += posList[playerID].lapsTime[i];
            }

            posList[playerID].lapsTime.Add(Mathf.Clamp(posList[playerID].globalTime - TotalLap, 0, posList[playerID].globalTime));

            string lapsText = "";
            for (var i = 0; i < posList[playerID].lapsTime.Count; i++)
            {
                lapsText += FormatTimer(posList[playerID].lapsTime[i]) + "\n";
            } 
            #endregion
        }

        //-> return Player Lap Timer Text;
        string ReturnLapTimer(int playerID, bool bStart = false)
        {
            #region 
            string newLaptext = FormatTimer(posList[playerID].globalTime);
            if (bStart) newLaptext = "";

            return newLaptext; 
            #endregion
        }

        string FormatTimer(float newTime)
        {
            #region 
            int FormatedTimer = (int)(newTime * 1000.0f);
            int minutes = FormatedTimer / (60000);
            int seconds = (FormatedTimer % 60000) / 1000;
            int milliseconds = FormatedTimer % 1000;
            return String.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds); 
            #endregion
        }

        void Update()
        {
            #region 
            if (b_InitDone &&
                   !PauseManager.instance.Bool_IsGamePaused)
            {
                UpdateLapInfo();
                CalaculateVehiclePositionOnThePath();
                CalculateVehiclesPosition();

                if (bAllowPositionToBeUpdated)
                {
                    DisplayPlayerPosition();
                    VehicleGlobalTimer();
                }
            } 
            #endregion
        }

        //-> Display Lap on UI for each player.
        void DisplayPlayerPosition()
        {
            #region 
            int HowManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            if (delay > .3f)
            {
                for (var i = 0; i < HowManyPlayer; i++)
                {

                    if (posList[i].lastRacePos != posList[i].RacePos)
                    {
                        ReturnTextPlayerPosition(i);
                    }


                    posList[i].lastRacePos = posList[i].RacePos;
                }
                delay = 0;
            }

            delay += Time.deltaTime; 
            #endregion
        }

        void CalculateVehiclesPosition()
        {
            #region
            //-> Calculate the vehicle progression
            for (var i = 0; i < VehiclesRef.instance.listVehicles.Count; i++)
            {
                posList[i]._progression = posList[i].howLapDone * Track.pathLength + posList[i].lastPathDistance;
            }

            //-> Calculate the player's position in the race
            var query = posList.ToArray().OrderByDescending(id => id._progression);

            List<PosInfo> tmpPosList = new List<PosInfo>();

            foreach (PosInfo id in query)
                tmpPosList.Add(new PosInfo(id._progression, id.ID));

            for (var i = 0; i < tmpPosList.Count; i++)
            {
                posList[tmpPosList[i].ID].RacePos = i;
                
            }
            #endregion
        }


        public void UpdateLapInfo()
        {
            #region 
            if (updateLapValueList.Count > lastLastValueUpdate && updateAllowed)
            {
                NewLapDone(updateLapValueList[lastLastValueUpdate]);
            } 
            #endregion
        }

        public void NewLapDone(int playerID)
        {
            #region 
            updateAllowed = false;

            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            if (posList[playerID].howLapDone == howManyLapsInTheCurrentRace + 1 &&
                AVechicleFinishTheRace != null)
            {
                //Debug.Log("Race is finished for player : " + playerID);
                posList[playerID].IsRaceComplete = true;
                AVechicleFinishTheRace.Invoke(playerID);
            }

            posList[playerID].howLapDone++;

            //-> Display the new lap on UI
            if (playerID < howManyPlayer)
            {
                if (!posList[playerID].IsRaceComplete)
                    ReturnTextNewLap(playerID);

                if (posList[playerID].howLapDone > 2)
                    AddNewLapTime(playerID);
            }

            lastLastValueUpdate++;
            updateAllowed = true; 
            #endregion
        }

        void CalaculateVehiclePositionOnThePath()
        {
            #region 
            if (Track != null && Time.frameCount % 10 == 0)
            {
                for (var j = 0; j < posList.Count; j++)
                {
                    float closerPos = 0;
                    float lastDistance = 100000;
                    int counter = 0;
                    for (var i = 0; i < howManyPosChecked; i++)
                    {
                        float newDis = (Track.pathLength + posList[j].lastPathDistance - 100 + 200 * (i / howManyPosChecked)) % Track.pathLength;

                        Vector3 testPos = Track.TargetPositionOnPath(newDis);
                        //Debug.Log("Track: " + newDis);

                        float distance = Vector3.Distance(listVehicles[j].transform.position, testPos);
                        if (distance < lastDistance)
                        {
                            lastDistance = distance;
                            closerPos = newDis;
                        }

                        counter++;
                    }

                    posList[j].lastPathDistance = closerPos;
                }

              

            }
            #endregion
        }

        void VehicleGlobalTimer()
        {
            #region 
            for (var j = 0; j < posList.Count; j++)
            {
                if (!posList[j].IsRaceComplete)
                {
                    posList[j].globalTime += Time.deltaTime;

                    if (Time.frameCount % interval == 0 && j < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer)
                        ReturnGlobalTimer(j, true);
                }
            } 
            #endregion
        }

        void OnDrawGizmos()
        {
            #region 
            if (Application.isPlaying)
            {
                if (Track != null)
                {
                    for (var j = 0; j < posList.Count; j++)
                    {
                        Gizmos.color = Color.white;                                                                                // Create a line between the car position and the target position
                        Gizmos.DrawLine(listVehicles[j].transform.position, Track.TargetPositionOnPath(posList[j].lastPathDistance));
                    }
                }
            } 
            #endregion
        }
    }
}
