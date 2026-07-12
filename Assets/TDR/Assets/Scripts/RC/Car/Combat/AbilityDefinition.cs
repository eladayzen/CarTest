// Description: AbilityDefinition. ScriptableObject describing one Combat Run ability pickup -
// display, buff duration, and the params CarAbilityController needs to run it. Not every field
// applies to every attackType (e.g. ramDamageMultiplier only matters for RamFrenzy); unused
// fields for a given type are simply ignored, same shape kept for all three so a designer picks
// a type and fills in the relevant section. Create instances via Assets > Create > Combat Run >
// Ability Definition.
using UnityEngine;

namespace TS.Generics
{
    public enum CombatAttackType { RamFrenzy, ProjectileDagger, ElectroZap }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "Combat Run/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Display")]
        public string                 abilityName = "New Ability";
        public Sprite                 icon;
        // Also used as the pickup's HDR emissive tint (CombatPickupSpawner).
        public Color                  tintColor = Color.white;

        [Header("Buff")]
        public float                  duration = 8f;
        public CombatAttackType       attackType = CombatAttackType.RamFrenzy;

        [Header("Auto-Attack (ProjectileDagger / ElectroZap - not yet implemented)")]
        public float                  range = 25f;
        public float                  fireInterval = 0.6f;
        public float                  damage = 15f;
        public float                  aoeRadius = 3f;
        public float                  projectileSpeed = 30f;

        [Header("Ram Frenzy only")]
        // Multiplies CombatRamDamageDealer.damageMultiplier for the buff's duration.
        public float                  ramDamageMultiplier = 3f;
    }
}
