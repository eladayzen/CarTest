// Description: CarF: attavhed to Grp_Cam_P1 and Grp_Cam_P2. Follow the vehicle.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
	public class CarF : MonoBehaviour
	{
		public bool				IsInitDone = false;
		public int				PlayerID = 0;

		public List<GameObject> interfaceObjList = new List<GameObject>();
		// TODO: Create class for the next 2 variables
		public List<IMultiCam> interfaceList = new List<IMultiCam>();
		public int				currentCam = 0;


		public Rigidbody		rb;
		public CarSide			carSide;
		public CarState			carState;
		VehicleInfo				vehicleInfo;

		public bool forceIsItAPlayer = false;

		public List<CamPreset> presetList = new List<CamPreset>();

		public GameObject enemyDetector;


		public bool InitCamera(GameObject vehicle)
        {
            #region
            rb = vehicle.GetComponent<Rigidbody>();
            carSide = vehicle.GetComponent<CarSide>();
            carState = vehicle.GetComponent<CarState>();
			vehicleInfo = vehicle.GetComponent<VehicleInfo>();


			int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

			currentCam = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentCamStyle;

			for (var i = 0; i < interfaceObjList.Count; i++)
			{
				interfaceList.Add(interfaceObjList[i].GetComponent<IMultiCam>());
				

				if((PlayerID == 1 && howManyPlayer == 1) || currentCam == -1)
					interfaceObjList[i].transform.gameObject.SetActive(false);
				else if (currentCam == i)
                {
					interfaceObjList[i].GetComponent<IMultiCam>().InitCam(rb.gameObject, i, presetList[i]);
				}
				else
					interfaceObjList[i].transform.gameObject.SetActive(false);
			}

			IsInitDone = true;
            return true; 
            #endregion
        }

		public float currentRotationDamping = 10;
	

		void FixedUpdate()
		{
			#region
			if (IsInitDone && IsItAPlayer() && currentCam != -1)
			{
				interfaceList[currentCam].UpdateCam(null);
			}
			#endregion
		}

	
		bool IsItAPlayer()
		{
			#region
			if (!IsInitDone)
				return false;


			if (forceIsItAPlayer)
				return true;

			int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

			if (carState.carPlayerType == CarState.CarPlayerType.Human ||
			   (carState.carPlayerType == CarState.CarPlayerType.AI && vehicleInfo.playerNumber == 0) ||
			  ( carState.carPlayerType == CarState.CarPlayerType.AI && vehicleInfo.playerNumber == 1 && howManyPlayer ==2))
			{
				return true;
			}


			return false;
			#endregion
		}

	}

}
