using System;
using UnityEngine;

/// <summary>
/// Định nghĩa các ngôn ngữ được hỗ trợ trong game.
/// </summary>
public enum GameLanguage
{
    Vietnamese,
    English
}

/// <summary>
/// Quản lý hệ thống bản địa hóa / đa ngôn ngữ (Localization).
/// Tự động lưu và tải lựa chọn ngôn ngữ qua PlayerPrefs.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    /// <summary>
    /// Sự kiện kích hoạt khi người chơi thay đổi ngôn ngữ.
    /// </summary>
    public static event Action<GameLanguage> OnLanguageChanged;

    public const string PREF_KEY = "GameLanguage_Selected";

    private static GameLanguage currentLanguage = GameLanguage.Vietnamese;
    private static bool isInitialized = false;

    /// <summary>
    /// Ngôn ngữ hiện tại của game (Vietnamese hoặc English).
    /// </summary>
    public static GameLanguage CurrentLanguage
    {
        get
        {
            if (!isInitialized)
            {
                InitializeFromPrefs();
            }
            return currentLanguage;
        }
        set
        {
            if (!isInitialized || currentLanguage != value)
            {
                currentLanguage = value;
                isInitialized = true;
                PlayerPrefs.SetString(PREF_KEY, currentLanguage == GameLanguage.Vietnamese ? "VN" : "EN");
                PlayerPrefs.Save();
                Debug.Log($"[LocalizationManager] Ngôn ngữ chuyển sang: {currentLanguage}");
                OnLanguageChanged?.Invoke(currentLanguage);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        InitializeFromPrefs();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFromPrefs();
    }

    /// <summary>
    /// Khởi tạo trạng thái ngôn ngữ từ PlayerPrefs hoặc mặc định là Tiếng Việt.
    /// </summary>
    public static void InitializeFromPrefs()
    {
        if (isInitialized) return;

        string saved = PlayerPrefs.GetString(PREF_KEY, "VN");
        if (saved == "EN" || saved.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            currentLanguage = GameLanguage.English;
        }
        else
        {
            currentLanguage = GameLanguage.Vietnamese;
        }
        isInitialized = true;
    }

    /// <summary>
    /// Chuyển đổi qua lại giữa Tiếng Việt (VN) và Tiếng Anh (EN).
    /// </summary>
    public static void ToggleLanguage()
    {
        CurrentLanguage = (CurrentLanguage == GameLanguage.Vietnamese) 
            ? GameLanguage.English 
            : GameLanguage.Vietnamese;
    }

    /// <summary>
    /// Thiết lập ngôn ngữ trực tiếp.
    /// </summary>
    public static void SetLanguage(GameLanguage lang)
    {
        CurrentLanguage = lang;
    }

    /// <summary>
    /// Lấy chuỗi tương ứng theo ngôn ngữ hiện tại.
    /// </summary>
    public static string Get(string vi, string en)
    {
        return CurrentLanguage == GameLanguage.Vietnamese ? vi : en;
    }

    /// <summary>
    /// Mã viết tắt của ngôn ngữ hiện tại ("VN" hoặc "EN").
    /// </summary>
    public static string LanguageCode => CurrentLanguage == GameLanguage.Vietnamese ? "VN" : "EN";

    /// <summary>
    /// Tên hiển thị đầy đủ của ngôn ngữ hiện tại.
    /// </summary>
    public static string LanguageDisplayName => CurrentLanguage == GameLanguage.Vietnamese ? "Tiếng Việt" : "English";
}
