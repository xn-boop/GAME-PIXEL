using System;
using System.Collections;
using UnityEngine;

namespace Game.Map0
{
    public class Map0_LightingController : MonoBehaviour
    {
        public static Map0_LightingController Instance { get; private set; }

        [Header("Lighting Overlay References")]
        [SerializeField] private SpriteRenderer lightingOverlayRenderer;

        [Header("Sunset Colors")]
        [SerializeField] private Color sunsetOrange = new Color(1.0f, 0.62f, 0.25f, 0.28f);

        [Header("Mystical Purple Colors")]
        [SerializeField] private Color mysticalPurple = new Color(0.35f, 0.12f, 0.55f, 0.42f);

        [Header("Particle Dust")]
        [SerializeField] private ParticleSystem magicDustParticles;

        private Coroutine transitionCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ApplyInitialLighting();
        }

        public void ApplyInitialLighting()
        {
            if (lightingOverlayRenderer != null)
            {
                lightingOverlayRenderer.color = sunsetOrange;
            }

            if (magicDustParticles != null)
            {
                magicDustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void TriggerCapsuleEventSequence(Action onComplete = null)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(FlickerAndShiftRoutine(onComplete));
        }

        private IEnumerator FlickerAndShiftRoutine(Action onComplete)
        {
            if (lightingOverlayRenderer == null)
            {
                if (magicDustParticles != null) magicDustParticles.Play();
                onComplete?.Invoke();
                yield break;
            }

            // Phase 1: Soft mystical flickers (3 pulses)
            for (int i = 0; i < 3; i++)
            {
                // Dip
                lightingOverlayRenderer.color = new Color(sunsetOrange.r, sunsetOrange.g, sunsetOrange.b, sunsetOrange.a * 0.35f);
                yield return new WaitForSeconds(0.08f);

                // Surge
                lightingOverlayRenderer.color = new Color(sunsetOrange.r, sunsetOrange.g, sunsetOrange.b, sunsetOrange.a * 1.5f);
                yield return new WaitForSeconds(0.12f);
            }

            lightingOverlayRenderer.color = sunsetOrange;
            yield return new WaitForSeconds(0.2f);

            // Start magic dust particles during color shift
            if (magicDustParticles != null)
            {
                magicDustParticles.Play();
            }

            // Phase 2: Smooth transition from sunset orange to deep mystical purple over 2.5 seconds
            float duration = 2.5f;
            float elapsed = 0f;
            Color startColor = lightingOverlayRenderer.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                lightingOverlayRenderer.color = Color.Lerp(startColor, mysticalPurple, t);
                yield return null;
            }

            lightingOverlayRenderer.color = mysticalPurple;
            transitionCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
