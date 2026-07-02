using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class Cam2DBehind : MonoBehaviour, IMultiCam
	{
		public bool IsInitDone = false;

		[HideInInspector]
		public bool seeInspector;

		Rigidbody rb;
		CarSide carSide;

		[HideInInspector]
		public bool b_Pause = false;
		[Space]
		public float camSpeed = 8;
		public float camRotSpeed = 1f;


		[Space]
		[Space]
		public float objFollowingTargetSpeed = 50;
		public float objFollowingTargetRotSpeed = .25f;


		private float rotSpeedCoef = 0;

		[HideInInspector]
		public bool IsProcessDone = true;

		Camera cam;

		[Space]
		[Space]
		public Rect camRectTransform1PSolo_ = new Rect(0, 0, 1, 1);
		public Rect camRectTransform1PSplit_ = new Rect(0, 0, .5f, 1);
		public Rect camRectTransform2PSplit_ = new Rect(.5f, 0, .5f, 1);
		[Space]


		public Transform target;
		public Vector3 targetlocalPosition = new Vector3(0f, 0f, 27f);
		public Transform lookAtTarget;
		public Transform camThatFollowTarget;
		public float camHeight = 100;
		public int PlayerID = 0;

		public float editFOV = 0;
		public float editYConstantDirection = 212;

		bool isOffsetNeededProcessDone = false;

		public List<VehicleOverrideValues> overrideList = new List<VehicleOverrideValues>();
		[System.Serializable]
		public class VehicleOverrideValues
		{
			public List<int> vehicleIdList = new List<int>();
			public float camSpeed = 0;
			public float camRotSpeed = 0;
			public float camHeight = 0;
			public float objFollowingTargetSpeed = 0;
			public float objFollowingTargetRotSpeed = 0;
			public Vector3 targetlocalPosition = Vector3.zero;
		}

		public bool isPlayer2ValuesInit = false;
		public Camera camRef;
		public float sensorSizeXFor2P = .5f;
		public void InitCam(GameObject vehicle, int index, CamPreset camPreset)
		{
            #region 
            rb = vehicle.GetComponent<Rigidbody>();
            carSide = vehicle.GetComponent<CarSide>();

            cam = transform.GetChild(0).GetChild(0).GetComponent<Camera>();
            cam.gameObject.SetActive(true);

            cam.transform.parent.SetParent(camThatFollowTarget);
            cam.transform.parent.localPosition = Vector3.zero;
            cam.transform.parent.localRotation = Quaternion.identity;

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

			cam.orthographicSize = editFOV;


			NewCameraView(camPreset);

            while (!cam) { };

            IsInitDone = true; 
            #endregion
        }

		public void UpdateCam(GameObject obj)
		{
            #region 
            if (IsInitDone && IsProcessDone)
            {
                UpdatePositionOfObjLookAtConstantDirection();
                UpdateCameraPosition();
                //UpdateFOVDependingVehicleSpeed();
                //Debug.Log("Cam_01: Follow Behind");
            }
            #endregion
        }

		void UpdatePositionOfObjLookAtConstantDirection()
		{
			#region 
			if (target)
			{
				//objLookAtConstantDirection.transform.position = lookAtTarget.position;
				camThatFollowTarget.transform.position =
				Vector3.Lerp(camThatFollowTarget.transform.position, transform.position, Time.deltaTime * camSpeed);
				camThatFollowTarget.transform.rotation = ReturnRotation02();
			}
            #endregion
        }

		Quaternion ReturnRotation02(bool dampEnable = true)
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
			Vector3 offsetLocalPos = target.right * targetlocalPosition.x + target.up * targetlocalPosition.y + target.forward * targetlocalPosition.z;
			Quaternion lookRotation = Quaternion.LookRotation((lookAtTarget.transform.position + offsetLocalPos) - camThatFollowTarget.transform.position);

			rotSpeedCoef = Mathf.MoveTowards(rotSpeedCoef, 1, Time.deltaTime);

			Quaternion newRotation;

			newRotation = Quaternion.Slerp(camThatFollowTarget.transform.rotation, lookRotation, Time.deltaTime * camRotSpeed * impactCoef);

			return newRotation;
			#endregion
		}

		void UpdateCameraPosition()
		{
			#region
			if (!b_Pause && target)
			{
				if (IsProcessDone)
				{
					transform.rotation = ReturnRotation();
					transform.position = Vector3.Lerp(transform.position, ReturnPosition(), Time.deltaTime * objFollowingTargetSpeed);
				}
			}
			#endregion
		}

		Vector3 ReturnPosition(bool dampEnable = true)
		{
			#region
			// New Cam position
			Vector3 offsetLocalPos = target.right * targetlocalPosition.x + target.up * targetlocalPosition.y + target.forward * targetlocalPosition.z;

			Vector3 newPos = target.position + offsetLocalPos;

			// New Cam Height
			newPos = new Vector3(newPos.x, camHeight, newPos.z);

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
			Quaternion lookRotation = Quaternion.LookRotation(lookAtTarget.position - transform.position);

			rotSpeedCoef = Mathf.MoveTowards(rotSpeedCoef, 1, Time.deltaTime);

			Quaternion newRotation;

			if (dampEnable)
				newRotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * objFollowingTargetRotSpeed * impactCoef);
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
			lookAtTarget = preset.Target.GetChild(0).transform;

			ForceUpdateCameraPosition();

			IsProcessDone = true;
			#endregion
		}

		public void ForceUpdateCameraPosition()
		{
			#region
			IsProcessDone = false;
			camThatFollowTarget.transform.position = ReturnPosition(false);
			Quaternion lookRotation = Quaternion.LookRotation(lookAtTarget.transform.position - camThatFollowTarget.transform.position);
			camThatFollowTarget.transform.rotation = lookRotation;
			camThatFollowTarget.transform.position = ReturnPosition(false);

			transform.position = camThatFollowTarget.transform.position;
			transform.rotation = lookRotation;
			IsProcessDone = true;
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
				if (overrideList[selectedList].camSpeed != 0) camSpeed = overrideList[selectedList].camSpeed;
				if (overrideList[selectedList].camRotSpeed != 0) camRotSpeed = overrideList[selectedList].camRotSpeed;
				if (overrideList[selectedList].camHeight != 0) camHeight = overrideList[selectedList].camHeight;
				if (overrideList[selectedList].objFollowingTargetSpeed != 0) camHeight = overrideList[selectedList].objFollowingTargetSpeed;
				if (overrideList[selectedList].objFollowingTargetRotSpeed != 0) camHeight = overrideList[selectedList].objFollowingTargetRotSpeed;
				if (overrideList[selectedList].targetlocalPosition != Vector3.zero) targetlocalPosition = overrideList[selectedList].targetlocalPosition;
			}

			isOffsetNeededProcessDone = true;
			#endregion
		}

		void InitPlayerValues()
		{
			InfoRememberMainMenuSelection info = FindFirstObjectByType<InfoRememberMainMenuSelection>(FindObjectsInactive.Include);
			if (info)
			{
				int currentCam = 4;
				CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

				for (var i = 0; i < grpCams.Length; i++)
				{
					if (grpCams[i].PlayerID == 0)
					{
						if (grpCams[i].interfaceObjList.Count > currentCam)
						{
							Cam2DBehind camP1 = grpCams[i].interfaceObjList[currentCam].GetComponent<Cam2DBehind>();
							camSpeed = camP1.camSpeed;
							camRotSpeed = camP1.camRotSpeed;
							camHeight = camP1.camHeight;
							objFollowingTargetSpeed = camP1.objFollowingTargetSpeed;
							objFollowingTargetRotSpeed = camP1.objFollowingTargetRotSpeed;
							targetlocalPosition = camP1.targetlocalPosition;
							editFOV = camP1.editFOV;
							editYConstantDirection = camP1.editYConstantDirection;
							overrideList = new List<VehicleOverrideValues>(camP1.overrideList);
						}
					
					}
				}

			}

			isPlayer2ValuesInit = true;
		}
	}

}
