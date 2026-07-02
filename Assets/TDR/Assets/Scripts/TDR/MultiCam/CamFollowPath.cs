using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CamFollowPath : MonoBehaviour, IMultiCam
    {
		[HideInInspector]
		public bool seeInspector;

		public bool             isInitDone = false;

		[Space]
		[Tooltip("Increase or reduce camera height. Default value: 0.")]
		public float			height = 2;
		[Space]
		[Space]

		[Tooltip("The speed at which the camera rotates to look at the vehicle. Default value: 30.")]
		public float			rotSpeed = 15f;
		[Tooltip("Increase or reduce the rotation damping. Default value: 1.")]
		public float			rotationDamping;
		[Tooltip("Increase or reduce the camera height damping. Default value: 2.")]
		public float			heightDamping;

		[HideInInspector]
		public bool				b_Pause = false;

		private float			wantedRotationAngle;
		private float			currentRotationAngle;

		[Space]
		[Space]
		[Tooltip("The speed at which the camera moves to reach the target on the path. Default value: 5.")]
		public float			camSpeed = 5;
		[Tooltip("Increase or reduce the distance between the camera and the vehicle. Default value: 0.")]
		public float			distance = 4;

		[HideInInspector]
		public bool				IsProcessDone = true;

		[Space]
		[Space]
		Camera					cam;

		[Space]
		[Space]
		public Rect				camRectTransform1PSolo_ = new Rect(0, 0, 1, 1);
		public Rect				camRectTransform1PSplit_ = new Rect(0, 0, .5f, 1);
		public Rect				camRectTransform2PSplit_ = new Rect(.5f, 0, .5f, 1);

		public bool				isEnabled = true;

		public bool				b_Find_Target_Automatically = false;
		public Transform		target;
		public Transform		lookAtTarget;
		public Vector3			lookAtTargetlocalPosition = new Vector3(0f, 0f, 0f);
		public CamPathManager	camPathManager;

		public int				PlayerID = 0;

		bool isOffsetNeededProcessDone = false;

		public List<VehicleOverrideValues> overrideList = new List<VehicleOverrideValues>();
		[System.Serializable]
		public class VehicleOverrideValues
		{
			public List<int> vehicleIdList = new List<int>();
			public float height = 0;
			public float heightDamping = 0;
			public float rotSpeed = 0;
			public float rotationDamping = 0;

			public float camSpeed = 0;

			public float distance = 0;

			public Vector3 lookAtTargetlocalPosition = Vector3.zero;
		}


		public float editFOV = 10;

		public bool isPlayer2ValuesInit = false;
		public float sensorSizeXFor2P = .5f;
		public void InitCam(GameObject vehicle, int index, CamPreset camPreset)
        {
			#region 
			//Debug.Log("01 vehicle:" + vehicle.name);
			camPathManager.StartCoroutine(camPathManager.InitRoutine());

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
            #endregion
        }


        private void OnDisable()
        {
			camPathManager.transform.parent.gameObject.SetActive(false);

		}

        public void UpdateCam(GameObject obj)
        {
            #region 
            if (isInitDone && IsProcessDone)
            {
                transform.rotation = ReturnRotation();
                UpdateCameraPosition();
            } 
            #endregion
        }


        Quaternion ReturnRotation(bool dampEnable = true)
        {
			#region
			// Look at the target
			Vector3 offsetLocalPos = lookAtTarget.right * lookAtTargetlocalPosition.x + lookAtTarget.up * lookAtTargetlocalPosition.y + lookAtTarget.forward * lookAtTargetlocalPosition.z;
			Vector3 currentLookAtTargetPos = lookAtTarget.position + offsetLocalPos;

			Quaternion lookRotation = Quaternion.LookRotation(currentLookAtTargetPos - transform.position);
            Quaternion newRotation;
            newRotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
            return newRotation;
            #endregion
        }


		void UpdateCameraPosition()
		{
			#region
			if (/*!b_Pause && */target)
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

		public void NewCameraView(CamPreset preset)
		{
			#region
			IsProcessDone = false;
			lookAtTarget = preset.Target;

			transform.position = ReturnPosition(false);
			transform.rotation = ReturnRotation(false);
			transform.position = ReturnPosition(false);
			
			IsProcessDone = true;
			#endregion
		}

		public void ForceUpdateCameraPosition()
		{
			#region
			IsProcessDone = false;

			transform.position = ReturnPosition(false);
			Quaternion lookRotation = Quaternion.LookRotation(lookAtTarget.position - transform.position);
			transform.rotation = lookRotation;

			isInitDone = true;
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
				if (overrideList[selectedList].height != 0) height = overrideList[selectedList].height;
				if (overrideList[selectedList].heightDamping != 0) heightDamping = overrideList[selectedList].heightDamping;
				if (overrideList[selectedList].rotSpeed != 0) rotSpeed = overrideList[selectedList].rotSpeed;
				if (overrideList[selectedList].rotationDamping != 0) rotationDamping = overrideList[selectedList].rotationDamping;
				if (overrideList[selectedList].camSpeed != 0) camSpeed = overrideList[selectedList].camSpeed;
				if (overrideList[selectedList].distance != 0) distance = overrideList[selectedList].distance;
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
				int currentCam = 2;
				CarF[] grpCams = FindObjectsByType<CarF>(FindObjectsSortMode.None);

				for (var i = 0; i < grpCams.Length; i++)
				{
					if (grpCams[i].PlayerID == 0)
					{
						if (grpCams[i].interfaceObjList.Count > currentCam)
						{
							CamFollowPath camP1 = grpCams[i].interfaceObjList[currentCam].GetComponent<CamFollowPath>();

							distance = camP1.distance;
							height = camP1.height;
							heightDamping = camP1.heightDamping;
							rotSpeed = camP1.rotSpeed;
							rotationDamping = camP1.rotationDamping;
							camSpeed = camP1.camSpeed;
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
