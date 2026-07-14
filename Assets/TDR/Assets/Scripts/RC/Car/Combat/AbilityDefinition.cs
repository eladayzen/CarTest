// Description: AbilityDefinition. ScriptableObject describing one Combat Run ability pickup -
// display, buff duration, and the params CarAbilityController needs to run it. Not every field
// applies to every effectType (e.g. ramDamageMultiplier only matters for RamFrenzy); unused
// fields for a given type are simply ignored, same shape kept for all types so a designer picks
// a type and fills in the relevant section. Create instances via Assets > Create > Combat Run >
// Ability Definition.
//
// `category` is the buff-slot key CarAbilityController cancels-on-conflict by: two abilities in
// the same category replace each other (e.g. a future second Attack pickup would replace Ram
// Frenzy), abilities in different categories run fully independently (Speed Boost never cancels
// an active attack buff, and vice versa).
using UnityEngine;

namespace TS.Generics
{
    public enum CombatEffectType { RamFrenzy, ProjectileDagger, ElectroZap, SpeedBoost }

    public enum AbilityCategory { Attack, Speed }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "Combat Run/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Display")]
        public string                 abilityName = "New Ability";
        public Sprite                 icon;
        // CombatBuffHUD's ring fill color for this ability's category.
        public Color                  barColor = Color.white;
        // CombatPickupSpawner's base + HDR emissive tint on the in-world pickup sphere.
        public Color                  pickupGlowColor = Color.white;

        [Header("Buff")]
        public float                  duration = 8f;
        public AbilityCategory        category = AbilityCategory.Attack;
        public CombatEffectType       effectType = CombatEffectType.RamFrenzy;

        [Header("Auto-Attack (ProjectileDagger / ElectroZap - not yet implemented)")]
        public float                  range = 25f;
        public float                  fireInterval = 0.6f;
        public float                  damage = 15f;
        public float                  aoeRadius = 3f;
        public float                  projectileSpeed = 30f;

        [Header("Ram Frenzy only")]
        // Multiplies CombatRamDamageDealer.damageMultiplier for the buff's duration.
        public float                  ramDamageMultiplier = 3f;

        [Header("Speed Boost only")]
        // Multiplies the player's cached baseline CarController.maxSpeed for the buff's duration.
        public float                  speedMultiplier = 1.5f;
    }
}
