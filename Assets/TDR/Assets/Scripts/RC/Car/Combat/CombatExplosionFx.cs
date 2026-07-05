// Description: CombatExplosionFx. One-shot, fully code-built explosion effect for Combat Run
// enemy deaths - no prefab, no external assets (the project ships zero explosion VFX, verified).
// Spawned via CombatExplosionFx.Spawn(position): a burst of fire particles + slower smoke puffs
// (procedural soft-circle texture on Sprites/Default - safe under URP, respects vertex color),
// an HDR emissive flash sphere that Bloom picks up (same trick as PathLaneGlow), and a fading
// point light. Self-destroys after `lifetime`.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CombatExplosionFx : MonoBehaviour
    {
        [Header("Fire Burst")]
        public int                   fireCount = 45;
        public Color                 fireColorA = new Color(1f, 0.85f, 0.2f, 1f);   // yellow
        public Color                 fireColorB = new Color(1f, 0.35f, 0.05f, 1f);  // orange-red

        [Header("Smoke Burst")]
        public int                   smokeCount = 18;
        public Color                 smokeColor = new Color(0.15f, 0.13f, 0.12f, 0.8f);

        [Header("Flash / Light")]
        public Color                 flashColor = new Color(1f, 0.55f, 0.15f, 1f);
        public float                 flashHDRIntensity = 6f;
        public float                 flashMaxScale = 5f;
        public float                 flashDuration = 0.22f;
        public float                 lightIntensity = 10f;
        public float                 lightRange = 18f;
        public float                 lightFadeDuration = 0.35f;

        public float                 lifetime = 3f;

        static Texture2D             softCircleTex;

        public static void Spawn(Vector3 position)
        {
            #region
            GameObject fx = new GameObject("CombatExplosionFx");
            fx.transform.position = position;
            fx.AddComponent<CombatExplosionFx>();
            #endregion
        }

        void Start()
        {
            #region
            BuildFireBurst();
            BuildSmokeBurst();
            StartCoroutine(FlashAndLightRoutine());
            Destroy(gameObject, lifetime);
            #endregion
        }

        void BuildFireBurst()
        {
            #region
            ParticleSystem ps = CreateParticleChild("Fire");
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(fireColorA, fireColorB);
            main.gravityModifier = 0.05f;

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)fireCount) });

            ApplyFadeAndShrink(ps, 0.25f);
            ps.Play();
            #endregion
        }

        void BuildSmokeBurst()
        {
            #region
            ParticleSystem ps = CreateParticleChild("Smoke");
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
            main.startColor = smokeColor;
            main.gravityModifier = -0.04f;   // slow upward drift

            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0.05f, (short)smokeCount) });

            ApplyFadeAndShrink(ps, 0.6f);
            ps.Play();
            #endregion
        }

        ParticleSystem CreateParticleChild(string childName)
        {
            #region
            GameObject obj = new GameObject(childName);
            obj.transform.SetParent(transform, false);
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 2f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;

            // Sprites/Default: unlit, transparent, vertex-color-driven - renders correctly
            // under URP without the version-fragile Particles/Unlit surface/blend setup.
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = SoftCircleTexture();
            renderer.material = mat;

            return ps;
            #endregion
        }

        void ApplyFadeAndShrink(ParticleSystem ps, float endSizeRatio)
        {
            #region
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, endSizeRatio));
            #endregion
        }

        IEnumerator FlashAndLightRoutine()
        {
            #region
            // HDR emissive sphere -> Bloom glow (Bloom confirmed active in the URP volume
            // profile; same approach PathLaneGlow.mat uses for the lane lines).
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(flash.GetComponent<Collider>());
            flash.transform.SetParent(transform, false);
            flash.transform.localScale = Vector3.one * 0.5f;
            Material flashMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            flashMat.SetColor("_BaseColor", flashColor * flashHDRIntensity);
            flash.GetComponent<MeshRenderer>().material = flashMat;

            GameObject lightObj = new GameObject("Light");
            lightObj.transform.SetParent(transform, false);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = flashColor;
            light.intensity = lightIntensity;
            light.range = lightRange;

            float t = 0f;
            float longest = Mathf.Max(flashDuration, lightFadeDuration);
            while (t < longest)
            {
                t += Time.deltaTime;

                if (t < flashDuration)
                    flash.transform.localScale =
                        Vector3.one * Mathf.Lerp(0.5f, flashMaxScale, t / flashDuration);
                else if (flash.activeSelf)
                    flash.SetActive(false);

                light.intensity = Mathf.Lerp(lightIntensity, 0f, t / lightFadeDuration);
                yield return null;
            }
            lightObj.SetActive(false);
            #endregion
        }

        // Shared with EnemyDamageFx.
        public static Texture2D SoftCircleTexture()
        {
            #region
            if (softCircleTex != null) return softCircleTex;

            const int size = 64;
            softCircleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - size * 0.5f) / (size * 0.5f);
                    float dy = (y - size * 0.5f) / (size * 0.5f);
                    float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    alpha *= alpha;   // soft falloff
                    softCircleTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            softCircleTex.Apply();
            return softCircleTex;
            #endregion
        }
    }
}
