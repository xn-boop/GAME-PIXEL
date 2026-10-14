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

        private void OnDestroy() { if (Instance == this) Instance = null; }

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
        public (QuizSubmitStatus status, string feedback, string rewardItemId) SubmitQuizAnswer(string quizId, QuizOption selectedOption)
        {
            if (string.IsNullOrEmpty(quizId))
            {
                return (QuizSubmitStatus.InvalidQuiz, "Lỗi: Không tìm thấy mã câu hỏi!", null);
            }

            var quiz = GetQuizData(quizId);
            if (quiz == null)
            {
                return (QuizSubmitStatus.InvalidQuiz, $"Lỗi: Câu hỏi '{quizId}' không tồn tại trong hệ thống.", null);
            }

            if (selectedOption == QuizOption.None)
                return (QuizSubmitStatus.InvalidOption, "Vui lòng chọn một đáp án.", null);

            // Kiểm tra trạng thái đã giải chưa
            if (IsQuizSolved(quizId))
            {
                return (QuizSubmitStatus.AlreadySolved, "Câu hỏi này đã được hoàn thành trước đó.", quiz.RewardItemId);
            }

            // Nhánh Sai
            if (selectedOption != quiz.CorrectOption)
            {
                Debug.Log($"[QuizManager] Trả lời sai cho {quizId}. Người chơi chọn: {selectedOption}, Đáp án đúng: {quiz.CorrectOption}");
                OnQuizAnswerWrong?.Invoke(quizId);
                return (QuizSubmitStatus.Incorrect, quiz.HintWrong, null);
            }

            // Chỉ đánh dấu solved sau khi chắc chắn cấp được phần thưởng.
            if (!string.IsNullOrEmpty(quiz.RewardItemId))
            {
                if (InventoryManager.Instance == null || !InventoryManager.Instance.CanAddItem(quiz.RewardItemId))
                    return (QuizSubmitStatus.InventoryFull, "Túi đồ đã đầy hoặc vật phẩm đã tồn tại. Hãy kiểm tra túi đồ rồi thử lại.", null);

                SaveLoadManager.Instance?.BeginBatch();
                InventoryManager.Instance.AddItem(quiz.RewardItemId);
            }

            solvedQuizIds.Add(quizId);
            Debug.Log($"[QuizManager] Trả lời đúng cho {quizId}!");

            OnQuizSolved?.Invoke(quizId, quiz.RewardItemId);
            GameStateManager.Instance?.CheckObjectives();
            SaveLoadManager.Instance?.RequestAutoSave();
            SaveLoadManager.Instance?.EndBatch(true);

            return (QuizSubmitStatus.Success, quiz.FeedbackCorrect, quiz.RewardItemId);
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

        public void ResetProgress()
        {
            solvedQuizIds.Clear();
        }
    }
}
