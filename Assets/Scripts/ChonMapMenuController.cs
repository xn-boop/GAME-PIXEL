using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JourneyOfMemories.Core.Managers;

/// <summary>
/// Quản lý giao diện và logic màn hình "Chọn map" (Menu màn chơi).
/// Thiết kế pixel-art chuẩn, hoạt động độc lập (standalone), không phụ thuộc hay làm thay đổi bất kỳ scene nào khác.
/// Hỗ trợ đầy đủ:
/// - Nút "Chơi": Mở bảng chọn màn chơi (Màn 0, Màn 1, Màn 2, Màn 3) kèm thông báo trực quan.
/// - Nút "Cài đặt": Bật/tắt bảng điều chỉnh âm lượng (Volume Slider), âm thanh SFX, phím tắt Escape.
/// - Nút "Thoát": Thoát trò chơi hoặc dừng PlayMode.
/// - Bảng chọn map chi tiết: Cho phép chọn từng màn chơi với phản hồi âm thanh retro.
/// - Hiệu ứng chuyển cảnh ScreenFadeOverlay mượt mà.
/// </summary>
public class ChonMapMenuController : MonoBehaviour
{
    [Header("Menu Main Buttons (Chơi - Cài đặt - Thoát)")]
    [Tooltip("Nút Chơi trên biển gỗ")]
    public Button playButton;

    [Tooltip("Nút Cài đặt trên biển gỗ")]
    public Button settingsButton;

    [Tooltip("Nút Thoát trên biển gỗ")]
    public Button quitButton;

    [Header("Settings Panel")]
    [Tooltip("Panel cài đặt")]
    public GameObject settingsPanel;

    [Tooltip("Slider điều chỉnh âm lượng nhạc nền (BGM)")]
    public Slider volumeSlider;

    [Tooltip("Slider điều chỉnh âm lượng hiệu ứng (SFX)")]
    public Slider sfxSlider;

    [Tooltip("Nút Đóng bảng cài đặt")]
    public Button closeSettingsButton;

    [Header("Map Selection Panel (Chọn Map)")]
    [Tooltip("Panel chọn màn chơi")]
    public GameObject mapSelectionPanel;

    [Tooltip("Nút chọn Màn 0 (Mở Đầu - Prologue)")]
    public Button map0Button;

    [Tooltip("Nút chọn Màn 1 (Quán Cơm Việt - Hàn)")]
    public Button map1Button;

    [Tooltip("Nút chọn Màn 2 (Phố Cổ Hội An)")]
    public Button map2Button;

    [Tooltip("Nút chọn Màn 3 (Đền Ký Ức - Final)")]
    public Button map3Button;

    [Tooltip("Nút Đóng bảng chọn map")]
    public Button closeMapSelectButton;

    [Header("Standalone / Scene Transition Configuration")]
    [Tooltip("Chế độ màn riêng lẻ: Không liên kết/chuyển scene thực tế để giữ độc lập hoàn toàn")]
    public bool isStandaloneMode = false;

    [Tooltip("Cho phép chuyển scene khi click map nếu tắt chế độ độc lập")]
    public bool allowSceneTransition = true;

    [Tooltip("Tên scene Map 0 để chuyển vào thẳng khi bấm Chơi")]
    public string targetPlaySceneName = "Map0_Prologue";

    [Tooltip("Tên scene Menu chính để quay về khi bấm Thoát")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Âm lượng hiệu ứng SFX cục bộ")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.85f;

    [Header("Feedback & Status Display")]
    [Tooltip("Text hiển thị thông báo trạng thái hoặc phản hồi người dùng")]
    public Text statusText;

    [Tooltip("Lớp phủ mờ dần màn hình khi vào/thoát cảnh")]
    public CanvasGroup screenFadeOverlay;

    [Tooltip("Thời gian chuyển cảnh mờ dần (giây)")]
    public float fadeDuration = 0.35f;

    [Header("Audio Feedback")]
    [Tooltip("Nguồn phát âm thanh UI")]
    public AudioSource audioSource;

    [Tooltip("Clip âm thanh nhấn nút (tự tạo nếu để trống)")]
    public AudioClip clickClip;

    private bool isTransitioning = false;
    private Coroutine statusCoroutine;

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

        // Loại bỏ hoàn toàn menu chọn map màu nâu theo yêu cầu người dùng
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
            Destroy(mapSelectionPanel);
            mapSelectionPanel = null;
        }

