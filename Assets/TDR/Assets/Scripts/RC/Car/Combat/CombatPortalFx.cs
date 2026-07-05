// Description: CombatPortalFx. Code-built one-shot "portal" effect played where a recycled
// enemy vehicle teleports in: an HDR emissive cyan disc that snaps open across the track
// (Bloom picks it up), a swirl of particles sucked inward, and a light flash. No prefab, no
// assets - same construction approach as CombatExplosionFx. Self-destroys.
using System.Collections;
using UnityEngine;

namespace TS.Generics
{
    public class CombatPortalFx : MonoBehaviour
    {
        public Color                 portalColor = new Color(0.2f, 0.8f, 1f, 1f);   // cyan
        public float                 hdrIntensity = 4.5f;
        public float                 maxScale = 6f;
        public float                 openDuration = 0.3f;
        public float                 holdDuration = 0.6f;
        public float                 closeDuration = 0.35f;
        public float                 lifetime = 2f;

        Quaternion                   facing;

        public static void Spawn(Vector3 position, Quaternion _facing)
        {
            #region
            GameObject fx = new GameObject("CombatPortalFx");
            fx.transform.position = position;
            CombatPortalFx portal = fx.AddComponent<CombatPortalFx>();
            portal.facing = _facing;
            #endregion
        }

        void Start()
        {
            #region
            BuildSwirl();
            StartCoroutine(DiscRoutine());
            Destroy(gameObject, lifetime);
            #endregion
        }

        IEnumerator DiscRoutine()
        {
            #region
            // Portal disc: quad perpendicular to the track direction. Sprites/Default is
            // double-sided and vertex-tinted, so an HDR color on the soft-circle texture
            // reads as a glowing energy disc under Bloom.
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(disc.GetComponent<Collider>());
            disc.transform.SetParent(transform, false);
            disc.transform.rotation = facing;
            disc.transform.localScale = Vector3.zero;
            Material discMat = new Material(Shader.Find("Sprites/Default"));
            discMat.mainTexture = CombatExplosionFx.SoftCircleTexture();
            discMat.color = portalColor * hdrIntensity;
            disc.GetComponent<MeshRenderer>().material = discMat;

            GameObject lightObj = new GameObject("Light");
            lightObj.transform.SetParent(transform, false);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = portalColor;
            light.range = 16f;
            light.intensity = 0f;

            // Open (ease-out) -> hold with a subtle pulse -> close.
            float t = 0f;
            while (t < openDuration)
            {
                t += Time.deltaTime;
                float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / openDuration), 3f);
                disc.transform.localScale = Vector3.one * (maxScale * p);
                light.intensity = 8f * p;
                yield return null;
            }

            t = 0f;
            while (t < holdDuration)
            {
                t += Time.deltaTime;
                float pulse = 1f + 0.06f * Mathf.Sin(t * 25f);
                disc.transform.localScale = Vector3.one * (maxScale * pulse);
                yield return null;
            }

            t = 0f;
            while (t < closeDuration)
            {
                t += Time.deltaTime;
                float p = 1f - Mathf.Clamp01(t / closeDuration);
                disc.transform.localScale = Vector3.one * (maxScale * p);
                light.intensity = 8f * p;
                yield return null;
            }
            disc.SetActive(false);
            lightObj.SetActive(false);
            #endregion
        }

        void BuildSwirl()
        {
            #region
            // Particles emitted on a ring around the portal, drifting inward (negative start
            // speed = suction toward the center).
            GameObject obj = new GameObject("PortalSwirl");
            obj.transform.SetParent(transform, false);
            obj.transform.rotation = facing;
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(-4f, -1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                portalColor, new Color(0.6f, 0.4f, 1f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 60f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 3.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = CombatExplosionFx.SoftCircleTexture();
            renderer.material = mat;

            ps.Play();
            #endregion
        }
    }
}
