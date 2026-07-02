using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class CamLookAtConstantDirection : MonoBehaviour, IMultiCam
	{
		[HideInInspector]
		public bool seeInspector;
		[HideInInspector]
		public bool moreOptions;
		[HideInInspector]
		public bool helpBox = true;

		public bool				IsInitDone = false;

		Rigidbody				rb;
		CarSide					carSide;

		[Space]
		public float			distance = 4;
		public float			height = 2;

		[Space]
		public float			objLookAtConstantDirectionSpeed = 8;

		[Space]
		[Space]
		[Space]
		public float			heightDamping;
		public float			rotSpeed = .1f;
		public float			rotationDamping = .1f;

		[Space]
		[HideInInspector]
		public bool				b_Pause = false;

		private float			wantedRotationAngle;
		[HideInInspector]
		public float			currentRotationAngle;

		private float			rotSpeedCoef = 0;

		[HideInInspector]
		public bool				IsProcessDone = true;

		public float			camSpeed = 0;

		//float					fieldOfViewRef = 0;
		[HideInInspector]
		public Camera cam;
		/*	[Space]
			public float			changeFOVSpeed = 5;
			public float			increaseFOVWhenVehicleAccelerate = 5;

			public float			offset2P_FOV = -15;
		*/
		[Space]
		public Rect				camRectTransform1PSolo_ = new Rect(0, 0, 1, 1);
		public Rect				camRectTransform1PSplit_ = new Rect(0, 0, .5f, 1);
		public Rect				camRectTransform2PSplit_ = new Rect(.5f, 0, .5f, 1);


		public GameObject		objLookAtConstantDirection;
		public bool				b_Find_Target_Automatically = false;

		public Transform		target;
		//public Transform		lookAtTarget;
		public Vector3			lookAtTargetlocalPosition = new Vector3(0f, 0f, 15.2f);
		public int				PlayerID = 0;
		bool isOffsetNeededProcessDone = false;

		public List<VehicleOverrideValues> overrideList = new List<VehicleOverrideValues>();
		[System.Serializable]
		public class VehicleOverrideValues
		{
			public List<int> vehicleIdList = new List<int>();
			public float distance = 0;
			public float height = 0;
			public float heightDamping = 0;

			public float rotSpeed = 0;
			public float rotationDamping = 0;

			public float camSpeed = 0;

			public Vector3 lookAtTargetlocalPosition = Vector3.zero;
		}

		public float editFOV = 0;
		public float editYConstantDirection = 212;

		public bool isPlayer2ValuesInit = false;

		public float sensorSizeXFor2P = .5f; 

		public void InitCam(GameObject vehicle, int index, CamPreset camPreset)
		{
            #region 
            rb = vehicle.GetComponent<Rigidbody>();
            carSide = vehicle.GetComponent<CarSide>();

            cam = transform.GetChild(0).GetChild(0).GetComponent<Camera>();

            cam.gameObject.SetActive(true);

			if(PlayerID == 1)
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

			Vector3 eulerAngle = objLookAtConstantDirection.transform.localRotation.eulerAngles;
			eulerAngle = new Vector3(eulerAngle.x, editYConstantDirection, eulerAngle.z);
			objLookAtConstantDirection.transform.localRotation = Quaternion.Euler(eulerAngle);


			cam.focalLength = editFOV;

			NewCameraView(camPreset);

            while (!cam) { };


            IsInitDone = true; 
            #endregion
        }

		public void UpdateCam(GameObject obj)
		{
            #region 
            if (IsInitDone  && IsProcessDone)
            {
                UpdatePositionOfObjLookAtConstantDirection();
                UpdateCameraPosition();
                //UpdateFOVDependingVehicleSpeed();
            } 
            #endregion
        }

		void UpdatePositionOfObjLookAtConstantDirection()
        {
			#region 
			// Look at the target
			Vector3 offsetLocalPos = target.right * lookAtTargetlocalPosition.x + target.up * lookAtTargetlocalPosition.y + target.forward * lookAtTargetlocalPosition.z;
			Vector3 currentLookAtTargetPos = target.position + offsetLocalPos;


			objLookAtConstantDirection.transform.position =
                    Vector3.Lerp(objLookAtConstantDirection.transform.position, currentLookAtTargetPos, Time.deltaTime * objLookAtConstantDirectionSpeed); 
            #endregion
        }

		void UpdateCameraPosition()
		{
			#region
			if (!b_Pause)
			{
				if (IsProcessDone)
				{
					transform.rotation = ReturnRotation();
					transform.position = Vector3.Lerp(transform.position, ReturnPosition(), Time.deltaTime * camSpeed);
				}
			}
			#endregion
		}

		Vector3 ReturnPosition(bool dampEnable = true)
		{
			#region
			wantedRotationAngle = objLookAtConstantDirection.transform.eulerAngles.y;

			float wantedHeight = objLookAtConstantDirection.transform.position.y + height;

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
			Vector3 newPos = objLookAtConstantDirection.transform.position;

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
			Quaternion lookRotation = Quaternion.LookRotation(objLookAtConstantDirection.transform.position - transform.position);

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
			IsProcessDone = false;
			target = preset.Target;

			ForceUpdateCameraPosition();
			IsProcessDone = true;
			#endregion
		}

		public void ForceUpdateCameraPosition()
		{
			#region
			IsProcessDone = false;

			Vector3 offsetLocalPos = target.right * lookAtTargetlocalPosition.x + target.up * lookAtTargetlocalPosition.y + target.forward * lookAtTargetlocalPosition.z;
			Vector3 currentLookAtTargetPos = target.position + offsetLocalPos;

			objLookAtConstantDirection.transform.position = currentLookAtTargetPos;

			transform.position = objLookAtConstantDirection.transform.position
				- objLookAtConstantDirection.transform.forward * distance
				+ objLookAtConstantDirection.transform.up * height;

			Quaternion lookRotation = Quaternion.LookRotation(objLookAtConstantDirection.transform.position - transform.position);
			transform.rotation = lookRotation;

			currentRotationAngle = objLookAtConstantDirection.transform.eulerAngles.y;

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
				if (overrideList[selectedList].distance != 0) distance = overrideList[selectedList].distance;
				if (overrideList[selectedList].height != 0) height = overrideList[selectedList].height;
				if (overrideList[selectedList].heightDamping != 0) heightDamping = overrideList[selectedList].heightDamping;
				if (overrideList[selectedList].rotSpeed != 0) rotSpeed = overrideList[selectedList].rotSpeed;
				if (overrideList[selectedList].rotationDamping != 0) rotationDamping = overrideList[selectedList].rotationDamping;
				if (overrideList[selectedList].camSpeed != 0) camSpeed = overrideList[selectedList].camSpeed;
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
				int currentCam = 1;
				CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

				for (var i = 0; i < grpCams.Length; i++)
				{
					if (grpCams[i].PlayerID == 0)
					{
						if (grpCams[i].interfaceObjList.Count > currentCam)
                        {
							CamLookAtConstantDirection camP1 = grpCams[i].interfaceObjList[currentCam].GetComponent<CamLookAtConstantDirection>();

							distance = camP1.distance;
							height = camP1.height;
							objLookAtConstantDirectionSpeed = camP1.objLookAtConstantDirectionSpeed;
							heightDamping = camP1.heightDamping;
							rotSpeed = camP1.rotSpeed;
							rotationDamping = camP1.rotationDamping;
							camSpeed = camP1.camSpeed;
							lookAtTargetlocalPosition = camP1.lookAtTargetlocalPosition;
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
