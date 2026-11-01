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

            // 2. Hiện 2 viên ngọc lọt vào bệ đá
            if (jadeVietVisual != null) jadeVietVisual.SetActive(true);
            if (jadeKoreaVisual != null) jadeKoreaVisual.SetActive(true);

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
            if (glowParticles != null) glowParticles.Play();

            // 4. Các hiệu ứng gốc của game
            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.SetInteractPrompt(false);
                Map3_UIManager.Instance.ShowToast("Hai nửa ngọc bội Sen - Mugunghwa tỏa sáng rực rỡ...");
            }

            if (jadeAuraEffect != null) jadeAuraEffect.SetActive(true);
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