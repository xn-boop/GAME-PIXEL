using UnityEngine;

namespace Game.Map3
{
    public class Map3_KeyJar : Map3_Interactable
    {
        [Header("Key Jar State")]
        [SerializeField] private bool hasKey = true;

        public override bool CanInteract => hasKey;

        protected override void Awake()
        {
            objectDisplayName = "Lu Sành Cổ";
            promptText = "[E] Kiểm tra Lu Sành";
            base.Awake();
        }

        public override void Interact()
        {
            if (!hasKey)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Chiếc lu sành đã trống rỗng.");
                }
                return;
            }

            if (Map3_GameManager.Instance != null && !Map3_GameManager.Instance.hasTalkedToCat)
            {
                Debug.Log("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!");
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Hãy nói chuyện với chú Mèo để bắt đầu hành trình!", 5.0f);
                }
                return;
            }

            if (Map3_QuizManager.Instance != null)
            {
                Map3_QuizManager.Instance.OpenQuiz(Map3_QuizDatabase.Quiz_Key, OnQuizCorrect);
            }
        }

        private void OnQuizCorrect()
        {
            if (!hasKey) return;

            hasKey = false;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.AddItem(Map3_ItemIds.Key);
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Bạn đã nhận được Chìa khóa cổ từ Lu sành! Hãy mang vào Đền Việt để mở Rương.");
                Map3_UIManager.Instance.SetInteractPrompt(false);
            }

            ShowIndicator(false);
        }
    }
}
