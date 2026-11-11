using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class L2_QuizManager : MonoBehaviour
{
    public static L2_QuizManager Instance { get; private set; }

    [Header("UI Dialog References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtQuestion;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI txtFeedback;
    [SerializeField] private Button btnClose;

    [Header("Illustration References")]
    [SerializeField] private GameObject illustrationContainer;
    [SerializeField] private Image imgIllustration;
    [SerializeField] private TextMeshProUGUI txtIllustrationName;

    [Header("Item Sprites (Fallback / Direct Mapping)")]
    [SerializeField] private Sprite aoDaiSprite;
    [SerializeField] private Sprite nonLaSprite;
    [SerializeField] private Sprite hanbokSprite;
    [SerializeField] private Sprite muGatSprite;
    [SerializeField] private Sprite catSprite;

    private L2_QuizData currentQuiz;
    private Action onCorrectCallback;
    private Coroutine closeCoroutine;
    private bool isHandlingAnswer = false;

    public bool IsQuizOpen => quizPanel != null && quizPanel.activeSelf;
    public GameObject QuizPanel { get => quizPanel; set => quizPanel = value; }
    public GameObject IllustrationContainer => illustrationContainer;
    public Image IllustrationImage => imgIllustration;
    public TextMeshProUGUI IllustrationNameText => txtIllustrationName;

    public Sprite AoDaiSprite { get => aoDaiSprite; set => aoDaiSprite = value; }
    public Sprite NonLaSprite { get => nonLaSprite; set => nonLaSprite = value; }
    public Sprite HanbokSprite { get => hanbokSprite; set => hanbokSprite = value; }
    public Sprite MuGatSprite { get => muGatSprite; set => muGatSprite = value; }
    public Sprite CatSprite { get => catSprite; set => catSprite = value; }

    private readonly Color colorNormal = new Color(0.31f, 0.20f, 0.18f, 1f); // Brown #4E342E
    private readonly Color colorCorrect = new Color(0.18f, 0.7f, 0.35f, 1f); // Green #4CAF50
    private readonly Color colorWrong = new Color(0.9f, 0.22f, 0.21f, 1f); // Red #E53935

    private void Awake()
    {
#if UNITY_EDITOR
        Instance = this;
#else
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
#endif

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

    public void SetQuizPanel(GameObject panel)
    {
        quizPanel = panel;
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
    }

    public void SetIllustrationReferences(GameObject container, Image image, TextMeshProUGUI nameText)
    {
        illustrationContainer = container;
        imgIllustration = image;
        txtIllustrationName = nameText;
    }

    public void SetItemSprites(Sprite aoDai, Sprite nonLa, Sprite hanbok, Sprite muGat, Sprite cat = null)
    {
        aoDaiSprite = aoDai;
        nonLaSprite = nonLa;
        hanbokSprite = hanbok;
        muGatSprite = muGat;
        catSprite = cat;
    }

    public void ResolveIllustration(L2_QuizData data, out Sprite sprite, out string displayName)
    {
        sprite = null;
        displayName = string.Empty;
        if (data == null) return;

        if (data.illustration != null)
        {
            sprite = data.illustration;
        }

        string idUpper = (data.id ?? "").ToUpperInvariant();
        string rewardUpper = (data.rewardItemId ?? "").ToUpperInvariant();

        if (idUpper.Contains("AODAI") || rewardUpper.Contains("AODAI"))
        {
            if (sprite == null)
            {
                sprite = aoDaiSprite != null ? aoDaiSprite : L2_InventoryManager.Instance?.GetItemSprite(L2_ItemType.AoDai);
            }
            displayName = "ÁO DÀI";
        }
        else if (idUpper.Contains("HANBOK") || rewardUpper.Contains("HANBOK"))
        {
            if (sprite == null)
            {
                sprite = hanbokSprite != null ? hanbokSprite : L2_InventoryManager.Instance?.GetItemSprite(L2_ItemType.Hanbok);
            }
            displayName = "HANBOK";
        }
        else if (idUpper.Contains("NONLA") || rewardUpper.Contains("NONLA"))
        {
            if (sprite == null)
            {
                sprite = nonLaSprite != null ? nonLaSprite : L2_InventoryManager.Instance?.GetItemSprite(L2_ItemType.NonLa);
            }
            displayName = "NÓN LÁ";
        }
        else if (idUpper.Contains("MUGAT") || rewardUpper.Contains("MUGAT"))
        {
            if (sprite == null)
            {
                sprite = muGatSprite != null ? muGatSprite : L2_InventoryManager.Instance?.GetItemSprite(L2_ItemType.MuGat);
            }
            displayName = "MŨ GAT";
        }
        else if (idUpper.Contains("CAT") || idUpper == "NPC_CAT")
        {
            if (sprite == null)
            {
                sprite = catSprite;
            }
            displayName = "CHÚ MÈO";
        }
        else if (idUpper.StartsWith("PAINTING_"))
        {
            if (sprite == null)
            {
                sprite = catSprite;
            }
            displayName = "KÝ ỨC";
        }
    }


    public void OpenQuiz(L2_QuizData data, Action onCorrect)
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
            txtTitle.color = new Color(1f, 0.835f, 0.31f, 1f); // #FFD54F Bright gold
        }

        if (txtQuestion != null)
        {
            txtQuestion.text = data.question;
        }

        // Set Illustration
        ResolveIllustration(data, out Sprite illustrationSprite, out string displayName);

        if (imgIllustration != null)
        {
            if (illustrationSprite != null)
            {
                imgIllustration.sprite = illustrationSprite;
                imgIllustration.color = Color.white;
                imgIllustration.preserveAspect = true;
                imgIllustration.enabled = true;
            }
            else
            {
                imgIllustration.sprite = null;
                imgIllustration.color = new Color(1f, 1f, 1f, 0.05f);
            }
        }

        if (txtIllustrationName != null)
        {
            txtIllustrationName.text = displayName;
            txtIllustrationName.gameObject.SetActive(!string.IsNullOrEmpty(displayName));
        }

        if (illustrationContainer != null)
        {
            illustrationContainer.SetActive(true);
        }

        string[] prefixes = { "A. ", "B. ", "C. ", "D. " };
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < data.options.Length)
            {
                if (answerTexts[i] != null)
                {
                    answerTexts[i].text = prefixes[i] + data.options[i];
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

        if (L2_PlayerController.Instance != null)
        {
            L2_PlayerController.Instance.SetMovementLocked(true);
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
            // Correct answer
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

            // Disable all answer buttons while waiting
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null) answerButtons[i].interactable = false;
            }

            onCorrectCallback?.Invoke();
            closeCoroutine = StartCoroutine(AutoCloseRoutine(0.8f));
        }
        else
        {
            // Wrong answer
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

        if (L2_PlayerController.Instance != null)
        {
            L2_PlayerController.Instance.SetMovementLocked(false);
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
