// Description: VehicleCamPreset. Manage the camera presets.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class VehicleCamPreset : MonoBehaviour
	{
		[HideInInspector]
		public int				CurrentSelectedPreset = 0;
		[HideInInspector]
		public bool				IsCameraEditingEnable = false;
		public GameObject		CamToVisualizePreset;

		public CarF				PlayerCamera;
		public List<CamPreset>	PresetList = new List<CamPreset>();


	/*	public void SetCameraViewZero(CameraSelector cameraSelector)
		{
			#region
			cameraSelector.IsSubProcessDone = false;
			//if (PlayerCamera)
            //{
			//	CamPreset preset = PresetList[0];
			//	PlayerCamera.NewCameraView(preset);
			//}
			cameraSelector.IsSubProcessDone = true;
			#endregion
		}

		public void SetCameraViewOne(CameraSelector cameraSelector)
		{
			#region
			cameraSelector.IsSubProcessDone = false;
			//if (PlayerCamera)
			//{
			//	CamPreset preset = PresetList[1];
			//	PlayerCamera.NewCameraView(preset);
			//}
			cameraSelector.IsSubProcessDone = true;
			#endregion
		}

		public void SetCameraViewTwo(CameraSelector cameraSelector)
		{
			#region
			cameraSelector.IsSubProcessDone = false;
			//if (PlayerCamera)
			//{
			//	CamPreset preset = PresetList[2];
			//	PlayerCamera.NewCameraView(preset);
			//}
			cameraSelector.IsSubProcessDone = true;
			#endregion
		}

		public void SetCameraViewThree(CameraSelector cameraSelector)
		{
			#region
			cameraSelector.IsSubProcessDone = false;
			
			//if (PlayerCamera)
			//{
			//	CamPreset preset = PresetList[3];
			//	PlayerCamera.NewCameraView(preset);
			//}
			cameraSelector.IsSubProcessDone = true;
			#endregion
		}

		public void SetCameraViewFour(CameraSelector cameraSelector)
		{
			#region
			cameraSelector.IsSubProcessDone = false;
			
			//if (PlayerCamera)
			//{
			//	CamPreset preset = PresetList[4];
			//	PlayerCamera.NewCameraView(preset);
			//}
			cameraSelector.IsSubProcessDone = true;
			#endregion
		}
	*/
	}

	[System.Serializable]
	public class CamPreset
	{
		public string			Name;

		//public bool				IsCamFixed = false;
		public Transform		Target;
		//public Transform		LookAtTarget;
		/*public float			DistanceToTarget;
		public float			HeightToTarget;
		public float			RotationDamp;
		public float			HeightDamp;
		public float			SpeedRot;
		public bool				SmoothTransition;
		public float			SmoothSpeedView;*/
	}
}

