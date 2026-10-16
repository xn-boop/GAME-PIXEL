using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiệu ứng nhấn nút phản hồi sống động (Scale Punch, Sound Chime) chuẩn phong cách Pixel Art.
/// </summary>
public class RegisterButtonEffect : MonoBehaviour
{
    [Header("Punch Settings")]
    [Tooltip("Thời gian hiệu ứng nảy nút (giây)")]
    public float punchDuration = 0.22f;

    [Tooltip("Mức co dẹt khi nhấn")]
    public float pressedScale = 0.92f;

    [Tooltip("Mức nảy bật tối đa trước khi hồi vị")]
    public float bounceScale = 1.06f;

    [Header("Audio Feedback")]
    [Tooltip("Âm thanh retro phát ra khi nhấn nút (tự tạo âm procedural nếu để trống)")]
    public AudioClip clickClip;

    [Tooltip("Tần số âm thanh retro (Hz)")]
    public float chimeFrequency = 980f;

    [Header("Timing")]
    public bool unscaledTime = true;

    private Vector3 originalScale;
    private AudioSource audioSource;
    private Coroutine currentRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (clickClip == null)
        {
            clickClip = CreatePixelClickChime(chimeFrequency);
        }
    }

    /// <summary>
    /// Kích hoạt hiệu ứng nảy nút kèm âm thanh và gọi callback sau khi hoàn thành.
    /// </summary>
    public void TriggerEffect(Action onComplete = null)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(AnimateRoutine(onComplete));
    }

    private IEnumerator AnimateRoutine(Action onComplete)
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip, 0.85f);
        }

        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / punchDuration);

            float s;
            if (t < 0.3f)
            {
                s = Mathf.Lerp(1.0f, pressedScale, t / 0.3f);
            }
            else if (t < 0.75f)
            {
                s = Mathf.Lerp(pressedScale, bounceScale, (t - 0.3f) / 0.45f);
            }
            else
            {
                s = Mathf.Lerp(bounceScale, 1.0f, (t - 0.75f) / 0.25f);
            }

            transform.localScale = originalScale * s;
            yield return null;
        }

        transform.localScale = originalScale;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Tạo âm thanh Pixel retro chime chuẩn 8-bit/16-bit.
    /// </summary>
    private AudioClip CreatePixelClickChime(float baseFreq)
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = (t < 0.04f) ? baseFreq : baseFreq * 1.33f;
            float env = Mathf.Exp(-t * 26f);
            float wave = 0.65f * Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.PingPong(t * freq * 3f, 1f);
            data[i] = wave * env * 0.4f;
        }

        AudioClip clip = AudioClip.Create("PixelClick_" + baseFreq, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
