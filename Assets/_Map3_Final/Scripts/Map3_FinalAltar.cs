using System.Collections;
using UnityEngine;

namespace Game.Map3
{
    public class Map3_FinalAltar : Map3_Interactable
    {
        [Header("Hiệu ứng gốc (Altar Effects)")]
        [SerializeField] private GameObject jadeAuraEffect;
        [SerializeField] private SpriteRenderer altarRenderer;
        [SerializeField] private Color consecratedColor = new Color(0.2f, 1f, 0.6f, 1f);

        [Header("Hình ảnh 2 viên ngọc trên bệ")]
        [SerializeField] private GameObject jadeVietVisual;
        [SerializeField] private GameObject jadeKoreaVisual;

        [Header("Hiệu ứng phát sáng riêng")]
        [SerializeField] private GameObject glowVietVisual;
        [SerializeField] private GameObject glowKoreaVisual;
        [SerializeField] private ParticleSystem glowParticles;

        [Header("State")]
        [SerializeField] private bool isCompleted = false;

        public override bool CanInteract => !isCompleted;

        protected override void Awake()
        {
            objectDisplayName = "Đàn Tế Ngọc Bội";
            promptText = "[E] Cử Hành Nghi Thức";
            base.Awake();

            FixVisualTransforms();

            if (jadeAuraEffect != null) jadeAuraEffect.SetActive(false);
            if (jadeVietVisual != null) jadeVietVisual.SetActive(false);
            if (jadeKoreaVisual != null) jadeKoreaVisual.SetActive(false);
            if (glowVietVisual != null) glowVietVisual.SetActive(false);
            if (glowKoreaVisual != null) glowKoreaVisual.SetActive(false);

            if (altarRenderer == null)
            {
                altarRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Tự động cấu hình chuẩn hiển thị 2D cho ngọc, đốm sáng, aura và hạt hiệu ứng:
        /// - Ép toàn bộ localPosition.z về 0f.
        /// - jadeVietVisual & jadeKoreaVisual: sortingLayerName = "Foreground", sortingOrder = 60.
        /// - glowVietVisual & glowKoreaVisual: sortingLayerName = "Foreground", sortingOrder = 55, alpha = 0.6f.
        /// - jadeAuraEffect: localPosition.z = 0f, Renderer = 55, ParticleSystemRenderer = 58.
        /// - Các ParticleSystem trong glow/aura: sortingLayerName = "Foreground", sortingOrder = 58.
        /// - glowParticles: localPosition = Vector3.zero, sortingLayerName = "Foreground", sortingOrder = 100.
        /// </summary>
        public void FixVisualTransforms()
        {
            ConfigureJadeVisual(jadeVietVisual);
            ConfigureJadeVisual(jadeKoreaVisual);
            ConfigureGlowVisual(glowVietVisual);
            ConfigureGlowVisual(glowKoreaVisual);
            ConfigureJadeAura();
            ConfigureGlowParticles();
        }

        private void ConfigureJadeVisual(GameObject jadeGo)
        {
            if (jadeGo == null) return;

            Vector3 pos = jadeGo.transform.localPosition;
            pos.z = 0f;
            jadeGo.transform.localPosition = pos;

            SpriteRenderer[] renderers = jadeGo.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    sr.sortingLayerName = "Foreground";
                    sr.sortingOrder = 60;
                }
            }
        }

        private void ConfigureGlowVisual(GameObject glowGo)
        {
            if (glowGo == null) return;

            Vector3 pos = glowGo.transform.localPosition;
            pos.z = 0f;
            glowGo.transform.localPosition = pos;

            Renderer[] renderers = glowGo.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                if (r is ParticleSystemRenderer psr)
                {
                    psr.sortingLayerName = "Foreground";
                    psr.sortingOrder = 58;
                }
                else if (r is SpriteRenderer sr)
                {
                    sr.sortingLayerName = "Foreground";
                    sr.sortingOrder = 55;
                    Color c = sr.color;
                    c.a = 0.6f;
                    sr.color = c;
                }
                else
                {
                    r.sortingLayerName = "Foreground";
                    r.sortingOrder = 55;
                }
            }
        }

