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

    [Header("Illustration References")]
    [SerializeField] private GameObject illustrationContainer;
    [SerializeField] private Image imgIllustration;
    [SerializeField] private TextMeshProUGUI txtIllustrationName;

    [Header("Item Sprites (Fallback / Direct Mapping)")]
    [SerializeField] private Sprite thitKhoSprite;
    [SerializeField] private Sprite kimchiSprite;
    [SerializeField] private Sprite kimbapSprite;
    [SerializeField] private Sprite flagVnSprite;
    [SerializeField] private Sprite flagKrSprite;
    [SerializeField] private Sprite banhMiSprite;
    [SerializeField] private Sprite tteokbokkiSprite;
    [SerializeField] private Sprite catSprite;

    private L1_QuizData currentQuiz;
    private Action onCorrectCallback;
    private Coroutine closeCoroutine;
    private bool isHandlingAnswer = false;

    public bool IsQuizOpen => quizPanel != null && quizPanel.activeSelf;
    public GameObject QuizPanel { get => quizPanel; set => quizPanel = value; }
    public GameObject IllustrationContainer => illustrationContainer;
    public Image IllustrationImage => imgIllustration;
    public TextMeshProUGUI IllustrationNameText => txtIllustrationName;

    public Sprite ThitKhoSprite { get => thitKhoSprite; set => thitKhoSprite = value; }
    public Sprite KimchiSprite { get => kimchiSprite; set => kimchiSprite = value; }
    public Sprite KimbapSprite { get => kimbapSprite; set => kimbapSprite = value; }
    public Sprite FlagVnSprite { get => flagVnSprite; set => flagVnSprite = value; }
    public Sprite FlagKrSprite { get => flagKrSprite; set => flagKrSprite = value; }
    public Sprite BanhMiSprite { get => banhMiSprite; set => banhMiSprite = value; }
    public Sprite TteokbokkiSprite { get => tteokbokkiSprite; set => tteokbokkiSprite = value; }
    public Sprite CatSprite { get => catSprite; set => catSprite = value; }

    private readonly Color colorNormal = new Color(0.31f, 0.20f, 0.18f, 1f); // Brown #4E342E
    private readonly Color colorCorrect = new Color(0.18f, 0.7f, 0.35f, 1f); // Green #4CAF50
    private readonly Color colorWrong = new Color(0.9f, 0.22f, 0.21f, 1f);   // Red #E53935

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

        ConfigureAllTexts();

        // Fallback item sprites from inventory manager if not wired
        if (thitKhoSprite == null && L1_InventoryManager.Instance != null)
            thitKhoSprite = L1_InventoryManager.Instance.thitKhoIcon;
        if (kimchiSprite == null && L1_InventoryManager.Instance != null)
            kimchiSprite = L1_InventoryManager.Instance.kimchiIcon;
        if (kimbapSprite == null && L1_InventoryManager.Instance != null)
            kimbapSprite = L1_InventoryManager.Instance.kimbapIcon;
    }

    public void ConfigureAllTexts()
    {
        if (txtQuestion != null)
        {
            ConfigureQuestionText(txtQuestion);
        }

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (answerTexts[i] != null)
            {
                ConfigureAnswerText(answerTexts[i]);
            }
        }
    }

    public static void ConfigureAnswerText(TextMeshProUGUI txt)
    {
        if (txt == null) return;
        txt.textWrappingMode = TextWrappingModes.Normal;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 10f;
        txt.fontSizeMax = 17f;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.margin = new Vector4(16f, 3f, 16f, 3f);
        txt.overflowMode = TextOverflowModes.Ellipsis;

        RectTransform rt = txt.rectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }
    }

    public static void ConfigureQuestionText(TextMeshProUGUI txt)
    {
        if (txt == null) return;
        txt.textWrappingMode = TextWrappingModes.Normal;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 13f;
        txt.fontSizeMax = 18f;
        txt.alignment = TextAlignmentOptions.TopLeft;
        txt.margin = new Vector4(6f, 2f, 6f, 2f);
        txt.overflowMode = TextOverflowModes.Ellipsis;
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

    public void SetItemSprites(Sprite thitKho, Sprite kimchi, Sprite kimbap, Sprite flagVn, Sprite flagKr, Sprite banhMi, Sprite tteokbokki, Sprite cat)
    {
        thitKhoSprite = thitKho;
        kimchiSprite = kimchi;
        kimbapSprite = kimbap;
        flagVnSprite = flagVn;
        flagKrSprite = flagKr;
        banhMiSprite = banhMi;
        tteokbokkiSprite = tteokbokki;
        catSprite = cat;
    }

    public void ResolveIllustration(L1_QuizData data, out Sprite sprite, out string displayName)
    {
        sprite = null;
        displayName = string.Empty;
        if (data == null) return;

        if (data.illustration != null)
        {
            sprite = data.illustration;
        }

        string idUpper = (data.id ?? "").ToUpperInvariant();
        string titleUpper = (data.title ?? "").ToUpperInvariant();
        string rewardUpper = (data.rewardItemId ?? "").ToUpperInvariant();

        if (idUpper.Contains("THITKHO") || rewardUpper.Contains("THITKHO") || idUpper.Contains("THIT") || titleUpper.Contains("THỊT KHO"))
        {
            if (sprite == null)
            {
                sprite = thitKhoSprite != null ? thitKhoSprite : L1_InventoryManager.Instance?.GetItemSprite(L1_ItemType.ThitKho);
            }
            displayName = "THỊT KHO TRỨNG";
        }
        else if (idUpper.Contains("KIMCHI") || rewardUpper.Contains("KIMCHI") || titleUpper.Contains("KIM CHI"))
        {
            if (sprite == null)
            {
                sprite = kimchiSprite != null ? kimchiSprite : L1_InventoryManager.Instance?.GetItemSprite(L1_ItemType.Kimchi);
            }
            displayName = "KIM CHI";
        }
        else if (idUpper.Contains("KIMBAP") || rewardUpper.Contains("KIMBAP") || titleUpper.Contains("KIMBAP"))
        {
            if (sprite == null)
            {
                sprite = kimbapSprite != null ? kimbapSprite : L1_InventoryManager.Instance?.GetItemSprite(L1_ItemType.Kimbap);
            }
            displayName = "KIMBAP";
        }
        else if (idUpper.Contains("BANHMI") || idUpper.Contains("BANH_MI") || titleUpper.Contains("BÁNH MÌ"))
        {
            if (sprite == null)
            {
                sprite = banhMiSprite;
            }
            displayName = "BÁNH MÌ";
        }
        else if (idUpper.Contains("TTEOKBOKKI") || idUpper.Contains("TOPOKKI") || titleUpper.Contains("TTEOKBOKKI") || titleUpper.Contains("BÁNH GẠO"))
        {
            if (sprite == null)
            {
                sprite = tteokbokkiSprite;
            }
            displayName = "TTEOKBOKKI";
        }
        else if (idUpper.Contains("FLAGVN") || idUpper.Contains("FLAG_VN") || titleUpper.Contains("QUỐC KỲ VIỆT NAM") || titleUpper.Contains("CỜ ĐỎ SAO VÀNG"))
        {
            if (sprite == null)
            {
                sprite = flagVnSprite;
            }
            displayName = "QUỐC KỲ VIỆT NAM";
        }
        else if (idUpper.Contains("FLAGKR") || idUpper.Contains("FLAG_KR") || titleUpper.Contains("QUỐC KỲ HÀN QUỐC") || titleUpper.Contains("CỜ THÁI CỰC") || titleUpper.Contains("TAEGEUKGI"))
        {
            if (sprite == null)
            {
                sprite = flagKrSprite;
            }
            displayName = "QUỐC KỲ HÀN QUỐC";
        }
        else if (idUpper.Contains("CAT") || idUpper == "NPC_CAT" || titleUpper.Contains("MÈO"))
        {
            if (sprite == null)
            {
                sprite = catSprite;
            }
            displayName = "CHÚ MÈO";
        }
        else
        {
            if (sprite == null)
            {
                sprite = catSprite;
            }
            displayName = "GỢI Ý";
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
            txtTitle.color = new Color(1f, 0.835f, 0.31f, 1f); // #FFD54F Bright gold
        }

        if (txtQuestion != null)
        {
            ConfigureQuestionText(txtQuestion);
            txtQuestion.text = data.question;
            txtQuestion.ForceMeshUpdate();
        }

        // Set Illustration
        ResolveIllustration(data, out Sprite illustrationSprite, out string displayName);

        if (imgIllustration != null)
        {
            imgIllustration.type = Image.Type.Simple;
            imgIllustration.preserveAspect = true;
            if (illustrationSprite != null)
            {
                imgIllustration.sprite = illustrationSprite;
                imgIllustration.color = Color.white;
                imgIllustration.enabled = true;
            }
            else if (catSprite != null)
            {
                imgIllustration.sprite = catSprite;
                imgIllustration.color = Color.white;
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
                    ConfigureAnswerText(answerTexts[i]);
                    string opt = data.options[i];
                    if (!opt.StartsWith("A. ") && !opt.StartsWith("B. ") && !opt.StartsWith("C. ") && !opt.StartsWith("D. "))
                    {
                        opt = prefixes[i] + opt;
                    }
                    answerTexts[i].text = opt;
                    answerTexts[i].ForceMeshUpdate();
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
