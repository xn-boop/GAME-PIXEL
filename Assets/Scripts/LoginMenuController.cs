using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện và logic màn hình Đăng nhập (LoginMenu).
/// Tương thích pixel-art, hỗ trợ kiểm tra tính hợp lệ dữ liệu, ghi nhớ đăng nhập (PlayerPrefs),
/// mô phỏng xác thực máy chủ cục bộ (Local Mock Demo), hiệu ứng nhấn nút retro và chuyển cảnh mượt mà.
/// </summary>
public class LoginMenuController : MonoBehaviour
{
    [Header("Form Inputs")]
    [Tooltip("Ô nhập Email")]
    public InputField emailInput;

    [Tooltip("Ô nhập Mật khẩu")]
    public InputField passwordInput;

    [Tooltip("Checkbox Ghi nhớ đăng nhập")]
    public Toggle rememberToggle;

    [Header("Buttons & Links")]
    [Tooltip("Nút ĐĂNG NHẬP")]
    public Button loginButton;

    [Tooltip("Nút QUAY LẠI")]
    public Button backButton;

    [Tooltip("Nút Quên mật khẩu?")]
    public Button forgotPasswordButton;

    [Tooltip("Dòng Chưa có tài khoản? ĐĂNG KÝ")]
    public Button registerLinkButton;

    [Header("Feedback & Status Display")]
    [Tooltip("Text hiển thị thông báo lỗi/thành công")]
    public Text statusText;

    [Tooltip("Lớp phủ mờ dần màn hình khi chuyển cảnh")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("Thời gian chuyển cảnh mờ dần (giây)")]
    public float fadeDuration = 0.35f;

    [Header("Scene Navigation")]
    [Tooltip("Tên scene Menu chính để quay lại")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Tên scene Đăng ký")]
    public string registerMenuSceneName = "RegisterMenu";

    [Tooltip("Tên scene game bắt đầu sau khi đăng nhập thành công")]
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
    }

    private void Start()
    {
        // 1. Khởi tạo trạng thái thông báo
        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        // 2. Gán sự kiện cho các nút bấm
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnLoginClicked);
            loginButton.onClick.AddListener(OnLoginClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (forgotPasswordButton != null)
        {
            forgotPasswordButton.onClick.RemoveListener(OnForgotPasswordClicked);
            forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        }

        if (registerLinkButton != null)
        {
            registerLinkButton.onClick.RemoveListener(OnRegisterClicked);
            registerLinkButton.onClick.AddListener(OnRegisterClicked);
        }

        // 3. Khôi phục trạng thái Ghi nhớ đăng nhập từ PlayerPrefs
        if (rememberToggle != null)
        {
            bool isRemembered = PlayerPrefs.GetInt("RememberLogin", 0) == 1;
            rememberToggle.isOn = isRemembered;
            OnRememberToggleChanged(isRemembered);

            rememberToggle.onValueChanged.RemoveListener(OnRememberToggleChanged);
            rememberToggle.onValueChanged.AddListener(OnRememberToggleChanged);

            if (isRemembered && PlayerPrefs.HasKey("SavedLogin_Email") && emailInput != null)
            {
                emailInput.text = PlayerPrefs.GetString("SavedLogin_Email", string.Empty);
            }
        }

        // 4. Hiệu ứng Fade In khi mở Scene
        if (screenFadeOverlay != null)
        {
            StartCoroutine(FadeInRoutine());
        }

        // 5. Tự động chọn ô nhập liệu
        if (emailInput != null)
        {
            if (string.IsNullOrEmpty(emailInput.text))
            {
                emailInput.Select();
                emailInput.ActivateInputField();
            }
            else if (passwordInput != null)
            {
                passwordInput.Select();
                passwordInput.ActivateInputField();
            }
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Phím tắt bàn phím:
        // - Escape: Quay lại MainMenu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackClicked();
            return;
        }

        // - Enter: Đăng nhập
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnLoginClicked();
            return;
        }

