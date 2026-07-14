// Description: CombatPickupSpawner. Scatters CombatPickup instances along the track path -
// same PathRef.instance.Track wait + tangent/lateral-offset sampling as PathLaneGlowRenderer -
// cycling through CombatRunManager's assigned theme's abilities. Mesh, HDR emissive tinted
// material, and trigger collider are all built procedurally at runtime (no prefab/material
// assets to author), matching the rest of Combat Run's FX (CombatExplosionFx, CombatPortalFx,
// EnemyHealthBar). Scene GameObject + Inspector flag, same prototype-wiring convention as
// CombatRunManager/PathLaneGlowRenderer.
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
        public float                 visualScale = 1.2f;
        public float                 triggerRadius = 1.5f;
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

            // Root carries the trigger at a fixed world-space radius; the visual mesh is a
            // separate scaled child so pickup-generosity and visible size can differ.
            GameObject root = new GameObject("CombatPickup_" + definition.abilityName);
            root.transform.SetParent(transform, true);
            root.transform.position = position;

            SphereCollider trig = root.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = triggerRadius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * visualScale;
            Destroy(visual.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", definition.tintColor);
            mat.SetColor("_EmissionColor", definition.tintColor * hdrIntensity);
            mat.EnableKeyword("_EMISSION");
            visual.GetComponent<MeshRenderer>().material = mat;

            if (definition.icon != null)
                BuildIconBillboard(root.transform, definition.icon);

            CombatPickup pickup = root.AddComponent<CombatPickup>();
            pickup.definition = definition;
            #endregion
        }

        void BuildIconBillboard(Transform center, Sprite icon)
        {
            #region
            const float canvasSize = 100f;

            GameObject canvasObj = new GameObject("Icon_Canvas");
            canvasObj.transform.SetParent(center, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(canvasSize, canvasSize);
            canvasObj.transform.localScale = Vector3.one * (visualScale * 0.9f / canvasSize);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(canvasObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;

            CombatPickupIconBillboard billboard = canvasObj.AddComponent<CombatPickupIconBillboard>();
            billboard.center = center;
            billboard.offsetRadius = visualScale * 0.55f;
            #endregion
        }
    }
}
