// Description: EnemyHealthBar. Attached at runtime by CombatRunManager next to
// EnemyVehicleHealth. Builds its own small worldspace canvas above the car roof (no scene/prefab
// UI edits), billboards it to the camera, and polls HP every frame - color shifts green -> red
// as HP drops. Everything is code-built so removing the component leaves no asset residue.
using UnityEngine;
using UnityEngine.UI;

namespace TS.Generics
{
    public class EnemyHealthBar : MonoBehaviour
    {
        public float                 heightAboveCar = 2.6f;
        public float                 barWorldWidth = 2.2f;
        public float                 barWorldHeight = 0.25f;

        EnemyVehicleHealth           health;
        Transform                    barRoot;
        RectTransform                fillRect;
        Image                        fillImage;
        Camera                       cam;

        // Canvas is authored at 100x12 canvas units, then scaled down to world size - keeps UI
        // math in comfortable numbers instead of sub-unit rect sizes.
        const float                  canvasWidth = 100f;
        const float                  canvasHeight = 12f;

        public void InitBar(EnemyVehicleHealth _health)
        {
            #region
            health = _health;
            Build();
            #endregion
        }

        void Build()
        {
            #region
            GameObject rootObj = new GameObject("EnemyHealthBar_Canvas");
            barRoot = rootObj.transform;
            barRoot.SetParent(transform, false);
            barRoot.localPosition = new Vector3(0f, heightAboveCar, 0f);
            barRoot.localScale = Vector3.one * (barWorldWidth / canvasWidth);

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            RectTransform canvasRect = rootObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(canvasWidth,
                canvasHeight * (barWorldHeight / barWorldWidth) * (canvasWidth / canvasHeight));

            // Background
            GameObject bgObj = new GameObject("Bg");
            bgObj.transform.SetParent(barRoot, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.65f);

            // Fill - left-pivoted so scaling X shrinks toward the left edge.
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(1f, 1f);
            fillRect.offsetMax = new Vector2(1f, -1f);
            fillRect.sizeDelta = new Vector2(canvasWidth - 2f, fillRect.sizeDelta.y);
            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.green;
            #endregion
        }

        void LateUpdate()
        {
            #region
            if (health == null || barRoot == null) return;

            if (cam == null) cam = Camera.main;
            if (cam != null)
                barRoot.rotation = Quaternion.LookRotation(barRoot.position - cam.transform.position);

            float ratio = health.maxHP > 0f ? health.currentHP / health.maxHP : 0f;
            fillRect.localScale = new Vector3(ratio, 1f, 1f);
            fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
            #endregion
        }
    }
}
