// Description: ResultVehicleMethods.
// Attached to ArcadeResult | TimeTrialScoreManager | ChampionshipResult objects.
// Useful methods for the race result.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class ResultVehicleMethods : MonoBehaviour
    {
        public void StopMovingAllVehicles()
        {
            #region
            //Debug.Log("stop moving all car");
            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = true;

            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            for (var i = 0; i < howManyPlayer; i++)
                VehiclesRef.instance.listVehicles[i].GetComponent<Rigidbody>().isKinematic = true;

            TagCharacterInsideVehicle[] chara = FindObjectsByType<TagCharacterInsideVehicle>(FindObjectsSortMode.None);
            for (var i = 0; i < chara.Length; i++)
                chara[i].gameObject.SetActive(false) ;

            PodiumPlayerOne(); 
            #endregion
        }

        void PodiumPlayerOne()
        {
            #region 
            StartCoroutine(PodiumPlayerOneRoutine()); 
            #endregion
        }

        IEnumerator PodiumPlayerOneRoutine()
        {
            #region
            GrpPodiumTag[] podiums = FindObjectsByType<GrpPodiumTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            int howManyPlayer = InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            int playerPosInRace = PlayerPositionDependingGameMode(0);
            int bestPlayer = 0;
            if(howManyPlayer == 2)
            {
                int playerTwoPosInRace = PlayerPositionDependingGameMode(1);

                if (playerPosInRace > playerTwoPosInRace)
                {
                    playerPosInRace = playerTwoPosInRace;
                    bestPlayer = 1;
                }    
            }

            GrpPodiumTag podium = podiums[0];
            for (var i = 0; i < podiums.Length; i++)
            {
                podium = podiums[i];
                if (podium.id == 0)
                    break;
            }

            // Player 1
            VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<CarState>().carSkid = CarState.CarSkid.NoSkid;

            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<CarSkidmark>().StopSkidMark());
            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<WheelSurfaceParticle>().StopSkidMark());

            VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.position = new Vector3(0, -50, 0);
            VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.rotation = Quaternion.identity;

            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.position == new Vector3(0, -50, 0));
            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.rotation == Quaternion.identity);

            VehiclesRef.instance.listVehicles[bestPlayer].transform.localPosition = Vector3.zero;
            VehiclesRef.instance.listVehicles[bestPlayer].transform.localRotation = Quaternion.identity;

            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.localPosition == Vector3.zero);
            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.localRotation == Quaternion.identity);

            VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<CarState>().carPlayerType = CarState.CarPlayerType.Human;
            VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<CarPlayerInputs>().isInputEnabled = false;


            // Reset Wheels rotation
            CarController carController = VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<CarController>();
            carController.steer = 0;
            carController.acceleration = 0;
            for (var i = 0; i < carController.wheelsList.Count; i++)
                carController.wheelsList[i].wheelAxisY.rotation = Quaternion.identity;

            VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.position = podium.playerOne.position;
            VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.rotation = podium.playerOne.rotation;

            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.position == podium.playerOne.position);
            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.parent.transform.rotation == podium.playerOne.rotation);


            VehiclesRef.instance.listVehicles[bestPlayer].transform.localPosition = Vector3.zero;
            VehiclesRef.instance.listVehicles[bestPlayer].transform.localRotation = Quaternion.identity;

            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.localPosition == Vector3.zero);
            yield return new WaitUntil(() => VehiclesRef.instance.listVehicles[bestPlayer].transform.localRotation == Quaternion.identity);

            RibodyConstraint(VehiclesRef.instance.listVehicles[bestPlayer].GetComponent<Rigidbody>());

            yield return new WaitForSeconds(.5f);

            InfoPlayerTS.instance.b_IsPageCustomPartInProcess = false;
            yield return null; 
            #endregion
        }

        public void RibodyConstraint(Rigidbody rb)
        {
            #region
            rb.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationY;

            rb.isKinematic = false;
            #endregion
        }

        int PlayerPositionDependingGameMode(int playerID)
        {
            #region
            // Arcade Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 0)
            {
                int howManyVehicleInTheList = ArcadeResult.instance.vehicleList.Count;
                for (var i = 0; i < howManyVehicleInTheList++; i++)
                {
                    if (ArcadeResult.instance.vehicleList[i] == playerID)
                        return i;
                }
            }

            // Championship Mode
            if (InfoRememberMainMenuSelection.instance.playerMainMenuSelection.currentGameMode == 2)
            {
                int howManyVehicleInTheList = ChampionshipResult.instance.vehicleList.Count;
                for (var i = 0; i < howManyVehicleInTheList++; i++)
                {
                    if (ChampionshipResult.instance.vehicleList[i] == playerID)
                        return i;
                }
            }

            return 0; 
            #endregion
        }
    }

}
