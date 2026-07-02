// Description: StartLine: Managed start race line
using System.Collections.Generic;
using UnityEngine;


namespace TS.Generics
{
    public class StartLine : MonoBehaviour
    {
        [HideInInspector]
        public bool                 SeeInspector;
        [HideInInspector]
        public bool                 moreOptions;
        [HideInInspector]
        public bool                 helpBox = true;

        public int                  editorSelectedList;
        public string               editorNewCountdownName;

        public static StartLine     instance = null;

        public Color                GizmoColor = new Color(0, .9f, 1f, .5f);
        public Color                GizmoGridPosColor = new Color(0, .9f, 1f, .5f);
        public Color                GizmoColorContdownEnd = new Color(0, .9f, 1f, .5f);

        public float                StartPosDistanceFromSTart = 250;
        public PathRef              pathRef;
        public Transform            Grp_StartLineColliders;
        public Transform            objStartLineColliders;
        public Transform            objBufferZoneIn;

        public Transform            Grp_StartLine_3DModels;

        public Transform            EndLineTrackIsLooped;

        public float                gizmoVehicleGridSphereSize = 2;

        public int                  HowManyVehicleGridPos = 1;
        public int                  HowManyVehicleByGridLine = 2;
        public float                ForwardDistanceFromOtherOneVehicle = 50;

        public List<Vector3>        listOffsetOnGrid = new List<Vector3>();

        Vector3                     origin;
        Vector3                     origin2;

        public float                countdownDuration = 3;
        public float                vehicleDistanceEachSecond = 75;

        public  enum StartLineType  { MoveForward, FollowPath };
        public StartLineType        LineType = StartLineType.MoveForward;
        public enum CountdownState  { CountdownDistance, NoCountdownDistance };
        public CountdownState       CountdownType = CountdownState.CountdownDistance;

        public float                DistanceFromStartLineToFirstGridPos = 0;

        public float                InvertMultiplierForARC = -1;



        void Awake()
        {
            #region
            //-> Check if instance already exists
            if (instance == null)
                instance = this;
            #endregion
        }

        public bool ReturnGridPosition(int whichPosOnGrid,Transform Grp_Vehicle)
        {
            #region
            Gizmos.color = GizmoColor;
            if (pathRef)
            {
                Path Track = pathRef.Track;

                int lineCounter = 1;

                //-> Find the grid line
                int count = 0;
                int offsetCounter = 0;

                HowManyVehicleGridPos = ReturnHowManyVehicleDependingCurrentGameMode();

                for (var k = 0; k < whichPosOnGrid; k++)
                {
                    count++;
                    count %= HowManyVehicleByGridLine;
                    if (count == 0)
                        lineCounter++;

                    offsetCounter++;
                    offsetCounter %= listOffsetOnGrid.Count;
                }

                switch (LineType)
                {
                    case StartLineType.MoveForward:
                        MoveForwardGridPosition(Track, lineCounter, offsetCounter, Grp_Vehicle);
                        break;

                    case StartLineType.FollowPath:
                        FollowPathGridPosition(Track, lineCounter, offsetCounter, Grp_Vehicle);
                        break;
                }

              
            }
            return true;
            #endregion
        }

