using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện và logic màn hình Đăng ký tài khoản (RegisterMenu).
/// Hỗ trợ kiểm tra hợp lệ dữ liệu, mô phỏng lưu trữ tài khoản cục bộ (Local Mock Demo),
/// hiệu ứng nhấn nút retro và chuyển cảnh mượt mà bằng Screen Fade.
/// </summary>
public class RegisterMenuController : MonoBehaviour
{
    [Header("Form Inputs")]
    [Tooltip("Ô nhập Tên người chơi")]
    public InputField usernameInput;

    [Tooltip("Ô nhập Email")]
    public InputField emailInput;

    [Tooltip("Ô nhập Mật khẩu")]
    public InputField passwordInput;

    [Tooltip("Checkbox xác nhận đồng ý điều khoản")]
    public Toggle termsToggle;

    [Header("Buttons")]
    [Tooltip("Nút bấm ĐĂNG KÝ")]
    public Button registerButton;

    [Tooltip("Nút bấm QUAY LẠI")]
    public Button backButton;

    [Tooltip("Nút bấm chuyển tab ĐĂNG NHẬP")]
    public Button loginLinkButton;

    [Header("Feedback & Status Display")]
    [Tooltip("Text hiển thị thông báo kết quả/lỗi kiểm tra")]
    public Text statusText;

    [Tooltip("Lớp phủ mờ dần màn hình khi chuyển cảnh")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("Thời gian chuyển cảnh mờ dần (giây)")]
    public float fadeDuration = 0.35f;

    [Header("Scene Navigation")]
    [Tooltip("Tên scene Menu chính để quay lại")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Tên scene Đăng nhập để chuyển sang")]
    public string loginSceneName = "LoginMenu";

    [Tooltip("Tên scene game bắt đầu sau khi đăng ký thành công")]
    public string gameSceneName = "Level1_Prototype";

    [Header("Audio Feedback")]
    [Tooltip("Nguồn phát âm thanh UI")]
    public AudioSource audioSource;

    private bool isTransitioning = false;
    private Coroutine statusBlinkCoroutine;

    // Pattern regex kiểm tra định dạng email tiêu chuẩn
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        EnsureLoginPromptExists();
    }

