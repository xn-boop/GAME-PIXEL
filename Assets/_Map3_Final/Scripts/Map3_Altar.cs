using System.Collections;
using UnityEngine;

namespace Game.Map3
{
    public class Map3_Altar : Map3_Interactable
    {
        [Header("Hình ảnh 2 viên ngọc")]
        [SerializeField] private GameObject jadeVietVisual;
        [SerializeField] private GameObject jadeKoreaVisual;

        [Header("Hiệu ứng phát sáng riêng")]
        [SerializeField] private GameObject glowVietVisual;
        [SerializeField] private GameObject glowKoreaVisual;
        [SerializeField] private ParticleSystem glowParticles; // (Tùy chọn) Hạt bụi bay chung

        [Header("Tên định danh vật phẩm (Item ID)")]
        [SerializeField] private string vietJadeID = "viet_stone";
        [SerializeField] private string koreaJadeID = "korea_stone";

        private bool isActivated = false;

        protected override void Awake()
        {
            objectDisplayName = "Bàn Tế";
            promptText = "[E] Hợp nhất Ngọc Bội";
            base.Awake();
        }

        public override void Interact()
        {
            if (isActivated) return;

            bool hasViet = Map3_InventoryManager.Instance != null && Map3_InventoryManager.Instance.HasItem(vietJadeID);
            bool hasKorea = Map3_InventoryManager.Instance != null && Map3_InventoryManager.Instance.HasItem(koreaJadeID);

            if (hasViet && hasKorea)
            {
                ActivateAltar();
            }
            else
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Cần thu thập đủ 2 mảnh ngọc để khảm vào bệ đá!", 5.0f);
                }
            }
        }

        private void ActivateAltar()
        {
            isActivated = true;

            // Thu hồi vật phẩm
            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.RemoveItem(vietJadeID);
                Map3_InventoryManager.Instance.RemoveItem(koreaJadeID);
            }

            // Hiện ngọc
            if (jadeVietVisual != null) jadeVietVisual.SetActive(true);
            if (jadeKoreaVisual != null) jadeKoreaVisual.SetActive(true);

            // Bật 2 vầng sáng riêng và cho nhấp nháy
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

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Ngọc Bội đã được hợp nhất! Linh khí khu vườn được đánh thức.", 5.0f);
                Map3_UIManager.Instance.SetQuestHint("Nhiệm vụ: Hoàn thành phong ấn!");
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