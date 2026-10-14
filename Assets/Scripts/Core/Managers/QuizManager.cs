using System;
using System.Collections.Generic;
using UnityEngine;
using JourneyOfMemories.Core.Data;

namespace JourneyOfMemories.Core.Managers
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [SerializeField]
        private List<string> solvedQuizIds = new List<string>();

        private Dictionary<string, QuizData> quizDatabase;

        public event Action<string, string> OnQuizSolved; // quizId, rewardItemId
        public event Action<string> OnQuizAnswerWrong;

        public IReadOnlyList<string> SolvedQuizIds => solvedQuizIds.AsReadOnly();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Nạp cơ sở dữ liệu câu đố mặc định
            quizDatabase = new Dictionary<string, QuizData>(DefaultQuizDatabase.Quizzes);
        }

        public bool IsQuizSolved(string quizId)
        {
            if (string.IsNullOrEmpty(quizId)) return false;
            return solvedQuizIds.Contains(quizId);
        }

        public QuizData GetQuizData(string quizId)
        {
            if (string.IsNullOrEmpty(quizId)) return null;
            if (quizDatabase.TryGetValue(quizId, out var data))
            {
                return data;
            }
            return null;
        }

        /// <summary>
        /// API Hợp đồng: SubmitQuizAnswer
        /// </summary>
        public (bool isCorrect, string feedback) SubmitQuizAnswer(string quizId, QuizOption selectedOption)
        {
            if (string.IsNullOrEmpty(quizId))
            {
                return (false, "Lỗi: Không tìm thấy mã câu hỏi!");
            }

            var quiz = GetQuizData(quizId);
            if (quiz == null)
            {
                return (false, $"Lỗi: Câu hỏi '{quizId}' không tồn tại trong hệ thống.");
            }

            // Kiểm tra trạng thái đã giải chưa
            if (IsQuizSolved(quizId))
            {
                return (true, "Câu hỏi này đã được hoàn thành trước đó.");
            }

            // Nhánh Sai
            if (selectedOption != quiz.CorrectOption)
            {
                Debug.Log($"[QuizManager] Trả lời sai cho {quizId}. Người chơi chọn: {selectedOption}, Đáp án đúng: {quiz.CorrectOption}");
                OnQuizAnswerWrong?.Invoke(quizId);
                return (false, quiz.HintWrong);
            }

            // Nhánh Đúng
            solvedQuizIds.Add(quizId);
            Debug.Log($"[QuizManager] Trả lời đúng cho {quizId}!");

            // Cấp thưởng vật phẩm vào Inventory
            if (!string.IsNullOrEmpty(quiz.RewardItemId))
            {
                bool added = false;
                if (InventoryManager.Instance != null)
                {
                    added = InventoryManager.Instance.AddItem(quiz.RewardItemId);
                }

                if (!added)
                {
                    Debug.LogWarning($"[QuizManager] Không thể thêm {quiz.RewardItemId} vào túi đồ (có thể túi đã đầy)!");
                }
            }

            OnQuizSolved?.Invoke(quizId, quiz.RewardItemId);
            GameStateManager.Instance?.CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();

            return (true, quiz.FeedbackCorrect);
        }

        public void LoadSolvedQuizzes(IEnumerable<string> loadedIds)
        {
            solvedQuizIds.Clear();
            if (loadedIds != null)
            {
                foreach (var id in loadedIds)
                {
                    if (!string.IsNullOrEmpty(id) && !solvedQuizIds.Contains(id))
                    {
                        solvedQuizIds.Add(id);
                    }
                }
            }
        }
    }
}