        // Đăng ký sự kiện các nút chính
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

        // Đăng ký sự kiện bảng Cài đặt
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        // Đăng ký sự kiện bảng Chọn Map (nếu có dự phòng)
        if (closeMapSelectButton != null)
        {
            closeMapSelectButton.onClick.RemoveListener(CloseMapSelect);
            closeMapSelectButton.onClick.AddListener(CloseMapSelect);
        }

        if (map0Button != null)
        {
            map0Button.onClick.RemoveListener(OnMap0Clicked);
            map0Button.onClick.AddListener(OnMap0Clicked);
        }

        if (map1Button != null)
        {
            map1Button.onClick.RemoveListener(OnMap1Clicked);
            map1Button.onClick.AddListener(OnMap1Clicked);
        }

        if (map2Button != null)
        {
            map2Button.onClick.RemoveListener(OnMap2Clicked);
            map2Button.onClick.AddListener(OnMap2Clicked);
        }

        if (map3Button != null)
        {
            map3Button.onClick.RemoveListener(OnMap3Clicked);
            map3Button.onClick.AddListener(OnMap3Clicked);
        }
    }

    private void Start()
    {
        // Ẩn các popup khi khởi đầu
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);

        // Khởi tạo slider âm lượng và phát bài nhạc nền menu (zephiramusic) ở trang Chọn map
        float currentBgm = 0.75f;

        if (AudioManager.Instance != null)
        {
            currentBgm = AudioManager.Instance.musicVolume;
            try { AudioManager.Instance.PlayMainMenuMusic(); } catch { }
        }

        if (volumeSlider != null) volumeSlider.value = currentBgm;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        // Fade In vào cảnh
        if (screenFadeOverlay != null)
        {
            StartCoroutine(FadeInRoutine());
        }

        // Tự động gắn icon âm thanh góc trên bên phải chuẩn Map 1
        EnsureSoundController();

        ShowStatus("Chào mừng đến Menu Chọn Map!", 3f);
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Phím Escape: Đóng panel Cài đặt hoặc mở Cài đặt
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

        // Phím Enter hoặc Space: Bấm nút Chơi nếu không có panel nào mở
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (settingsPanel == null || !settingsPanel.activeSelf)
            {
                OnPlayClicked();
            }
        }
    }

    #region Main Button Handlers

    /// <summary>
    /// Xử lý khi nhấn nút "Chơi" -> Chuyển thẳng vào Màn 0 (Map0_Prologue)
    /// </summary>
    public void OnPlayClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
        }

        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }

        ShowStatus("Bắt đầu: Màn 0 - Căn Phòng Ký Ức...", 2f);
        StartCoroutine(LoadSceneRoutine(targetPlaySceneName));
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Cài đặt" -> Mở bảng Cài đặt
    /// </summary>
    public void OnSettingsClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();

        var soundCtrl = GetComponent<MenuSoundController>() ?? UnityEngine.Object.FindAnyObjectByType<MenuSoundController>();
        if (soundCtrl != null)
        {
            soundCtrl.OpenSoundModal();
            return;
        }

        if (mapSelectionPanel != null && mapSelectionPanel.activeSelf)
        {
            mapSelectionPanel.SetActive(false);
        }

        OpenSettings();
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Thoát" -> Quay về trang MainMenu
    /// </summary>
    public void OnQuitClicked()
    {
        if (isTransitioning) return;
        PlayClickSound();
        ShowStatus("Đang quay về Menu chính...", 2f);
        StartCoroutine(QuitToMainMenuRoutine());
    }

    private IEnumerator QuitToMainMenuRoutine()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOutRoutine());

        Debug.Log($"[ChonMapMenu] Đang chuyển về Menu chính: '{mainMenuSceneName}'...");
        string target = !string.IsNullOrEmpty(mainMenuSceneName) ? mainMenuSceneName : "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(target))
        {
            SceneManager.LoadScene(target);
        }
        else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/" + target + ".unity"))
        {
            SceneManager.LoadScene("Assets/Scenes/" + target + ".unity");
        }
        else
        {
            Debug.LogWarning($"[ChonMapMenu] Không thể kiểm tra CanStreamedLevelBeLoaded cho '{target}', tải trực tiếp qua SceneManager.LoadScene.");
            SceneManager.LoadScene(target);
        }
    }

    #endregion

    #region Settings Panel Logic

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            ShowStatus("Đã mở bảng Cài đặt", 2f);
        }
    }

    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            ShowStatus("Đã đóng bảng Cài đặt", 2f);
        }
    }

    public void OnMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    public void OnSfxVolumeChanged(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(sfxVolume);
        }
        if (audioSource != null)
        {
            audioSource.volume = sfxVolume;
        }
    }

    #endregion

    #region Map Selection Logic

    public void OpenMapSelect()
    {
        OnPlayClicked();
    }

    public void CloseMapSelect()
    {
        PlayClickSound();
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
            ShowStatus("Đã đóng bảng Chọn Map", 2f);
        }
    }

    public void OnMap0Clicked()
    {
        HandleMapSelected("Màn 0: Mở Đầu (Prologue)", "Map0_Prologue");
    }

    public void OnMap1Clicked()
    {
        HandleMapSelected("Màn 1: Quán Cơm Việt - Hàn", "Level1_Prototype");
    }

    public void OnMap2Clicked()
    {
        HandleMapSelected("Màn 2: Phố Cổ Hội An", "Scene_Level2");
    }

    public void OnMap3Clicked()
    {
        HandleMapSelected("Màn 3: Đền Ký Ức (Final)", "Map3_Final");
    }

    private void HandleMapSelected(string mapDisplayName, string sceneName)
    {
        if (isTransitioning) return;
        PlayClickSound();

        Debug.Log($"[ChonMapMenu] Người chơi đã chọn: {mapDisplayName} (Scene: {sceneName})");

        if (isStandaloneMode || !allowSceneTransition)
        {
            // Chế độ độc lập: Hiển thị thông báo xác nhận thành công mà không ép chuyển scene
            ShowStatus($"Đã chọn: {mapDisplayName} (Chế độ độc lập)", 3.5f);
        }
        else
        {
            // Nếu được bật cho phép chuyển cảnh
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;
        if (AudioManager.Instance != null)
        {
            try { AudioManager.Instance.StopMusic(); } catch { }
        }
        yield return StartCoroutine(FadeOutRoutine());

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/" + sceneName + ".unity"))
        {
            SceneManager.LoadScene("Assets/Scenes/" + sceneName + ".unity");
        }
        else
        {
            Debug.LogWarning($"[ChonMapMenu] Scene '{sceneName}' chưa được kích hoạt trong Build Settings!");
            isTransitioning = false;
            if (screenFadeOverlay != null)
            {
                StartCoroutine(FadeInRoutine());
            }
        }
    }

    #endregion

    #region Helper Methods

    public void ShowStatus(string message, float duration = 2.5f)
    {
        if (statusText == null) return;

        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }
        statusCoroutine = StartCoroutine(StatusRoutine(message, duration));
    }

    private IEnumerator StatusRoutine(string message, float duration)
    {
        statusText.text = message;
        statusText.color = new Color(0.28f, 0.15f, 0.10f, 1f); // #47261A
        yield return new WaitForSecondsRealtime(duration);

        // Fade text out
        float fade = 0.5f;
        float elapsed = 0f;
        Color c = statusText.color;
        while (elapsed < fade)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fade);
            statusText.color = c;
            yield return null;
        }
        statusText.text = "";
    }

    private void PlayClickSound()
    {
        if (clickClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickClip, sfxVolume);
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSound(sfxVolume);
            return;
        }

        if (audioSource != null)
        {
            if (clickClip == null)
            {
                clickClip = CreateClickChime();
            }
            audioSource.PlayOneShot(clickClip, sfxVolume);
        }
    }

    private AudioClip CreateClickChime()
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * 0.09f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float wave = Mathf.Sin(2f * Mathf.PI * 980f * t);
            float envelope = Mathf.Exp(-5f * progress);
            data[i] = wave * envelope * 0.45f;
        }
        AudioClip clip = AudioClip.Create("ChonMapClick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
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

    #endregion

    private void EnsureSoundController()
    {
        var soundCtrl = GetComponent<MenuSoundController>();
        if (soundCtrl == null)
        {
            soundCtrl = gameObject.AddComponent<MenuSoundController>();
        }
    }
}
