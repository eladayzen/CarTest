using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class CamFollowBehind : MonoBehaviour, IMultiCam
	{
		[HideInInspector]
		public bool seeInspector;

		public bool						IsInitDone = false;
	
		Rigidbody						rb;
		CarSide							carSide;
		CarState						carState;

		public float					distance = 4;
		public float					height = 2;
		public float					heightDamping;

		public float					rotSpeed = 15f;
		public float					rotationDamping;

		public float					carSpeed = 30;

		[HideInInspector]
		public bool						b_Pause = false;

		private float					wantedRotationAngle;
		private float					currentRotationAngle;

		private float					rotSpeedCoef = 0;

		[Space]
		[Space]
		public List<Vector3>			offsetCamPosition = new List<Vector3>();

		[HideInInspector]
		public bool						IsProcessDone = true;

		public AnimationCurve			CamDistanceDependingSpeedAxisZ;
		public AnimationCurve			CamDistanceDependingSpeedAxisY;

		float							fieldOfViewRef = 0;
		Camera							cam;
		[Space]

		public float					changeFOVSpeed = 5;
		public float					increaseFOVWhenVehicleAccelerate = 5;

		public float					offset2P_FOV = -15;

		[Space]
		public Rect						camRectTransform1PSolo_ = new Rect(0, 0, 1, 1);
		public Rect						camRectTransform1PSplit_ = new Rect(0, 0, .5f, 1);
		public Rect						camRectTransform2PSplit_ = new Rect(.5f, 0, .5f, 1);

		[Space]
		[Space]
		public Transform target;
		//public Transform lookAtTarget;
		public Vector3 lookAtTargetlocalPosition = new Vector3(0f,0f,15.2f);
		public bool b_Find_Target_Automatically = false;
		public int PlayerID = 0;
		bool isOffsetNeededProcessDone = false;

		public List<VehicleOverrideValues> overrideList = new List<VehicleOverrideValues>();
		[System.Serializable]
		public class VehicleOverrideValues
		{
			public List<int> vehicleIdList = new List<int>();
			public float distance =0;
			public float height = 0;
			public float heightDamping = 0;

			public float rotSpeed = 0;
			public float rotationDamping = 0;

			public float carSpeed = 0;

			public Vector3 lookAtTargetlocalPosition = Vector3.zero;
		}

		public float editFOV = 0;
		public bool isPlayer2ValuesInit = false;
		public float sensorSizeXFor2P = .5f;

		public void InitCam(GameObject vehicle, int index, CamPreset camPreset)
		{
			rb = vehicle.GetComponent<Rigidbody>();
			carSide = vehicle.GetComponent<CarSide>();
			carState = vehicle.GetComponent<CarState>();

			cam = transform.GetChild(0).GetChild(0).GetComponent<Camera>();

			cam.gameObject.SetActive(true);

			if (PlayerID == 1)
			{
				InitPlayerValues();
				while (!isPlayer2ValuesInit) ;
			}

			ApplyOffsetIfNeeded();
			while (!isOffsetNeededProcessDone) ;

			if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2 && 
				PlayerID == 1)
            {
				cam.rect = camRectTransform2PSplit_;
				cam.sensorSize = new Vector2(cam.sensorSize.x * sensorSizeXFor2P, cam.sensorSize.y);
			}
				
			else if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2 &&
				PlayerID == 0)
            {
				cam.rect = camRectTransform1PSplit_;
				cam.sensorSize = new Vector2(cam.sensorSize.x * sensorSizeXFor2P, cam.sensorSize.y);
			}
			else
				cam.rect = camRectTransform1PSolo_;

			cam.focalLength = editFOV;

			NewCameraView(camPreset);

			while (!cam) { };

			IsInitDone = true;
		}

		public void UpdateCam(GameObject obj)
		{
			if (IsInitDone && IsProcessDone)
			{
				UpdateCameraPosition();
				//UpdateFOVDependingVehicleSpeed();
				//Debug.Log("Cam_01: Follow Behind");
			}
		}

		void UpdateCameraPosition()
		{
			#region
			if (!b_Pause && target)
			{
				target.localPosition = DefineTargetOffsetPositionWhenCarMoveBackward();

				if (IsProcessDone)
				{
					transform.rotation = ReturnRotation();

					transform.position = Vector3.Lerp(transform.position, ReturnPosition(), Time.deltaTime * carSpeed);
				}
			}
			#endregion
		}

		Vector3 DefineTargetOffsetPositionWhenCarMoveBackward()
		{
			#region
			if (carState)
			{
				if (carState.carDirection == CarState.CarDirection.Backward)
				{
					if (carState.steeringDir == TS.Generics.CarSteeringDirection.Left)
						return Vector3.MoveTowards(target.localPosition, offsetCamPosition[1], Time.deltaTime * .2f);
					else if (carState.steeringDir == TS.Generics.CarSteeringDirection.Right)
						return Vector3.MoveTowards(target.localPosition, offsetCamPosition[2], Time.deltaTime * .2f);
					else
						return Vector3.MoveTowards(target.localPosition, offsetCamPosition[0], Time.deltaTime * .2f);
				}
				else
					return Vector3.MoveTowards(target.localPosition, offsetCamPosition[0], Time.deltaTime);
			}

			return Vector3.MoveTowards(target.localPosition, offsetCamPosition[0], Time.deltaTime * .2f);
			#endregion
		}

		Vector3 ReturnPosition(bool dampEnable = true)
		{
			#region
			wantedRotationAngle = target.eulerAngles.y;


			float wantedHeight = target.position.y + height;

			currentRotationAngle = transform.eulerAngles.y;
			float currentHeight = transform.position.y;

			// Y damping rotation
			currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Height damping
			if (dampEnable)
				currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
			else
				currentHeight = wantedHeight;

			// EulerAngle to quaternion
			Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

			// New Cam position
			Vector3 newPos = target.position;

			newPos -= currentRotation * Vector3.forward * distance;

			// New Cam Height
			newPos = new Vector3(newPos.x, currentHeight, newPos.z);

			return newPos;
			#endregion
		}

		Quaternion ReturnRotation(bool dampEnable = true)
		{
			#region
			float impactCoef = 1;
			for (var i = 0; i < carSide.wheelsList.Count; i++)
			{
				if (carSide.wheelsList[i].isObstacle)
				{
					impactCoef = .5f;
					break;
				}
			}

			// Look at the target
			Vector3 offsetLocalPos = target.right * lookAtTargetlocalPosition.x + target.up * lookAtTargetlocalPosition.y + target.forward * lookAtTargetlocalPosition.z;
			Vector3 currentLookAtTargetPos = target.position + offsetLocalPos;
			Quaternion lookRotation = Quaternion.LookRotation(currentLookAtTargetPos - transform.position);

			rotSpeedCoef = Mathf.MoveTowards(rotSpeedCoef, 1, Time.deltaTime);

			Quaternion newRotation;

			if (dampEnable)
				newRotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed * impactCoef);
			else
				newRotation = lookRotation;

			return newRotation;
			#endregion
		}

		public void NewCameraView(CamPreset preset)
		{
			#region
			//Debug.Log(preset.Name);
			IsProcessDone = false;
			// Change parameters
			target = preset.Target;
			//lookAtTarget = preset.LookAtTarget;

			// Init the new Camera View
			//transform.position = ReturnPosition(false);
			//transform.rotation = ReturnRotation(false);
			//transform.position = ReturnPosition(false);

			
			
			transform.position = target.transform.position
				- target.transform.forward * distance
				+ target.transform.up * height;

			Quaternion lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);
			transform.rotation = lookRotation;

			currentRotationAngle = target.transform.eulerAngles.y;



			IsProcessDone = true;
			#endregion
		}

		public void UpdateFOVDependingVehicleSpeed()
		{
			#region 
			if (rb)
			{
				float vehicleMag = rb.linearVelocity.magnitude;

				float currentFieldOfViewRef = fieldOfViewRef;

				if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer == 2)
				{
					currentFieldOfViewRef = fieldOfViewRef + offset2P_FOV;
				}

				//if (vehicleMag > 5)
				//	cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, currentFieldOfViewRef + increaseFOVWhenVehicleAccelerate, Time.deltaTime * changeFOVSpeed);
				//else
					cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, currentFieldOfViewRef, Time.deltaTime * changeFOVSpeed);

			}
			#endregion
		}

		public void ApplyOffsetIfNeeded()
        {
            #region 
            int vehicleID = InfoVehicle.instance.listSelectedVehicles[PlayerID];
			//Debug.Log("PlayerID: " + PlayerID);
            int selectedList = -1;
            for (var i = 0; i < overrideList.Count; i++)
            {
                for (var j = 0; j < overrideList[i].vehicleIdList.Count; j++)
                {
                    if (overrideList[i].vehicleIdList[j] == vehicleID)
                    {
                        selectedList = i;
                        break;
                    }
                }
            }

            if (selectedList != -1)
            {
				if(overrideList[selectedList].distance!=0) distance = overrideList[selectedList].distance;
				if (overrideList[selectedList].height !=0) height = overrideList[selectedList].height;
				if (overrideList[selectedList].heightDamping != 0) heightDamping = overrideList[selectedList].heightDamping;
				if (overrideList[selectedList].rotSpeed != 0) rotSpeed = overrideList[selectedList].rotSpeed;
				if (overrideList[selectedList].rotationDamping != 0) rotationDamping = overrideList[selectedList].rotationDamping;
				if (overrideList[selectedList].carSpeed != 0) carSpeed = overrideList[selectedList].carSpeed;
				if (overrideList[selectedList].lookAtTargetlocalPosition != Vector3.zero) lookAtTargetlocalPosition = overrideList[selectedList].lookAtTargetlocalPosition;
            }

			isOffsetNeededProcessDone = true;
			#endregion
		}

		void InitPlayerValues()
		{
			InfoRememberMainMenuSelection info = FindFirstObjectByType<InfoRememberMainMenuSelection>(FindObjectsInactive.Include);
			if (info)
			{
				int currentCam = 0;
				CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

				for (var i = 0; i < grpCams.Length; i++)
				{
					if (grpCams[i].PlayerID == 0)
					{
						if (grpCams[i].interfaceObjList.Count > currentCam)
						{
							CamFollowBehind camP1 = grpCams[i].interfaceObjList[currentCam].GetComponent<CamFollowBehind>();
							//Debug.Log(grpCams[i].interfaceObjList[currentCam].name);
							distance = camP1.distance;
							height = camP1.height;
							heightDamping = camP1.heightDamping;
							rotSpeed = camP1.rotSpeed;
							rotationDamping = camP1.rotationDamping;
							carSpeed = camP1.carSpeed;
							lookAtTargetlocalPosition = camP1.lookAtTargetlocalPosition;
							editFOV = camP1.editFOV;

							overrideList = new List<VehicleOverrideValues>(camP1.overrideList);
						}
					}
				}
			}

			isPlayer2ValuesInit = true;
		}
	}

}
