using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Core.UI
{
    public class QuizUIController : MonoBehaviour
    {
        public static QuizUIController Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject quizPanel;

        [Header("Text Displays")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Option Buttons")]
        [SerializeField] private Button buttonA;
        [SerializeField] private TMP_Text textA;

        [SerializeField] private Button buttonB;
        [SerializeField] private TMP_Text textB;

        [SerializeField] private Button buttonC;
        [SerializeField] private TMP_Text textC;

        [SerializeField] private Button buttonD;
        [SerializeField] private TMP_Text textD;

        [Header("Controls")]
        [SerializeField] private Button closeButton;

        private string currentQuizId;
        private bool isAnsweredCorrectly = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (buttonA != null) buttonA.onClick.AddListener(() => OnOptionSelected(QuizOption.A));
            if (buttonB != null) buttonB.onClick.AddListener(() => OnOptionSelected(QuizOption.B));
            if (buttonC != null) buttonC.onClick.AddListener(() => OnOptionSelected(QuizOption.C));
            if (buttonD != null) buttonD.onClick.AddListener(() => OnOptionSelected(QuizOption.D));
            if (closeButton != null) closeButton.onClick.AddListener(CloseQuiz);
        }

        public void OpenQuiz(string quizId)
        {
            if (QuizManager.Instance == null)
            {
                Debug.LogError("[QuizUIController] Không tìm thấy QuizManager!");
                return;
            }

            var data = QuizManager.Instance.GetQuizData(quizId);
            if (data == null)
            {
                Debug.LogError($"[QuizUIController] Không tìm thấy dữ liệu cho câu đố '{quizId}'");
                return;
            }

            currentQuizId = quizId;
            isAnsweredCorrectly = QuizManager.Instance.IsQuizSolved(quizId);

            if (titleText != null) titleText.text = data.Title;
            if (questionText != null) questionText.text = data.QuestionText;

            if (textA != null) textA.text = "A. " + data.OptionA;
            if (textB != null) textB.text = "B. " + data.OptionB;
            if (textC != null) textC.text = "C. " + data.OptionC;
            if (textD != null) textD.text = "D. " + data.OptionD;

            if (feedbackText != null)
            {
                if (isAnsweredCorrectly)
                {
                    feedbackText.text = "Câu đố này đã hoàn thành: " + data.FeedbackCorrect;
                }
                else
                {
                    feedbackText.text = string.Empty;
                }
            }

            // Khóa điều khiển nhân vật khi mở bảng câu đố
            GameStateManager.Instance?.AcquirePlayerControlLock("Quiz");

            if (quizPanel != null)
            {
                quizPanel.SetActive(true);
            }
        }

        private void OnOptionSelected(QuizOption option)
        {
            if (QuizManager.Instance == null || string.IsNullOrEmpty(currentQuizId)) return;

            var (status, feedback, _) = QuizManager.Instance.SubmitQuizAnswer(currentQuizId, option);

            if (feedbackText != null)
            {
                feedbackText.text = feedback;
            }

            if (status == QuizSubmitStatus.Success || status == QuizSubmitStatus.AlreadySolved)
            {
                isAnsweredCorrectly = true;
                Debug.Log($"[QuizUIController] Hoàn thành câu hỏi {currentQuizId}!");
            }
        }

        public void CloseQuiz()
        {
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            currentQuizId = null;

            // Mở lại điều khiển nhân vật nếu không có cảnh ký ức đang chạy
            GameStateManager.Instance?.ReleasePlayerControlLock("Quiz");
        }
    }
}
