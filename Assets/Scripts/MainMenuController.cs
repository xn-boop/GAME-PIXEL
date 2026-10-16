using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý màn hình Menu chính / Bắt đầu game.
/// Tích hợp hiệu ứng Tap cho nút "TAP TO START", chuyển cảnh mượt mà với Screen Fade,
/// và hỗ trợ đa ngôn ngữ.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Tên scene đăng nhập sẽ chuyển đến khi nhấn TAP TO START")]
    public string loginSceneName = "LoginMenu";

    [Tooltip("Tên scene đăng ký dự phòng")]
    public string registerSceneName = "RegisterMenu";

    [Tooltip("Tên scene gameplay dự phòng")]
    public string firstLevelSceneName = "Level1_Prototype";

    [Header("UI Panels")]
    [Tooltip("Panel menu chính")]
    public GameObject mainPanel;

    [Tooltip("Panel cài đặt (nếu có)")]
    public GameObject settingsPanel;

    [Header("Tap Feedback & Transition")]
    [Tooltip("Hiệu ứng phản hồi khi chạm nút Tap To Start")]
    public TapToStartEffect tapToStartEffect;

    [Tooltip("Lớp phủ mờ dần màn hình khi chuyển cảnh (Fade to Black)")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("Thời gian chuyển cảnh mờ màn hình (giây)")]
    public float fadeDuration = 0.35f;

    [Header("Audio (Optional)")]
    [Tooltip("Slider chỉnh âm lượng nhạc nền")]
    public Slider musicVolumeSlider;

    [Header("Confirmation Dialog")]
    [Tooltip("Panel hộp thoại xác nhận Chơi mới khi đã có save")]
    public GameObject confirmNewGameDialog;

    [Tooltip("Text nội dung cảnh báo trên hộp thoại xác nhận")]
    public Text confirmDialogMessageText;

    [Tooltip("Nút xác nhận Chơi mới")]
    public Button confirmDialogYesButton;

    [Tooltip("Nút hủy Chơi mới")]
    public Button confirmDialogNoButton;

    [Header("Feedback / Status Display")]
    [Tooltip("Text hiển thị thông báo phản hồi (lỗi/trạng thái) trên menu chính")]
    public Text statusText;

    private bool isStartingGame = false;
    public bool IsStartingGame => isStartingGame;

    private void Awake()
    {
        // Tự tìm TapToStartEffect nếu chưa gán
        if (tapToStartEffect == null)
        {
            tapToStartEffect = FindObjectOfType<TapToStartEffect>();
        }

        if (confirmDialogYesButton != null)
        {
            confirmDialogYesButton.onClick.RemoveListener(ConfirmStartNewGame);
            confirmDialogYesButton.onClick.AddListener(ConfirmStartNewGame);
        }
        if (confirmDialogNoButton != null)
        {
            confirmDialogNoButton.onClick.RemoveListener(CancelStartNewGame);
            confirmDialogNoButton.onClick.AddListener(CancelStartNewGame);
        }
        if (confirmNewGameDialog != null)
        {
            confirmNewGameDialog.SetActive(false);
        }

        // Đảm bảo màn hình không bị tối ban đầu
        if (screenFadeOverlay != null)
        {
            screenFadeOverlay.alpha = 0f;
            screenFadeOverlay.blocksRaycasts = false;
        }
        else
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var fadeObj = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                fadeObj.transform.SetParent(canvas.transform, false);
                fadeObj.transform.SetAsLastSibling();

                var fRect = fadeObj.GetComponent<RectTransform>();
                fRect.anchorMin = Vector2.zero;
                fRect.anchorMax = Vector2.one;
                fRect.sizeDelta = Vector2.zero;

                var fImg = fadeObj.GetComponent<Image>();
                fImg.color = Color.black;
                fImg.raycastTarget = true;

                screenFadeOverlay = fadeObj.GetComponent<CanvasGroup>();
                screenFadeOverlay.alpha = 0f;
                screenFadeOverlay.blocksRaycasts = false;
                fadeObj.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Hiển thị panel chính, ẩn panel cài đặt
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Tự động phát nhạc nền Main Menu nếu có AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }
    }

    private void Update()
    {
        if (isStartingGame) return;

        // Cho phép nhấn Space hoặc Enter để vào game
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (settingsPanel == null || !settingsPanel.activeSelf)
            {
                PlayGame();
            }
        }

        // Nhấn Escape để mở/đóng bảng cài đặt
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (settingsPanel != null)
            {
                OpenSettings();
            }
        }
    }

    /// <summary>
    /// Bắt đầu game - Kích hoạt hiệu ứng Tap và chuyển cảnh mượt mà
    /// </summary>
    public void PlayGame()
    {
        if (isStartingGame) return;
        isStartingGame = true;

        Debug.Log("MainMenuController: Đang kích hoạt hiệu ứng Tap và vào game...");

        StartCoroutine(StartGameRoutine());
    }

    public IEnumerator StartGameRoutineForTest()
    {
        return StartGameRoutine();
    }

    private IEnumerator StartGameRoutine()
    {
        // 1. Kích hoạt hiệu ứng Tap trên nút
        if (tapToStartEffect != null)
        {
            tapToStartEffect.TriggerTapEffect();
        }

        // Chờ hiệu ứng Tap diễn ra để người chơi cảm nhận độ nảy và âm thanh
        yield return new WaitForSecondsRealtime(0.25f);

        // 2. Chuyển dần màn hình sang đen (Fade to black)
        if (screenFadeOverlay != null)
        {
            screenFadeOverlay.gameObject.SetActive(true);
            screenFadeOverlay.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                screenFadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            screenFadeOverlay.alpha = 1f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.15f);
        }

        // 3. Tải scene tiếp theo: Ưu tiên LoginMenu theo luồng, fallback sang RegisterMenu hoặc luồng an toàn
        if (!string.IsNullOrEmpty(loginSceneName) && Application.CanStreamedLevelBeLoaded(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
        else if (!string.IsNullOrEmpty(registerSceneName) && Application.CanStreamedLevelBeLoaded(registerSceneName))
        {
            SceneManager.LoadScene(registerSceneName);
        }
        else
        {
            var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
            if (save == null)
            {
                save = FindObjectOfType<JourneyOfMemories.Core.Managers.SaveLoadManager>();
            }

            if (save != null && save.HasSaveData())
            {
                // Có save -> Tiếp tục save hiện có, không được tự ý xóa hoặc tạo mới
                ContinueGame();
            }
            else
            {
                // Chưa có save -> Vào ván mới
                StartNewGameConfirmed();
            }
        }
    }

    /// <summary>
    /// BR-06: Chơi mới là hành động riêng. Nếu có save, hiển thị xác nhận. Nếu chưa có save, tạo lượt mới.
    /// </summary>
    public void OnNewGameClicked()
    {
        if (isStartingGame) return;

        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        if (save == null)
        {
            save = FindObjectOfType<JourneyOfMemories.Core.Managers.SaveLoadManager>();
        }

        if (save != null && save.HasSaveData())
        {
            // Có save -> Hiển thị xác nhận: "Chơi mới sẽ thay thế tiến trình hiện tại trên thiết bị này. Bạn có muốn tiếp tục?"
            ShowConfirmNewGameDialog();
        }
        else
        {
            // Chưa có save -> Vào ván mới trực tiếp
            StartNewGameConfirmed();
        }
    }

    public void ShowConfirmNewGameDialog()
    {
        EnsureConfirmDialogExists();
        if (confirmNewGameDialog != null)
        {
            if (confirmDialogMessageText != null)
            {
                confirmDialogMessageText.text = GetText(
                    "Chơi mới sẽ thay thế tiến trình hiện tại trên thiết bị này. Bạn có muốn tiếp tục?",
                    "Starting a new game will overwrite your current progress on this device. Do you want to continue?");
            }
            confirmNewGameDialog.SetActive(true);
        }
    }

    public void HideConfirmNewGameDialog()
    {
        if (confirmNewGameDialog != null)
        {
            confirmNewGameDialog.SetActive(false);
        }
    }

    public void CancelStartNewGame()
    {
        HideConfirmNewGameDialog();
        isStartingGame = false;
        Debug.Log("[MainMenuController] Người chơi đã hủy Chơi mới; tiến trình được giữ nguyên.");
    }

    public void ConfirmStartNewGame()
    {
        HideConfirmNewGameDialog();
        StartNewGameConfirmed();
    }

    private void StartNewGameConfirmed()
    {
        isStartingGame = true;
        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        if (save == null)
        {
            save = FindObjectOfType<JourneyOfMemories.Core.Managers.SaveLoadManager>();
        }
        save?.StartNewGame();

        var flow = JourneyOfMemories.Core.Managers.SceneFlowManager.Instance;
        if (flow == null)
        {
            flow = FindObjectOfType<JourneyOfMemories.Core.Managers.SceneFlowManager>();
        }

        if (flow != null && flow.LoadCurrentMap())
        {
            return;
        }

        if (!string.IsNullOrEmpty(firstLevelSceneName) && Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuController: firstLevelSceneName chưa được thiết lập hoặc chưa có trong Build Settings!");
            SetStatusMessage(GetText("Scene bắt đầu chưa có trong Build Settings!", "Start scene is not in Build Settings!"), isError: true);
            isStartingGame = false;
        }
    }

    /// <summary>
    /// BR-05: Nút Tiếp tục chỉ mở map khi LoadGame() trả về thành công.
    /// Nếu không có save hoặc tải lỗi, ở lại menu và hiển thị thông báo phù hợp.
    /// </summary>
    public void ContinueGame()
    {
        if (isStartingGame) return;

        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        if (save == null)
        {
            save = FindObjectOfType<JourneyOfMemories.Core.Managers.SaveLoadManager>();
        }
        var flow = JourneyOfMemories.Core.Managers.SceneFlowManager.Instance;
        if (flow == null)
        {
            flow = FindObjectOfType<JourneyOfMemories.Core.Managers.SceneFlowManager>();
        }

        if (save == null || !save.HasSaveData())
        {
            Debug.LogWarning("[MainMenuController] Chưa có tiến trình để tiếp tục.");
            SetStatusMessage(GetText("Chưa có tiến trình để tiếp tục!", "No saved progress to continue!"), isError: true);
            return;
        }

        isStartingGame = true;

        if (!save.LoadGame())
        {
            Debug.LogError($"[MainMenuController] Không thể tải dữ liệu save: {save.LastErrorMessage}. Giữ nguyên file save.");
            SetStatusMessage(GetText("Không thể tải tiến trình đã lưu! File save có thể bị lỗi.",
                                     "Failed to load saved progress! Save data may be corrupted."), isError: true);
            isStartingGame = false;
            return;
        }

        if (flow == null || !flow.LoadCurrentMap())
        {
            Debug.LogError("[MainMenuController] Scene của bản đồ đã lưu chưa có trong Build Settings! Giữ nguyên save.");
            SetStatusMessage(GetText("Scene của bản đồ đã lưu chưa có trong Build Settings!",
                                     "The scene for the saved map is not in Build Settings!"), isError: true);
            isStartingGame = false;
            return;
        }
    }

    private string GetText(string vi, string en)
    {
        return (LocalizationManager.CurrentLanguage == GameLanguage.Vietnamese) ? vi : en;
    }

    private void SetStatusMessage(string message, bool isError)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? new Color(0.92f, 0.25f, 0.2f, 1f) : new Color(0.15f, 0.72f, 0.35f, 1f);
        }
    }

    private void EnsureConfirmDialogExists()
    {
        if (confirmNewGameDialog != null) return;

        var existingObj = GameObject.Find("ConfirmNewGameDialog");
        if (existingObj != null)
        {
            confirmNewGameDialog = existingObj;
            if (confirmDialogMessageText == null)
            {
                var txt = existingObj.GetComponentInChildren<Text>();
                if (txt != null) confirmDialogMessageText = txt;
            }
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Tạo dialog overlay động nếu chưa có trong scene
        var dialogObj = new GameObject("ConfirmNewGameDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dialogObj.transform.SetParent(canvas.transform, false);
        dialogObj.transform.SetAsLastSibling();

        var dRect = dialogObj.GetComponent<RectTransform>();
        dRect.anchorMin = Vector2.zero;
        dRect.anchorMax = Vector2.one;
        dRect.sizeDelta = Vector2.zero;

        var dImg = dialogObj.GetComponent<Image>();
        dImg.color = new Color(0f, 0f, 0f, 0.75f);
        dImg.raycastTarget = true;

        // Hộp thoại bên trong
        var boxObj = new GameObject("DialogBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        boxObj.transform.SetParent(dialogObj.transform, false);
        var bRect = boxObj.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 0.5f);
        bRect.anchorMax = new Vector2(0.5f, 0.5f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
        bRect.sizeDelta = new Vector2(500f, 260f);

        var bImg = boxObj.GetComponent<Image>();
        bImg.color = new Color(0.15f, 0.12f, 0.18f, 0.96f);

        // Text cảnh báo
        var msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        msgObj.transform.SetParent(boxObj.transform, false);
        var mRect = msgObj.GetComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0.08f, 0.38f);
        mRect.anchorMax = new Vector2(0.92f, 0.92f);
        mRect.sizeDelta = Vector2.zero;

        var mText = msgObj.GetComponent<Text>();
        mText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (mText.font == null) mText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        mText.fontSize = 20;
        mText.alignment = TextAnchor.MiddleCenter;
        mText.color = new Color(1f, 0.95f, 0.8f, 1f);
        mText.text = "Chơi mới sẽ thay thế tiến trình hiện tại trên thiết bị này. Bạn có muốn tiếp tục?";
        confirmDialogMessageText = mText;

        // Nút HỦY (Cancel)
        var cancelBtnObj = new GameObject("CancelButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        cancelBtnObj.transform.SetParent(boxObj.transform, false);
        var cRect = cancelBtnObj.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.15f, 0.10f);
        cRect.anchorMax = new Vector2(0.45f, 0.32f);
        cRect.sizeDelta = Vector2.zero;

        var cImg = cancelBtnObj.GetComponent<Image>();
        cImg.color = new Color(0.4f, 0.2f, 0.2f, 1f);

        var cBtn = cancelBtnObj.GetComponent<Button>();
        cBtn.onClick.AddListener(CancelStartNewGame);
        confirmDialogNoButton = cBtn;

        var cTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        cTextObj.transform.SetParent(cancelBtnObj.transform, false);
        var ctRect = cTextObj.GetComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.sizeDelta = Vector2.zero;
        var ctText = cTextObj.GetComponent<Text>();
        ctText.font = mText.font;
        ctText.fontSize = 18;
        ctText.alignment = TextAnchor.MiddleCenter;
        ctText.color = Color.white;
        ctText.text = "HỦY";

        // Nút CHƠI MỚI (Confirm)
        var yesBtnObj = new GameObject("ConfirmButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        yesBtnObj.transform.SetParent(boxObj.transform, false);
        var yRect = yesBtnObj.GetComponent<RectTransform>();
        yRect.anchorMin = new Vector2(0.55f, 0.10f);
        yRect.anchorMax = new Vector2(0.85f, 0.32f);
        yRect.sizeDelta = Vector2.zero;

        var yImg = yesBtnObj.GetComponent<Image>();
        yImg.color = new Color(0.2f, 0.5f, 0.3f, 1f);

        var yBtn = yesBtnObj.GetComponent<Button>();
        yBtn.onClick.AddListener(ConfirmStartNewGame);
        confirmDialogYesButton = yBtn;

        var ytTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        ytTextObj.transform.SetParent(yesBtnObj.transform, false);
        var ytRect = ytTextObj.GetComponent<RectTransform>();
        ytRect.anchorMin = Vector2.zero;
        ytRect.anchorMax = Vector2.one;
        ytRect.sizeDelta = Vector2.zero;
        var ytText = ytTextObj.GetComponent<Text>();
        ytText.font = mText.font;
        ytText.fontSize = 18;
        ytText.alignment = TextAnchor.MiddleCenter;
        ytText.color = Color.white;
        ytText.text = "CHƠI MỚI";

        confirmNewGameDialog = dialogObj;
        dialogObj.SetActive(false);
    }

    /// <summary>
    /// Mở bảng Cài đặt
    /// </summary>
    public void OpenSettings()
    {
        if (isStartingGame) return;
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Đóng bảng Cài đặt, quay về Menu chính
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    /// <summary>
    /// Thay đổi âm lượng từ Slider
    /// </summary>
    public void OnVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    /// <summary>
    /// Thoát game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("MainMenuController: Thoát game.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