        void FollowPathGridPosition(Path Track, int lineCounter, int offsetCounter, Transform Grp_Vehicle)
        {
            #region
            if (listOffsetOnGrid.Count > 0)
            {

                int howManyPlayer = HowManyVehicleGridPos;
                int count = 0;
                int howManyLinesOnGrid = 1;
                for (var k = 0; k < howManyPlayer; k++)
                {
                    count++;
                    count %= HowManyVehicleByGridLine;
                    if (count == 1)
                        howManyLinesOnGrid++;
                }
               
                //     Debug.Log("whichPosOnGrid: " + whichPosOnGrid);
                float dist = howManyLinesOnGrid * ForwardDistanceFromOtherOneVehicle + DistanceFromStartLineToFirstGridPos - ForwardDistanceFromOtherOneVehicle;


                float countdownDistance = countdownDuration * vehicleDistanceEachSecond - dist + ForwardDistanceFromOtherOneVehicle;
                float dir = 1;
                if (CountdownType == CountdownState.NoCountdownDistance)
                {
                    countdownDistance = 0;
                    // dir = -1;
                }

                if (!Track.TrackIsLooped && Track.checkpoints.Count > 3)
                {
                    countdownDistance -= Track.DistanceFromCheckpointToStart(2);
                }

                float distOnPath = (Track.pathLength + dir * (ForwardDistanceFromOtherOneVehicle * lineCounter - countdownDistance - dist)) % Track.pathLength;

                float distOnPath2 = (Track.pathLength + dir * ((ForwardDistanceFromOtherOneVehicle + 1) * lineCounter - countdownDistance - dist)) % Track.pathLength;


                Vector3 newPos = Track.PositionOnPath(distOnPath, 0);
                Vector3 forwardPos = Track.PositionOnPath(distOnPath2, 0);

                Vector3 forward = (forwardPos - newPos).normalized;
                Vector3 left = Vector3.Cross(forward, Vector3.up).normalized;
                Vector3 Up = Vector3.Cross(left, Vector3.left).normalized;

                if (Up.y < 0) Up = new Vector3(Up.x, Up.y * -1, Up.z);
                newPos += left * listOffsetOnGrid[offsetCounter].x + Up * listOffsetOnGrid[offsetCounter].y - forward * listOffsetOnGrid[offsetCounter].z;

                UpdateVehiclePositionAndRotation(newPos, Grp_Vehicle, -Up, -forward, offsetCounter);
            }
            #endregion
        }

        void MoveForwardGridPosition(Path Track,int lineCounter, int offsetCounter, Transform Grp_Vehicle)
        {
            #region
            origin = Track.checkpoints[0].position;
            origin2 = Track.checkpoints[1].position;

            if (!Track.TrackIsLooped && Track.checkpoints.Count > 3)
            {
                origin = Track.checkpoints[2].position;
                origin2 = Track.checkpoints[3].position;
            }


            Vector3 dir = (origin2 - origin).normalized;
           

            float countdownDistance = countdownDuration * vehicleDistanceEachSecond;
            float dist = DistanceFromStartLineToFirstGridPos;
            if (CountdownType == CountdownState.NoCountdownDistance)
            {
                int howManyPlayer = HowManyVehicleGridPos;
                int count = 0;
                int howManyLinesOnGrid = 1;
                for (var l = 0; l < howManyPlayer; l++)
                {
                    count++;
                    count %= HowManyVehicleByGridLine;
                    if (count == 1)
                        howManyLinesOnGrid++;
                }

                // Debug.Log(howManyLinesOnGrid);
                dist = howManyLinesOnGrid * ForwardDistanceFromOtherOneVehicle + DistanceFromStartLineToFirstGridPos - 2 * ForwardDistanceFromOtherOneVehicle;
                countdownDistance = 0;
            }


            if (listOffsetOnGrid.Count > 0)
            {
                Vector3 newPos = origin - dir * (-ForwardDistanceFromOtherOneVehicle * (lineCounter - 1) + countdownDistance + dist);

                Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
                Vector3 Up = Vector3.Cross(left, Vector3.left).normalized;

                if (Up.y < 0) Up = new Vector3(Up.x, Up.y * -1, Up.z);
                newPos += left * listOffsetOnGrid[offsetCounter].x + Up * listOffsetOnGrid[offsetCounter].y - dir * listOffsetOnGrid[offsetCounter].z;

                UpdateVehiclePositionAndRotation(newPos,  Grp_Vehicle,  -Up * InvertMultiplierForARC,  -dir * InvertMultiplierForARC,  offsetCounter);
            }
            #endregion
        }

        void UpdateVehiclePositionAndRotation(Vector3 newPos,Transform Grp_Vehicle,Vector3 Up,Vector3 dir,int offsetCounter)
        {
            #region
            IVehicleStartLine<Vector3> kPosition = Grp_Vehicle.GetComponent<VehiclePrefabInit>().vehicleInfo.transform.GetComponent<IVehicleStartLine<Vector3>>();
            IGyroStartLine<Quaternion> kRotation = Grp_Vehicle.GetComponent<VehiclePrefabInit>().vehicleInfo.transform.GetComponent<IGyroStartLine<Quaternion>>();

            // Init Vehicle poisition
            kPosition.InitVehiclePosition(newPos);
            // Init Vechicle rotation
            kRotation.InitVehicleGyroPosition(Quaternion.LookRotation(-dir, -Up));
            // Init AI Offset Position on path
            kPosition.InitVehicleOffsetPosition(new Vector3(-listOffsetOnGrid[offsetCounter].x, listOffsetOnGrid[offsetCounter].y, 0));
            #endregion
        }

