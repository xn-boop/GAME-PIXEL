using UnityEngine;

namespace Game.Map3
{
    public class Map3_RockPile : Map3_Interactable
    {
        [Header("Rock Pile State")]
        [SerializeField] private bool hasCollected = false;

        public override bool CanInteract => !hasCollected;

        protected override void Awake()
        {
            objectDisplayName = "Bãi Đá Cuội";
            promptText = "[E] Nhặt Đá Cầu Nguyện";
            base.Awake();
        }

        public override void Interact()
        {
            if (hasCollected)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Bãi đá cuội bình yên bên gốc hoa anh đào.");
                }
                return;
            }

            if (Map3_GameManager.Instance != null && !Map3_GameManager.Instance.hasTalkedToCat)
            {
                Debug.Log("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!");
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!", 3.0f);
                }
                return;
            }

            if (Map3_QuizManager.Instance != null)
            {
                Map3_QuizManager.Instance.OpenQuiz(Map3_QuizDatabase.Quiz_Stone, OnQuizCorrect);
            }
        }

        private void OnQuizCorrect()
        {
            if (hasCollected) return;

            hasCollected = true;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.AddItem(Map3_ItemIds.PrayerStone);
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Đã nhặt được Viên Đá Cầu Nguyện! Hãy mang tới Tháp đá Doltap trong Đền Hàn.");
                Map3_UIManager.Instance.SetInteractPrompt(false);
            }

            ShowIndicator(false);
        }
    }
}