    private void Start()
    {
        EnsureLoginPromptExists();

        // 1. Khởi tạo trạng thái ban đầu của thông báo
        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        // 2. Gán sự kiện cho các nút bấm nếu chưa được gắn trong Inspector
        if (registerButton != null)
        {
            registerButton.onClick.RemoveListener(OnRegisterClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (loginLinkButton != null)
        {
            loginLinkButton.onClick.RemoveListener(OnLoginClicked);
            loginLinkButton.onClick.AddListener(OnLoginClicked);
        }

        // Chụp ảnh màn hình thực tế sau khi giao diện đã hiển thị đầy đủ
        StartCoroutine(CaptureScreenshotRoutine());

        // 3. Đồng bộ trạng thái checkbox điều khoản
        if (termsToggle != null)
        {
            termsToggle.onValueChanged.RemoveListener(OnTermsToggleChanged);
            termsToggle.onValueChanged.AddListener(OnTermsToggleChanged);
            OnTermsToggleChanged(termsToggle.isOn);
        }

        // 4. Hiệu ứng Fade In từ đen sang cảnh mượt mà khi mở scene
        if (screenFadeOverlay != null)
        {
            StartCoroutine(FadeInRoutine());
        }

        // Tự động focus vào ô Tên người chơi
        if (usernameInput != null)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Phím tắt bàn phím:
        // - Nhấn Escape để Quay lại Menu chính
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
            return;
        }

        // - Nhấn Enter để thực hiện Đăng ký
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnRegisterClicked();
            return;
        }

        // - Nhấn Tab để di chuyển focus giữa các ô nhập liệu
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleTabNavigation();
        }
    }

    private void HandleTabNavigation()
    {
        if (usernameInput != null && usernameInput.isFocused && emailInput != null)
        {
            emailInput.Select();
            emailInput.ActivateInputField();
        }
        else if (emailInput != null && emailInput.isFocused && passwordInput != null)
        {
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        else if (passwordInput != null && passwordInput.isFocused && usernameInput != null)
        {
            usernameInput.Select();
            usernameInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút ĐĂNG KÝ: Kiểm tra tính hợp lệ dữ liệu và mô phỏng tạo tài khoản.
    /// </summary>
    public void OnRegisterClicked()
    {
        if (isTransitioning) return;

        // Kích hoạt hiệu ứng nút nhấn nếu có component RegisterButtonEffect
        RegisterButtonEffect btnEffect = registerButton != null ? registerButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }

        // 1. Kiểm tra Tên người chơi
        string username = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(username))
        {
            SetStatusMessage(GetText("Vui lòng nhập Tên người chơi!", "Please enter Player Name!"), isError: true);
            if (usernameInput != null) usernameInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }
        if (username.Length < 3)
        {
            SetStatusMessage(GetText("Tên người chơi cần tối thiểu 3 ký tự!", "Player Name must be at least 3 characters!"), isError: true);
            if (usernameInput != null) usernameInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }

        // 2. Kiểm tra Email
        string email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(email))
        {
            SetStatusMessage(GetText("Vui lòng nhập địa chỉ Email!", "Please enter your Email address!"), isError: true);
            if (emailInput != null) emailInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }
        if (!EmailRegex.IsMatch(email))
        {
            SetStatusMessage(GetText("Email không đúng định dạng (ví dụ: user@example.com)!", "Invalid email format (e.g. user@example.com)!"), isError: true);
            if (emailInput != null) emailInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }

        // 3. Kiểm tra Mật khẩu
        string password = passwordInput != null ? passwordInput.text : string.Empty;
        if (string.IsNullOrEmpty(password))
        {
            SetStatusMessage(GetText("Vui lòng nhập Mật khẩu!", "Please enter your Password!"), isError: true);
            if (passwordInput != null) passwordInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }
        if (password.Length < 6)
        {
            SetStatusMessage(GetText("Mật khẩu cần tối thiểu 6 ký tự bảo mật!", "Password must be at least 6 characters!"), isError: true);
            if (passwordInput != null) passwordInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }

        // 4. Kiểm tra Checkbox đồng ý điều khoản
        if (termsToggle != null && !termsToggle.isOn)
        {
            SetStatusMessage(GetText("Bạn cần đồng ý với Điều khoản sử dụng để tiếp tục!", "You must agree to the Terms of Service to continue!"), isError: true);
            PlayFeedbackTone(320f, 0.18f);
            return;
        }

        // 5. Dữ liệu hoàn toàn hợp lệ -> Thực hiện mô phỏng đăng ký tài khoản (Local Mock)
        StartCoroutine(SimulateRegistrationRoutine(username, email));
    }

    private string GetText(string vi, string en)
    {
        return (LocalizationManager.CurrentLanguage == GameLanguage.Vietnamese) ? vi : en;
    }

    /// <summary>
    /// Đồng bộ trạng thái hiển thị của checkmark theo giá trị bật/tắt của toggle.
    /// </summary>
    public void OnTermsToggleChanged(bool isOn)
    {
        if (termsToggle != null && termsToggle.graphic != null)
        {
            termsToggle.graphic.gameObject.SetActive(isOn);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn dòng "Đã có tài khoản? ĐĂNG NHẬP": Kích hoạt hiệu ứng nảy nút và chuyển mượt mà sang LoginMenu.
    /// </summary>
    public void OnLoginClicked()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        SetInteractable(false);

        // 1. Kích hoạt hiệu ứng nút nhấn (Scale Punch + Chime sound)
        RegisterButtonEffect btnEffect = loginLinkButton != null ? loginLinkButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }
        else
        {
            PlayFeedbackTone(1150f, 0.15f);
        }

        SetStatusMessage(GetText("Đang chuyển sang ĐĂNG NHẬP...", "Switching to LOG IN..."), isError: false);
        Debug.Log("[RegisterMenu] Đã nhấn 'ĐĂNG NHẬP': Kích hoạt hiệu ứng nhấn và chuyển cảnh sang LoginMenu.");

        StartCoroutine(TransitionToLoginRoutine());
    }

    private IEnumerator TransitionToLoginRoutine()
    {
        yield return new WaitForSecondsRealtime(0.18f);
        yield return StartCoroutine(FadeOutRoutine());

        if (!string.IsNullOrEmpty(loginSceneName) && Application.CanStreamedLevelBeLoaded(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
        else
        {
            Debug.LogWarning($"[RegisterMenu] Scene '{loginSceneName}' chưa sẵn sàng, giữ nguyên.");
            isTransitioning = false;
            SetInteractable(true);
            if (screenFadeOverlay != null)
            {
                StartCoroutine(FadeInRoutine());
            }
        }
    }

    /// <summary>
    /// Quy trình mô phỏng đăng ký và chuyển cảnh vào game.
    /// Nêu rõ giới hạn mô phỏng cục bộ trong log console và UI.
    /// </summary>
    private IEnumerator SimulateRegistrationRoutine(string username, string email)
    {
        isTransitioning = true;

        // Khóa tương tác trong quá trình xử lý
        SetInteractable(false);

        SetStatusMessage(GetText("Đang xác thực thông tin...", "Verifying information..."), isError: false);
        yield return new WaitForSecondsRealtime(0.4f);

        // Lưu trữ thông tin tài khoản mô phỏng vào PlayerPrefs cục bộ
        PlayerPrefs.SetString("RegisteredUser_Name", username);
        PlayerPrefs.SetString("RegisteredUser_Email", email);
        PlayerPrefs.SetInt("RegisteredUser_IsMock", 1);
        PlayerPrefs.SetString("RegisteredUser_Timestamp", DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();

        // Ghi log chi tiết nêu rõ giới hạn mô phỏng backend
        Debug.Log($"[RegisterMenu - Simulation] Đăng ký thành công giả lập (Local Mock). " +
                  $"Tên: '{username}', Email: '{email}'. " +
                  $"Lưu ý: Chưa có máy chủ Backend thực tế; tài khoản được lưu cục bộ trên máy.");

        // Thông báo thành công trên giao diện
        SetStatusMessage(GetText("Đăng ký thành công! (Mô phỏng máy chủ cục bộ). Đang vào game...", "Registration successful! (Local simulation). Entering game..."), isError: false);
        PlayFeedbackTone(1050f, 0.22f);

        yield return new WaitForSecondsRealtime(0.7f);

        // Khởi tạo trạng thái game mới nếu hệ sinh thái Core Logic đã khởi tạo
        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        save?.StartNewGame();

        // Hiệu ứng mờ dần màn hình sang đen trước khi đổi Scene
        yield return StartCoroutine(FadeOutRoutine());

        // Chuyển sang scene Gameplay
        if (!string.IsNullOrEmpty(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            var flow = JourneyOfMemories.Core.Managers.SceneFlowManager.Instance;
            if (flow == null || !flow.LoadCurrentMap())
            {
                Debug.LogWarning($"[RegisterMenu] Scene '{gameSceneName}' chưa sẵn sàng, thử tải 'Level1_Prototype'.");
                SceneManager.LoadScene("Level1_Prototype");
            }
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút QUAY LẠI: Chuyển mượt mà về MainMenu.
    /// </summary>
    public void OnBackClicked()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        SetInteractable(false);

        RegisterButtonEffect btnEffect = backButton != null ? backButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }
        else
        {
            PlayFeedbackTone(600f, 0.12f);
        }

        StartCoroutine(BackToMainMenuRoutine());
    }

    private IEnumerator BackToMainMenuRoutine()
    {
        yield return new WaitForSecondsRealtime(0.18f);

        // Fade màn hình sang đen
        yield return StartCoroutine(FadeOutRoutine());

        // Tải MainMenu
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Cập nhật nội dung và màu sắc thông báo hiển thị cho người chơi.
    /// </summary>
    private void SetStatusMessage(string message, bool isError)
    {
        if (statusText == null) return;

        statusText.text = message;
        // Màu đỏ ấm pixel art cho lỗi, màu xanh tươi pixel art cho thành công
        statusText.color = isError ? new Color(0.92f, 0.25f, 0.2f, 1f) : new Color(0.15f, 0.72f, 0.35f, 1f);

        if (statusBlinkCoroutine != null)
        {
            StopCoroutine(statusBlinkCoroutine);
        }
        statusBlinkCoroutine = StartCoroutine(BlinkStatusRoutine());
    }

    private IEnumerator BlinkStatusRoutine()
    {
        if (statusText == null) yield break;

        Transform t = statusText.transform;
        Vector3 orig = Vector3.one;

        // Nhấp nháy nảy nhẹ gây chú ý
        for (float time = 0; time < 0.18f; time += Time.unscaledDeltaTime)
        {
            float scale = Mathf.Lerp(1.1f, 1f, time / 0.18f);
            t.localScale = orig * scale;
            yield return null;
        }
        t.localScale = orig;
    }

    private void SetInteractable(bool state)
    {
        if (usernameInput != null) usernameInput.interactable = state;
        if (emailInput != null) emailInput.interactable = state;
        if (passwordInput != null) passwordInput.interactable = state;
        if (termsToggle != null) termsToggle.interactable = state;
        if (registerButton != null) registerButton.interactable = state;
        if (backButton != null) backButton.interactable = state;
        if (loginLinkButton != null) loginLinkButton.interactable = state;
    }

    private IEnumerator FadeInRoutine()
    {
        if (screenFadeOverlay == null) yield break;

        screenFadeOverlay.gameObject.SetActive(true);
        screenFadeOverlay.blocksRaycasts = true;
        screenFadeOverlay.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenFadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        screenFadeOverlay.alpha = 0f;
        screenFadeOverlay.blocksRaycasts = false;
        screenFadeOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutRoutine()
    {
        if (screenFadeOverlay == null)
        {
            yield return new WaitForSecondsRealtime(fadeDuration);
            yield break;
        }

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

    private void PlayFeedbackTone(float frequency, float duration)
    {
        if (audioSource == null) return;

        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * (1f / duration * 3.5f));
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
            data[i] = wave * env * 0.35f;
        }

        AudioClip clip = AudioClip.Create("Tone_" + frequency, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip, 0.75f);
    }

    private IEnumerator CaptureScreenshotRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        try
        {
            ScreenCapture.CaptureScreenshot("D:/game/picture/reggister/actual_game_screenshot.png");
            Debug.Log("[RegisterMenuController] Đã chụp ảnh màn hình Game View vào: D:/game/picture/reggister/actual_game_screenshot.png");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[RegisterMenuController] Không thể chụp ảnh màn hình: " + ex.Message);
        }
    }

    /// <summary>
    /// Đảm bảo dòng chữ 'Đã có tài khoản? ĐĂNG NHẬP' luôn tồn tại và hiển thị chính xác.
    /// Tự động khởi tạo nếu thiếu trong Scene để người chơi luôn nhìn thấy.
    /// </summary>
    public void EnsureLoginPromptExists()
    {
        GameObject promptObj = GameObject.Find("LoginPrompt");
        if (promptObj == null)
        {
            Transform formTransform = null;
            var formObj = GameObject.Find("RegisterForm");
            if (formObj != null) formTransform = formObj.transform;

            if (formTransform == null)
            {
                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null) formTransform = canvas.transform;
            }

            if (formTransform != null)
            {
                promptObj = new GameObject("LoginPrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
                promptObj.transform.SetParent(formTransform, false);

                RectTransform rect = promptObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -315f);
                rect.sizeDelta = new Vector2(400f, 36f);

                Image img = promptObj.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.001f);
                img.raycastTarget = true;

                Button btn = promptObj.GetComponent<Button>();
                RegisterButtonEffect effect = promptObj.GetComponent<RegisterButtonEffect>();
                effect.chimeFrequency = 1150f;
                effect.punchDuration = 0.18f;
                effect.pressedScale = 0.94f;

                loginLinkButton = btn;
            }
        }

        if (promptObj != null)
        {
            if (loginLinkButton == null)
            {
                loginLinkButton = promptObj.GetComponent<Button>();
            }

            RectTransform pRect = promptObj.GetComponent<RectTransform>();
            if (pRect != null)
            {
                pRect.anchoredPosition = new Vector2(0f, -315f);
                pRect.sizeDelta = new Vector2(400f, 36f);
            }

            // Text
            Transform textChild = promptObj.transform.Find("Text");
            GameObject textObj = textChild != null ? textChild.gameObject : null;
            if (textObj == null)
            {
                textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObj.transform.SetParent(promptObj.transform, false);
            }

            RectTransform tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.anchoredPosition = Vector2.zero;

            Text txt = textObj.GetComponent<Text>();
            if (txt.font == null)
            {
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.color = new Color(0.28f, 0.15f, 0.10f, 1f); // #47261A
            txt.text = "Đã có tài khoản? <color=#D23C3C>ĐĂNG NHẬP</color>";
            txt.raycastTarget = false;

            // Underline
            Transform underChild = promptObj.transform.Find("Underline");
            GameObject underObj = underChild != null ? underChild.gameObject : null;
            if (underObj == null)
            {
                underObj = new GameObject("Underline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                underObj.transform.SetParent(promptObj.transform, false);
            }

            RectTransform uRect = underObj.GetComponent<RectTransform>();
            uRect.anchorMin = new Vector2(0.5f, 0.5f);
            uRect.anchorMax = new Vector2(0.5f, 0.5f);
            uRect.pivot = new Vector2(0.5f, 0.5f);
            uRect.anchoredPosition = new Vector2(83.5f, -11f);
            uRect.sizeDelta = new Vector2(124f, 2f);

            Image uImg = underObj.GetComponent<Image>();
            uImg.color = new Color(0.824f, 0.235f, 0.235f, 1f); // #D23C3C
            uImg.raycastTarget = false;
        }
    }
}