        // - Tab: Chuyển focus giữa Email và Mật khẩu
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleTabNavigation();
        }
    }

    private void HandleTabNavigation()
    {
        if (emailInput != null && emailInput.isFocused && passwordInput != null)
        {
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        else if (passwordInput != null && passwordInput.isFocused && emailInput != null)
        {
            emailInput.Select();
            emailInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Xử lý bật/tắt checkbox Ghi nhớ đăng nhập
    /// </summary>
    public void OnRememberToggleChanged(bool isOn)
    {
        if (rememberToggle != null && rememberToggle.graphic != null)
        {
            rememberToggle.graphic.gameObject.SetActive(isOn);
        }

        PlayerPrefs.SetInt("RememberLogin", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Xử lý khi nhấn nút ĐĂNG NHẬP: Kiểm tra dữ liệu và thực hiện mô phỏng đăng nhập.
    /// </summary>
    public void OnLoginClicked()
    {
        if (isTransitioning) return;

        RegisterButtonEffect btnEffect = loginButton != null ? loginButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }

        // 1. Kiểm tra Email
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

        // 2. Kiểm tra Mật khẩu
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
            SetStatusMessage(GetText("Mật khẩu cần tối thiểu 6 ký tự!", "Password must be at least 6 characters!"), isError: true);
            if (passwordInput != null) passwordInput.Select();
            PlayFeedbackTone(350f, 0.15f);
            return;
        }

        // 3. Dữ liệu hợp lệ -> Khởi động mô phỏng đăng nhập
        StartCoroutine(SimulateLoginRoutine(email, password));
    }

    /// <summary>
    /// Xử lý khi nhấn Quên mật khẩu?
    /// </summary>
    public void OnForgotPasswordClicked()
    {
        if (isTransitioning) return;

        RegisterButtonEffect btnEffect = forgotPasswordButton != null ? forgotPasswordButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }
        else
        {
            PlayFeedbackTone(800f, 0.12f);
        }

        string email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(email))
        {
            SetStatusMessage(GetText("Vui lòng nhập Email để nhận hướng dẫn khôi phục!", "Please enter Email to receive recovery instructions!"), isError: false);
        }
        else
        {
            SetStatusMessage(GetText("Tính năng khôi phục đang phát triển. Vui lòng đăng ký tài khoản mới!", "Password recovery is in development. Please create a new account!"), isError: false);
        }
    }

    /// <summary>
    /// Chuyển sang màn hình Đăng ký tài khoản (RegisterMenu)
    /// </summary>
    public void OnRegisterClicked()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        SetInteractable(false);

        RegisterButtonEffect btnEffect = registerLinkButton != null ? registerLinkButton.GetComponent<RegisterButtonEffect>() : null;
        if (btnEffect != null)
        {
            btnEffect.TriggerEffect();
        }
        else
        {
            PlayFeedbackTone(1050f, 0.15f);
        }

        StartCoroutine(TransitionToSceneRoutine(registerMenuSceneName));
    }

    /// <summary>
    /// Quay lại Menu chính (MainMenu)
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

        StartCoroutine(TransitionToSceneRoutine(mainMenuSceneName));
    }

    private IEnumerator SimulateLoginRoutine(string email, string password)
    {
        isTransitioning = true;
        SetInteractable(false);

        SetStatusMessage(GetText("Đang xác thực thông tin...", "Verifying credentials..."), isError: false);
        yield return new WaitForSecondsRealtime(0.4f);

        // Lưu thông tin phiên đăng nhập mô phỏng vào PlayerPrefs
        PlayerPrefs.SetString("LoggedInUser_Email", email);
        PlayerPrefs.SetInt("LoggedInUser_IsMock", 1);
        PlayerPrefs.SetString("LoggedInUser_Timestamp", DateTime.UtcNow.ToString("o"));

        if (rememberToggle != null && rememberToggle.isOn)
        {
            PlayerPrefs.SetString("SavedLogin_Email", email);
        }
        else
        {
            PlayerPrefs.DeleteKey("SavedLogin_Email");
        }
        PlayerPrefs.Save();

        // Ghi log chi tiết nêu rõ giới hạn mô phỏng backend
        Debug.Log($"[LoginMenu - Simulation] Đăng nhập thành công giả lập (Local Mock Demo). " +
                  $"Email: '{email}'. " +
                  $"Lưu ý: Chưa có máy chủ Backend thực tế; tài khoản và phiên đăng nhập được lưu cục bộ trên máy.");

        // Thông báo thành công trên giao diện
        SetStatusMessage(GetText("Đăng nhập thành công! (Mô phỏng máy chủ cục bộ). Đang vào game...", "Login successful! (Local simulation). Entering game..."), isError: false);
        PlayFeedbackTone(1050f, 0.22f);

        yield return new WaitForSecondsRealtime(0.7f);

        // Khởi tạo trạng thái game
        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        save?.StartNewGame();

        // Hiệu ứng mờ dần màn hình sang đen
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
                Debug.LogWarning($"[LoginMenu] Scene '{gameSceneName}' chưa sẵn sàng, thử tải 'Level1_Prototype'.");
                SceneManager.LoadScene("Level1_Prototype");
            }
        }
    }

    private IEnumerator TransitionToSceneRoutine(string targetScene)
    {
        yield return new WaitForSecondsRealtime(0.18f);
        yield return StartCoroutine(FadeOutRoutine());

        if (!string.IsNullOrEmpty(targetScene) && Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning($"[LoginMenu] Không thể tải scene '{targetScene}', quay về 'MainMenu'.");
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void SetStatusMessage(string message, bool isError)
    {
        if (statusText == null) return;

        statusText.text = message;
        // Màu đỏ ấm pixel art cho lỗi, màu xanh tươi cho thành công/thông tin
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
        if (emailInput != null) emailInput.interactable = state;
        if (passwordInput != null) passwordInput.interactable = state;
        if (rememberToggle != null) rememberToggle.interactable = state;
        if (loginButton != null) loginButton.interactable = state;
        if (backButton != null) backButton.interactable = state;
        if (forgotPasswordButton != null) forgotPasswordButton.interactable = state;
        if (registerLinkButton != null) registerLinkButton.interactable = state;
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

    private string GetText(string vi, string en)
    {
        return (LocalizationManager.CurrentLanguage == GameLanguage.Vietnamese) ? vi : en;
    }
}
