using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JourneyOfMemories.Core.Managers;

/// <summary>
/// Quản lý màn hình "Menu màn chơi" (Chơi - Cài đặt - Thoát).
/// Hiển thị 30 frames pixel art hoạt họa mượt mà, hỗ trợ đầy đủ các tính năng:
/// - Nút "Chơi": Tiếp tục tiến trình đã lưu hoặc bắt đầu ván mới vào game.
/// - Nút "Cài đặt": Bật/tắt bảng điều chỉnh âm lượng (Volume Slider), nút Đăng xuất, phím tắt Escape.
/// - Nút "Thoát": Thoát trò chơi.
/// </summary>
public class MenuManChoiController : MonoBehaviour
{
    [Header("Scenes Configuration")]
    [Tooltip("Scene đăng nhập nếu người dùng chọn Đăng xuất")]
    public string loginSceneName = "LoginMenu";

    [Tooltip("Scene gameplay mặc định khi bắt đầu chơi")]
    public string firstLevelSceneName = "Map1";

    [Tooltip("Scene gameplay dự phòng (nếu Map1 chưa có trong Build Settings)")]
    public string fallbackLevelSceneName = "Level1_Prototype";

    [Header("Menu Buttons (Chơi - Cài đặt - Thoát)")]
    [Tooltip("Nút Chơi")]
    public Button playButton;

    [Tooltip("Nút Cài đặt")]
    public Button settingsButton;

    [Tooltip("Nút Thoát")]
    public Button quitButton;

    [Header("Settings Panel")]
    [Tooltip("Panel cài đặt")]
    public GameObject settingsPanel;

    [Tooltip("Slider điều chỉnh âm lượng")]
    public Slider volumeSlider;

    [Tooltip("Nút Đóng bảng cài đặt")]
    public Button closeSettingsButton;

    [Tooltip("Nút Đăng xuất quay lại màn hình Login")]
    public Button logoutButton;

    [Header("Screen Fade Transition")]
    [Tooltip("Overlay mờ màn hình (Fade to Black) khi chuyển cảnh")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("Thời gian hiệu ứng Fade")]
    public float fadeDuration = 0.35f;

    [Header("Audio Feedback")]
    [Tooltip("Âm thanh khi bấm nút (tùy chọn)")]
    public AudioClip clickSound;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Gán sự kiện cho các nút chính
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Gán sự kiện cho bảng Cài đặt
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Khởi tạo âm lượng từ AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioManager.Instance.musicVolume;
            }
        }
        else if (volumeSlider != null)
        {
            volumeSlider.value = 0.5f;
        }

        // Hiệu ứng mờ dần (Fade In) khi vừa vào scene
        if (screenFadeOverlay != null)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Phím Escape: Bật/tắt bảng Cài đặt
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }

        // Phím Enter hoặc Space: Bấm nút Chơi nhanh nếu chưa mở cài đặt
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (settingsPanel == null || !settingsPanel.activeSelf)
            {
                OnPlayClicked();
            }
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Chơi"
    /// </summary>
    public void OnPlayClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();
        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        isTransitioning = true;

        // Fade out màn hình
        yield return StartCoroutine(FadeOutRoutine());

        // Kiểm tra tiến trình chơi đã lưu qua SceneFlowManager hoặc SaveLoadManager
        bool loaded = false;
        var flow = SceneFlowManager.Instance;
        if (flow == null)
        {
            flow = Object.FindAnyObjectByType<SceneFlowManager>();
        }

        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        if (save == null)
        {
            save = Object.FindAnyObjectByType<JourneyOfMemories.Core.Managers.SaveLoadManager>();
        }

        // Nếu đã có save data và SceneFlowManager hỗ trợ load map hiện tại
        if (save != null && save.HasSaveData() && flow != null)
        {
            loaded = flow.LoadCurrentMap();
        }

        // Nếu chưa load được hoặc là ván mới
        if (!loaded)
        {
            if (save != null && !save.HasSaveData())
            {
                save.StartNewGame();
            }

            string target = firstLevelSceneName;
            if (!Application.CanStreamedLevelBeLoaded(target))
            {
                target = fallbackLevelSceneName;
            }

            if (Application.CanStreamedLevelBeLoaded(target))
            {
                SceneManager.LoadScene(target);
                loaded = true;
            }
        }

        if (!loaded)
        {
            Debug.LogWarning("[MenuManChoi] Không thể tìm thấy scene gameplay để tải. Mở lại menu.");
            isTransitioning = false;
            if (screenFadeOverlay != null)
            {
                StartCoroutine(FadeInRoutine());
            }
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Cài đặt"
    /// </summary>
    public void OnSettingsClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();
        OpenSettings();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Thay đổi âm lượng
    /// </summary>
    public void OnVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn "Đăng xuất" trong bảng cài đặt
    /// </summary>
    public void OnLogoutClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();
        StartCoroutine(LogoutRoutine());
    }

    private IEnumerator LogoutRoutine()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOutRoutine());

        string target = !string.IsNullOrEmpty(loginSceneName) ? loginSceneName : "LoginMenu";
        if (Application.CanStreamedLevelBeLoaded(target))
        {
            SceneManager.LoadScene(target);
        }
        else
        {
            isTransitioning = false;
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Thoát"
    /// </summary>
    public void OnQuitClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();
        Debug.Log("[MenuManChoi] Đang thoát trò chơi...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);
        }
    }

    private IEnumerator FadeInRoutine()
    {
        if (screenFadeOverlay == null) yield break;

        screenFadeOverlay.gameObject.SetActive(true);
        screenFadeOverlay.blocksRaycasts = true;

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
}
