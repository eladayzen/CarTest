// Description: CarAbilityController. Attached at runtime by CombatRunManager to the player's
// vehicle only (same GameObject as CombatRamDamageDealer) - structural typing, same as the rest
// of Combat Run: only the player has this component, so CombatPickup can tell player from AI
// just by checking for its presence. One buff active at a time: activating a new pickup cancels
// and reverts whatever was running first, then starts the new one. Duration is a pause-aware
// coroutine (CarInvisibility.InvisibilityRoutine pattern). A worldspace billboard quad above the
// car's rear shows the active ability's icon (EnemyHealthBar.Build pattern), hidden when no buff
// is active.
//
// Auto-attack execution is dispatched by AbilityDefinition.attackType; only RamFrenzy is wired
// so far (multiplies CombatRamDamageDealer.damageMultiplier for the buff's duration).
// ProjectileDagger/ElectroZap are declared in the enum for a stable asset shape but not yet
// implemented - see Documentation/CombatRun_Plan.md Phase C.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CarAbilityController : MonoBehaviour
    {
        public float                 iconHeightAboveCar = 1.6f;
        public float                 iconWorldSize = 1f;

        CombatRamDamageDealer        ramDealer;
        Transform                    iconRoot;
        Image                        iconImage;
        Camera                       cam;

        AbilityDefinition            activeDefinition;
        Coroutine                    activeRoutine;

        // Canvas authored at 100x100 canvas units, then scaled down to world size - same trick
        // as EnemyHealthBar's canvasWidth/canvasHeight.
        const float                  canvasSize = 100f;

        public void InitCombat(CombatRunManager manager)
        {
            #region
            ramDealer = GetComponent<CombatRamDamageDealer>();
            BuildIcon();
            #endregion
        }

        void BuildIcon()
        {
            #region
            GameObject rootObj = new GameObject("AbilityIcon_Canvas");
            iconRoot = rootObj.transform;
            iconRoot.SetParent(transform, false);
            iconRoot.localPosition = new Vector3(0f, iconHeightAboveCar, -2.2f);   // car rear
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
            iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;

            rootObj.SetActive(false);   // hidden until the first Activate()
            #endregion
        }

        void LateUpdate()
        {
            #region
            if (iconRoot == null || !iconRoot.gameObject.activeSelf) return;

            if (cam == null) cam = Camera.main;
            if (cam != null)
                iconRoot.rotation = Quaternion.LookRotation(iconRoot.position - cam.transform.position);
            #endregion
        }

        public void Activate(AbilityDefinition definition)
        {
            #region
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                RevertEffect(activeDefinition);
            }

            activeDefinition = definition;
            ApplyEffect(definition);

            iconImage.sprite = definition.icon;
            iconRoot.gameObject.SetActive(true);

            activeRoutine = StartCoroutine(BuffRoutine(definition));
            #endregion
        }

        IEnumerator BuffRoutine(AbilityDefinition definition)
        {
            #region
            float t = 0f;
            while (t < definition.duration)
            {
                if (!PauseManager.instance.Bool_IsGamePaused) t += Time.deltaTime;
                yield return null;
            }

            RevertEffect(definition);
            iconRoot.gameObject.SetActive(false);
            activeDefinition = null;
            activeRoutine = null;
            #endregion
        }

        void ApplyEffect(AbilityDefinition definition)
        {
            #region
            switch (definition.attackType)
            {
                case CombatAttackType.RamFrenzy:
                    if (ramDealer != null) ramDealer.damageMultiplier = definition.ramDamageMultiplier;
                    break;

                case CombatAttackType.ProjectileDagger:
                case CombatAttackType.ElectroZap:
                    Debug.LogWarning("[CombatRun] Ability '" + definition.abilityName + "' (" +
                        definition.attackType + ") is not implemented yet - the buff timer and " +
                        "rear icon still run, but it deals no damage. See CombatRun_Plan.md Phase C.");
                    break;
            }
            #endregion
        }

        void RevertEffect(AbilityDefinition definition)
        {
            #region
            if (definition == null) return;

            if (definition.attackType == CombatAttackType.RamFrenzy && ramDealer != null)
                ramDealer.damageMultiplier = 1f;
            #endregion
        }
    }
}
