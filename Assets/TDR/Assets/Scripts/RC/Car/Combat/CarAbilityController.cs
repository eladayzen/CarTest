// Description: CarAbilityController. Attached at runtime by CombatRunManager to the player's
// vehicle only (same GameObject as CombatRamDamageDealer) - structural typing, same as the rest
// of Combat Run: only the player has this component, so CombatPickup can tell player from AI
// just by checking for its presence.
//
// Buffs are scoped by AbilityDefinition.category: activating a new pickup only cancels/reverts
// whatever is currently running in THAT category - a different-category buff (e.g. an active
// Speed Boost while Ram Frenzy is running) is left completely untouched. Each category gets its
// own pause-aware duration coroutine (CarInvisibility.InvisibilityRoutine pattern) and its own
// worldspace billboard icon above the car (EnemyHealthBar.Build pattern), laid out side by side
// so multiple simultaneous buffs never fight over one shared icon.
//
// Effect execution is dispatched by AbilityDefinition.effectType; RamFrenzy (multiplies
// CombatRamDamageDealer.damageMultiplier) and SpeedBoost (multiplies the player's cached
// baseline CarController.maxSpeed/refMaxSpeed - a single-shot apply/restore is safe here because,
// unlike AI cars, nothing re-touches the player's maxSpeed every frame; CarAI's per-frame speed
// logic is gated behind IsPlayerAI(), which is false for the human player) are wired end-to-end.
// ProjectileDagger/ElectroZap are declared for a stable asset shape but not yet implemented - see
// Documentation/CombatRun_Plan.md Phase C.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CarAbilityController : MonoBehaviour
    {
        public float                 iconHeightAboveCar = 1.6f;
        public float                 iconWorldSize = 1f;

        CombatRamDamageDealer        ramDealer;
        CarController                carController;
        float                        baseMaxSpeed;
        float                        baseRefMaxSpeed;
        Camera                       cam;

        class ActiveBuff
        {
            public AbilityDefinition definition;
            public Coroutine         routine;
            public float             elapsed;
        }

        Dictionary<AbilityCategory, ActiveBuff>  activeBuffs = new Dictionary<AbilityCategory, ActiveBuff>();
        Dictionary<AbilityCategory, Transform>   iconRoots = new Dictionary<AbilityCategory, Transform>();
        Dictionary<AbilityCategory, Image>       iconImages = new Dictionary<AbilityCategory, Image>();
        AbilityCategory[]                        knownCategories;

        // Canvas authored at 100x100 canvas units, then scaled down to world size - same trick
        // as EnemyHealthBar's canvasWidth/canvasHeight.
        const float                  canvasSize = 100f;

        public void InitCombat(CombatRunManager manager)
        {
            #region
            ramDealer = GetComponent<CombatRamDamageDealer>();
            carController = GetComponent<CarController>();

            // Cached once, after the normal race/assist init has already run (Combat Run only
            // attaches components post-countdown) - CarPathFollowPlayerInput.InitRoutine has
            // already applied its own maxSpeedMultiplier by this point, so a boost stacks on top
            // of the assist-adjusted baseline rather than clobbering it.
            if (carController != null)
            {
                baseMaxSpeed = carController.maxSpeed;
                baseRefMaxSpeed = carController.refMaxSpeed;
            }

            knownCategories = (AbilityCategory[])System.Enum.GetValues(typeof(AbilityCategory));
            EnsureIconSlots();
            #endregion
        }

        // Builds any icon slot that's missing or has gone stale (Unity's == overload treats a
        // destroyed-but-non-null Transform as == null). Cheap to call defensively - a past bug
        // here (blind iconRoot.gameObject access with no null-check) let a stale reference crash
        // Activate() mid-method and strand an already-applied gameplay effect with no revert
        // scheduled; every icon touch below goes through this instead.
        void EnsureIconSlots()
        {
            #region
            for (int i = 0; i < knownCategories.Length; i++)
            {
                AbilityCategory category = knownCategories[i];
                if (!iconRoots.TryGetValue(category, out Transform root) || root == null)
                    BuildIconSlot(category, i, knownCategories.Length);
            }
            #endregion
        }

        void BuildIconSlot(AbilityCategory category, int index, int total)
        {
            #region
            GameObject rootObj = new GameObject("AbilityIcon_Canvas_" + category);
            Transform iconRoot = rootObj.transform;
            iconRoot.SetParent(transform, false);

            float xOffset = (index - (total - 1) / 2f) * iconWorldSize * 1.2f;
            iconRoot.localPosition = new Vector3(xOffset, iconHeightAboveCar, -2.2f);   // car rear
            iconRoot.localScale = Vector3.one * (iconWorldSize / canvasSize);

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            RectTransform canvasRect = rootObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(canvasSize, canvasSize);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconRoot, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;

            rootObj.SetActive(false);   // hidden until this category's first Activate()

            iconRoots[category] = iconRoot;
            iconImages[category] = iconImage;
            #endregion
        }

        void LateUpdate()
        {
            #region
            if (iconRoots.Count == 0) return;

            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            foreach (Transform root in iconRoots.Values)
            {
                if (root != null && root.gameObject.activeSelf)
                    root.rotation = Quaternion.LookRotation(root.position - cam.transform.position);
            }
            #endregion
        }

        public void Activate(AbilityDefinition definition)
        {
            #region
            AbilityCategory category = definition.category;

            if (activeBuffs.TryGetValue(category, out ActiveBuff old))
            {
                StopCoroutine(old.routine);
                RevertEffect(old.definition);
            }

            // Gameplay state first, UI second: an icon failure must never strand the effect
            // applied here with no revert scheduled (see EnsureIconSlots comment above).
            ActiveBuff buff = new ActiveBuff { definition = definition, elapsed = 0f };
            activeBuffs[category] = buff;
            ApplyEffect(definition);
            buff.routine = StartCoroutine(BuffRoutine(category, buff));

            EnsureIconSlots();
            iconImages[category].sprite = definition.icon;
            iconRoots[category].gameObject.SetActive(true);

            Debug.Log("[CombatRun] Ability activated: " + definition.abilityName +
                " (" + definition.effectType + ", category=" + category + ", " +
                definition.duration + "s)");
            #endregion
        }

        IEnumerator BuffRoutine(AbilityCategory category, ActiveBuff buff)
        {
            #region
            while (buff.elapsed < buff.definition.duration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused) buff.elapsed += Time.deltaTime;
                yield return null;
            }

            RevertEffect(buff.definition);
            if (iconRoots.TryGetValue(category, out Transform root) && root != null)
                root.gameObject.SetActive(false);
            activeBuffs.Remove(category);
            Debug.Log("[CombatRun] Ability expired: " + buff.definition.abilityName);
            #endregion
        }

        // Read-only query for the bottom-of-screen HUD (CombatBuffHUD) - remaining01 is 1 at
        // activation, 0 at expiry.
        public bool TryGetActiveBuff(AbilityCategory category, out AbilityDefinition definition, out float remaining01)
        {
            #region
            if (activeBuffs.TryGetValue(category, out ActiveBuff buff))
            {
                definition = buff.definition;
                remaining01 = buff.definition.duration > 0f
                    ? 1f - Mathf.Clamp01(buff.elapsed / buff.definition.duration)
                    : 0f;
                return true;
            }
            definition = null;
            remaining01 = 0f;
            return false;
            #endregion
        }

        void ApplyEffect(AbilityDefinition definition)
        {
            #region
            switch (definition.effectType)
            {
                case CombatEffectType.RamFrenzy:
                    if (ramDealer != null) ramDealer.damageMultiplier = definition.ramDamageMultiplier;
                    break;

                case CombatEffectType.SpeedBoost:
                    if (carController != null)
                    {
                        carController.maxSpeed = baseMaxSpeed * definition.speedMultiplier;
                        carController.refMaxSpeed = carController.maxSpeed;
                    }
                    break;

                case CombatEffectType.ProjectileDagger:
                case CombatEffectType.ElectroZap:
                    Debug.LogWarning("[CombatRun] Ability '" + definition.abilityName + "' (" +
                        definition.effectType + ") is not implemented yet - the buff timer and " +
                        "rear icon still run, but it deals no damage. See CombatRun_Plan.md Phase C.");
                    break;
            }
            #endregion
        }

        void RevertEffect(AbilityDefinition definition)
        {
            #region
            if (definition == null) return;

            switch (definition.effectType)
            {
                case CombatEffectType.RamFrenzy:
                    if (ramDealer != null) ramDealer.damageMultiplier = 1f;
                    break;

                case CombatEffectType.SpeedBoost:
                    if (carController != null)
                    {
                        carController.maxSpeed = baseMaxSpeed;
                        carController.refMaxSpeed = baseRefMaxSpeed;
                    }
                    break;
            }
            #endregion
        }
    }
}
