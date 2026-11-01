using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Map3
{
    public class Map3_QuizManager : MonoBehaviour
    {
        public static Map3_QuizManager Instance { get; private set; }

        [Header("UI Dialog References")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtQuestion;
        [SerializeField] private Button[] answerButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
        [SerializeField] private TextMeshProUGUI txtFeedback;
        [SerializeField] private TextMeshProUGUI txtExplanation;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button btnClose;

        [Header("Auto Close Settings")]
        [SerializeField] private float autoCloseDelay = 2.5f;

        private Map3_QuizData currentQuiz;
        private Action currentOnCorrectCallback;
        private Coroutine autoCloseCoroutine;
        private bool isHandlingAnswer = false;

        public bool IsQuizOpen => quizPanel != null && quizPanel.activeSelf;
        public float AutoCloseDelay { get => autoCloseDelay; set => autoCloseDelay = value; }

        // Màu mặc định của nút đáp án (#4E342E - Nâu ấm đồng bộ Map 1 & Map 2)
        private static readonly Color colorNormal = new Color32(78, 52, 46, 255); // Hex #4E342E
        private static readonly Color colorCorrect = new Color(0.18f, 0.70f, 0.35f, 1f); // #4CAF50
        private static readonly Color colorWrong = new Color(0.90f, 0.22f, 0.21f, 1f);   // #E53935

        public Color ColorNormal => colorNormal;

        // Static references to Lore Quizzes
        public static Map3_QuizData Quiz_Lore_BinhPhong => Map3_QuizDatabase.Quiz_Lore_BinhPhong;
        public static Map3_QuizData Quiz_Lore_GomSu => Map3_QuizDatabase.Quiz_Lore_GomSu;
        public static Map3_QuizData Quiz_Lore_DanTranh => Map3_QuizDatabase.Quiz_Lore_DanTranh;
        public static Map3_QuizData Quiz_Lore_DinhDong => Map3_QuizDatabase.Quiz_Lore_DinhDong;

        private Button ActiveContinueBtn => btnContinue != null ? btnContinue : continueButton;

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

            if (btnClose != null)
            {
                btnClose.onClick.AddListener(CloseQuiz);
            }

            if (ActiveContinueBtn != null)
            {
                ActiveContinueBtn.onClick.AddListener(CloseQuiz);
                ActiveContinueBtn.gameObject.SetActive(false);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                if (answerButtons[i] != null)
                {
                    answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
                }
            }

            ResetButtonColors();
        }

        public void DisplayQuestion(Map3_QuizData data, Action onCorrect)
        {
            OpenQuiz(data, onCorrect);
        }

        public void ResetButtonColors()
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    var img = answerButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = colorNormal;
                    }
                }
            }
        }

        public void ResetButtons()
        {
            ResetButtonColors();
        }

        public void OpenQuiz(Map3_QuizData data, Action onCorrect)
        {
            if (data == null) return;
            if (IsQuizOpen) return;

            currentQuiz = data;
            currentOnCorrectCallback = onCorrect;
            isHandlingAnswer = false;

            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }

            if (txtTitle != null) txtTitle.text = data.title;
            if (txtQuestion != null) txtQuestion.text = data.question;

            string[] prefixes = { "A. ", "B. ", "C. ", "D. " };
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < data.options.Length)
                {
                    if (answerTexts[i] != null)
                    {
                        string opt = data.options[i];
                        if (!opt.StartsWith("A. ") && !opt.StartsWith("B. ") && !opt.StartsWith("C. ") && !opt.StartsWith("D. "))
                        {
                            opt = prefixes[i] + opt;
                        }
                        answerTexts[i].text = opt;
                    }
                    if (answerButtons[i] != null)
                    {
                        answerButtons[i].gameObject.SetActive(true);
                        answerButtons[i].interactable = true;
                        var img = answerButtons[i].GetComponent<Image>();
                        if (img != null) img.color = colorNormal;
                    }
                }
                else
                {
                    if (answerButtons[i] != null) answerButtons[i].gameObject.SetActive(false);
                }
            }

            if (txtFeedback != null) txtFeedback.text = "";
            if (txtExplanation != null)
            {
                txtExplanation.text = "";
                txtExplanation.gameObject.SetActive(false);
            }

            // Đảm bảo nút Tiếp tục bị ẩn
            if (ActiveContinueBtn != null)
            {
                ActiveContinueBtn.gameObject.SetActive(false);
            }

            EnablePlayerMovement(false);

            if (quizPanel != null)
            {
                quizPanel.SetActive(true);
            }
        }

        public void OnAnswerClicked(int selectedIndex)
        {
            if (isHandlingAnswer || currentQuiz == null) return;

            if (selectedIndex == currentQuiz.correctIndex)
            {
                isHandlingAnswer = true;

                if (txtFeedback != null)
                {
                    txtFeedback.text = "Chính xác!";
                    txtFeedback.color = colorCorrect;
                }

                if (selectedIndex < answerButtons.Length && answerButtons[selectedIndex] != null)
                {
                    var img = answerButtons[selectedIndex].GetComponent<Image>();
                    if (img != null) img.color = colorCorrect;
                }

                // Khóa tạm thời 4 nút đáp án (để tránh người chơi bấm spam nhiều lần)
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    if (answerButtons[i] != null) answerButtons[i].interactable = false;
                }

                if (txtExplanation != null && !string.IsNullOrEmpty(currentQuiz.explanation))
                {
                    txtExplanation.text = "<b>Giải thích:</b> " + currentQuiz.explanation;
                    txtExplanation.gameObject.SetActive(true);
                }

                // Ẩn / Vô hiệu hóa nút Tiếp tục
                if (ActiveContinueBtn != null)
                {
                    ActiveContinueBtn.gameObject.SetActive(false);
                }

                // Kích hoạt Coroutine tự động đóng
                autoCloseCoroutine = StartCoroutine(AutoCloseRoutine(true));
            }
            else
            {
                // Chọn sai: Đổi màu nút sang đỏ, báo chọn lại và cho phép người chơi suy nghĩ chọn tiếp
                if (txtFeedback != null)
                {
                    txtFeedback.text = "Chưa chính xác, hãy suy nghĩ và chọn lại!";
                    txtFeedback.color = colorWrong;
                }

                if (selectedIndex < answerButtons.Length && answerButtons[selectedIndex] != null)
                {
                    var img = answerButtons[selectedIndex].GetComponent<Image>();
                    if (img != null) img.color = colorWrong;
                }
            }
        }

        private IEnumerator AutoCloseRoutine(bool isCorrect)
        {
            // 1. Chờ vài giây để người chơi đọc giải thích
            yield return new WaitForSeconds(autoCloseDelay);

            // 2. Kích hoạt Callback trao thưởng vật phẩm (nếu có)
            if (isCorrect)
            {
                currentOnCorrectCallback?.Invoke();
                currentOnCorrectCallback = null;
            }

            // 3. Đóng bảng câu hỏi
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            if (ActiveContinueBtn != null)
            {
                ActiveContinueBtn.gameObject.SetActive(false);
            }

            ResetButtonColors();

            // 4. Mở lại quyền di chuyển cho Player
            EnablePlayerMovement(true);

            currentQuiz = null;
            isHandlingAnswer = false;
            autoCloseCoroutine = null;
        }

        public void EnablePlayerMovement(bool enable)
        {
            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(!enable);
            }
        }

        public void CloseQuiz()
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }

            if (isHandlingAnswer)
            {
                currentOnCorrectCallback?.Invoke();
                currentOnCorrectCallback = null;
            }

            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            if (ActiveContinueBtn != null)
            {
                ActiveContinueBtn.gameObject.SetActive(false);
            }

            ResetButtonColors();
            EnablePlayerMovement(true);

            currentQuiz = null;
            currentOnCorrectCallback = null;
            isHandlingAnswer = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
