using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class Cam2DConstantDirection : MonoBehaviour, IMultiCam
	{
		public bool IsInitDone = false;

		[HideInInspector]
		public bool seeInspector;

		[Space]
		[Tooltip("The speed at which objLookAtConstantDirection moves to reach the vehicle target position. Default value: 10.")]
		public float camSpeed = 10;

		//public float distance = 4;
		//[Tooltip("Increase or reduce the rotation damping. Default value: 0.1")]
		//public float rotationDamping;

		[HideInInspector]
		public bool b_Pause = false;

		private float wantedRotationAngle;
		private float currentRotationAngle;

		[HideInInspector]
		public bool IsProcessDone = true;

		Camera cam;

		[Space]
		[Space]
		public Rect camRectTransform1PSolo_ = new Rect(0, 0, 1, 1);
		public Rect camRectTransform1PSplit_ = new Rect(0, 0, .5f, 1);
		public Rect camRectTransform2PSplit_ = new Rect(.5f, 0, .5f, 1);
		
		[Space]
		[Space]
		public bool b_Find_Target_Automatically = false;
		[Tooltip("The speed at which the camera moves to reach objLookAtConstantDirection position. Default value: 20.")]
		public float objLookAtConstantDirectionSpeed = 20;
		[Tooltip("The height of the camera relative to the target. Default value: 100.")]
		public float camHeight = 100;
		public Transform target;
		//public Transform lookAtTarget;
		public Vector3 lookAtTargetlocalPosition = new Vector3(0f, 0f, 15.2f);
		public GameObject objLookAtConstantDirection;

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
			public float objLookAtConstantDirectionSpeed = 0;
			public float camHeight = 0;
			public Vector3 lookAtTargetlocalPosition = Vector3.zero;
		}

		public bool isPlayer2ValuesInit = false;
		public float sensorSizeXFor2P = .5f;

		public void InitCam(GameObject vehicle, int index, CamPreset camPreset)
		{
            #region 
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

			Vector3 eulerAngle = objLookAtConstantDirection.transform.localRotation.eulerAngles;
			eulerAngle = new Vector3(eulerAngle.x, editYConstantDirection, eulerAngle.z);
			objLookAtConstantDirection.transform.localRotation = Quaternion.Euler(eulerAngle);


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
				UpdateCameraRotation();
                //UpdateFOVDependingVehicleSpeed();
                //Debug.Log("Cam_01: Follow Behind");
            } 
            #endregion
        }

		void UpdateCameraRotation()
        {
            #region 
            transform.rotation = objLookAtConstantDirection.transform.rotation; 
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
			if (!b_Pause && target)
			{
				if (IsProcessDone)
				{
					transform.position = Vector3.Lerp(transform.position, ReturnPosition(), Time.deltaTime * camSpeed);
				}
			}
			#endregion
		}

	
		Vector3 ReturnPosition(bool dampEnable = true)
		{
			#region
			// New Cam position
			Vector3 newPos = objLookAtConstantDirection.transform.position;

			// New Cam Height
			newPos = new Vector3(newPos.x, camHeight, newPos.z);

			return newPos;
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

			transform.position = objLookAtConstantDirection.transform.position;

			//Quaternion lookRotation = Quaternion.LookRotation(objLookAtConstantDirection.transform.position - transform.position);
			transform.rotation = objLookAtConstantDirection.transform.rotation;

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
				if (overrideList[selectedList].camSpeed != 0) camSpeed = overrideList[selectedList].camSpeed;
				if (overrideList[selectedList].objLookAtConstantDirectionSpeed != 0) objLookAtConstantDirectionSpeed = overrideList[selectedList].objLookAtConstantDirectionSpeed;
				if (overrideList[selectedList].camHeight != 0) camHeight = overrideList[selectedList].camHeight;
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
				int currentCam = 3;
				CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

				for (var i = 0; i < grpCams.Length; i++)
				{
					if (grpCams[i].PlayerID == 0)
					{
						if (grpCams[i].interfaceObjList.Count > currentCam)
						{
							Cam2DConstantDirection camP1 = grpCams[i].interfaceObjList[currentCam].GetComponent<Cam2DConstantDirection>();
							camSpeed = camP1.camSpeed;
							objLookAtConstantDirectionSpeed = camP1.objLookAtConstantDirectionSpeed;
							camHeight = camP1.camHeight;
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
