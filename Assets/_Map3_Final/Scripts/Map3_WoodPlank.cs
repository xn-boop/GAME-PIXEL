using UnityEngine;

namespace Game.Map3
{
    public class Map3_WoodPlank : Map3_Interactable
    {
        [Header("Wood Spot Config")]
        [Range(1, 3)]
        [SerializeField] private int spotIndex = 1;
        [SerializeField] private bool isCollected = false;

        public override bool CanInteract => !isCollected;

        protected override void Awake()
        {
            objectDisplayName = $"Mảnh Gỗ {spotIndex}";
            promptText = "[E] Nhặt Mảnh Gỗ";
            base.Awake();
        }

        public void SetSpotIndex(int index)
        {
            spotIndex = Mathf.Clamp(index, 1, 3);
            objectDisplayName = $"Mảnh Gỗ {spotIndex}";
        }

        public override void Interact()
        {
            if (isCollected) return;

            if (Map3_GameManager.Instance != null && !Map3_GameManager.Instance.hasTalkedToCat)
            {
                Debug.Log("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!");
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!", 5.0f);
                }
                return;
            }

            Map3_QuizData quiz = null;
            switch (spotIndex)
            {
                case 1: quiz = Map3_QuizDatabase.Quiz_Wood1; break;
                case 2: quiz = Map3_QuizDatabase.Quiz_Wood2; break;
                case 3: quiz = Map3_QuizDatabase.Quiz_Wood3; break;
                default: quiz = Map3_QuizDatabase.Quiz_Wood1; break;
            }

            if (Map3_QuizManager.Instance != null && quiz != null)
            {
                Map3_QuizManager.Instance.OpenQuiz(quiz, OnQuizCorrect);
            }
        }

        private void OnQuizCorrect()
        {
            if (isCollected) return;

            isCollected = true;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.AddItem(Map3_ItemIds.WoodPlank);
                int count = Map3_InventoryManager.Instance.GetItemCount(Map3_ItemIds.WoodPlank);

                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast($"Đã thu thập Mảnh gỗ ({count}/3)! Thu thập đủ 3 mảnh để sửa cầu sang bờ Hàn.");
                    Map3_UIManager.Instance.SetInteractPrompt(false);
                }
            }

            ShowIndicator(false);
            gameObject.SetActive(false);
        }
    }
}
