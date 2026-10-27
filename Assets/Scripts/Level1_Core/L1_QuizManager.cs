using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class L1_QuizManager : MonoBehaviour
{
    public static L1_QuizManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<L1_QuizManager>();
            }
            return instance;
        }
        private set => instance = value;
    }
    private static L1_QuizManager instance;

    [Header("UI Dialog References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtQuestion;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI txtFeedback;
    [SerializeField] private Button btnClose;

    private L1_QuizData currentQuiz;
    private Action onCorrectCallback;
    private Coroutine closeCoroutine;
    private bool isHandlingAnswer = false;

    public bool IsQuizOpen => quizPanel != null && quizPanel.activeSelf;

    private readonly Color colorNormal = new Color(0.31f, 0.20f, 0.18f, 1f); // Brown #4E342E
    private readonly Color colorCorrect = new Color(0.18f, 0.7f, 0.35f, 1f); // Green #4CAF50
    private readonly Color colorWrong = new Color(0.9f, 0.22f, 0.21f, 1f);   // Red #E53935

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

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            if (answerButtons[i] != null)
            {
                answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
            }
        }
    }

    public void OpenQuiz(L1_QuizData data, Action onCorrect)
    {
        if (data == null) return;
        if (IsQuizOpen) return;

        currentQuiz = data;
        onCorrectCallback = onCorrect;
        isHandlingAnswer = false;

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (txtTitle != null)
        {
            txtTitle.text = data.title;
        }

        if (txtQuestion != null)
        {
            txtQuestion.text = data.question;
        }

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

        if (txtFeedback != null)
        {
            txtFeedback.text = "";
        }

        if (L1_PlayerController.Instance != null)
        {
            L1_PlayerController.Instance.SetMovementLocked(true);
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
    }

    private void OnAnswerClicked(int selectedIndex)
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

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null) answerButtons[i].interactable = false;
            }

            onCorrectCallback?.Invoke();
            closeCoroutine = StartCoroutine(AutoCloseRoutine(0.85f));
        }
        else
        {
            if (txtFeedback != null)
            {
                txtFeedback.text = "Chưa chính xác, hãy thử lại!";
                txtFeedback.color = colorWrong;
            }

            if (selectedIndex < answerButtons.Length && answerButtons[selectedIndex] != null)
            {
                var img = answerButtons[selectedIndex].GetComponent<Image>();
                if (img != null) img.color = colorWrong;
            }
        }
    }

    private IEnumerator AutoCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseQuiz();
    }

    public void CloseQuiz()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (L1_PlayerController.Instance != null)
        {
            L1_PlayerController.Instance.SetMovementLocked(false);
        }

        currentQuiz = null;
        onCorrectCallback = null;
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
