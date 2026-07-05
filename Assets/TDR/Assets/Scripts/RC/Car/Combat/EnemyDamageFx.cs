// Description: EnemyDamageFx. Visual damage feedback on an enemy vehicle that is losing HP but
// isn't dead yet (worldspace HP bars proved unreliable - this replaces them as the primary
// signal). Two code-built particle systems on the car: orange impact sparks emitted on each
// damage tick (throttled), and a looping dark smoke trail whose intensity ramps up as HP drops
// below ~70%. Built lazily on first damage; everything code-generated, no assets.
using UnityEngine;

namespace TS.Generics
{
    public class EnemyDamageFx : MonoBehaviour
    {
        public float                 smokeStartsBelowHPRatio = 0.7f;
        public float                 maxSmokeRate = 35f;
        public float                 sparkThrottleSeconds = 0.12f;
        public int                   sparksPerBurst = 6;

        [Header("Damage Glow Pulses")]
        public Color                 glowColor = new Color(1f, 0.25f, 0.05f, 1f);   // hot orange-red
        public float                 glowPerHit = 0.5f;    // added per damage tick
        public float                 glowMax = 2.5f;       // HDR emission cap
        public float                 glowDecayPerSecond = 2.2f;

        ParticleSystem               smoke;
        ParticleSystem               sparks;
        float                        lastSparkTime = -999f;
        bool                         isBuilt = false;

        System.Collections.Generic.List<Material> glowMaterials;
        float                        glowPulse = 0f;
        static readonly int          emissionColorId = Shader.PropertyToID("_EmissionColor");

        public void OnDamaged(float hpRatio)
        {
            #region
            if (!isBuilt) Build();

            // Smoke ramps from nothing at the threshold to full plume near death.
            float intensity = Mathf.Clamp01((smokeStartsBelowHPRatio - hpRatio) / smokeStartsBelowHPRatio);
            var emission = smoke.emission;
            emission.rateOverTime = maxSmokeRate * intensity;

            if (Time.time - lastSparkTime > sparkThrottleSeconds)
            {
                lastSparkTime = Time.time;
                sparks.Emit(sparksPerBurst);
            }

            // Each hit stacks a pulse of emissive glow on the car body (decays in Update).
            glowPulse = Mathf.Min(glowMax, glowPulse + glowPerHit);
            #endregion
        }

        void Update()
        {
            #region
            if (glowMaterials == null || glowPulse <= 0f) return;

            glowPulse = Mathf.Max(0f, glowPulse - glowDecayPerSecond * Time.deltaTime);
            Color emission = glowColor * glowPulse;
            for (int i = 0; i < glowMaterials.Count; i++)
                glowMaterials[i].SetColor(emissionColorId, emission);
            #endregion
        }

        public void ResetFx()
        {
            #region
            if (!isBuilt) return;
            var emission = smoke.emission;
            emission.rateOverTime = 0f;
            smoke.Clear();
            sparks.Clear();

            glowPulse = 0f;
            if (glowMaterials != null)
                for (int i = 0; i < glowMaterials.Count; i++)
                    glowMaterials[i].SetColor(emissionColorId, Color.black);
            #endregion
        }

        void Build()
        {
            #region
            isBuilt = true;

            // Glow targets: instantiate this car's body materials (per-instance copies - the
            // shared prefab materials are never touched) and enable emission on them. URP Lit
            // needs the _EMISSION keyword on for _EmissionColor to render.
            glowMaterials = new System.Collections.Generic.List<Material>();
            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>(true))
            {
                foreach (Material mat in mr.materials)
                {
                    if (!mat.HasProperty(emissionColorId)) continue;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor(emissionColorId, Color.black);
                    glowMaterials.Add(mat);
                }
            }

            // Smoke: looping plume above the hood, world-space so it trails behind the car.
            smoke = CreateChild("DamageSmoke");
            var main = smoke.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
            main.startColor = new Color(0.12f, 0.11f, 0.1f, 0.75f);
            main.gravityModifier = -0.06f;   // drift upward
            var emission = smoke.emission;
            emission.rateOverTime = 0f;
            smoke.Play();

            // Sparks: burst-only orange impact hits.
            sparks = CreateChild("DamageSparks");
            var sMain = sparks.main;
            sMain.loop = false;
            sMain.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            sMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
            sMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);
            sMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.8f, 0.2f, 1f), new Color(1f, 0.45f, 0.1f, 1f));
            sMain.gravityModifier = 0.4f;
            var sEmission = sparks.emission;
            sEmission.rateOverTime = 0f;
            sparks.Play();
            #endregion
        }

        ParticleSystem CreateChild(string childName)
        {
            #region
            GameObject obj = new GameObject(childName);
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.4f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = CombatExplosionFx.SoftCircleTexture();
            renderer.material = mat;

            return ps;
            #endregion
        }
    }
}
