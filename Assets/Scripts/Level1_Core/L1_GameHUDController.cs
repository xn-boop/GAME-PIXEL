using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class L1_GameHUDController : MonoBehaviour
{
    public static L1_GameHUDController Instance { get; private set; }

    [Header("Top-Right Buttons")]
    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnSound;
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnHelp;

    [Header("Sound Icon Toggle")]
    [SerializeField] private Image imgSoundIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Modals / Popups")]
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelGuide;
    [SerializeField] private GameObject panelConfirmHome;
    [SerializeField] private GameObject panelSound;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button btnResumePause;
    [SerializeField] private Button btnRestartPause;
    [SerializeField] private Button btnHomePause;
    [SerializeField] private Button btnClosePause;

    [Header("Guide Panel Buttons")]
    [SerializeField] private Button btnCloseGuide;
    [SerializeField] private Button btnCloseGuideAlt;

    [Header("Confirm Home Buttons")]
    [SerializeField] private Button btnConfirmHomeYes;
    [SerializeField] private Button btnConfirmHomeNo;
    [SerializeField] private Button btnCloseConfirmHome;

    [Header("Sound Panel Controls")]
    [SerializeField] private Button btnToggleSoundModal;
    [SerializeField] private TextMeshProUGUI txtSoundStatusModal;
    [SerializeField] private Button btnCloseSound;
    [SerializeField] private Button btnCloseSoundAlt;

    private bool isPaused = false;
    private bool isMuted = false;
    private bool isGuideOpen = false;
    private bool isConfirmHomeOpen = false;
    private bool isSoundOpen = false;

    public bool IsPaused => isPaused;
    public bool IsMuted => isMuted;
    public bool IsGuideOpen => isGuideOpen;
    public bool IsConfirmHomeOpen => isConfirmHomeOpen;
    public bool IsSoundOpen => isSoundOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load saved mute state
        isMuted = AudioManager.Instance != null ? AudioManager.Instance.IsMuted : (PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1);
        ApplyAudioMute(isMuted, notify: false);

        // Wire Top Right Buttons
        if (btnPause != null) btnPause.onClick.AddListener(TogglePause);
        if (btnSound != null) btnSound.onClick.AddListener(OnSoundButtonClicked);
        if (btnHome != null) btnHome.onClick.AddListener(OpenConfirmHome);
        if (btnHelp != null) btnHelp.onClick.AddListener(ToggleGuide);

        // Wire Pause Panel Buttons
        if (btnResumePause != null) btnResumePause.onClick.AddListener(ResumeGame);
        if (btnRestartPause != null) btnRestartPause.onClick.AddListener(RestartGame);
        if (btnHomePause != null) btnHomePause.onClick.AddListener(OpenConfirmHome);
        if (btnClosePause != null) btnClosePause.onClick.AddListener(ResumeGame);

        // Wire Guide Panel Buttons
        if (btnCloseGuide != null) btnCloseGuide.onClick.AddListener(CloseGuide);
        if (btnCloseGuideAlt != null) btnCloseGuideAlt.onClick.AddListener(CloseGuide);

        // Wire Confirm Home Buttons
        if (btnConfirmHomeYes != null) btnConfirmHomeYes.onClick.AddListener(ReturnToHome);
        if (btnConfirmHomeNo != null) btnConfirmHomeNo.onClick.AddListener(CloseConfirmHome);
        if (btnCloseConfirmHome != null) btnCloseConfirmHome.onClick.AddListener(CloseConfirmHome);

        // Wire Sound Panel Controls
        if (btnToggleSoundModal != null) btnToggleSoundModal.onClick.AddListener(ToggleSound);
        if (btnCloseSound != null) btnCloseSound.onClick.AddListener(CloseSound);
        if (btnCloseSoundAlt != null) btnCloseSoundAlt.onClick.AddListener(CloseSound);

        // Ensure modals initially hidden
        if (panelPause != null) panelPause.SetActive(false);
        if (panelGuide != null) panelGuide.SetActive(false);
        if (panelConfirmHome != null) panelConfirmHome.SetActive(false);
        if (panelSound != null) panelSound.SetActive(false);
    }

    private void Start()
    {
        UpdateSoundIcon();
        UpdateSoundModalUI();
    }

    private void Update()
    {
        // Sync mute state across scenes
        if (AudioManager.Instance != null && isMuted != AudioManager.Instance.IsMuted)
        {
            isMuted = AudioManager.Instance.IsMuted;
            UpdateSoundIcon();
            UpdateSoundModalUI();
        }

        // Don't intercept shortcuts if quiz is active
        if (L1_QuizManager.Instance != null && L1_QuizManager.Instance.IsQuizOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isConfirmHomeOpen)
            {
                CloseConfirmHome();
            }
            else if (isGuideOpen)
            {
                CloseGuide();
            }
            else if (isSoundOpen)
            {
                CloseSound();
            }
            else
            {
                TogglePause();
            }
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleSound();
        }
        else if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1))
        {
            ToggleGuide();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (panelPause != null)
        {
            panelPause.SetActive(true);
        }

        L1_PlayerController.Instance?.SetMovementLocked(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (panelPause != null)
        {
            panelPause.SetActive(false);
        }

        // Only resume time & player movement if no other modals are active
        if (!isGuideOpen && !isConfirmHomeOpen && !isSoundOpen)
        {
            Time.timeScale = 1f;
            L1_PlayerController.Instance?.SetMovementLocked(false);
        }
    }

    public void OnSoundButtonClicked()
    {
        ToggleSound();
    }

    public void ToggleSoundModal()
    {
        if (isSoundOpen)
        {
            CloseSound();
        }
        else
        {
            OpenSound();
        }
    }

    public void OpenSound()
    {
        isSoundOpen = true;
        Time.timeScale = 0f;

        if (panelSound != null)
        {
            panelSound.SetActive(true);
        }

        L1_PlayerController.Instance?.SetMovementLocked(true);
        UpdateSoundModalUI();
    }

    public void CloseSound()
    {
        isSoundOpen = false;
        if (panelSound != null)
        {
            panelSound.SetActive(false);
        }

        if (!isPaused && !isGuideOpen && !isConfirmHomeOpen)
        {
            Time.timeScale = 1f;
            L1_PlayerController.Instance?.SetMovementLocked(false);
        }
    }

    public void ToggleSound()
    {
        SetAudioMuted(!isMuted);
    }

    public void SetAudioMuted(bool muted)
    {
        isMuted = muted;
        PlayerPrefs.SetInt("Game_Audio_Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyAudioMute(isMuted, notify: true);
        UpdateSoundIcon();
        UpdateSoundModalUI();
    }

    private void ApplyAudioMute(bool muted, bool notify)
    {
        AudioListener.pause = muted;
        AudioListener.volume = muted ? 0f : 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMuted(muted);
        }

        if (notify && L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.SetQuestHint(muted ? "Âm thanh: ĐÃ TẮT" : "Âm thanh: ĐÃ BẬT");
        }
    }

    private void UpdateSoundIcon()
    {
        if (imgSoundIcon != null)
        {
            if (isMuted && soundOffSprite != null)
            {
                imgSoundIcon.sprite = soundOffSprite;
            }
            else if (!isMuted && soundOnSprite != null)
            {
                imgSoundIcon.sprite = soundOnSprite;
            }
        }
    }

    private void UpdateSoundModalUI()
    {
        if (txtSoundStatusModal != null)
        {
            txtSoundStatusModal.text = isMuted ? "Âm thanh: ĐÃ TẮT" : "Âm thanh: ĐANG BẬT";
        }
    }

    public void ToggleGuide()
    {
        if (isGuideOpen)
        {
            CloseGuide();
        }
        else
        {
            OpenGuide();
        }
    }

    public void OpenGuide()
    {
        isGuideOpen = true;
        Time.timeScale = 0f;

        if (panelGuide != null)
        {
            panelGuide.SetActive(true);
        }

        L1_PlayerController.Instance?.SetMovementLocked(true);
    }

    public void CloseGuide()
    {
        isGuideOpen = false;
        if (panelGuide != null)
        {
            panelGuide.SetActive(false);
        }

        if (!isPaused && !isConfirmHomeOpen && !isSoundOpen)
        {
            Time.timeScale = 1f;
            L1_PlayerController.Instance?.SetMovementLocked(false);
        }
    }

    public void OpenConfirmHome()
    {
        isConfirmHomeOpen = true;
        Time.timeScale = 0f;

        if (panelConfirmHome != null)
        {
            panelConfirmHome.SetActive(true);
        }

        L1_PlayerController.Instance?.SetMovementLocked(true);
    }

    public void CloseConfirmHome()
    {
        isConfirmHomeOpen = false;
        if (panelConfirmHome != null)
        {
            panelConfirmHome.SetActive(false);
        }

        if (!isPaused && !isGuideOpen && !isSoundOpen)
        {
            Time.timeScale = 1f;
            L1_PlayerController.Instance?.SetMovementLocked(false);
        }
    }

    public void ReturnToHome()
    {
        Time.timeScale = 1f;
        string targetScene = "Chọn map";

        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
            return;
        }

        string fullPath = $"Assets/Scenes/{targetScene}.unity";
        if (Application.CanStreamedLevelBeLoaded(fullPath))
        {
            SceneManager.LoadScene(fullPath);
            return;
        }

#if UNITY_EDITOR
        foreach (var s in UnityEditor.EditorBuildSettings.scenes)
        {
            if (s.enabled && (s.path == fullPath || System.IO.Path.GetFileNameWithoutExtension(s.path).Equals(targetScene, System.StringComparison.OrdinalIgnoreCase)))
            {
                SceneManager.LoadScene(s.path);
                return;
            }
        }
#endif

        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (L1_GameManager.Instance != null)
        {
            L1_GameManager.Instance.RestartGame();
        }
        else
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }

    // Wiring helpers for unit testing & programmatic setup
    public void SetButtons(Button pause, Button sound, Button home, Button help)
    {
        btnPause = pause;
        btnSound = sound;
        btnHome = home;
        btnHelp = help;
    }

    public void SetSoundSprites(Image img, Sprite onSprite, Sprite offSprite)
    {
        imgSoundIcon = img;
        soundOnSprite = onSprite;
        soundOffSprite = offSprite;
        UpdateSoundIcon();
    }

    public void SetModals(GameObject pause, GameObject guide, GameObject confirmHome)
    {
        panelPause = pause;
        panelGuide = guide;
        panelConfirmHome = confirmHome;
    }

    public void SetPauseButtons(Button resume, Button restart, Button home, Button close)
    {
        btnResumePause = resume;
        btnRestartPause = restart;
        btnHomePause = home;
        btnClosePause = close;
    }

    public void SetGuideButtons(Button closeBtn, Button closeBtnAlt = null)
    {
        btnCloseGuide = closeBtn;
        btnCloseGuideAlt = closeBtnAlt;
    }

    public void SetConfirmHomeButtons(Button yesBtn, Button noBtn, Button closeBtn = null)
    {
        btnConfirmHomeYes = yesBtn;
        btnConfirmHomeNo = noBtn;
        btnCloseConfirmHome = closeBtn;
    }

    public void SetSoundModal(GameObject soundPanel, Button toggleBtn, TextMeshProUGUI statusTxt, Button closeBtn, Button closeBtnAlt = null)
    {
        panelSound = soundPanel;
        btnToggleSoundModal = toggleBtn;
        txtSoundStatusModal = statusTxt;
        btnCloseSound = closeBtn;
        btnCloseSoundAlt = closeBtnAlt;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Time.timeScale = 1f;
    }
}
