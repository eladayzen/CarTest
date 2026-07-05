// Description: CombatRamDamageDealer. Attached at runtime by CombatRunManager to the player's
// vehicle only (same GameObject as the Rigidbody, so it receives the same collision callbacks
// CarController already gets). Damage architecture is structural: only the player has a dealer,
// only enemies have EnemyVehicleHealth - so enemy-vs-enemy contact does nothing and nothing can
// damage the (invulnerable) player. DPS-primary model: holding contact is the core loop; the
// capped OnCollisionEnter burst just makes hard hits feel chunkier than a graze, without ever
// rewarding bouncing over grinding.
using UnityEngine;

namespace TS.Generics
{
    public class CombatRamDamageDealer : MonoBehaviour
    {
        public float                 ramDamagePerSecond = 20f;
        public float                 impactBurstScale = 0.5f;
        public float                 impactBurstCap = 15f;
        // Ability hook (Ram Frenzy scales this for its duration). 1 = normal.
        public float                 damageMultiplier = 1f;

        public void InitCombat(CombatRunManager manager)
        {
            #region
            ramDamagePerSecond = manager.ramDamagePerSecond;
            impactBurstScale = manager.impactBurstScale;
            impactBurstCap = manager.impactBurstCap;
            #endregion
        }

        void OnCollisionEnter(Collision collision)
        {
            #region
            EnemyVehicleHealth enemy = EnemyFromCollision(collision);
            if (enemy == null) return;

            float burst = Mathf.Min(impactBurstCap,
                collision.relativeVelocity.magnitude * impactBurstScale);
            enemy.ApplyDamage(burst * damageMultiplier, CombatDamageSource.Ram);
            #endregion
        }

        void OnCollisionStay(Collision collision)
        {
            #region
            EnemyVehicleHealth enemy = EnemyFromCollision(collision);
            if (enemy == null) return;

            enemy.ApplyDamage(ramDamagePerSecond * damageMultiplier * Time.fixedDeltaTime,
                CombatDamageSource.Ram);
            #endregion
        }

        // collision.rigidbody resolves to the other car's root Rigidbody GameObject - the same
        // object EnemyVehicleHealth is attached to. Null for static geometry (walls, props).
        EnemyVehicleHealth EnemyFromCollision(Collision collision)
        {
            #region
            return collision.rigidbody != null
                ? collision.rigidbody.GetComponent<EnemyVehicleHealth>()
                : null;
            #endregion
        }
    }
}
