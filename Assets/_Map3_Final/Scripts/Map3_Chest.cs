using UnityEngine;

namespace Game.Map3
{
    public class Map3_Chest : Map3_Interactable
    {
        [Header("Chest State")]
        [SerializeField] private bool isOpened = false;
        [SerializeField] private SpriteRenderer chestRenderer;
        [SerializeField] private Sprite openedSprite;
        [SerializeField] private Color openedColor = new Color(0.85f, 0.7f, 0.4f, 1f);

        public override bool CanInteract => !isOpened;

        protected override void Awake()
        {
            objectDisplayName = "Rương Gỗ Cổ";
            promptText = "[E] Mở Rương";
            base.Awake();

            if (chestRenderer == null)
            {
                chestRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        public override void Interact()
        {
            if (isOpened)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Rương gỗ đã mở, bên trong trống rỗng.");
                }
                return;
            }

            if (Map3_InventoryManager.Instance == null || !Map3_InventoryManager.Instance.HasItem(Map3_ItemIds.Key))
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.ShowToast("Chiếc rương gỗ đang khóa chặt. Bạn cần tìm Chìa khóa cổ ở khu vườn bên ngoài.");
                }
                return;
            }

            // Has key -> open quiz
            if (Map3_QuizManager.Instance != null)
            {
                Map3_QuizManager.Instance.OpenQuiz(Map3_QuizDatabase.Quiz_VietJade, OnQuizCorrect);
            }
        }

        private void OnQuizCorrect()
        {
            if (isOpened) return;

            isOpened = true;

            if (Map3_InventoryManager.Instance != null)
            {
                Map3_InventoryManager.Instance.RemoveItem(Map3_ItemIds.Key);
                Map3_InventoryManager.Instance.AddItem(Map3_ItemIds.VietJade);
            }

            if (chestRenderer != null)
            {
                if (openedSprite != null)
                {
                    chestRenderer.sprite = openedSprite;
                }
                else
                {
                    chestRenderer.color = openedColor;
                }
            }

            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowToast("Mở rương thành công! Nhận được Nửa Ngọc Bội Việt (Hoa Sen).");
                Map3_UIManager.Instance.SetInteractPrompt(false);
            }

            ShowIndicator(false);
        }
    }
}
