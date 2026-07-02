// Description: CarPathFollowPlayerInput. On-rails control assist: auto-throttle + auto-steer
// reused from CarAI's path-follow targets, with a player-controlled clamped lateral nudge.
// Independent of currentGameMode - works in any race mode. Attached to the vehicle
// (same GameObject as CarAI / CarController / CarState / CarPlayerInputs / VehiclePathFollow).
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CarPathFollowPlayerInput : MonoBehaviour
    {
        [Header("Manual toggle (no menu integration yet) - read once at Start")]
        public bool                 enablePathFollowMode = false;

        [Header("Lateral Nudge")]
        public float                 maxLateralOffset  = 3.5f;   // world units either side of centerline
        public float                 lateralInputSpeed = 8f;     // units/sec while actively nudging
        public float                 centeringSpeed    = 5f;     // units/sec drifting back to 0 with no input
        [Range(0f, 0.5f)]
        public float                 inputDeadzone     = 0.05f;

        CarState                     carState;
        CarController                carController;
        CarAI                        carAI;
        VehiclePathFollow            vehiclePathFollow;
        VehicleInfo                  vehicleInfo;
        CarPlayerInputs              carPlayerInputs;

        float                        currentLateralOffset = 0f;
        bool                         isReady = false;

        void Start()
        {
            #region
            carState          = GetComponent<CarState>();
            carController     = GetComponent<CarController>();
            carAI             = GetComponent<CarAI>();
            vehiclePathFollow = GetComponent<VehiclePathFollow>();
            vehicleInfo       = GetComponent<VehicleInfo>();
            carPlayerInputs   = GetComponent<CarPlayerInputs>();

            StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            yield return new WaitUntil(() =>
                carController.isInitDone &&
                vehiclePathFollow.b_InitDone &&
                carPlayerInputs.b_InitDone &&
                InfoRememberMainMenuSelection.instance);

            // Only ever arm this for an actual human-controlled slot, never for AI-filled slots
            // sharing the same prefab.
            bool isHumanSlot = vehicleInfo.playerNumber <
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            bool active = enablePathFollowMode && isHumanSlot;
            carState.isPathFollowAssistEnabled = active;
            isReady = active;
            #endregion
        }

        void OnDisable()
        {
            #region
            if (carState) carState.isPathFollowAssistEnabled = false;
            #endregion
        }

        void FixedUpdate()
        {
            #region
            if (!isReady) return;
            if (!vehiclePathFollow.b_InitDone || vehiclePathFollow.Track == null) return;
            if (PauseManager.instance != null && PauseManager.instance.Bool_IsGamePaused) return;

            // 1) Signed lateral input in [-1..1]: negative = left, positive = right.
            //    Reuses CarPlayerInputs' own key/axis reading so remapped bindings keep working.
            float lateralInput = carPlayerInputs.ReturnPlayerSteerRatio();

            // 2) Integrate our own smoothed lateral offset with independent nudge/center rates.
            //    Centering is actively driven toward 0 every frame, not merely "stop increasing".
            if (Mathf.Abs(lateralInput) > inputDeadzone)
            {
                currentLateralOffset = Mathf.Clamp(
                    currentLateralOffset + lateralInput * lateralInputSpeed * Time.fixedDeltaTime,
                    -maxLateralOffset, maxLateralOffset);
            }
            else
            {
                currentLateralOffset = Mathf.MoveTowards(currentLateralOffset, 0f,
                    centeringSpeed * Time.fixedDeltaTime);
            }

            // 3) Apply immediately - same call CarAI's own obstacle-avoidance uses
            //    (already-smoothed value, no extra smoothing layer needed).
            vehiclePathFollow.UpdateOffsetPathPosition(currentLateralOffset, 0f);

            // 4) Baseline throttle + steer reused from CarAI - pure reuse, no duplicated logic.
            float steer = carAI.DesiredSteer();
            float accel = carAI.DesiredAcceleration();

            // 5) Single shared entry point, same as AI/Human normally use.
            carController.CarMoveParameters(steer, accel);

            // 6) Keep carState bookkeeping alive for downstream systems (wheel-turn animation,
            //    dashboard, etc.) since CarAI.UpdateCarState() doesn't run for a Human-flagged car.
            UpdateCarStateBookkeeping(steer, accel);
            #endregion
        }

        void UpdateCarStateBookkeeping(float steer, float accel)
        {
            #region
            if (accel > 0) { carState.lastMoveDir = carState.moveDir; carState.moveDir = CarMoveDirection.forward; }
            else if (accel < 0) { carState.lastMoveDir = carState.moveDir; carState.moveDir = CarMoveDirection.backward; }
            else carState.moveDir = CarMoveDirection.center;

            if (steer > 0) { carState.lastSteeringDir = carState.steeringDir; carState.steeringDir = CarSteeringDirection.Right; }
            else if (steer < 0) { carState.lastSteeringDir = carState.steeringDir; carState.steeringDir = CarSteeringDirection.Left; }
            else carState.steeringDir = CarSteeringDirection.Center;
            #endregion
        }
    }
}
