// Description: CombatPickupSpawner. Scatters CombatPickup instances along the track path -
// same PathRef.instance.Track wait + tangent/lateral-offset sampling as PathLaneGlowRenderer -
// cycling through CombatRunManager's assigned theme's abilities. Each spawn instantiates
// pickupPrefab (Assets/TDR/Assets/Prefabs/CombatRun/CombatPickup.prefab - mesh/collider/icon
// billboard authored there, hand-editable) and only overrides the per-ability bits at runtime:
// tinted HDR material and icon sprite. Scene GameObject + Inspector flag, same prototype-wiring
// convention as CombatRunManager/PathLaneGlowRenderer.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class CombatPickupSpawner : MonoBehaviour
    {
        [Header("Placement")]
        public float                 spacing = 60f;
        public float                 spacingJitter = 15f;
        // Stay inside the Path-Follow assist's own +/-4 lane band (PathLaneGlowRenderer.laneHalfWidth).
        public float                 lateralOffsetMax = 3.5f;
        public float                 heightOffset = 0.9f;
        public LayerMask             groundLayerMask = ~0;

        [Header("Pickup Visual")]
        // Assets/TDR/Assets/Prefabs/CombatRun/CombatPickup.prefab - edit mesh/collider/icon
        // layout directly there; only tint + icon sprite are set per-ability at spawn time.
        public GameObject            pickupPrefab;
        public float                 hdrIntensity = 3f;

        IEnumerator Start()
        {
            #region
            yield return new WaitUntil(() =>
                PathRef.instance != null && PathRef.instance.Track != null &&
                CombatRunManager.instance != null && CombatRunManager.instance.theme != null);

            Path track = PathRef.instance.Track;
            CombatThemeDefinition theme = CombatRunManager.instance.theme;

            if (theme.abilities == null || theme.abilities.Count == 0)
            {
                Debug.LogWarning("[CombatRun] CombatPickupSpawner: theme '" + theme.themeName +
                    "' has no abilities assigned - nothing to spawn.");
                yield break;
            }

            int abilityIndex = 0;
            float dist = spacing * 0.5f;

            while (dist < track.pathLength)
            {
                Vector3 centerPos = track.TargetPositionOnPath(dist);
                Vector3 tangent = track.TargetRotationOnPath(dist);
                Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized;

                float lateral = Random.Range(-lateralOffsetMax, lateralOffsetMax);
                Vector3 pos = centerPos + side * lateral;

                if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f, groundLayerMask))
                    pos = hit.point;

                pos += Vector3.up * heightOffset;

                SpawnPickup(pos, theme.abilities[abilityIndex]);

                abilityIndex = (abilityIndex + 1) % theme.abilities.Count;
                dist += spacing + Random.Range(-spacingJitter, spacingJitter);
            }
            #endregion
        }

        void SpawnPickup(Vector3 position, AbilityDefinition definition)
        {
            #region
            if (definition == null) return;
            if (pickupPrefab == null)
            {
                Debug.LogError("[CombatRun] CombatPickupSpawner.pickupPrefab is not assigned - " +
                    "nothing to spawn. Assets/TDR/Assets/Prefabs/CombatRun/CombatPickup.prefab.");
                return;
            }

            GameObject root = Instantiate(pickupPrefab, position, Quaternion.identity, transform);
            root.name = "CombatPickup_" + definition.abilityName;

            Transform visual = root.transform.Find("Visual");
            if (visual != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", definition.pickupGlowColor);
                mat.SetColor("_EmissionColor", definition.pickupGlowColor * hdrIntensity);
                mat.EnableKeyword("_EMISSION");
                visual.GetComponent<MeshRenderer>().material = mat;
            }

            Transform iconCanvas = root.transform.Find("Icon_Canvas");
            if (iconCanvas != null)
            {
                if (definition.icon != null)
                {
                    Image iconImage = iconCanvas.GetComponentInChildren<Image>();
                    if (iconImage != null) iconImage.sprite = definition.icon;
                }
                else
                {
                    iconCanvas.gameObject.SetActive(false);
                }
            }

            CombatPickup pickup = root.GetComponent<CombatPickup>();
            if (pickup != null) pickup.definition = definition;
            #endregion
        }
    }
}