        void OnDrawGizmos()
        {
            #region
            Gizmos.color = GizmoColor;
            if (pathRef && pathRef.Track.checkpoints.Count > 3)
            {
                switch (LineType)
                {
                    case StartLineType.MoveForward:
                        GizmosMoveForward();
                        break;

                    case StartLineType.FollowPath:
                        GizmosFollowPath();
                        break;
                }
            }
            #endregion
        }

        void GizmosFollowPath()
        {
            #region
            Path Track = pathRef.Track;
            Gizmos.color = GizmoColorContdownEnd;

            int lineCounter = 0;
            int offsetCounter = 0;
            for (var k = 0; k < HowManyVehicleGridPos; k++)
            {
                if (listOffsetOnGrid.Count > 0)
                {
                    if (k % HowManyVehicleByGridLine == 0)
                    {
                        lineCounter++;
                    }

                    int howManyPlayer = HowManyVehicleGridPos;
                    int count = 0;
                    int howManyLinesOnGrid = 1;
                    for (var l = 0; l < howManyPlayer; l++)
                    {
                        count++;
                        count %= HowManyVehicleByGridLine;
                        if (count == 1)
                            howManyLinesOnGrid++;
                    }

                   // Debug.Log(howManyLinesOnGrid);
                    float dist = howManyLinesOnGrid * ForwardDistanceFromOtherOneVehicle + DistanceFromStartLineToFirstGridPos - ForwardDistanceFromOtherOneVehicle;

                    float countdownDistance = countdownDuration * vehicleDistanceEachSecond - dist + ForwardDistanceFromOtherOneVehicle;
                    float dir = 1;
                    if (CountdownType == CountdownState.NoCountdownDistance)
                    {
                        countdownDistance = 0;
                        // dir = -1;
                    }


                    if (!Track.TrackIsLooped && Track.checkpoints.Count > 3)
                    {
                        countdownDistance -= Track.DistanceFromCheckpointToStart(2);
                    }
                       
                    float distOnPath = (Track.pathLength + dir * (ForwardDistanceFromOtherOneVehicle * lineCounter - countdownDistance - dist)) % Track.pathLength;

                    float distOnPath2 = (Track.pathLength + dir * ((ForwardDistanceFromOtherOneVehicle + 1) * lineCounter - countdownDistance - dist)) % Track.pathLength;



                    Vector3 newPos = Track.PositionOnPath(distOnPath, 0);
                   // Debug.Log(Track.pathLength + dir * ((ForwardDistanceFromOtherOneVehicle + 1) * lineCounter - countdownDistance - dist));
                    Vector3 forwardPos = Track.PositionOnPath(distOnPath2, 0);
                  /*  Gizmos.color = Color.red;
                    Gizmos.DrawSphere(newPos, gizmoVehicleGridSphereSize / 2);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(forwardPos, gizmoVehicleGridSphereSize / 2);
                  */

                    Vector3 forward = (forwardPos - newPos).normalized;
                    Vector3 left = Vector3.Cross(forward, Vector3.up).normalized;
                    Vector3 Up = Vector3.Cross(left, Vector3.left).normalized;

                    // Right
                     Gizmos.DrawLine(newPos, newPos + left * 10);
                    // Left
                     Gizmos.DrawLine(newPos, newPos - left * 10);
                    // Up
                    Gizmos.DrawLine(newPos, newPos + Up * 10);
                    // Down
                    Gizmos.DrawLine(newPos, newPos - Up * 10);


                    if (Up.y < 0) Up = new Vector3(Up.x, Up.y * -1, Up.z);

                     newPos += left * listOffsetOnGrid[offsetCounter].x + Up * listOffsetOnGrid[offsetCounter].y - forward * listOffsetOnGrid[offsetCounter].z;

                    Gizmos.DrawSphere(newPos, gizmoVehicleGridSphereSize / 2);
                    Gizmos.DrawWireSphere(newPos, gizmoVehicleGridSphereSize / 2);

                    offsetCounter++;
                    offsetCounter %= listOffsetOnGrid.Count;
                }
            }
            #endregion
        }

