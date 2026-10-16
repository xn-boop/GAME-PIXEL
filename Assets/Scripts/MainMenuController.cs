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

    private bool isStartingGame = false;

    private void Awake()
    {
        // Tự tìm TapToStartEffect nếu chưa gán
        if (tapToStartEffect == null)
        {
            tapToStartEffect = FindObjectOfType<TapToStartEffect>();
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

        // 3. Tải scene tiếp theo: Ưu tiên LoginMenu theo luồng, fallback sang RegisterMenu hoặc firstLevelSceneName
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
            StartNewGameConfirmed();
        }
    }

    public void ConfirmStartNewGame()
    {
        StartNewGameConfirmed();
    }

    private void StartNewGameConfirmed()
    {
        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        save?.StartNewGame();

        var flow = JourneyOfMemories.Core.Managers.SceneFlowManager.Instance;
        if (flow != null && flow.LoadCurrentMap())
        {
            return;
        }

        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuController: firstLevelSceneName chưa được thiết lập!");
            isStartingGame = false;
        }
    }

    public void ContinueGame()
    {
        var save = JourneyOfMemories.Core.Managers.SaveLoadManager.Instance;
        var flow = JourneyOfMemories.Core.Managers.SceneFlowManager.Instance;
        if (save == null || flow == null || !save.HasSaveData())
        {
            Debug.LogWarning("Không có lượt chơi để tiếp tục.");
            return;
        }
        save.LoadGame();
        flow.LoadCurrentMap();
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
