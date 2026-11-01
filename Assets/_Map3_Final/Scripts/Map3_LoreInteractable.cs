using UnityEngine;

namespace Game.Map3
{
    public class Map3_LoreInteractable : Map3_Interactable
    {
        [Header("Lore Quiz Configuration")]
        [SerializeField] private string quizId;

        public string QuizId
        {
            get => quizId;
            set => quizId = value;
        }

        public void Setup(string id, string displayName, string prompt)
        {
            quizId = id;
            objectDisplayName = displayName;
            promptText = prompt;
        }

        public override void Interact()
        {
            if (Map3_QuizManager.Instance == null) return;

            var quiz = Map3_QuizDatabase.GetById(quizId);
            if (quiz != null)
            {
                // Mở câu hỏi lore (tính năng phụ, không có callback trao thưởng vật phẩm, trả lời xong tự động đóng bảng sau 2.5s)
                Map3_QuizManager.Instance.OpenQuiz(quiz, null);
            }
        }
    }
}
