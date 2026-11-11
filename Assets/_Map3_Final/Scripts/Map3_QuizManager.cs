using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        [Header("Illustration References")]
        [SerializeField] private GameObject illustrationContainer;
        [SerializeField] private Image imgIllustration;
        [SerializeField] private TextMeshProUGUI txtIllustrationName;

        [Header("Illustration Sprites")]
        [SerializeField] private Sprite catSprite;
        [SerializeField] private Sprite keySprite;
        [SerializeField] private Sprite woodSprite;
        [SerializeField] private Sprite stoneSprite;
        [SerializeField] private Sprite vietGemSprite;
        [SerializeField] private Sprite hanGemSprite;
        [SerializeField] private Sprite denVietSprite;
        [SerializeField] private Sprite denHanSprite;

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

            LoadIllustrationSpritesIfNull();

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
            EnforceQuizUILayout();
        }

        public void LoadIllustrationSpritesIfNull()
        {
#if UNITY_EDITOR
            if (catSprite == null) catSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cat_question_mark.png");
            if (keySprite == null) keySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Key.png");
            if (woodSprite == null) woodSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Wood.png");
            if (stoneSprite == null) stoneSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Stone.png");
            if (vietGemSprite == null) vietGemSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Viet_Gem.png");
            if (hanGemSprite == null) hanGemSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Han_Gem.png");
            if (denVietSprite == null) denVietSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Den_Viet.png");
            if (denHanSprite == null) denHanSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level3/Den_Han.png");
#endif
        }

        public void SetIllustrationReferences(GameObject container, Image img, TextMeshProUGUI txt)
        {
            illustrationContainer = container;
            imgIllustration = img;
            txtIllustrationName = txt;
        }

        private static readonly Dictionary<string, Sprite> loadedQuizSprites = new Dictionary<string, Sprite>();

        public static Sprite GetIllustrationSprite(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            if (loadedQuizSprites.TryGetValue(filename, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Sprite sp = null;
#if UNITY_EDITOR
            string assetPath = "Assets/Sprites/Level3/QuizIllustrations/" + filename;
            sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#endif

            if (sp == null)
            {
                string fullPath = Path.Combine(Application.dataPath, "Sprites/Level3/QuizIllustrations", filename);
                if (File.Exists(fullPath))
                {
                    byte[] bytes = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Point;
                    if (tex.LoadImage(bytes))
                    {
                        sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                }
            }

            if (sp != null)
            {
                loadedQuizSprites[filename] = sp;
            }
            return sp;
        }

        public void ResolveIllustration(Map3_QuizData data, out Sprite sprite, out string displayName)
        {
            LoadIllustrationSpritesIfNull();
            sprite = null;
            displayName = "CÂU HỎI VĂN HÓA";

            if (data == null) return;
            string id = data.id ?? "";
            string title = data.title ?? "";

            if (id == "Quiz_CatHint" || title.IndexOf("LÚA", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("MÈO", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("LuaNuoc.png");
                displayName = "LÚA NƯỚC VIỆT NAM";
            }
            else if (id == "Quiz_Key" || title.IndexOf("LU SÀNH", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("LuSanh.png");
                displayName = "LU SÀNH TRUYỀN THỐNG";
            }
            else if (id == "Quiz_Wood1" || title.IndexOf("GIAO TIẾP", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("INSA", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("InsaGreeting.png");
                displayName = "VĂN HÓA CÚI CHÀO INSA";
            }
            else if (id == "Quiz_Wood2" || title.IndexOf("HANOK", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("ONDOL", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("HanokOndol.png");
                displayName = "NHÀ HANOK & SÀN ONDOL";
            }
            else if (id == "Quiz_Wood3" || title.IndexOf("BÀ BA", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("KHĂN RẰN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("AoBaBaKhanRan.png");
                displayName = "ÁO BÀ BA & KHĂN RẰN";
            }
            else if (id == "Quiz_Stone" || title.IndexOf("DOLTAP", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("THÁP ĐÁ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("ThapDaDoltap.png");
                displayName = "THÁP ĐÁ DOLTAP";
            }
            else if (id == "Quiz_VietJade" || title.IndexOf("HOA SEN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("HoaSen.png");
                displayName = "HOA SEN VIỆT NAM";
            }
            else if (id == "Quiz_KoreaJade" || title.IndexOf("MUGUNGHWA", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("HoaMugunghwa.png");
                displayName = "QUỐC HOA MUGUNGHWA";
            }
            else if (id == "Quiz_Lore_BinhPhong" || title.IndexOf("BÌNH PHONG", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("ILWOROBONGDO", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("BinhPhongIlworobongdo.png");
                displayName = "BÌNH PHONG ILWOROBONGDO";
            }
            else if (id == "Quiz_Lore_GomSu" || title.IndexOf("GỐM", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("CELADON", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("GomMenNgoc.png");
                displayName = "GỐM SỨ MEN NGỌC";
            }
            else if (id == "Quiz_Lore_DanTranh" || title.IndexOf("ĐÀN TRANH", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("DanTranh.png");
                displayName = "ĐÀN TRANH VIỆT NAM";
            }
            else if (id == "Quiz_Lore_DinhDong" || title.IndexOf("ĐỈNH ĐỒNG", StringComparison.OrdinalIgnoreCase) >= 0 || title.IndexOf("LƯ TRẦM", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                sprite = GetIllustrationSprite("DinhDong.png");
                displayName = "BỘ ĐỈNH ĐỒNG - LƯ TRẦM";
            }

            if (sprite == null)
            {
                sprite = catSprite != null ? catSprite : GetIllustrationSprite("LuaNuoc.png");
            }
        }

        public static void ConfigureAnswerText(TextMeshProUGUI txt)
        {
            if (txt == null) return;
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 18f;
            txt.fontSizeMax = 21f;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.margin = new Vector4(24f, 2f, 24f, 2f);
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
            txt.fontSizeMin = 24f;
            txt.fontSizeMax = 28f;
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.margin = new Vector4(12f, 4f, 12f, 4f);
            txt.overflowMode = TextOverflowModes.Ellipsis;
        }

        public void EnforceQuizUILayout()
        {
            if (quizPanel == null) return;
            Transform card = quizPanel.transform.Find("Card_Content");
            if (card == null && quizPanel.transform.childCount > 0)
            {
                card = quizPanel.transform.GetChild(0);
            }
            if (card == null) return;

            RectTransform cardRt = card.GetComponent<RectTransform>();
            if (cardRt != null)
            {
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = Vector2.zero;
                cardRt.sizeDelta = new Vector2(1400f, 760f);
            }

            // Cột trái: Panel_Illustration
            Transform illusTr = card.Find("Panel_Illustration");
            if (illusTr == null)
            {
                GameObject newIllusGo = new GameObject("Panel_Illustration", typeof(RectTransform), typeof(Image));
                newIllusGo.transform.SetParent(card, false);
                var img = newIllusGo.GetComponent<Image>();
                img.color = new Color32(20, 24, 32, 255);
                illustrationContainer = newIllusGo;
                illusTr = newIllusGo.transform;

                GameObject newImgGo = new GameObject("Img_Illustration", typeof(RectTransform), typeof(Image));
                newImgGo.transform.SetParent(illusTr, false);
                var subImg = newImgGo.GetComponent<Image>();
                subImg.preserveAspect = true;
                imgIllustration = subImg;

                GameObject newTxtGo = new GameObject("Txt_Illustration_Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                newTxtGo.transform.SetParent(illusTr, false);
                var tmp = newTxtGo.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 24f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = new Color(1f, 0.835f, 0.31f, 1f);
                tmp.alignment = TextAlignmentOptions.Center;
                txtIllustrationName = tmp;
            }
            else
            {
                illustrationContainer = illusTr.gameObject;
                Transform imgTr = illusTr.Find("Img_Illustration");
                if (imgTr != null) imgIllustration = imgTr.GetComponent<Image>();
                Transform txtTr = illusTr.Find("Txt_Illustration_Name");
                if (txtTr != null) txtIllustrationName = txtTr.GetComponent<TextMeshProUGUI>();
            }

            if (illusTr != null)
            {
                RectTransform illusRt = illusTr.GetComponent<RectTransform>();
                if (illusRt != null)
                {
                    illusRt.anchorMin = new Vector2(0.5f, 0.5f);
                    illusRt.anchorMax = new Vector2(0.5f, 0.5f);
                    illusRt.pivot = new Vector2(0.5f, 0.5f);
                    illusRt.anchoredPosition = new Vector2(-440f, -40f);
                    illusRt.sizeDelta = new Vector2(390f, 520f);
                }

                if (imgIllustration != null)
                {
                    RectTransform imgRt = imgIllustration.rectTransform;
                    if (imgRt != null)
                    {
                        imgRt.anchorMin = new Vector2(0.5f, 0.5f);
                        imgRt.anchorMax = new Vector2(0.5f, 0.5f);
                        imgRt.pivot = new Vector2(0.5f, 0.5f);
                        imgRt.anchoredPosition = new Vector2(0f, 40f);
                        imgRt.sizeDelta = new Vector2(310f, 310f);
                    }
                    imgIllustration.preserveAspect = true;
                }

                if (txtIllustrationName != null)
                {
                    RectTransform nameRt = txtIllustrationName.rectTransform;
                    if (nameRt != null)
                    {
                        nameRt.anchorMin = new Vector2(0.5f, 0.5f);
                        nameRt.anchorMax = new Vector2(0.5f, 0.5f);
                        nameRt.pivot = new Vector2(0.5f, 0.5f);
                        nameRt.anchoredPosition = new Vector2(0f, -175f);
                        nameRt.sizeDelta = new Vector2(370f, 46f);
                    }
                    txtIllustrationName.fontSize = 24f;
                    txtIllustrationName.fontStyle = FontStyles.Bold;
                    txtIllustrationName.alignment = TextAlignmentOptions.Center;
                    txtIllustrationName.color = new Color(1f, 0.835f, 0.31f, 1f);
                }
            }

            // Cột phải: Tiêu đề
            Transform titleTr = card.Find("Txt_Title");
            if (titleTr != null)
            {
                RectTransform titleRt = titleTr.GetComponent<RectTransform>();
                if (titleRt != null)
                {
                    titleRt.anchorMin = new Vector2(0.5f, 0.5f);
                    titleRt.anchorMax = new Vector2(0.5f, 0.5f);
                    titleRt.pivot = new Vector2(0.5f, 0.5f);
                    titleRt.anchoredPosition = new Vector2(0f, 312f);
                    titleRt.sizeDelta = new Vector2(950f, 54f);
                }
                var tTmp = titleTr.GetComponent<TextMeshProUGUI>();
                if (tTmp != null)
                {
                    tTmp.fontSize = 26f;
                    tTmp.alignment = TextAlignmentOptions.Center;
                    tTmp.color = new Color(1f, 0.835f, 0.31f, 1f);
                }
            }

            // Cột phải: Câu hỏi (hạ xuống Y = 135 để nằm trọn vẹn trong khung vàng, không tràn lên viền gỗ)
            if (txtQuestion != null)
            {
                RectTransform qRt = txtQuestion.rectTransform;
                if (qRt != null)
                {
                    qRt.anchorMin = new Vector2(0.5f, 0.5f);
                    qRt.anchorMax = new Vector2(0.5f, 0.5f);
                    qRt.pivot = new Vector2(0.5f, 0.5f);
                    qRt.anchoredPosition = new Vector2(235f, 125f);
                    qRt.sizeDelta = new Vector2(850f, 130f);
                }
                ConfigureQuestionText(txtQuestion);
                txtQuestion.color = new Color(0.22f, 0.13f, 0.09f, 1f); // #372016
            }

            // Cột phải: Phản hồi
            if (txtFeedback != null)
            {
                RectTransform fRt = txtFeedback.rectTransform;
                if (fRt != null)
                {
                    fRt.anchorMin = new Vector2(0.5f, 0.5f);
                    fRt.anchorMax = new Vector2(0.5f, 0.5f);
                    fRt.pivot = new Vector2(0.5f, 0.5f);
                    fRt.anchoredPosition = new Vector2(235f, 48f);
                    fRt.sizeDelta = new Vector2(850f, 26f);
                }
                txtFeedback.fontSize = 19f;
            }

            // Cột phải: 4 Nút Đáp Án
            float[] btnYs = { -6f, -92f, -178f, -264f };
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    RectTransform bRt = answerButtons[i].GetComponent<RectTransform>();
                    if (bRt != null)
                    {
                        bRt.anchorMin = new Vector2(0.5f, 0.5f);
                        bRt.anchorMax = new Vector2(0.5f, 0.5f);
                        bRt.pivot = new Vector2(0.5f, 0.5f);
                        bRt.anchoredPosition = new Vector2(235f, btnYs[i]);
                        bRt.sizeDelta = new Vector2(850f, 78f);
                    }

                    if (i < answerTexts.Length && answerTexts[i] != null)
                    {
                        ConfigureAnswerText(answerTexts[i]);
                    }
                }
            }

            // Nút đóng
            if (btnClose != null)
            {
                RectTransform cRt = btnClose.GetComponent<RectTransform>();
                if (cRt != null)
                {
                    cRt.anchorMin = new Vector2(1f, 1f);
                    cRt.anchorMax = new Vector2(1f, 1f);
                    cRt.pivot = new Vector2(0.5f, 0.5f);
                    cRt.anchoredPosition = new Vector2(-40f, -40f);
                    cRt.sizeDelta = new Vector2(44f, 44f);
                }
            }

            // Giải thích
            if (txtExplanation != null)
            {
                RectTransform expRt = txtExplanation.rectTransform;
                if (expRt != null)
                {
                    expRt.anchorMin = new Vector2(0.5f, 0.5f);
                    expRt.anchorMax = new Vector2(0.5f, 0.5f);
                    expRt.pivot = new Vector2(0.5f, 0.5f);
                    expRt.anchoredPosition = new Vector2(220f, -255f);
                    expRt.sizeDelta = new Vector2(840f, 76f);
                }
                txtExplanation.fontSize = 17f;
            }

            // Tiếp tục (Continue button)
            var contBtn = ActiveContinueBtn;
            if (contBtn != null)
            {
                RectTransform contRt = contBtn.GetComponent<RectTransform>();
                if (contRt != null)
                {
                    contRt.anchorMin = new Vector2(0.5f, 0.5f);
                    contRt.anchorMax = new Vector2(0.5f, 0.5f);
                    contRt.pivot = new Vector2(0.5f, 0.5f);
                    contRt.anchoredPosition = new Vector2(220f, -255f);
                    contRt.sizeDelta = new Vector2(220f, 50f);
                }
            }
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

            EnforceQuizUILayout();

            ResolveIllustration(data, out Sprite illSprite, out string displayName);
            if (imgIllustration != null)
            {
                imgIllustration.sprite = illSprite;
                imgIllustration.color = illSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.05f);
                imgIllustration.enabled = illSprite != null;
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

            if (txtTitle != null) txtTitle.text = data.title;
            if (txtQuestion != null)
            {
                ConfigureQuestionText(txtQuestion);
                txtQuestion.text = data.question;
                txtQuestion.ForceMeshUpdate();
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