        void GizmosMoveForward()
        {
            #region
            Path Track = pathRef.Track;
            Gizmos.color = GizmoColorContdownEnd;

            //-> V2 Line using checkpoint 0 and 1 direction
            origin = Track.checkpoints[0].position;
            origin2 = Track.checkpoints[1].position;

            if (!Track.TrackIsLooped && Track.checkpoints.Count > 3)
            {
                origin = Track.checkpoints[2].position;
                origin2 = Track.checkpoints[3].position;
            }

            Vector3 dir2 = (origin2 - origin).normalized;
            Gizmos.DrawLine(origin, origin - dir2 * countdownDuration * vehicleDistanceEachSecond);


            int lineCounter = 0;
            int offsetCounter = 0;
            for (var k = 0; k < HowManyVehicleGridPos; k++)
            {
                if (listOffsetOnGrid.Count > 0)
                {
                    if (k % HowManyVehicleByGridLine == 0)
                    {
                        lineCounter++;
                    }

                    float countdownDistance = countdownDuration * vehicleDistanceEachSecond;
                    float dist = DistanceFromStartLineToFirstGridPos;
                    if (CountdownType == CountdownState.NoCountdownDistance)
                    {
                        int howManyPlayer = HowManyVehicleGridPos;
                        int count = 0;
                        int howManyLinesOnGrid = 1;
                        for (var l = 0; l < howManyPlayer; l++)
                        {
                            count++;
                            count %= HowManyVehicleByGridLine;
                            if (count == 1)
                                howManyLinesOnGrid++;
                        }

                        // Debug.Log(howManyLinesOnGrid);
                        dist = howManyLinesOnGrid * ForwardDistanceFromOtherOneVehicle + DistanceFromStartLineToFirstGridPos - 2*ForwardDistanceFromOtherOneVehicle;
                        countdownDistance = 0;
                    }
                       



                    Vector3 newPos = origin - dir2 * (-ForwardDistanceFromOtherOneVehicle * (lineCounter - 1) + countdownDistance + dist);

                    Vector3 left = Vector3.Cross(dir2, Vector3.up).normalized;
                    Vector3 Up = Vector3.Cross(left, Vector3.left).normalized;

                    // Right
                    Gizmos.DrawLine(newPos, newPos + left * 10);
                    // Left
                    Gizmos.DrawLine(newPos, newPos - left * 10);

                    // Up
                    Gizmos.DrawLine(newPos, newPos + Up * 10);
                    // Down
                    Gizmos.DrawLine(newPos, newPos - Up * 10);

                    if (Up.y < 0) Up = new Vector3(Up.x, Up.y * -1, Up.z);
                    newPos += left * listOffsetOnGrid[offsetCounter].x + Up * listOffsetOnGrid[offsetCounter].y - dir2 * listOffsetOnGrid[offsetCounter].z;

                    Gizmos.DrawSphere(newPos, gizmoVehicleGridSphereSize / 2);
                    Gizmos.DrawWireSphere(newPos, gizmoVehicleGridSphereSize / 2);

                    offsetCounter++;
                    offsetCounter %= listOffsetOnGrid.Count;
                }
            }
            #endregion
        }

        public int ReturnHowManyVehicleDependingCurrentGameMode()
        {
            #region 
            
            //-> Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                int howManyVehicle = DataRef.instance.arcadeModeData.howManyVehicleByRace;
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    var removeVehicleInVersusMode = DataRef.instance.arcadeModeData.removeVehiclesInVersusMode;
                    howManyVehicle -= removeVehicleInVersusMode;
                }

                return howManyVehicle;
            }

            //-> Time Trial Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 1)
            {
                return 1;
            }

            //-> Championship Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                int currentChampionship = GameModeChampionship.instance.currentSelection;
                int currentTrack = GameModeChampionship.instance.currentTrackInTheList;

                var howManyVehicle = DataRef.instance.championshipModeData.listOfChampionship[currentChampionship].listTrackParams[currentTrack].howManyVehicleByRace;
                if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
                {
                    var removeVehicleInVersusMode = DataRef.instance.championshipModeData.removeVehiclesInVersusMode;
                    howManyVehicle -= removeVehicleInVersusMode;
                }

                return howManyVehicle;
            }

            //-> Test Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 3)
            {
                return InfoRememberMainMenuSelection.instance.playerMainMenuSelection.howManyVehicleInSelectedGameMode;
            }

            //-> Test 1P + No Collision
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 5)
            {
                return 1;
            }

            return InfoRememberMainMenuSelection.instance.playerMainMenuSelection.howManyVehicleInSelectedGameMode; 
            #endregion
        }
    }
}
