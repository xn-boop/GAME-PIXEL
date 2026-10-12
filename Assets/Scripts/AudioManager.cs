
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayLevel1Music()
    {
        PlayMusic(level1Music);
    }

    public void PlayLevel2Music()
    {
        PlayMusic(level2Music);
    }

    public void PlayLevel3Music()
    {
        PlayMusic(level3Music);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            Debug.LogWarning("AudioManager: Chưa gán AudioClip.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
}
