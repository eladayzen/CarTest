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
        [Range(0f, 0.5f)]
        public float                 inputDeadzone     = 0.05f;

        [Header("Release Behaviour")]
        // Config B (default): offset holds wherever the player left it - lane choice persists
        // through corners until the player moves again. Config A: offset drifts back to
        // centerline at centeringSpeed when there's no input.
        public bool                  holdOffsetOnRelease = true;
        public float                 centeringSpeed      = 5f;   // units/sec, only used when holdOffsetOnRelease = false

        [Header("Turn-Rate Boost")]
        // Raises the car's actual physical turn-rate ceiling (CarController.speedRotationRef) so
        // there's more real steering headroom to express lateral nudges against the path's own
        // curvature during hard corners, instead of the two competing for the same limited
        // budget. Applied once at init, only to this vehicle's own CarController instance -
        // never touches the shared prefab default or AI-driven cars. 1 = no change.
        public float                 turnRateMultiplier = 1.6f;

        [Header("Max Speed")]
        // Lowers the assisted car's top speed so it has more time/turn-rate budget to work with
        // on hard corners, at the cost of overall pace everywhere (not just corners). Applied
        // once at init, only to this vehicle's own CarController instance. 1 = no change.
        public float                 maxSpeedMultiplier = 0.85f;

        [Header("Corner Braking (Config C)")]
        // A held offset tightens the effective turn radius on the inside of a corner beyond what
        // CarAI's own centerline-based braking accounts for. Rather than let the offset bleed
        // back toward centerline when the car can't physically turn sharply enough, brake harder
        // so the exact chosen lane is always reached. 0 = no extra braking, 1 = full brake at
        // max corner sharpness * max offset severity.
        [Range(0f, 1f)]
        public float                 cornerBrakeStrength = 0f;
        public float                 minSpeedFloor        = 6f;  // never brake below this speed via this system

        [Header("General Corner Slowdown")]
        // Extra braking scaled purely by upcoming corner sharpness, independent of any held
        // offset - unlike Config C (which only kicks in while holding a lane offset), this gives
        // turn-rate breathing room on any hard corner even when centered or barely nudging, so
        // input doesn't feel like it's fighting the path's own curvature. 0 = no extra braking,
        // 1 = full brake at the sharpest corner.
        [Range(0f, 1f)]
        public float                 generalCornerBrakeStrength = 0.4f;

        CarState                     carState;
        CarController                carController;
        CarAI                        carAI;
        VehiclePathFollow            vehiclePathFollow;
        VehicleInfo                  vehicleInfo;
        CarPlayerInputs              carPlayerInputs;
        Rigidbody                    rb;

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
            rb                = GetComponent<Rigidbody>();

            StartCoroutine(InitRoutine());
            #endregion
        }

        IEnumerator InitRoutine()
        {
            #region
            // Waiting on VehiclesRef.b_InitDone (the whole fleet, not just this car) closes a
            // class of load-order races: this loop otherwise starts driving while other cars'
            // VehiclePathFollow.Track refs are still null (DetectCarAhead NRE) and while
            // PathObstacle's danger lists are still being built (ArgumentOutOfRange).
            yield return new WaitUntil(() =>
                carController.isInitDone &&
                vehiclePathFollow.b_InitDone &&
                carPlayerInputs.b_InitDone &&
                VehiclesRef.instance != null &&
                VehiclesRef.instance.b_InitDone &&
                InfoRememberMainMenuSelection.instance);

            // Only ever arm this for an actual human-controlled slot, never for AI-filled slots
            // sharing the same prefab.
            bool isHumanSlot = vehicleInfo.playerNumber <
                InfoRememberMainMenuSelection.instance.playerMainMenuSelection.HowManyPlayer;

            bool active = enablePathFollowMode && isHumanSlot;
            carState.isPathFollowAssistEnabled = active;
            isReady = active;

            // Only ever touch this vehicle's own CarController instance - CarController.Init()
            // has already run (waited on isInitDone above), so speedRotationRef/maxSpeed are set
            // and safe to scale here without being overwritten later.
            if (active)
            {
                carController.speedRotationRef *= turnRateMultiplier;

                carController.maxSpeed *= maxSpeedMultiplier;
                carController.refMaxSpeed = carController.maxSpeed;
            }
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
            //    holdOffsetOnRelease (Config B, default): no input leaves the offset untouched -
            //    the player's lane choice persists through corners until they move again.
            //    !holdOffsetOnRelease (Config A): actively drifts back to 0 at centeringSpeed.
            if (Mathf.Abs(lateralInput) > inputDeadzone)
            {
                currentLateralOffset = Mathf.Clamp(
                    currentLateralOffset + lateralInput * lateralInputSpeed * Time.fixedDeltaTime,
                    -maxLateralOffset, maxLateralOffset);
            }
            else if (!holdOffsetOnRelease)
            {
                currentLateralOffset = Mathf.MoveTowards(currentLateralOffset, 0f,
                    centeringSpeed * Time.fixedDeltaTime);
            }

            // 3) Apply immediately - same call CarAI's own obstacle-avoidance uses
            //    (already-smoothed value, no extra smoothing layer needed).
            vehiclePathFollow.UpdateOffsetPathPosition(currentLateralOffset, 0f);

            // 3b) Keep the obstacle-danger-list index fresh ourselves - CarAI.Update() normally
            //     does this every 4 frames, but that loop is gated behind IsPlayerAI() and never
            //     runs for this (Human-flagged) car, so without this DesiredAcceleration()'s
            //     obstacle branch below indexes with a stale/out-of-range value on tracks that
            //     have a PathObstacle configured.
            //     Guarded per-path: Track.selectedId switches to the alt-path index when the car
            //     drives through an alt-path trigger, and dangerListByPath has no entry for alt
            //     paths - calling through with that id throws ArgumentOutOfRange every physics
            //     frame (and an empty DangerList would divide-by-zero inside on the next call).
            if (carAI.PathObs != null &&
                carAI.PathObs.dangerListByPath != null &&
                vehiclePathFollow.Track.selectedId < carAI.PathObs.dangerListByPath.Count &&
                carAI.PathObs.dangerListByPath[vehiclePathFollow.Track.selectedId].DangerList.Count > 0)
                carAI.closestObstaclePathPos = carAI.CloseFromPathObstaclePosition();

            // 4) Baseline throttle + steer reused from CarAI - pure reuse, no duplicated logic.
            float steer = carAI.DesiredSteer();
            float accel = carAI.DesiredAcceleration();

            // 4b) Config C: brake harder when the held offset makes the upcoming corner tighter
            //     than CarAI's own centerline-based braking accounts for, so the exact chosen
            //     lane is always reached instead of bleeding back toward centerline.
            accel = ApplyCornerBraking(accel);

            // 5) Single shared entry point, same as AI/Human normally use.
            carController.CarMoveParameters(steer, accel);

            // 6) Keep carState bookkeeping alive for downstream systems (wheel-turn animation,
            //    dashboard, etc.) since CarAI.UpdateCarState() doesn't run for a Human-flagged car.
            UpdateCarStateBookkeeping(steer, accel);
            #endregion
        }

        // Extra brake cap layered on top of CarAI's own centerline-based result. The corner-
        // sharpness signal reuses the same targetOne/targetTwo forward angle CarAI's own
        // CautiousNeededDependingOnCornerAngle() uses. Two independent demand sources are
        // combined by taking the larger one (not stacked/added):
        //  - offsetBrakeDemand (Config C): scales with held offset severity, so an off-center
        //    lane is always physically reachable. Zero at centerline.
        //  - generalBrakeDemand: scales with corner sharpness alone, so hard corners get some
        //    breathing room even when centered or barely nudging.
        float ApplyCornerBraking(float accel)
        {
            #region
            if (carAI.targetOne == null || carAI.targetTwo == null)
                return accel;

            if (rb != null && rb.linearVelocity.magnitude <= minSpeedFloor)
                return accel;

            float cornerAngle = Vector3.Angle(carAI.targetOne.forward, carAI.targetTwo.forward);
            float cornerSharpness = Mathf.InverseLerp(0f, 180f, cornerAngle);

            float offsetSeverity = maxLateralOffset > 0f ? Mathf.Abs(currentLateralOffset) / maxLateralOffset : 0f;
            float offsetBrakeDemand = cornerSharpness * offsetSeverity * cornerBrakeStrength;
            float generalBrakeDemand = cornerSharpness * generalCornerBrakeStrength;

            float extraBrakeDemand = Mathf.Max(offsetBrakeDemand, generalBrakeDemand);
            if (extraBrakeDemand <= 0f)
                return accel;

            float accelCap = Mathf.Lerp(1f, -1f, extraBrakeDemand);
            return Mathf.Min(accel, accelCap);
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
