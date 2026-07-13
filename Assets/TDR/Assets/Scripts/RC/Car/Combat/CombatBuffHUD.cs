// Description: CombatBuffHUD. Attached at runtime by CombatRunManager to the player's vehicle
// alongside CarAbilityController (reads its TryGetActiveBuff query, doesn't own any state
// itself). Bottom-center Screen Space Overlay HUD showing one fixed slot per AbilityCategory -
// icon + a countdown fill bar underneath (EnemyHealthBar.Build's left-pivoted scale-X fill
// technique, reused here in screen space instead of world space). Slots are always present,
// dimmed when that category has no active buff. First code-built Screen Space UI in the project
// (EnemyHealthBar/CarAbilityController's rear icon are both World Space) - CanvasScaler settings
// below are copied from the in-race HUD prefab (CanvasInGame.prefab) so this new canvas scales
// identically despite being a separate, unparented canvas (same "zero scene/prefab coupling"
// convention as the rest of Combat Run's code-built UI).
//
// Stage 2 (not built yet): animate slots appearing on pickup / disappearing on expiry instead of
// always showing dimmed - revisit once this fixed-slot version is confirmed working in-game.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CombatBuffHUD : MonoBehaviour
    {
        public float                 bottomMargin = 20f;
        public float                 slotSize = 70f;
        public float                 slotSpacing = 16f;
        public float                 idleAlpha = 0.35f;

        CarAbilityController         abilityController;

        class Slot
        {
            public GameObject        root;
            public Image             icon;
            public RectTransform     fillRect;
            public Image             fillImage;
        }

        Dictionary<AbilityCategory, Slot> slots = new Dictionary<AbilityCategory, Slot>();

        // Reference resolution matches CanvasInGame.prefab / Canvas_RPM.prefab exactly, so this
        // canvas scales identically to the rest of the in-race HUD despite being unparented.
        const float                  refWidth = 800f;
        const float                  refHeight = 600f;

        public void InitCombat(CombatRunManager manager, CarAbilityController _abilityController)
        {
            #region
            abilityController = _abilityController;
            Build();
            #endregion
        }

        void Build()
        {
            #region
            GameObject canvasObj = new GameObject("CombatBuffHUD_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1;   // matches CanvasInGame - above the 3D scene, below the pause menu (order 0)

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(refWidth, refHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject container = new GameObject("Container");
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.SetParent(canvasObj.transform, false);
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, bottomMargin);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = slotSpacing;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AbilityCategory[] categories = (AbilityCategory[])System.Enum.GetValues(typeof(AbilityCategory));
            foreach (AbilityCategory category in categories)
                BuildSlot(container.transform, category);
            #endregion
        }

        void BuildSlot(Transform parent, AbilityCategory category)
        {
            #region
            GameObject slotRoot = new GameObject("Slot_" + category);
            RectTransform slotRect = slotRoot.AddComponent<RectTransform>();
            slotRect.SetParent(parent, false);
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);

            // Dim background square - always visible, tints toward each slot's own idle state.
            Image bgImage = slotRoot.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.5f);

            GameObject iconObj = new GameObject("Icon");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(slotRoot.transform, false);
            iconRect.anchorMin = new Vector2(0.1f, 0.25f);
            iconRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;

            // Countdown bar - left-pivoted scale-X fill, same technique as EnemyHealthBar.Fill.
            GameObject fillBgObj = new GameObject("TimerBarBg");
            RectTransform fillBgRect = fillBgObj.AddComponent<RectTransform>();
            fillBgRect.SetParent(slotRoot.transform, false);
            fillBgRect.anchorMin = new Vector2(0.1f, 0.06f);
            fillBgRect.anchorMax = new Vector2(0.9f, 0.18f);
            fillBgRect.offsetMin = Vector2.zero;
            fillBgRect.offsetMax = Vector2.zero;
            Image fillBgImage = fillBgObj.AddComponent<Image>();
            fillBgImage.color = new Color(0f, 0f, 0f, 0.65f);

            GameObject fillObj = new GameObject("TimerBarFill");
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.SetParent(fillBgObj.transform, false);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(1f, 1f);
            fillRect.offsetMax = new Vector2(1f, -1f);
            fillRect.sizeDelta = new Vector2(fillBgRect.rect.width - 2f, fillRect.sizeDelta.y);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.white;

            slots[category] = new Slot
            {
                root = slotRoot,
                icon = iconImage,
                fillRect = fillRect,
                fillImage = fillImage,
            };

            SetSlotIdle(slots[category]);
            #endregion
        }

        void Update()
        {
            #region
            if (abilityController == null) return;

            foreach (KeyValuePair<AbilityCategory, Slot> kv in slots)
            {
                AbilityCategory category = kv.Key;
                Slot slot = kv.Value;

                if (abilityController.TryGetActiveBuff(category, out AbilityDefinition definition, out float remaining01))
                {
                    slot.icon.sprite = definition.icon;
                    slot.icon.color = Color.white;
                    slot.fillImage.color = definition.tintColor;
                    slot.fillRect.localScale = new Vector3(remaining01, 1f, 1f);
                }
                else
                {
                    SetSlotIdle(slot);
                }
            }
            #endregion
        }

        void SetSlotIdle(Slot slot)
        {
            #region
            slot.icon.color = new Color(1f, 1f, 1f, idleAlpha);
            slot.fillRect.localScale = new Vector3(0f, 1f, 1f);
            #endregion
        }
    }
}
