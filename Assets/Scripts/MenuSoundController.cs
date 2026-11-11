using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bộ điều khiển âm thanh gắn góc trên bên phải (Top-Right Sound Controller)
/// Tham khảo từ Map 1 (Level1_Prototype):
/// - Nút icon âm thanh nhỏ dạng khối gỗ vuông (Btn_Wood_Square: 54x54) ở góc trên bên phải.
/// - Icon loa tự động đổi giữa Bật (Icon_Sound_On) và Tắt (Icon_Sound_Off).
/// - Khi nhấp vào sẽ mở Modal Panel_Sound cho phép người dùng điều chỉnh thanh trượt âm lượng và bật/tắt âm thanh.
/// - Tự động đồng bộ và lưu trạng thái vào AudioManager & PlayerPrefs xuyên suốt các cảnh.
/// </summary>
[ExecuteAlways]
public class MenuSoundController : MonoBehaviour
{
    [Header("Top-Right Sound Button")]
    [Tooltip("Nút icon âm thanh góc trên bên phải")]
    public Button btnSound;

    [Tooltip("Image hiển thị icon loa")]
    public Image imgSoundIcon;

    [Tooltip("Sprite icon loa khi bật")]
    public Sprite soundOnSprite;

    [Tooltip("Sprite icon loa khi tắt")]
    public Sprite soundOffSprite;

    [Tooltip("Sprite nền nút gỗ vuông")]
    public Sprite btnWoodSprite;

    [Header("Sound Settings Modal")]
    [Tooltip("Panel hộp thoại cài đặt âm thanh toàn màn hình")]
    public GameObject panelSound;

    [Tooltip("Thanh trượt điều chỉnh âm lượng nhạc nền")]
    public Slider musicVolumeSlider;

    [Tooltip("Thanh trượt điều chỉnh âm lượng hiệu ứng")]
    public Slider sfxVolumeSlider;

    [Tooltip("Text phần trăm âm lượng nhạc")]
    public Text txtMusicPercent;

    [Tooltip("Text phần trăm âm lượng hiệu ứng")]
    public Text txtSfxPercent;

    [Tooltip("Nút bật/tắt toàn bộ âm thanh")]
    public Button btnToggleMute;

    [Tooltip("Text hiển thị trạng thái âm thanh")]
    public Text txtMuteStatus;

    [Tooltip("Nút đóng bảng cài đặt (dưới cùng)")]
    public Button btnClose;

    [Tooltip("Nút đóng hình chữ X góc trên card")]
    public Button btnCloseX;

    public bool IsModalOpen => panelSound != null && panelSound.activeSelf;

    private void Awake()
    {
        LoadSpritesIfNull();
        EnsureUI();
    }

    private void Start()
    {
        EnsureUI();
        WireEvents();
        UpdateUIState(updateSliders: true);
    }

    private void OnEnable()
    {
        EnsureUI();
        UpdateUIState(updateSliders: true);
    }

    private bool? lastMutedState = null;

    private void Update()
    {
        bool currentMuted = AudioManager.Instance != null ? AudioManager.Instance.IsMuted : (PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1);
        if (!lastMutedState.HasValue || currentMuted != lastMutedState.Value)
        {
            lastMutedState = currentMuted;
            UpdateSoundIcon();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (btnSound == null || panelSound == null)
            {
                EnsureUI();
            }
        }
#endif
    }

    /// <summary>
    /// Nạp các sprite biểu tượng chuẩn từ dự án nếu chưa được kéo vào Inspector
    /// </summary>
    public void LoadSpritesIfNull()
    {
#if UNITY_EDITOR
        if (soundOnSprite == null)
            soundOnSprite = LoadSpriteSafe("Assets/UI/Icons/Icon_Sound_On.png");
        if (soundOffSprite == null)
            soundOffSprite = LoadSpriteSafe("Assets/UI/Icons/Icon_Sound_Off.png");
        if (btnWoodSprite == null)
            btnWoodSprite = LoadSpriteSafe("Assets/UI/Icons/Btn_Wood_Square.png");
#endif
    }

#if UNITY_EDITOR
    private static Sprite LoadSpriteSafe(string path)
    {
        Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        UnityEngine.Object[] allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        if (allAssets != null)
        {
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s) return s;
            }
        }

        Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        return null;
    }
