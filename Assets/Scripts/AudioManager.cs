using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý âm thanh toàn cục (BGM và SFX) cho toàn bộ trò chơi:
/// - Đảm bảo phát nhạc nền mượt mà liên tục qua các trang: "MainMenu", "LoginMenu", "RegisterMenu", "Chọn map".
/// - Tự động duy trì trạng thái qua DontDestroyOnLoad và hỗ trợ Singleton tự khởi tạo.
/// - Cung cấp hiệu ứng âm thanh retro (Click, Success, Error, Chime) bằng cả procedural tone và audio clip thực tế.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Music Clips (Nhạc nền)")]
    [Tooltip("Nhạc nền du dương dùng cho MainMenu, LoginMenu, RegisterMenu và Chọn map")]
    public AudioClip mainMenuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;

    [Header("UI SFX Clips (Hiệu ứng giao diện)")]
    public AudioClip clickSound;
    public AudioClip successSound;
    public AudioClip errorSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.45f;

    [Range(0f, 1f)]
    public float sfxVolume = 0.85f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var _ = Instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        LoadClipsIfNeeded();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();

        // Tự động kiểm tra phát hoặc dừng nhạc nền tùy theo cảnh hiện tại khi khởi chạy
        string currentScene = SceneManager.GetActiveScene().name;
        if (IsStopMusicScene(currentScene))
        {
            StopMusic();
        }
        else if (IsMenuScene(currentScene))
        {
            PlayMainMenuMusic();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;

        // Dừng nhạc nền menu khi đến trang Chọn map hoặc khi sang Map 0
        if (IsStopMusicScene(name))
        {
            StopMusic();
        }
        else if (IsMenuScene(name))
        {
            PlayMainMenuMusic();
        }
        else if (name == "Level1_Prototype" || name == "Map1")
        {
            PlayLevel1Music();
        }
        else if (name == "Scene_Level2" || name == "Map2")
        {
            PlayLevel2Music();
        }
        else if (name == "Map3_Final" || name == "Map3")
        {
            PlayLevel3Music();
        }
    }

    private bool IsStopMusicScene(string sceneName)
    {
        return sceneName == "Chọn map" ||
               sceneName == "Chá» n map" ||
               sceneName == "MenuManChoi" ||
               sceneName == "Map0_Prologue";
    }

    private bool IsMenuScene(string sceneName)
    {
        return sceneName == "MainMenu" ||
               sceneName == "LoginMenu" ||
               sceneName == "RegisterMenu";
    }

    public void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = sfxVolume;
        }
    }

    public void LoadClipsIfNeeded()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (mainMenuMusic == null || (mainMenuMusic != null && mainMenuMusic.name != "zephiramusic-lofi-gaming-606302"))
        {
            try { mainMenuMusic = Resources.Load<AudioClip>("Audio/BGM/zephiramusic-lofi-gaming-606302"); } catch { }
#if UNITY_EDITOR
            if (mainMenuMusic == null)
            {
                try { mainMenuMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/zephiramusic-lofi-gaming-606302.mp3"); } catch { }
            }
#endif
            if (mainMenuMusic == null)
            {
                try { mainMenuMusic = Resources.Load<AudioClip>("Audio/BGM/BGM_Level03_Bridge"); } catch { }
            }
#if UNITY_EDITOR
            if (mainMenuMusic == null)
            {
                try { mainMenuMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03_Bridge.mp3"); } catch { }
            }
#endif
            if (mainMenuMusic == null)
            {
                try { mainMenuMusic = Resources.Load<AudioClip>("Audio/BGM_MainMenu"); } catch { }
            }
#if UNITY_EDITOR
            if (mainMenuMusic == null)
            {
                try { mainMenuMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_MainMenu.mp3"); } catch { }
            }
#endif
        }

        if (level1Music == null)
        {
#if UNITY_EDITOR
            try { level1Music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level01_Food.mp3"); } catch { }
#endif
        }

        if (level2Music == null)
        {
#if UNITY_EDITOR
            try { level2Music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level02_Clothing.mp3"); } catch { }
#endif
        }

        if (level3Music == null)
        {
#if UNITY_EDITOR
            try { level3Music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03.mp3"); } catch { }
            if (level3Music == null)
            {
                try { level3Music = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_Level03_Bridge.mp3"); } catch { }
            }
#endif
        }

        if (clickSound == null)
        {
            clickSound = CreatePixelChimeClip(980f, 0.09f);
        }

        if (successSound == null)
        {
            successSound = CreateTwoToneChimeClip(880f, 1320f, 0.16f);
        }

        if (errorSound == null)
        {
            errorSound = CreateTwoToneChimeClip(360f, 240f, 0.20f);
        }
    }

    #region Music Playback

    public void PlayMainMenuMusic()
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        PlayMusic(mainMenuMusic);
    }

    public void PlayLevel1Music()
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        PlayMusic(level1Music);
    }

    public void PlayLevel2Music()
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        PlayMusic(level2Music);
    }

    public void PlayLevel3Music()
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        PlayMusic(level3Music);
    }

    public void PlayMusic(AudioClip clip)
    {
        EnsureAudioSources();
        if (musicSource == null || clip == null) return;

        // Nếu bài nhạc này đã đang phát rồi thì KHÔNG phát lại từ đầu mà tiếp tục mượt mà
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    #endregion

    #region SFX Playback

    public void PlayClickSound(float volume = -1f)
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        if (sfxSource != null && clickSound != null)
        {
            float v = volume >= 0f ? volume : sfxVolume;
            sfxSource.PlayOneShot(clickSound, v);
        }
    }

    public void PlaySuccessSound(float volume = -1f)
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        if (sfxSource != null && successSound != null)
        {
            float v = volume >= 0f ? volume : sfxVolume;
            sfxSource.PlayOneShot(successSound, v);
        }
    }

    public void PlayErrorSound(float volume = -1f)
    {
        EnsureAudioSources();
        LoadClipsIfNeeded();
        if (sfxSource != null && errorSound != null)
        {
            float v = volume >= 0f ? volume : sfxVolume;
            sfxSource.PlayOneShot(errorSound, v);
        }
    }

    public void PlayTone(float frequency, float duration = 0.12f, float volume = -1f)
    {
        EnsureAudioSources();
        if (sfxSource == null) return;

        AudioClip clip = CreatePixelChimeClip(frequency, duration);
        float v = volume >= 0f ? volume : sfxVolume;
        sfxSource.PlayOneShot(clip, v);
    }

    public void PlayOneShot(AudioClip clip, float volume = -1f)
    {
        EnsureAudioSources();
        if (sfxSource != null && clip != null)
        {
            float v = volume >= 0f ? volume : sfxVolume;
            sfxSource.PlayOneShot(clip, v);
        }
    }

    #endregion

    #region Volume Controls

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    #endregion

    #region Procedural Audio Generators

    private static AudioClip CreatePixelChimeClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            // Dạng sóng Sine + Triangle ấm áp phong cách Pixel Art
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
            float harmonic = Mathf.Sin(4f * Mathf.PI * frequency * t) * 0.25f;
            // Envelope suy giảm mượt (ADSR ngắn)
            float envelope = Mathf.Exp(-5f * progress);
            data[i] = (wave + harmonic) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("PixelChime_" + frequency, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateTwoToneChimeClip(float freq1, float freq2, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        int half = samples / 2;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = (i < half) ? freq1 : freq2;
            float progress = (i < half) ? ((float)i / half) : ((float)(i - half) / half);

            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            float envelope = Mathf.Exp(-4f * progress);
            data[i] = wave * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create($"TwoTone_{freq1}_{freq2}", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    #endregion
}
