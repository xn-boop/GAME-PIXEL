using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý màn hình Menu chính / Bắt đầu game.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Tên scene gameplay sẽ tải khi nhấn nút Bắt đầu")]
    public string firstLevelSceneName = "Level1_Prototype";

    [Header("UI Panels")]
    [Tooltip("Panel menu chính")]
    public GameObject mainPanel;

    [Tooltip("Panel cài đặt (nếu có)")]
    public GameObject settingsPanel;

    [Header("Audio (Optional)")]
    [Tooltip("Slider chỉnh âm lượng nhạc nền")]
    public Slider musicVolumeSlider;

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
    /// Bắt đầu game - Chuyển sang scene màn chơi
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("MainMenuController: Đang vào game...");
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuController: firstLevelSceneName chưa được thiết lập!");
        }
    }

    /// <summary>
    /// Mở bảng Cài đặt
    /// </summary>
    public void OpenSettings()
    {
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
