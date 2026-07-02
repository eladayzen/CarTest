//Description : CarPathFollow.cs : Allow car AI to follow a path. ||  Use to know the position of each car on race. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TS.Generics {
	public class VehiclePathFollow : MonoBehaviour
	{
        public bool							b_InitDone;
		private bool						b_InitInProgress;
		private VehiclePrefabInit			vehiclePrefabInit;

		[Header("Used during Initialisation")]
		public GameObject					TargetPart1Prefab;
		public GameObject					TargetPart2Prefab;
		public GameObject					TargetPart3Prefab;

		//[HideInInspector]
		public Transform					target;
		[HideInInspector]
		public Transform					refPathTarget;

        [Header ("Info Lap")]
        public float						progressDistance = 0;                   
		public int							lapCounter;
        public float						lapProgression;


        public float						nextPointDistance = 1.45f;
		[HideInInspector]
		public Transform					targetNextPointTurnCheckPos;
		public float						targetPart3Distance = 4f;
		
		[HideInInspector]
		public Transform					targetPart4;
		public float						targetPart4Distance = 1.5f;
		private Vector3						NextPointTurnCheckPos;
		private Quaternion					NextPointTurnCheckRot;
		[HideInInspector]
		public Vector3						refOverridePositionToPath;
		//[HideInInspector]
		public Path							Track;                                   // A reference to the waypoint-based route we should follow

		private bool						pathExist = false;                     // check if there are checkpoints on track path

        [HideInInspector]
		public VehicleInfo					vehicleInfo;
		private VehicleDamage				vehicleDamage;

		public Transform					objLookAtTargetX;
		public Transform					objLookAtTargetY;
		[HideInInspector]
		public Transform					objColliderForcedFollowTHePath;

		public bool							b_IsForcedTargetEnabled = false;

		private GameObject					lastTriggerPathOverride;
		private GameObject					lastTriggerUpdatePath;

		public Vector2						offsetAIPos = Vector2.zero;
		private Vector2						currentOffsetAIPos = Vector2.zero;
        [HideInInspector]
		public Vector2						currentTargetOffsetAIPos = Vector2.zero;
		private float						reachOffsetTargetSpeed = 8;
		public bool							b_MoveAvailable = true;

		private int							currentAltPath = 0;         // Use to determine which alt path is choosed by the AI
		float								distPPath;

		public bool							isAutoInit = false;

		public CarAI						carAI;

		public int							closestDifficultyPos = 0;
		public float						currentDiffOffset = 0;
		public float						difficultyValue = 0;

		private GameObject					lastTriggerTriggerPath;

		public enum WhichSideOfThePath { LeftSide,RightSide};

		public WhichSideOfThePath			whichSideOfThePath;

		public float						distanceToPath = 0;

		bool								isImproveAIModeEnable = false;

		CarState							carState;

		float								randomize = 0;

		float howManyPlayer = 0;


		[Serializable]
		public class PathInfo
		{
			public Path objPath;
			public int ID;
			public PathInfo(Path _objPath, int _ID)
			{
				this.objPath = _objPath;
				this.ID = _ID;
			}
		}

		void Start()
		{
            #region
            if (GetComponent<VehicleDamage>())
            {
                vehicleDamage = GetComponent<VehicleDamage>();
                vehicleDamage.VehicleExplosionAction += VehicleExplosion;
            }

            currentOffsetAIPos = offsetAIPos;
            currentTargetOffsetAIPos = currentOffsetAIPos;

            if (isAutoInit) StartCoroutine(InitRoutine()); 
            #endregion
        }

        void OnDestroy()
        {
            #region
            if (vehicleDamage)
                vehicleDamage.VehicleExplosionAction -= VehicleExplosion; 
            #endregion
        }

		//-> Initialisation
		public bool bInitVehiclePathFollow()
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
			Debug.Log("Init Path");

			howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

			//Clone and Init Path the path
			PathRef pathRef = ReturnPathRef(); 

			if (pathRef)
            {
				int howManyVehicleInTheRace = StartLine.instance.ReturnHowManyVehicleDependingCurrentGameMode();

				if(howManyVehicleInTheRace > 1)
					pathRef.Track.gizmoShowPath = false;
				
				pathRef.Track.difficultyGizmoCurve = false;
				// Connect track to the script
				Track = pathRef.Track;
				objColliderForcedFollowTHePath = pathRef.BonusSpot;

				if (Track != null && Track.checkpoints.Count > 0)
					pathExist = true;

				//-> Create Path Targets
				GameObject newTargetPart01 = Instantiate(TargetPart1Prefab);
				newTargetPart01.name = "P" + transform.GetComponent<VehicleInfo>().playerNumber + "_Target";
				newTargetPart01.transform.GetChild(0).name = "P" + transform.GetComponent<VehicleInfo>().playerNumber + "_Target_Part2";

				GameObject newTargetPart02 = Instantiate(TargetPart2Prefab);
				newTargetPart02.name = "P" + transform.GetComponent<VehicleInfo>().playerNumber + "_Target_Part3";

				GameObject newTargetPart03 = Instantiate(TargetPart3Prefab);
				newTargetPart03.name = "P" + transform.GetComponent<VehicleInfo>().playerNumber + "_Target_Part4";

				//-> Connect path targets to this script
				target = newTargetPart01.transform;                                                                          // access the target that follow the car																
				refPathTarget = target;
				refOverridePositionToPath = target.GetChild(0).transform.localPosition;

				targetNextPointTurnCheckPos = newTargetPart02.transform;
				targetPart4 = newTargetPart03.transform;

				vehicleInfo = GetComponent<VehicleInfo>();
				carState = GetComponent<CarState>();


				vehiclePrefabInit = transform.parent.GetComponent<VehiclePrefabInit>();


				if (Track.TrackIsLooped)
                {
					float distanceFromStartLineDependingGridPosition = StartLine.instance.ForwardDistanceFromOtherOneVehicle * (howManyVehicleInTheRace - 1 - vehiclePrefabInit.startGridPosition);

					float trackIsLoopCheckpointOneDistance = 0;
					progressDistance = (PathRef.instance.Track.pathLength - StartLine.instance.StartPosDistanceFromSTart - distanceFromStartLineDependingGridPosition + trackIsLoopCheckpointOneDistance) % PathRef.instance.Track.pathLength;
				}
                else
                {
					float distanceFromStartLineDependingGridPosition = StartLine.instance.ForwardDistanceFromOtherOneVehicle * (howManyVehicleInTheRace - 1 - vehiclePrefabInit.startGridPosition);

					float trackIsLoopCheckpointOneDistance = Track.checkpointsDistanceFromPathStart[2];
					progressDistance = trackIsLoopCheckpointOneDistance - StartLine.instance.StartPosDistanceFromSTart - distanceFromStartLineDependingGridPosition;

					if (progressDistance < 0)
						Debug.Log("[WARNING] (No Loop Track). Stop Play. Increase the distance between Checkpoint 0 and Checkpoint 1. Start Play mode. Repeat the process until this message disappear.");
				}

				if (carAI)
				{
					carAI.targetOne = target.transform;
					carAI.targetTwo = targetNextPointTurnCheckPos.transform;
				}
			}

			randomize = UnityEngine.Random.Range(-.5f, .5f);

			b_InitDone = true;
			yield return null;
			#endregion
		}

		void Update()
		{
            #region
            if (b_InitDone &&
        Track &&
		Track.isModifiedPathDone &&

		vehiclePrefabInit &&
        vehiclePrefabInit.b_InitDone &&
        !PauseManager.instance.Bool_IsGamePaused &&
        !vehicleInfo.b_IsRespawn/* &&
		Time.frameCount % 2 == vehicleInfo.playerNumber % 2*/)
            {
                if (Track != null && target != null && pathExist)
                {
                    float speedRatio = 1;
                    if (carAI)
                    {
                        speedRatio = carAI.m_Rigidbody.linearVelocity.magnitude / 50;
                    }

                    speedRatio = Mathf.Clamp01(speedRatio);
                    nextPointDistance = 20 - 15 * (1 - speedRatio);
                    currentOffsetAIPos = Vector2.MoveTowards(currentOffsetAIPos, currentTargetOffsetAIPos, Time.deltaTime * reachOffsetTargetSpeed);

                    target.position = Track.TargetPositionOnPath((progressDistance + nextPointDistance) % Track.pathLength);                                   // find the next position for the target	
                    target.position += target.up * currentOffsetAIPos.y + target.right * currentOffsetAIPos.x;
                    target.rotation = Quaternion.LookRotation(Track.TargetRotationOnPath((progressDistance + nextPointDistance) % Track.pathLength));                    // find the new rotation for the target

                    if (targetNextPointTurnCheckPos)
                    {
                        targetNextPointTurnCheckPos.position = Track.TargetPositionOnPath((progressDistance + targetPart3Distance * nextPointDistance) % Track.pathLength);                                   // find the next position for the target	
                        targetNextPointTurnCheckPos.position += targetNextPointTurnCheckPos.up * currentOffsetAIPos.y + targetNextPointTurnCheckPos.right * currentOffsetAIPos.x;
                        targetNextPointTurnCheckPos.rotation = Quaternion.LookRotation(Track.TargetRotationOnPath((progressDistance + targetPart3Distance * nextPointDistance) % Track.pathLength));                    // find the new rotation for the target

                        targetPart4.position = Track.TargetPositionOnPath((progressDistance + targetPart4Distance * nextPointDistance) % Track.pathLength);                                   // find the next position for the target	
                        targetPart4.position += targetPart4.up * currentOffsetAIPos.y + targetPart4.right * currentOffsetAIPos.x;
                        targetPart4.rotation = Quaternion.LookRotation(Track.TargetRotationOnPath((progressDistance + targetPart4Distance * nextPointDistance) % Track.pathLength));
                    }

                    Vector3 progressDelta = Track.TargetPositionOnPath(progressDistance) - transform.position;
                    if (Vector3.Dot(progressDelta, Track.TargetRotationOnPath(progressDistance)) < 0)
                    {                                               // if progress point position is behind the car

                        progressDistance += progressDelta.magnitude * 0.5f;                                                     // change the progress point position
                    }

                    //-> If The player 1 or 2 is too far from the target on the path.
                    // Recalculate the target position.
                    // The target is used to calculate the position of each vehicle.
                    if (carState.carPlayerType == CarState.CarPlayerType.Human &&
						vehicleInfo.playerNumber < howManyPlayer &&
						LapCounterAndPosition.instance.posList.Count > vehicleInfo.playerNumber)
                    {
                        distPPath = progressDistance - LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].lastPathDistance;

                        if (Mathf.Abs(distPPath) > 150)
                            progressDistance = LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].lastPathDistance;
                    }

                    //-> Look to the target on path
                    if (objLookAtTargetY)
                    {
                        objLookAtTargetY.LookAt(target.GetChild(0).transform);
                        objLookAtTargetY.localEulerAngles = new Vector3(0, objLookAtTargetY.localEulerAngles.y, 0);
                    }
                    if (objLookAtTargetX)
                    {
                        objLookAtTargetX.LookAt(target.GetChild(0).transform);
                        objLookAtTargetX.localEulerAngles = new Vector3(objLookAtTargetX.localEulerAngles.x, 0, 0);
                    }
                }

                if (Track != null && progressDistance / Track.pathLength > 1)
                {
                    progressDistance = progressDistance % Track.pathLength;
                    lapCounter++;
                }

                lapProgression = progressDistance / Track.pathLength;

                if (isImproveAIModeEnable && Track.improveAIPathLoopMode)
                    UpdateImproveAIMode();
			   
            }

            if (Track)
            {
				if(Time.frameCount % 4 == vehicleInfo.playerNumber % 4){
					FindClosestDifficultyPos();
					SmoothPathValueDependingGripSurface();

					CheckIfCarIsUsingAnAltPath();
					WhichSideOfTheRoadIsTheCarOn();
				}
               
            } 
            #endregion
        }
	
		void OnDrawGizmos()
		{
            #region
            if (Application.isPlaying && Track != null && target && target.GetChild(0).transform != null)
            {
				// Create a line between the car position and the target position
				Gizmos.color = Color.yellow;                                                                               
                Gizmos.DrawLine(transform.position, target.GetChild(0).transform.position);

				Gizmos.color = Color.red;                                                                               
                Gizmos.DrawSphere(NextPointTurnCheckPos, 5);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(NextPointTurnCheckPos, NextPointTurnCheckRot.eulerAngles * 20);

                DrawCurrentDifficultyPathPositions();
            } 
            #endregion
        }


        public void ForcedFollowSpecificTarget(Transform newTarget,Transform objInsertAfter)
        {
            #region
            b_IsForcedTargetEnabled = true;

            //Debug.Log("newTarget: " + newTarget.name);
            NewOffsetTarget(new Vector2(0, 0), false);

            objColliderForcedFollowTHePath.position = newTarget.position;
            objColliderForcedFollowTHePath.rotation = newTarget.rotation;

            for (var i = 0; i < Track.checkpoints.Count; i++)
            {
                if (Track.checkpoints[i] == objColliderForcedFollowTHePath)
                {
                    Track.checkpoints.RemoveAt(i);
                    break;
                }
            }

            for (var i = 0; i < Track.checkpoints.Count; i++)
            {
                if (objInsertAfter.position == Track.checkpoints[i].position)
                {
                    Transform trans;

                    trans = objColliderForcedFollowTHePath;

                    int insertPos = (i + 1) % Track.checkpoints.Count;
                    Track.checkpoints.Insert(insertPos, trans);
                    Track.ModifyThePath();

					break;
                }
            } 
            #endregion
        }

        public void ForcedFollowThePath()
        {
            #region
            b_IsForcedTargetEnabled = false;
            target = refPathTarget;
            NewOffsetTarget(Vector2.zero, true);    //-> Init the vehicle offset position 
            #endregion
        }

		public void NewOffsetTarget(Vector2 newOffset = default(Vector2), bool b_DefaultOffset = false)
		{
            #region
            if (!b_DefaultOffset)
                currentTargetOffsetAIPos = newOffset;
            else
                currentTargetOffsetAIPos = offsetAIPos; 
            #endregion
        }

        public void VehicleExplosion()
        {
            #region
            ForcedFollowThePath(); 
            #endregion
        }

		void OnTriggerEnter(Collider other)
		{
            if (b_InitDone && vehiclePrefabInit && vehiclePrefabInit.b_InitDone)
            {
				if (carAI && carAI.IsPlayerAI() &&
					!carAI.detectCarAltPath.IsCollisionDetected())
				{

					CheckAltPath(other.gameObject);

                    //->
					TriggerPathOverride triggerPathOverride = other.GetComponent<TriggerPathOverride>();
					if (triggerPathOverride &&
						other.gameObject != lastTriggerPathOverride)
					{
						if (!b_IsForcedTargetEnabled)
						{
							if (target)
							{
								if (triggerPathOverride.refPosition)
									target.GetChild(0).transform.localPosition = refOverridePositionToPath;
								else
									target.GetChild(0).transform.localPosition = triggerPathOverride.OverrideTargetPosition;
							}
						}
						else
						{
							if (refPathTarget)
							{
								if (triggerPathOverride.refPosition)
									refPathTarget.GetChild(0).transform.localPosition = refOverridePositionToPath;
								else
									refPathTarget.GetChild(0).transform.localPosition = triggerPathOverride.OverrideTargetPosition;
							}
						}

						lastTriggerPathOverride = other.gameObject;
					}
				}
                //-> Vehicle is not managed by the AI (P1 or P2)
                else
                {
					
					//-> The Player P1 P2 use an Alternative Path
					if (other.GetComponent<AltPathPlayerTrigger>() &&
						other.gameObject != lastTriggerUpdatePath)
					{
						//Debug.Log("Player Trigger");
						TriggerAltPath triggerAltPath = other.GetComponent<AltPathPlayerTrigger>().altPath.triggerAltPath;
						int whichAltPath = 0;
					    for (var i = 0; i < triggerAltPath.AltPathList.Count; i++)
						{
							if (triggerAltPath.AltPathList[i] == other.GetComponent<AltPathPlayerTrigger>().altPath)
							{
								break;
							}
							else
							{
								whichAltPath++;
							}
						}
						//Debug.Log("whichAltPath: " + whichAltPath + " --> ");
						CheckAltPath(other.GetComponent<AltPathPlayerTrigger>().altPath.triggerAltPath.gameObject, whichAltPath,true);
						lastTriggerUpdatePath = other.gameObject;
					}

					if (other.GetComponent<TriggerAltPath>() &&
						other.gameObject != lastTriggerUpdatePath)
					{
						int whichAltPath = -2;
						CheckAltPath(other.gameObject, whichAltPath, true);
						lastTriggerUpdatePath = other.gameObject;
					}
				}
			}
		}


		// WHen the AI enter into a TriggerAltPath. Determine if the AI Path must be updated 
		void CheckAltPath(GameObject other,int whichAltPath = -1,bool forceUpdate = false)
        {
			
			TriggerAltPath triggerAltPath = other.GetComponent<TriggerAltPath>();
			int aiID = vehicleInfo.playerNumber;


			if (triggerAltPath && 
				!LapCounterAndPosition.instance.posList[aiID].IsRaceComplete && 
				(lastTriggerTriggerPath != triggerAltPath.gameObject || forceUpdate))
			{
				lastTriggerTriggerPath = triggerAltPath.gameObject;
				//Debug.Log("other :" + other.name + " -> whichAltPath: " + whichAltPath);

				//-> Select a Path with random value
				// AI case
				if (whichAltPath == -1)
                {
					int howManyRealPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

					//-> Check best path probability
					int currentAIDifficulty = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentDifficulty;
					float proba = 0;

                    if (aiID >= howManyRealPlayer)
					{
						int pos = Mathf.Abs(aiID + 1 - GameModeGlobal.instance.vehicleIDList.Count);

						pos = Mathf.Clamp(pos, 0, DataRef.instance.difficultyManagerData.difficultyParamsList[currentAIDifficulty].aICarParams.Count - 1);
						proba = DataRef.instance.difficultyManagerData.difficultyParamsList[currentAIDifficulty].aICarParams[pos].chooseBestAltPath;
					}

					int rand = UnityEngine.Random.Range(0, 100);

                    //-> Use the best path
                    if(proba >= rand)
						currentAltPath = triggerAltPath.bestPath + 1;
                    //-> Use a random selected path except the best path
                    else
                    {
						while(currentAltPath == triggerAltPath.bestPath + 1)
							currentAltPath = UnityEngine.Random.Range(0, triggerAltPath.AltPathList.Count + 1);
						
					}
				}
                // P1|P2 case
                else
                {
					currentAltPath = whichAltPath;
				}

				// Find on which chekpoint the altpath starts;
				for (var i = 0; i < Track.checkpointsRef.Count; i++)
				{
					if (Track.checkpointsRef[i].position == triggerAltPath.checkpointParent.position)
					{
						//Debug.Log("Checkpoint :" + i + " -> currentAltPath: " + currentAltPath);
						Track.currentCheckpointID = i;
						Track.currentAltPathID = currentAltPath;
						
						StartCoroutine(WaitBeforeTheEndOfThePath());
						break;
					}
				}

				//Debug.Log("Alt Path numebr: " + currentAltPath);

				// The player P1 or P2 use the Main path
				if (whichAltPath == -2)
				{
					#region
					//-> Create a temporary list of path checkpoints using the Main Path Checkpoints
					List<Transform> tmpCheckpoints = new List<Transform>(Track.checkpointsRef);
					
					int counter = 0;
					for (var i = 0; i < tmpCheckpoints.Count; i++)
					{
						if (PathRef.instance.Track.checkpointsRef[i] == triggerAltPath.checkpointParent)
						{
							//counter++;
							break;
						}
						else
						{
							counter++;
						}
					}
					
					//-> Update Track Checkpoints
					Track.checkpoints = new List<Transform>(tmpCheckpoints);

					//-> Update All the parameters needed to use the Track
					Track.ModifyThePath();
                    
					//-> Recalculate the AI target to follow
					progressDistance = Track.DistanceFromCheckpointToStart(counter) + nextPointDistance;
					#endregion
				}
				//-> If currentAltPath != 0 an Alt path is selected. So add Alt Path checkpoints to the Main Path
				// currentAltPath != 0 means that it is a request done by AI
				// whichAltPath != -1  means that it is a request done by P1|P2
				else if (currentAltPath != 0 || whichAltPath != -1)
				{
					#region
					
					//-> Create a temporary list of path checkpoints using the Main Path Checkpoints
					List<Transform> tmpCheckpoints = new List<Transform>(Track.checkpointsRef);

                    if (whichAltPath == -1)
						currentAltPath--;

					//-> Find the Alt Path
					AltPath altPath = null;
					if (currentAltPath >= 0 && triggerAltPath.AltPathList.Count > currentAltPath)
						altPath = triggerAltPath.AltPathList[currentAltPath];

					if (altPath)
					{
						int counter = 0;
						// Find where the Alt Path starts on the Main Path Point
						for (var i = 0; i < tmpCheckpoints.Count; i++)
						{
							if (PathRef.instance.Track.checkpointsRef[i] == triggerAltPath.AltPathList[currentAltPath].checkpointStart)
							{
								counter++;
								break;
							}
							else
							{
								counter++;
							}
						}
						//Debug.Log("spot: " + counter + " :: " + "AltPath: " + currentAltPath);

						// Remove points between the checkpointStart and checkpointEnd
						int HowManyPointsBetweenTheTwoPoints = 0;
						for (var i = counter; i < tmpCheckpoints.Count; i++)
						{
							if (PathRef.instance.Track.checkpointsRef[i] == triggerAltPath.AltPathList[currentAltPath].checkpointEnd)
							{
								//HowManyPointsBetweenTheTwoPoints--;
								break;
							}
							else
							{
								HowManyPointsBetweenTheTwoPoints++;
							}
						}

						//Debug.Log("HowManyPointsBetweenTheTwoPoints: " + HowManyPointsBetweenTheTwoPoints);
						// Remove checkpoints that are not needed
						for (var i = 0; i < HowManyPointsBetweenTheTwoPoints; i++)
						{
							//Debug.Log(i + " : " + tmpCheckpoints[counter].name);
							tmpCheckpoints.RemoveAt(counter);
						}


						//-> Add Alt Path checkpoints
						for (var i = altPath.tmpCheckpoints.Count - 1; i >= 0; i--)
						{tmpCheckpoints.Insert(counter, altPath.tmpCheckpoints[i]);}

                        //-> Update Track Checkpoints
						Track.checkpoints = new List<Transform>(tmpCheckpoints);

                        //-> Update All the parameters needed to use the Track
						Track.ModifyThePath();

						//-> Recalculate the AI target to follow
						progressDistance = Track.DistanceFromCheckpointToStart(counter-1) + nextPointDistance;
						
					}
					
					#endregion
				}
					
			}
		}

		public void UpdateOffsetPathPosition(float xOffset = 0,float yOffset = 0)
        {
            #region
            //	Debug.Log("here: " + xOffset );
            offsetAIPos = new Vector2(xOffset + randomize, yOffset);
            currentOffsetAIPos = offsetAIPos;
            currentTargetOffsetAIPos = currentOffsetAIPos; 
            #endregion
        }

		void DrawCurrentDifficultyPathPositions()
		{
            #region
            if (Track.speedRatioDependingGripList.Count > 0)
            {
                Gizmos.color = Track.trackDifficulty.Evaluate(Track.speedRatioDependingGripList[Track.selectedId].speedGridList[carAI.carController.mostUseSurface].speedRatio[closestDifficultyPos]);
                Gizmos.DrawSphere(target.position, 1);
            } 
            #endregion
        }
		
		public void FindClosestDifficultyPos()
        {
			#region
			if (Track.speedRatioDependingGripList.Count > 0)
            {
				var closest = progressDistance / Track.pathLength;

				var howManyListValues = Track.speedRatioDependingGripList[Track.selectedId].speedGridList[carAI.carController.mostUseSurface].speedRatio.Count;

				closest *= howManyListValues;

				closestDifficultyPos = Mathf.RoundToInt(closest);
				closestDifficultyPos = Mathf.Clamp(closestDifficultyPos, 0, howManyListValues - 1);

				for(var i = 0;i< Track.difficultyOffset.Count; i++)
                {
					if (Track.difficultyOffset[i].spotID <= closestDifficultyPos)
						currentDiffOffset = -Track.difficultyOffset[i].difficultyOffset;

				}

				difficultyValue = Track.speedRatioDependingGripList[Track.selectedId].speedGridList[carAI.carController.mostUseSurface].speedRatio[closestDifficultyPos];

			}
			#endregion
		}

		public void SmoothPathValueDependingGripSurface()
		{
			#region
			var scaledValue = 1 - (carAI.carController.gripAmountDependingSurface - .25f) / (1 - .25f);
			scaledValue = Mathf.Clamp01(scaledValue);
			Track.smoothDifficultyDependingGripSurface = Mathf.RoundToInt(scaledValue * 60);

			Track.currentGizmoSurface = carAI.carController.mostUseSurface;


			#endregion
		}

		// Use to know if the vehicle is close to the end of the path
		public void CheckIfCarIsUsingAnAltPath()
        {
            #region
            if (Track.isUsingAltPath)
            {
                int indexOfLastAltPathSpotInList = Track.allPathList[Track.selectedId].indexOfLastAltPathSpotInList;
                //Debug.Log("selectedId: " + Track.selectedId + " | indexOfLastAltPathSpotInList:" + indexOfLastAltPathSpotInList);
                if (progressDistance >= Track.allPathList[Track.selectedId].distanceFromPathList[indexOfLastAltPathSpotInList])
                {
                    //-> Update Track Checkpoints
                    Track.checkpoints = new List<Transform>(Track.checkpointsRef);

                    //-> Update All the parameters needed to use the Track
                    Track.ModifyThePath();

                    //Debug.Log("Here: " + indexOfLastAltPathSpotInList);

                    //-> Recalculate the AI target to follow
                    progressDistance = Track.DistanceFromCheckpointToStart(indexOfLastAltPathSpotInList) + nextPointDistance;

                    Track.isUsingAltPath = false;
                }
            } 
            #endregion
        }

		public void WhichSideOfTheRoadIsTheCarOn()
		{
			#region
			Vector3 pathPos1 = Track.PositionOnPath(progressDistance);
			Vector3 pathPos2 = Track.PositionOnPath(progressDistance + 1);
			Vector3 pathDir = (pathPos2 - pathPos1).normalized;
			Vector3 targetDir = (transform.position - pathPos1).normalized;

			if (IsTheObstacleOnRightOrLeftOfThePath(pathDir, targetDir, Vector3.up) > 0)
            {
				whichSideOfThePath = WhichSideOfThePath.LeftSide;
			}
            else
            {
				whichSideOfThePath = WhichSideOfThePath.RightSide;
			}

			distanceToPath = Vector3.Distance(transform.position, pathPos1);

			#endregion
		}

		int IsTheObstacleOnRightOrLeftOfThePath(Vector3 fwd, Vector3 targetDir, Vector3 up)
		{
			#region
			Vector3 right = Vector3.Cross(up, fwd);
			float dir = Vector3.Dot(right, targetDir);

			if (dir > 0f)
				return -1;
			else if (dir < 0f)
				return 1;
			else
				return 0;
			#endregion
		}

		PathRef ReturnPathRef()
		{
			#region
			// Mode 3 (Improve Path 0 player + 1 AI)
			int currentGameMode = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode;
			if(currentGameMode == 3)
            {
				int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;
				int howManyVehicle = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.howManyVehicleInSelectedGameMode;

				if (howManyPlayer == 0 && howManyVehicle == 1)
                {
					isImproveAIModeEnable = true;
					LapCounterAndPosition.instance.howManyLapsInTheCurrentRace = 999;
					return PathRef.instance;
				}
			}

			// General case
			return PathRef.instance ? Instantiate(PathRef.instance, PathRef.instance.transform.parent) : null;
			#endregion
		}

		public void UpdateImproveAIMode(bool forceRespawn = false)
        {
			#region if isImproveAIModeEnable = true AI vehicle is respawn to start spotID when the vehicle reach end Spot ID
			float endDistance = Track.improveAIPathEndID * Track.spotDifficultyDistance;
			int howManySpots = Mathf.RoundToInt(Track.pathLength / Track.spotDifficultyDistance);

			//Debug.Log(forceRespawn + " | " + howManySpots);
			Track.improveAITimer += Time.deltaTime;

			if (progressDistance > endDistance && howManySpots-1 != endDistance
				||
				forceRespawn
				)
            {
				float startDistance = Track.improveAIPathStartID * Track.spotDifficultyDistance;
				progressDistance = startDistance;

				GetComponent<CarRespawnV>().StartCoroutine(GetComponent<CarRespawnV>().RespawnRoutine());


				string newLap = FormatTimer((int)MathF.Round(Track.improveAITimer*1000)).ToString();
				
				string lastLap = FormatTimer((int)MathF.Round(Track.lastLap*1000)).ToString();
				Track.lastLap = Track.improveAITimer;

				Track.improveLastTwoLaps = "Last lap: " + newLap + "\n" + "Penultimate lap: " + lastLap;

				Track.improveAITimer = 0;


				/*			improveAITimer = 0;

						public float lastLap = 0;
						public string improveLastTwoLaps = "Last Lap: "\n" +  Penultimate Lap:";
				*/



				Debug.Log(forceRespawn + " | " + howManySpots);

			}
			#endregion
		}

		public IEnumerator WaitBeforeTheEndOfThePath()
        {
            #region
            float t = 0;
            float duration = 1;
            while (t < duration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                {
                    t += Time.deltaTime;
                }
                yield return null;
            }
            Track.isUsingAltPath = true;
            lastTriggerTriggerPath = null;
            yield return null; 
            #endregion
        }

		string FormatTimer(int newTime)
		{
			#region 
			int FormatedTimer = newTime;
			int minutes = FormatedTimer / (60000);
			int seconds = (FormatedTimer % 60000) / 1000;
			int milliseconds = FormatedTimer % 1000;
			return String.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
			#endregion
		}
	}
}
