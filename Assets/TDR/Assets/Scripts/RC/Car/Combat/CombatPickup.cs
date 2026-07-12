// Description: CombatPickup. Attached to a pickup instance scattered by CombatPickupSpawner.
// Slowly rotates for visibility; on contact with the player's vehicle, activates its assigned
// AbilityDefinition and goes on a pause-aware respawn cooldown (2-lap races need pickups again on
// lap 2). Structural typing, same pattern as CombatRamDamageDealer/EnemyVehicleHealth: only the
// player's car has CarAbilityController, so an AI car driving through a pickup does nothing - no
// separate Human/AI check needed. Trigger detection mirrors OvertakeTrigger.cs: VehicleTriggerTag
// check, then walk parent.parent.parent up to the vehicle root.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CombatPickup : MonoBehaviour
    {
        public AbilityDefinition     definition;
        public float                 rotationSpeed = 90f;
        public float                 respawnCooldown = 20f;

        MeshRenderer[]                renderers;
        Collider                      trig;

        void Awake()
        {
            #region
            renderers = GetComponentsInChildren<MeshRenderer>();
            trig = GetComponent<Collider>();
            #endregion
        }

        void Update()
        {
            #region
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            #endregion
        }

        void OnTriggerEnter(Collider other)
        {
            #region
            if (definition == null) return;
            if (other.GetComponent<VehicleTriggerTag>() == null) return;

            CarAbilityController ability =
                other.transform.parent.parent.parent.GetComponent<CarAbilityController>();
            if (ability == null) return;   // AI car - structurally can't collect

            ability.Activate(definition);
            StartCoroutine(RespawnRoutine());
            #endregion
        }

        IEnumerator RespawnRoutine()
        {
            #region
            SetVisible(false);
            trig.enabled = false;

            float t = 0f;
            while (t < respawnCooldown)
            {
                if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                yield return null;
            }

            SetVisible(true);
            trig.enabled = true;
            #endregion
        }

        void SetVisible(bool visible)
        {
            #region
            foreach (MeshRenderer r in renderers) r.enabled = visible;
            #endregion
        }
    }
}
