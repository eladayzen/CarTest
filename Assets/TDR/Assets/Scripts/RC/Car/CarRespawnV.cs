// Description: CarRespawnV. Attached to the vehicle. Manage the repawn process.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS.Generics
{
    public class CarRespawnV : MonoBehaviour
    {
        VehicleDamage       vehicleDamage;
        VehicleInfo         vehicleInfo;

        int                 TSInputKeyRespawn = 7;
        bool                isManualRespawnAllowed = false;
       
        private void Start()
        {
            #region
            vehicleDamage = GetComponent<VehicleDamage>();
            vehicleInfo = GetComponent<VehicleInfo>();

            vehicleDamage.VehicleExplosionAction += VehicleExplosionAction;
            TSInputKeyRespawn = GetComponent<CarPlayerInputs>().TSInputKeyRespawn;

            StartCoroutine(InitRoutine());
            #endregion
        }

        public void OnDestroy()
        {
            #region
            if (vehicleDamage)
            vehicleDamage.VehicleExplosionAction -= VehicleExplosionAction;

            //-> Respawn button
            if (vehicleInfo.playerNumber < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer)
                InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyRespawn].OnGetKeyDownReceived -= OnGetKeyPressedRespawnAction;
            #endregion
        }

        void VehicleExplosionAction()
        {
            #region
            if (!vehicleInfo.b_IsRespawn)
                StartCoroutine(RespawnRoutine()); 
            #endregion
        }

        public IEnumerator RespawnRoutine()
        {
            #region
           // Debug.Log("Respawn Process");
            vehicleInfo.b_IsRespawn = true;

            // Display the respawn screen
            RespawnScreen[] respawnScreens = FindObjectsByType<RespawnScreen>(FindObjectsSortMode.None);

            RespawnScreen respawnScreen = null;
            foreach (RespawnScreen rs in respawnScreens)
            {
                if (rs.PlayerID == vehicleInfo.playerNumber)
                {
                    respawnScreen = rs;
                    break;
                }
            }

            if (respawnScreen)
            {
                respawnScreen.FadeIn();
                yield return new WaitUntil(() => respawnScreen.IsProcessDone);
            }

            // Respawn Part 1
            vehicleDamage.respawnPoint = GetClosestCheckpoint();

            if (vehicleDamage.VehicleRespawnPart1 != null)
                vehicleDamage.VehicleRespawnPart1.Invoke();

            // Disable screen
            if (respawnScreen)
            {
                respawnScreen.FadeOut();
                yield return new WaitUntil(() => respawnScreen.IsProcessDone);
            }


            // Respawn Part 2
            if (vehicleDamage.VehicleRespawnPart2 != null)
                vehicleDamage.VehicleRespawnPart2.Invoke();

            vehicleInfo.b_IsVehicleAvailableToMove = true;

            vehicleInfo.b_IsRespawn = false;
            yield return null; 
            #endregion
        }


        public Vector3 GetClosestCheckpoint()
        {
            #region
            return Vector3.zero;
            #endregion
        }

        public Quaternion NewRotationAfterRespawn()
        {
            #region
            //Quaternion newRotation;
            return Quaternion.identity; 
            #endregion
        }

        public Quaternion NewRotationAfterRespawnDist(float dist)
        {
            #region
            // Quaternion newRotation;
            return Quaternion.identity; 
            #endregion
        }

        void OnGetKeyPressedRespawnAction()
        {
            #region Respawn the vehicle if the player press the respawn button
            if (isManualRespawnAllowed &&
                (LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].howLapDone > 1
                ||
                 LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].globalTime > 5))
            {
                if (!LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].IsRaceComplete)
                {
                    vehicleDamage.VehicleExplosionAction.Invoke();
                    StartCoroutine(MinimumDelayBetweenTwoRespawn());
                }
            }

            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region 
            yield return new WaitUntil(() => vehicleInfo.b_InitDone);

            yield return new WaitUntil(() => LapCounterAndPosition.instance.posList.Count > 0);
            yield return new WaitUntil(() => LapCounterAndPosition.instance.posList[vehicleInfo.playerNumber].globalTime > 0);

            //-> Respawn button
            if (vehicleInfo.playerNumber < InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer)
                InfoInputs.instance.ListOfInputsForEachPlayer[vehicleInfo.playerNumber].listOfButtons[TSInputKeyRespawn].OnGetKeyDownReceived += OnGetKeyPressedRespawnAction;

            isManualRespawnAllowed = true;

            yield return null; 
            #endregion
        }

        IEnumerator MinimumDelayBetweenTwoRespawn()
        {
            #region
            isManualRespawnAllowed = false;
            float t = 0;
            float duration = 2;

            while (t < duration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused)
                    t += Time.deltaTime;

                yield return null;
            }

            isManualRespawnAllowed = true;

            yield return null; 
            #endregion
        }
    }
}
