using UnityEngine;

namespace Game.Map3
{
    public class Map3_Doltap : Map3_Interactable
    {
        [Header("Doltap State")]
        [SerializeField] private bool isStonePlaced = false;
        [SerializeField] private GameObject topStoneVisual;
        [SerializeField] private SpriteRenderer doltapRenderer;
        [SerializeField] private Color completedColor = new Color(0.95f, 0.9f, 0.8f, 1f);

        public override bool CanInteract => !isStonePlaced;

        protected override void Awake()
        {
            objectDisplayName = "Tháp Đá Doltap";
            promptText = "[E] Đặt Đá Cầu Nguyện";
            base.Awake();

            if (topStoneVisual != null)
            {
                topStoneVisual.SetActive(false);
            }
            if (doltapRenderer == null)
            {
                doltapRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public override void Interact()
        {
            if (isStonePlaced)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Tháp đá Doltap đã hoàn thiện với viên đá cầu nguyện trên đỉnh.");
                }
                return;
            }

            if (Map3_InventoryManager.Instance == null || !Map3_InventoryManager.Instance.HasItem(Map3_ItemIds.PrayerStone))
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Tháp đá cầu nguyện đang khuyết viên đá trên cùng. Hãy tìm một viên đá phù hợp ở bãi đá ngoài sân.");
                }
                return;
            }

            // Has PrayerStone -> open quiz
            if (Map3_QuizManager.Instance != null)
            {
                Map3_QuizManager.Instance.OpenQuiz(Map3_QuizDatabase.Quiz_KoreaJade, OnQuizCorrect);
            }
        }

        private void OnQuizCorrect()
        {
            if (isStonePlaced) return;

            isStonePlaced = true;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.RemoveItem(Map3_ItemIds.PrayerStone);
                Map3_InventoryManager.Instance.AddItem(Map3_ItemIds.KoreaJade);
            }

            if (topStoneVisual != null)
            {
                topStoneVisual.SetActive(true);
            }
            else if (doltapRenderer != null)
            {
                doltapRenderer.color = completedColor;
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Đặt đá cầu nguyện thành công! Nhận được Nửa Ngọc Bội Hàn (Hoa Mugunghwa).");
                Map3_UIManager.Instance.SetQuestHint("Nhiệm vụ: Tiến tới Đàn Tế Ngoài Trời (Góc dưới Bờ Hàn) để hợp nhất Ngọc Bội.");
                Map3_UIManager.Instance.SetInteractPrompt(false);
            }

            ShowIndicator(false);
        }
    }
}