#endif

    /// <summary>
    /// Đảm bảo các đối tượng UI cần thiết (Nút và Modal) tồn tại trong Canvas
    /// </summary>
    public void EnsureUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        }
        if (canvas == null) return;

        // 1. Tìm hoặc tạo nút Btn_Sound ở góc trên bên phải
        if (btnSound == null)
        {
            Transform foundBtn = canvas.transform.Find("Btn_Sound");
            if (foundBtn != null)
            {
                btnSound = foundBtn.GetComponent<Button>();
                imgSoundIcon = foundBtn.Find("Icon")?.GetComponent<Image>();
            }
            else
            {
                btnSound = CreateSoundButton(canvas.transform);
                if (btnSound != null)
                {
                    imgSoundIcon = btnSound.transform.Find("Icon")?.GetComponent<Image>();
                }
            }
        }

        // Đảm bảo nút Btn_Sound nằm ở vị trí render trên các background
        if (btnSound != null)
        {
            btnSound.transform.SetAsLastSibling();
        }

        // 2. Tìm hoặc tạo Panel_Sound
        if (panelSound == null)
        {
            Transform foundPanel = canvas.transform.Find("Panel_Sound");
            if (foundPanel != null)
            {
                panelSound = foundPanel.gameObject;
                BindPanelReferences(foundPanel);
            }
            else
            {
                panelSound = CreateSoundPanel(canvas.transform);
                if (panelSound != null)
                {
                    BindPanelReferences(panelSound.transform);
                }
            }
        }

        // Đảm bảo Panel_Sound luôn nằm trên cùng (last sibling) để đè lên các nút khác khi mở
        if (panelSound != null)
        {
            panelSound.transform.SetAsLastSibling();
        }
    }

    private void BindPanelReferences(Transform panelTr)
    {
        Transform card = panelTr.Find("Card_Sound") ?? panelTr;

        if (musicVolumeSlider == null)
            musicVolumeSlider = card.Find("Volume_Group/Slider_Music")?.GetComponent<Slider>()
                                ?? card.Find("VolumeSlider")?.GetComponent<Slider>();

        if (sfxVolumeSlider == null)
            sfxVolumeSlider = card.Find("Sfx_Group/Slider_Sfx")?.GetComponent<Slider>()
                              ?? card.Find("SfxSlider")?.GetComponent<Slider>();

        if (txtMusicPercent == null)
            txtMusicPercent = card.Find("Volume_Group/Txt_MusicPercent")?.GetComponent<Text>();

        if (txtSfxPercent == null)
            txtSfxPercent = card.Find("Sfx_Group/Txt_SfxPercent")?.GetComponent<Text>();

        if (btnToggleMute == null)
            btnToggleMute = card.Find("Btn_ToggleSound")?.GetComponent<Button>();

        if (txtMuteStatus == null && btnToggleMute != null)
            txtMuteStatus = btnToggleMute.GetComponentInChildren<Text>();

        if (btnClose == null)
            btnClose = card.Find("Btn_Close")?.GetComponent<Button>()
                       ?? card.Find("CloseSettingsButton")?.GetComponent<Button>();

        if (btnCloseX == null)
            btnCloseX = card.Find("Btn_Close_X")?.GetComponent<Button>();
    }

    /// <summary>
    /// Đăng ký các sự kiện tương tác
    /// </summary>
    public void WireEvents()
    {
        if (btnSound != null)
        {
            btnSound.onClick.RemoveListener(OnSoundButtonClicked);
            btnSound.onClick.AddListener(OnSoundButtonClicked);
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(CloseSoundModal);
            btnClose.onClick.AddListener(CloseSoundModal);
        }

        if (btnCloseX != null)
        {
            btnCloseX.onClick.RemoveListener(CloseSoundModal);
            btnCloseX.onClick.AddListener(CloseSoundModal);
        }

        if (btnToggleMute != null)
        {
            btnToggleMute.onClick.RemoveListener(ToggleMute);
            btnToggleMute.onClick.AddListener(ToggleMute);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        // Nhấp vào vùng overlay đen bên ngoài hộp thoại để đóng
        if (panelSound != null)
        {
            Button overlayBtn = panelSound.GetComponent<Button>();
            if (overlayBtn != null)
            {
                overlayBtn.onClick.RemoveListener(CloseSoundModal);
                overlayBtn.onClick.AddListener(CloseSoundModal);
            }
        }
    }

    public void OnSoundButtonClicked()
    {
        if (panelSound != null && panelSound.activeSelf)
        {
            panelSound.SetActive(false);
        }
        ToggleMute();
    }

    public void OpenSoundModal()
    {
        if (panelSound != null)
        {
            panelSound.SetActive(true);
            panelSound.transform.SetAsLastSibling();
        }
        UpdateUIState(updateSliders: true);
    }

    public void CloseSoundModal()
    {
        AudioManager.Instance?.PlayClickSound();
        if (panelSound != null)
        {
            panelSound.SetActive(false);
        }
        UpdateSoundIcon();
    }

    public void ToggleMute()
    {
        bool wasMuted = false;
        if (AudioManager.Instance != null)
        {
            wasMuted = AudioManager.Instance.IsMuted;
            AudioManager.Instance.ToggleMute();
            if (wasMuted) // Khi bật lại âm thanh thì phát tiếng click báo hiệu
            {
                AudioManager.Instance.PlayClickSound();
            }
        }
        else
        {
            wasMuted = PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1;
            bool next = !wasMuted;
            PlayerPrefs.SetInt("Game_Audio_Muted", next ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.pause = next;
            AudioListener.volume = next ? 0f : 1f;
        }

        UpdateUIState(updateSliders: true);
        UpdateSoundIcon();
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat("Game_Music_Volume", value);
            PlayerPrefs.Save();
        }
        UpdateUIState(updateSliders: false);
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat("Game_Sfx_Volume", value);
            PlayerPrefs.Save();
        }
        UpdateUIState(updateSliders: false);
    }

    public void UpdateUIState(bool updateSliders)
    {
        bool isMuted = false;
        float musicVol = 0.45f;
        float sfxVol = 0.85f;

        if (AudioManager.Instance != null)
        {
            isMuted = AudioManager.Instance.IsMuted;
            musicVol = AudioManager.Instance.musicVolume;
            sfxVol = AudioManager.Instance.sfxVolume;
        }
        else
        {
            isMuted = PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1;
            musicVol = PlayerPrefs.GetFloat("Game_Music_Volume", 0.45f);
            sfxVol = PlayerPrefs.GetFloat("Game_Sfx_Volume", 0.85f);
        }

        // Cập nhật trạng thái thanh trượt
        if (updateSliders)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(musicVol);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
            }
        }

        if (txtMusicPercent != null)
        {
            txtMusicPercent.text = $"{Mathf.RoundToInt(musicVol * 100f)}%";
        }

        if (txtSfxPercent != null)
        {
            txtSfxPercent.text = $"{Mathf.RoundToInt(sfxVol * 100f)}%";
        }

        // Cập nhật Text nút Mute
        if (txtMuteStatus != null)
        {
            if (isMuted || musicVol <= 0.001f)
            {
                txtMuteStatus.text = "Âm thanh: ĐÃ TẮT";
                txtMuteStatus.color = new Color(0.85f, 0.4f, 0.35f, 1f); // Màu đỏ nhạt
            }
            else
            {
                txtMuteStatus.text = "Âm thanh: ĐANG BẬT";
                txtMuteStatus.color = new Color(0.35f, 0.85f, 0.45f, 1f); // Màu xanh lá tươi
            }
        }

        UpdateSoundIcon();
    }

    public void UpdateSoundIcon()
    {
        LoadSpritesIfNull();

        if (imgSoundIcon == null && btnSound != null)
        {
            imgSoundIcon = btnSound.transform.Find("Icon")?.GetComponent<Image>();
        }
        if (imgSoundIcon == null)
        {
            GameObject found = GameObject.Find("Btn_Sound/Icon");
            if (found != null) imgSoundIcon = found.GetComponent<Image>();
        }

        bool isMuted = false;
        float musicVol = 0.45f;

        if (AudioManager.Instance != null)
        {
            isMuted = AudioManager.Instance.IsMuted;
            musicVol = AudioManager.Instance.musicVolume;
        }
        else
        {
            isMuted = PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1;
            musicVol = PlayerPrefs.GetFloat("Game_Music_Volume", 0.45f);
        }

        bool shouldBeOff = isMuted || musicVol <= 0.001f;

        if (imgSoundIcon != null)
        {
            Sprite targetSprite = shouldBeOff ? soundOffSprite : soundOnSprite;
            if (targetSprite != null)
            {
                imgSoundIcon.sprite = targetSprite;
            }
            imgSoundIcon.preserveAspect = true;
            imgSoundIcon.color = Color.white;
        }
    }

    #region Programmatic UI Builders

    private Button CreateSoundButton(Transform parent)
    {
        GameObject btnObj = new GameObject("Btn_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30f, -30f);
        rt.sizeDelta = new Vector2(54f, 54f);

        Image bgImage = btnObj.GetComponent<Image>();
        bgImage.sprite = btnWoodSprite;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = Color.white;

        Button btn = btnObj.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        // Tạo Icon con
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(btnObj.transform, false);

        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(34f, 34f);

        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.sprite = soundOnSprite;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;
        iconImg.color = Color.white;

        return btn;
    }

    private GameObject CreateSoundPanel(Transform parent)
    {
        Font gameFont = GetGameFont();

        // 1. Fullscreen Overlay (Khối tối mờ che nền)
        GameObject panelObj = new GameObject("Panel_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        panelObj.transform.SetParent(parent, false);

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.72f);
        panelImg.raycastTarget = true;

        // 2. Card_Sound (Hộp thoại chính)
        GameObject cardObj = new GameObject("Card_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObj.transform.SetParent(panelObj.transform, false);

        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(580f, 410f);

        Image cardImg = cardObj.GetComponent<Image>();
#if UNITY_EDITOR
        Sprite quizFrame = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Quiz.png");
        if (quizFrame != null)
        {
            cardImg.sprite = quizFrame;
            cardImg.type = Image.Type.Sliced;
        }
#endif
        if (cardImg.sprite == null)
        {
            cardImg.color = new Color(0.18f, 0.11f, 0.08f, 0.98f);
        }

        // 3. Txt_Title (Tiêu đề)
        GameObject titleObj = new GameObject("Txt_Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -32f);
        titleRt.sizeDelta = new Vector2(460f, 48f);

        Text titleText = titleObj.GetComponent<Text>();
        titleText.font = gameFont;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.98f, 0.88f, 0.65f, 1f); // Vàng kim pixel art
        titleText.text = "CÀI ĐẶT ÂM THANH";

        // 4. Btn_ToggleSound (Nút bật/tắt nhanh)
        GameObject muteBtnObj = new GameObject("Btn_ToggleSound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        muteBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform muteRt = muteBtnObj.GetComponent<RectTransform>();
        muteRt.anchorMin = new Vector2(0.5f, 1f);
        muteRt.anchorMax = new Vector2(0.5f, 1f);
        muteRt.pivot = new Vector2(0.5f, 1f);
        muteRt.anchoredPosition = new Vector2(0f, -92f);
        muteRt.sizeDelta = new Vector2(380f, 50f);

        Image muteImg = muteBtnObj.GetComponent<Image>();
        muteImg.sprite = btnWoodSprite;
        muteImg.type = Image.Type.Sliced;
        muteImg.color = Color.white;

        GameObject muteTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        muteTextObj.transform.SetParent(muteBtnObj.transform, false);
        RectTransform muteTextRt = muteTextObj.GetComponent<RectTransform>();
        muteTextRt.anchorMin = Vector2.zero;
        muteTextRt.anchorMax = Vector2.one;
        muteTextRt.sizeDelta = Vector2.zero;
        Text muteText = muteTextObj.GetComponent<Text>();
        muteText.font = gameFont;
        muteText.fontSize = 22;
        muteText.fontStyle = FontStyle.Bold;
        muteText.alignment = TextAnchor.MiddleCenter;
        muteText.text = "Âm thanh: ĐANG BẬT";
        muteText.color = new Color(0.35f, 0.85f, 0.45f, 1f);

        // 5. Volume_Group (Nhạc nền BGM)
        CreateSliderRow(cardObj.transform, "Volume_Group", "Nhạc nền (BGM):", -160f, gameFont, out Slider musSlider, out Text musPct);

        // 6. Sfx_Group (Hiệu ứng SFX)
        CreateSliderRow(cardObj.transform, "Sfx_Group", "Hiệu ứng (SFX):", -240f, gameFont, out Slider sfxSld, out Text sfxPct);

        // 7. Btn_Close (Nút Đóng dưới cùng)
        GameObject closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 28f);
        closeRt.sizeDelta = new Vector2(200f, 48f);

        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.sprite = btnWoodSprite;
        closeImg.type = Image.Type.Sliced;
        closeImg.color = Color.white;

        GameObject closeTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTextRt = closeTextObj.GetComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.sizeDelta = Vector2.zero;
        Text closeText = closeTextObj.GetComponent<Text>();
        closeText.font = gameFont;
        closeText.fontSize = 22;
        closeText.fontStyle = FontStyle.Bold;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.text = "ĐÓNG";
        closeText.color = Color.white;

        // 8. Btn_Close_X (Nút X góc trên)
        GameObject xBtnObj = new GameObject("Btn_Close_X", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        xBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform xRt = xBtnObj.GetComponent<RectTransform>();
        xRt.anchorMin = new Vector2(1f, 1f);
        xRt.anchorMax = new Vector2(1f, 1f);
        xRt.pivot = new Vector2(1f, 1f);
        xRt.anchoredPosition = new Vector2(-20f, -20f);
        xRt.sizeDelta = new Vector2(40f, 40f);

        Image xImg = xBtnObj.GetComponent<Image>();
        xImg.sprite = btnWoodSprite;
        xImg.type = Image.Type.Sliced;
        xImg.color = Color.white;

        GameObject xTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        xTextObj.transform.SetParent(xBtnObj.transform, false);
        RectTransform xTextRt = xTextObj.GetComponent<RectTransform>();
        xTextRt.anchorMin = Vector2.zero;
        xTextRt.anchorMax = Vector2.one;
        xTextRt.sizeDelta = Vector2.zero;
        Text xText = xTextObj.GetComponent<Text>();
        xText.font = gameFont;
        xText.fontSize = 22;
        xText.fontStyle = FontStyle.Bold;
        xText.alignment = TextAnchor.MiddleCenter;
        xText.text = "✕";
        xText.color = new Color(0.95f, 0.85f, 0.75f, 1f);

        panelObj.SetActive(false);
        return panelObj;
    }

    private void CreateSliderRow(Transform parent, string groupName, string label, float yPos, Font font, out Slider slider, out Text percentText)
    {
        GameObject group = new GameObject(groupName, typeof(RectTransform));
        group.transform.SetParent(parent, false);
        RectTransform grpRt = group.GetComponent<RectTransform>();
        grpRt.anchorMin = new Vector2(0.5f, 1f);
        grpRt.anchorMax = new Vector2(0.5f, 1f);
        grpRt.pivot = new Vector2(0.5f, 1f);
        grpRt.anchoredPosition = new Vector2(0f, yPos);
        grpRt.sizeDelta = new Vector2(460f, 65f);

        // Label
        GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        lblObj.transform.SetParent(group.transform, false);
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0f, 1f);
        lblRt.anchorMax = new Vector2(0.7f, 1f);
        lblRt.pivot = new Vector2(0f, 1f);
        lblRt.anchoredPosition = Vector2.zero;
        lblRt.sizeDelta = new Vector2(0f, 26f);
        Text lbl = lblObj.GetComponent<Text>();
        lbl.font = font;
        lbl.fontSize = 20;
        lbl.fontStyle = FontStyle.Bold;
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.color = new Color(0.92f, 0.85f, 0.75f, 1f);
        lbl.text = label;

        // Percent
        GameObject pctObj = new GameObject(groupName == "Volume_Group" ? "Txt_MusicPercent" : "Txt_SfxPercent",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        pctObj.transform.SetParent(group.transform, false);
        RectTransform pctRt = pctObj.GetComponent<RectTransform>();
        pctRt.anchorMin = new Vector2(0.7f, 1f);
        pctRt.anchorMax = new Vector2(1f, 1f);
        pctRt.pivot = new Vector2(1f, 1f);
        pctRt.anchoredPosition = Vector2.zero;
        pctRt.sizeDelta = new Vector2(0f, 26f);
        percentText = pctObj.GetComponent<Text>();
        percentText.font = font;
        percentText.fontSize = 20;
        percentText.fontStyle = FontStyle.Bold;
        percentText.alignment = TextAnchor.MiddleRight;
        percentText.color = new Color(1f, 0.9f, 0.5f, 1f);
        percentText.text = "100%";

        // Slider
        string sliderName = groupName == "Volume_Group" ? "Slider_Music" : "Slider_Sfx";
        GameObject sliderObj = new GameObject(sliderName, typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(group.transform, false);
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 0f);
        sliderRt.anchorMax = new Vector2(1f, 0f);
        sliderRt.pivot = new Vector2(0.5f, 0f);
        sliderRt.anchoredPosition = new Vector2(0f, 4f);
        sliderRt.sizeDelta = new Vector2(0f, 26f);

        slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.12f, 0.08f, 0.06f, 0.95f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.sizeDelta = new Vector2(-12f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.96f, 0.65f, 0.22f, 1f); // Màu cam vàng ấm áp
        slider.fillRect = fillRt;

        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = new Vector2(-16f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(24f, 24f);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(1f, 0.95f, 0.85f, 1f);
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
    }

    private Font GetGameFont()
    {
#if UNITY_EDITOR
        Font font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.otf");
        if (font != null) return font;
#endif
        Text anyText = UnityEngine.Object.FindAnyObjectByType<Text>();
        if (anyText != null && anyText.font != null) return anyText.font;
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    #endregion
}
