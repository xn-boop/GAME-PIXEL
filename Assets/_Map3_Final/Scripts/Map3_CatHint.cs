using System;
using UnityEngine;

namespace Game.Map3
{
    public class Map3_CatHint : Map3_Interactable
    {
        [Header("Cat Settings")]
        [SerializeField] private bool hasGivenHint = false;

        [Header("Mũi Tên Chỉ Dẫn Mảnh Gỗ")]
        [SerializeField] private GameObject[] woodIndicators;

        public const string CatHintMessage = "Hãy tìm chiếc chìa khóa cổ quanh các chum sành để mở rương báu trong Đền Việt, đồng thời thu thập đủ 3 mảnh gỗ để nối lại nhịp cầu sang bờ bên kia.";

        public bool HasGivenHint => hasGivenHint;

        protected override void Awake()
        {
            objectDisplayName = "Chú Mèo";
            promptText = "[E] Hỏi Mèo Gợi Ý";
            base.Awake();
        }

        public override void Interact()
        {
            // Nếu đã từng trả lời đúng quiz và nhận gợi ý trước đó -> trực tiếp mở lại hộp thoại gợi ý
            if (hasGivenHint)
            {
                DisplayCatHint();
                return;
            }

            if (Map3_QuizManager.Instance == null)
            {
                DisplayCatHint();
                return;
            }

            Map3_QuizManager.Instance.OpenQuiz(Map3_QuizDatabase.Quiz_CatHint, OnQuizCorrect);
        }

        private void OnQuizCorrect()
        {
            hasGivenHint = true;

            if (Map3_GameManager.Instance != null)
            {
                Map3_GameManager.Instance.hasTalkedToCat = true;
            }

            // Kích hoạt hiển thị toàn bộ mũi tên chỉ vị trí gỗ
            if (woodIndicators != null)
            {
                foreach (GameObject arrow in woodIndicators)
                {
                    if (arrow != null)
                    {
                        arrow.SetActive(true);
                    }
                }
            }

            DisplayCatHint();
        }

        private void DisplayCatHint()
        {
            if (Map3_UIManager.Instance != null)
            {
                Map3_UIManager.Instance.ShowHintDialog("GỢI Ý TỪ CHÚ MÈO", CatHintMessage, () =>
                {
                    Map3_UIManager.Instance.SetQuestHint("Nhiệm vụ: Tìm Chìa khóa cổ & 3 Mảnh gỗ quanh bờ Việt");
                });
            }
        }
    }
}
