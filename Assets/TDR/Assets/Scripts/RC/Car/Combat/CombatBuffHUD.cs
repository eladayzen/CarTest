// Description: CombatBuffHUD. Attached at runtime by CombatRunManager to the player's vehicle
// alongside CarAbilityController (reads its TryGetActiveBuff query, doesn't own any state
// itself). Instantiates hudPrefab (Assets/TDR/Assets/Prefabs/CombatRun/CombatBuffHUD.prefab -
// layout, sizes, colors, ring sprite all hand-editable there) and only wires the per-frame data:
// icon sprite, ring fill amount, ring color. Slots are always present, dimmed to a default icon
// when that category has no active buff.
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
        public GameObject            hudPrefab;
        public float                 idleAlpha = 0.35f;

        [Header("Default Icons (shown greyed-out before first pickup)")]
        public Sprite                 defaultAttackIcon;
        public Sprite                 defaultSpeedIcon;

        CarAbilityController         abilityController;

        class Slot
        {
            public Image             icon;
            public Image             ringFill;
            public Sprite            defaultIcon;
        }

        Dictionary<AbilityCategory, Slot> slots = new Dictionary<AbilityCategory, Slot>();

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
            if (hudPrefab == null)
            {
                Debug.LogError("[CombatRun] CombatBuffHUD.hudPrefab is not assigned - " +
                    "nothing to show. Assets/TDR/Assets/Prefabs/CombatRun/CombatBuffHUD.prefab.");
                return;
            }

            GameObject instance = Instantiate(hudPrefab);
            instance.name = "CombatBuffHUD_Canvas";

            BindSlot(instance.transform, "Slot_Attack", AbilityCategory.Attack, defaultAttackIcon);
            BindSlot(instance.transform, "Slot_Speed", AbilityCategory.Speed, defaultSpeedIcon);
            #endregion
        }

        void BindSlot(Transform root, string slotPath, AbilityCategory category, Sprite defaultIcon)
        {
            #region
            Transform slotRoot = root.Find("Container/" + slotPath);
            if (slotRoot == null)
            {
                Debug.LogError("[CombatRun] CombatBuffHUD: '" + slotPath + "' not found in hudPrefab.");
                return;
            }

            Image icon = slotRoot.Find("Icon")?.GetComponent<Image>();
            Image ringFill = slotRoot.Find("RingFill")?.GetComponent<Image>();
            if (icon == null || ringFill == null)
            {
                Debug.LogError("[CombatRun] CombatBuffHUD: '" + slotPath + "' is missing Icon/RingFill.");
                return;
            }

            Slot slot = new Slot { icon = icon, ringFill = ringFill, defaultIcon = defaultIcon };
            slots[category] = slot;
            SetSlotIdle(slot);
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
                    slot.ringFill.color = definition.barColor;
                    slot.ringFill.fillAmount = remaining01;
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
            slot.icon.sprite = slot.defaultIcon;
            slot.icon.color = new Color(1f, 1f, 1f, idleAlpha);
            slot.ringFill.fillAmount = 0f;
            #endregion
        }
    }
}