        private void ConfigureJadeAura()
        {
            if (jadeAuraEffect == null) return;

            Vector3 pos = jadeAuraEffect.transform.localPosition;
            pos.z = 0f;
            jadeAuraEffect.transform.localPosition = pos;

            Renderer[] renderers = jadeAuraEffect.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                if (r is ParticleSystemRenderer psr)
                {
                    psr.sortingLayerName = "Foreground";
                    psr.sortingOrder = 58;
                }
                else
                {
                    r.sortingLayerName = "Foreground";
                    r.sortingOrder = 55;
                }
            }
        }

        private void ConfigureGlowParticles()
        {
            if (glowParticles == null) return;

            glowParticles.transform.localPosition = Vector3.zero;
            var psRenderer = glowParticles.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sortingLayerName = "Foreground";
                psRenderer.sortingOrder = 100;
            }
        }

        public override void Interact()
        {
            if (isCompleted) return;

            // Sử dụng Map3_ItemIds gốc của game
            bool hasVietJade = Map3_InventoryManager.Instance != null && Map3_InventoryManager.Instance.HasItem(Map3_ItemIds.VietJade);
            bool hasKoreaJade = Map3_InventoryManager.Instance != null && Map3_InventoryManager.Instance.HasItem(Map3_ItemIds.KoreaJade);

            if (!hasVietJade || !hasKoreaJade)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Trên mặt bệ đá cổ có 2 rãnh khuyết. Bạn cần tìm đủ cả Nửa Ngọc Việt và Nửa Ngọc Hàn để cử hành nghi thức hợp nhất.");
                }
                return;
            }

            StartCoroutine(UnifyJadesRoutine());
        }

        private IEnumerator UnifyJadesRoutine()
        {
            isCompleted = true;

            // 1. Khóa nhân vật & Thu hồi vật phẩm
            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.RemoveItem(Map3_ItemIds.VietJade);
                Map3_InventoryManager.Instance.RemoveItem(Map3_ItemIds.KoreaJade);
            }

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(true);
            }

            // 2. Hiện 2 viên ngọc lọt vào bệ đá và kích hoạt vòng sáng tròn quanh bệ đá
            FixVisualTransforms();
            if (jadeVietVisual != null) jadeVietVisual.SetActive(true);
            if (jadeKoreaVisual != null) jadeKoreaVisual.SetActive(true);
            if (jadeAuraEffect != null) jadeAuraEffect.SetActive(true);

            // 3. Bật vầng sáng riêng và nhấp nháy
            if (glowVietVisual != null)
            {
                glowVietVisual.SetActive(true);
                StartCoroutine(PulseGlowEffect(glowVietVisual.transform));
            }
            if (glowKoreaVisual != null)
            {
                glowKoreaVisual.SetActive(true);
                StartCoroutine(PulseGlowEffect(glowKoreaVisual.transform));
            }
            if (glowParticles != null)
            {
                glowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                glowParticles.Play();
            }

            // 4. Các hiệu ứng gốc của game
            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.SetInteractPrompt(false);
                Map3_UIManager.Instance.ShowToast("Hai nửa ngọc bội Sen - Mugunghwa tỏa sáng rực rỡ...");
            }

            if (altarRenderer != null) altarRenderer.color = consecratedColor;

            ShowIndicator(false);

            // 5. Chờ 1.5 giây để người chơi ngắm hiệu ứng
            yield return new WaitForSeconds(1.5f);

            // 6. Hiện bảng Victory
            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowVictory(
                    "HOÀN THÀNH MÀN CUỐI: KHU VƯỜN GIAO THOA!",
                    "Chúc mừng bạn đã hợp nhất thành công hai nửa Ngọc Bội Việt - Hàn! Ký ức gia đình và tình hữu nghị hai dân tộc đã được gắn kết trọn vẹn!"
                );
            }
        }

        private IEnumerator PulseGlowEffect(Transform glowTransform)
        {
            Vector3 baseScale = glowTransform.localScale;
            while (true)
            {
                float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.08f;
                glowTransform.localScale = baseScale * scale;
                yield return null;
            }
        }
    }
}