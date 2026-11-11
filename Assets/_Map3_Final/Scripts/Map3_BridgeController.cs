using System.Collections;
using UnityEngine;

namespace Game.Map3
{
    public class Map3_BridgeController : Map3_Interactable
    {
        [Header("Bridge Renderers")]
        [SerializeField] private SpriteRenderer brokenBridgeRenderer;
        [SerializeField] private SpriteRenderer fixedBridgeRenderer;

        [Header("Foreground Layers (Tán cây / Mái nhà)")]
        [SerializeField] private SpriteRenderer brokenLayerRenderer; // Gán Map3_Broken layer
        [SerializeField] private SpriteRenderer fixedLayerRenderer;  // Gán Map3_Fixed layer

        [Header("Collision & Passage")]
        [SerializeField] private Collider2D waterBlockCollider;

        [Header("Effects & Audio")]
        [SerializeField] private ParticleSystem woodDustParticles;
        [SerializeField] private ParticleSystem cherryBlossomParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hammerClip;

        [Header("State")]
        [SerializeField] private bool isBridgeRepaired = false;

        public bool IsBridgeRepaired => isBridgeRepaired;
        public override bool CanInteract => !isBridgeRepaired;

        public SpriteRenderer BrokenBridgeRenderer => brokenBridgeRenderer;
        public SpriteRenderer FixedBridgeRenderer => fixedBridgeRenderer;
        public SpriteRenderer BrokenLayerRenderer => brokenLayerRenderer;
        public SpriteRenderer FixedLayerRenderer => fixedLayerRenderer;

        protected override void Awake()
        {
            objectDisplayName = "Cây Cầu Gãy";
            promptText = "[E] Sửa Cầu Gỗ";
            base.Awake();

            // Auto-resolve foreground layers if not assigned in Inspector
            if (brokenLayerRenderer == null)
            {
                var go = GameObject.Find("Map3_Broken layer");
                if (go != null) brokenLayerRenderer = go.GetComponent<SpriteRenderer>();
            }

            if (fixedLayerRenderer == null)
            {
                var go = GameObject.Find("Map3_Fixed layer");
                if (go != null) fixedLayerRenderer = go.GetComponent<SpriteRenderer>();
            }

            if (brokenBridgeRenderer != null)
            {
                Color c = brokenBridgeRenderer.color;
                c.a = 1f;
                brokenBridgeRenderer.color = c;
            }

            if (fixedBridgeRenderer != null)
            {
                Color c = fixedBridgeRenderer.color;
                c.a = 0f;
                fixedBridgeRenderer.color = c;
            }

            if (brokenLayerRenderer != null)
            {
                Color c = brokenLayerRenderer.color;
                c.a = 1f;
                brokenLayerRenderer.color = c;
            }

            if (fixedLayerRenderer != null)
            {
                Color c = fixedLayerRenderer.color;
                c.a = 0f;
                fixedLayerRenderer.color = c;
            }

            if (waterBlockCollider != null)
            {
                waterBlockCollider.enabled = true;
            }

            if (cherryBlossomParticles != null)
            {
                cherryBlossomParticles.Stop();
            }
        }

        public override void Interact()
        {
            if (isBridgeRepaired)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Cây cầu đã lành lặn, nối liền hai bờ Việt - Hàn.");
                }
                return;
            }

            int count = Map3_InventoryManager.Instance != null ? Map3_InventoryManager.Instance.GetItemCount(Map3_ItemIds.WoodPlank) : 0;

            if (count < 3)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast($"Cây cầu bị gãy nhịp. Bạn cần thu thập đủ 3 mảnh gỗ (Hiện có: {count}/3) để sửa cầu.");
                }
                return;
            }

            // Has 3 planks -> repair!
            StartCoroutine(RepairBridgeRoutine());
        }

        private IEnumerator RepairBridgeRoutine()
        {
            isBridgeRepaired = true;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.RemoveItem(Map3_ItemIds.WoodPlank, 3);
            }

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(true);
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.SetInteractPrompt(false);
                Map3_UIManager.Instance.ShowToast("Đang sửa chữa cây cầu...");
            }

            if (woodDustParticles != null)
            {
                woodDustParticles.Play();
            }

            if (audioSource != null && hammerClip != null)
            {
                audioSource.PlayOneShot(hammerClip);
            }

            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (brokenBridgeRenderer != null)
                {
                    Color cb = brokenBridgeRenderer.color;
                    cb.a = 1f - t;
                    brokenBridgeRenderer.color = cb;
                }

                if (fixedBridgeRenderer != null)
                {
                    Color cf = fixedBridgeRenderer.color;
                    cf.a = t;
                    fixedBridgeRenderer.color = cf;
                }

                if (brokenLayerRenderer != null)
                {
                    Color cb = brokenLayerRenderer.color;
                    cb.a = 1f - t;
                    brokenLayerRenderer.color = cb;
                }

                if (fixedLayerRenderer != null)
                {
                    Color cf = fixedLayerRenderer.color;
                    cf.a = t;
                    fixedLayerRenderer.color = cf;
                }

                yield return null;
            }

            if (brokenBridgeRenderer != null)
            {
                Color cb = brokenBridgeRenderer.color;
                cb.a = 0f;
                brokenBridgeRenderer.color = cb;
            }

            if (fixedBridgeRenderer != null)
            {
                Color cf = fixedBridgeRenderer.color;
                cf.a = 1f;
                fixedBridgeRenderer.color = cf;
            }

            if (brokenLayerRenderer != null)
            {
                Color cb = brokenLayerRenderer.color;
                cb.a = 0f;
                brokenLayerRenderer.color = cb;
            }

            if (fixedLayerRenderer != null)
            {
                Color cf = fixedLayerRenderer.color;
                cf.a = 1f;
                fixedLayerRenderer.color = cf;
            }

            // Open water block collider
            if (waterBlockCollider != null)
            {
                waterBlockCollider.enabled = false;
            }

            // Activate cherry blossoms
            if (cherryBlossomParticles != null)
            {
                cherryBlossomParticles.Play();
            }

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(false);
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Cây cầu đã được sửa xong! Bờ Hàn Quốc bừng sáng sắc hồng hoa anh đào.");
                Map3_UIManager.Instance.SetQuestHint("Nhiệm vụ: Sang bờ Hàn Quốc, tìm Viên Đá Cầu Nguyện tại bãi đá.");
            }

            ShowIndicator(false);
        }
    }
}
