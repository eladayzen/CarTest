// Description: EnemyVehicleHealth. Attached at runtime by CombatRunManager to AI vehicles in
// Combat Run mode. Float HP + the single damage entry point every damage source (ram, abilities)
// funnels through. Deliberately independent of VehicleDamage.lifePoints - that class's
// VehicleExplosionAction semantically means "respawn to checkpoint" (CarRespawnV subscribes),
// which must not be conflated with combat destruction.
using UnityEngine;

namespace TS.Generics
{
    public enum CombatDamageSource { Ram, Ability }

    public class EnemyVehicleHealth : MonoBehaviour
    {
        public float                 maxHP = 100f;
        public float                 currentHP = 100f;
        public bool                  isDead = false;
        [HideInInspector] public float deathTime = -999f;   // read by the wave-respawn loop
        // Cached "normal" CarAI.maxSpeedRef (post global enemySpeedMultiplier, pre Chase Assist)
        // that Chase Assist restores to when this car stops being the chase target.
        [HideInInspector] public float baseMaxSpeedRef;

        CombatRunManager             manager;
        EnemyDamageFx                damageFx;
        float                        lastLogTime = -999f;

        public void InitCombat(CombatRunManager _manager, float _maxHP)
        {
            #region
            manager = _manager;
            maxHP = _maxHP;
            currentHP = _maxHP;
            isDead = false;
            #endregion
        }

        public void ApplyDamage(float amount, CombatDamageSource source)
        {
            #region
            if (isDead || amount <= 0f) return;

            currentHP = Mathf.Max(0f, currentHP - amount);

            // Visual feedback while hurt-but-alive: impact sparks + smoke ramping with damage.
            if (damageFx == null) damageFx = gameObject.AddComponent<EnemyDamageFx>();
            damageFx.OnDamaged(maxHP > 0f ? currentHP / maxHP : 0f);

            // Throttled log so OnCollisionStay's per-physics-step ticks stay readable.
            if (Time.time - lastLogTime > 0.5f)
            {
                lastLogTime = Time.time;
                Debug.Log("[CombatRun] " + name + " HP " + currentHP.ToString("F1") + "/" + maxHP
                    + " (" + source + ")");
            }

            if (currentHP <= 0f)
                Die();
            #endregion
        }

        public void ResetCombatHealth()
        {
            #region
            currentHP = maxHP;
            isDead = false;
            if (damageFx != null) damageFx.ResetFx();
            #endregion
        }

        // Phase A: one-shot guard + notify only. The actual death sequence (explosion FX +
        // SetActive(false) disable-in-place) lands in Phase B.
        void Die()
        {
            #region
            if (isDead) return;
            isDead = true;
            deathTime = Time.time;

            if (manager != null)
                manager.NotifyEnemyDestroyed(this);
            #endregion
        }
    }
}
