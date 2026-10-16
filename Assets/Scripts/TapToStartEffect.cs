using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển hiệu ứng sống động cho nút "TAP TO START":
/// - Trạng thái chờ: Nhịp thở co giãn nhẹ (Scale Pulse) kết hợp ánh sáng nhấp nháy (Alpha Blink).
/// - Khi người chơi chạm/nhấp: Kích hoạt hiệu ứng Tap bùng nổ (Punch Scale, Flash sáng rực rỡ, vòng sóng Ripple lan tỏa)
///   và âm thanh phản hồi vui tai trước khi chuyển màn.
/// </summary>
public class TapToStartEffect : MonoBehaviour
{
    [Header("Idle Breathing (Nhịp thở chờ chạm)")]
    [Tooltip("CanvasGroup điều khiển độ trong suốt khi chờ")]
    public CanvasGroup canvasGroup;

    [Tooltip("Tốc độ chu kỳ thở và nhấp nháy")]
    public float idleSpeed = 2.5f;

    [Range(0f, 1f)]
    public float idleMinAlpha = 0.35f;

    [Range(0f, 1f)]
    public float idleMaxAlpha = 0.95f;

    public bool enableIdlePulse = true;
    public float idleMinScale = 0.97f;
    public float idleMaxScale = 1.03f;

    [Header("Tap Feedback Effect (Hiệu ứng khi chạm)")]
    [Tooltip("Lớp phủ chớp sáng (Flash Overlay) rực rỡ khi nhấp")]
    public Graphic flashOverlay;

    [Tooltip("Vòng sóng năng lượng lan tỏa (Ripple Ring)")]
    public RectTransform rippleRing;

    [Tooltip("Thời gian diễn ra hiệu ứng Tap (giây)")]
    public float tapDuration = 0.45f;

    [Header("Audio Feedback")]
    [Tooltip("Âm thanh phản hồi khi chạm (sẽ tự động tạo âm pixel chime nếu để trống)")]
    public AudioClip tapSoundClip;

    [Header("Timing")]
    public bool unscaledTime = true;

    private Vector3 initialScale;
    private bool isTapped = false;
    private AudioSource audioSource;
    private Coroutine tapCoroutine;

    private void Awake()
    {
        initialScale = transform.localScale;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (tapSoundClip == null)
        {
            tapSoundClip = CreatePixelTapChime();
        }

        EnsureVisualElements();

        if (rippleRing != null)
        {
            rippleRing.gameObject.SetActive(false);
        }

        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(false);
        }
    }

    private void EnsureVisualElements()
    {
        if (flashOverlay == null)
        {
            Transform fTrans = transform.Find("FlashOverlay");
            if (fTrans == null)
            {
                var fObj = new GameObject("FlashOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fObj.transform.SetParent(transform, false);

                var fRect = fObj.GetComponent<RectTransform>();
                fRect.anchorMin = Vector2.zero;
                fRect.anchorMax = Vector2.one;
                fRect.sizeDelta = Vector2.zero;

                var fImg = fObj.GetComponent<Image>();
                fImg.color = new Color(1f, 0.98f, 0.85f, 0f);
                fImg.raycastTarget = false;
                flashOverlay = fImg;
                fObj.SetActive(false);
            }
            else
            {
                flashOverlay = fTrans.GetComponent<Image>();
            }
        }

        if (rippleRing == null)
        {
            Transform rTrans = transform.Find("RippleRing");
            if (rTrans == null)
            {
                var rObj = new GameObject("RippleRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                rObj.transform.SetParent(transform, false);

                var rRect = rObj.GetComponent<RectTransform>();
                rRect.anchorMin = new Vector2(0.5f, 0.5f);
                rRect.anchorMax = new Vector2(0.5f, 0.5f);
                rRect.sizeDelta = new Vector2(440f, 90f);

                var rImg = rObj.GetComponent<Image>();
                rImg.color = new Color(1f, 0.9f, 0.6f, 0.7f);
                rImg.raycastTarget = false;
                rippleRing = rRect;
                rObj.SetActive(false);
            }
            else
            {
                rippleRing = rTrans.GetComponent<RectTransform>();
            }
        }
    }

    private void Update()
    {
        if (isTapped) return;

        float timeValue = unscaledTime ? Time.unscaledTime : Time.time;
        float t = (Mathf.Sin(timeValue * idleSpeed) + 1f) * 0.5f;

        // Cập nhật Alpha nhịp thở
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(idleMinAlpha, idleMaxAlpha, t);
        }

        // Cập nhật Scale nhịp thở
        if (enableIdlePulse)
        {
            float scaleMultiplier = Mathf.Lerp(idleMinScale, idleMaxScale, t);
            transform.localScale = initialScale * scaleMultiplier;
        }
    }

    /// <summary>
    /// Kích hoạt hiệu ứng Tap bùng nổ và gọi callback khi kết thúc.
    /// </summary>
    public void TriggerTapEffect(Action onComplete = null)
    {
        if (isTapped) return;
        isTapped = true;

        if (!gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }

        if (tapCoroutine != null)
        {
            StopCoroutine(tapCoroutine);
        }
        tapCoroutine = StartCoroutine(TapAnimationRoutine(onComplete));
    }

    private IEnumerator TapAnimationRoutine(Action onComplete)
    {
        // 1. Phát âm thanh phản hồi
        if (audioSource != null && tapSoundClip != null)
        {
            audioSource.PlayOneShot(tapSoundClip, 0.9f);
        }

        // 2. Kích hoạt Flash Overlay nếu có
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
        }

        // 3. Kích hoạt Ripple Ring nếu có
        if (rippleRing != null)
        {
            rippleRing.gameObject.SetActive(true);
            rippleRing.localScale = Vector3.one * 0.6f;
        }

        float elapsed = 0f;
        float duration = tapDuration;

        while (elapsed < duration)
        {
            elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            // --- Hiệu ứng biến dạng đàn hồi nút bấm (Punch Bounce) ---
            // 0 -> 0.25: Ép dẹt xuống 0.88
            // 0.25 -> 0.65: Nảy bật mạnh lên 1.18
            // 0.65 -> 1.0: Ổn định nhẹ quanh 1.05
            float scale;
            if (progress < 0.25f)
            {
                scale = Mathf.Lerp(1.0f, 0.88f, progress / 0.25f);
            }
            else if (progress < 0.65f)
            {
                scale = Mathf.Lerp(0.88f, 1.18f, (progress - 0.25f) / 0.40f);
            }
            else
            {
                scale = Mathf.Lerp(1.18f, 1.05f, (progress - 0.65f) / 0.35f);
            }
            transform.localScale = initialScale * scale;

            // --- Hiệu ứng Chớp sáng (Flash Light) ---
            if (canvasGroup != null)
            {
                // Nhấp nháy sáng chói trong giai đoạn nảy
                float flashAlpha = (progress < 0.5f)
                    ? Mathf.Lerp(idleMaxAlpha, 1.0f, progress / 0.5f)
                    : Mathf.Lerp(1.0f, 0.85f, (progress - 0.5f) / 0.5f);
                canvasGroup.alpha = flashAlpha;
            }

            if (flashOverlay != null)
            {
                Color c = flashOverlay.color;
                c.a = Mathf.Lerp(0.85f, 0f, progress);
                flashOverlay.color = c;
            }

            // --- Hiệu ứng Vòng sóng lan tỏa (Ripple Shockwave) ---
            if (rippleRing != null)
            {
                float rippleScale = Mathf.Lerp(0.6f, 2.4f, progress);
                rippleRing.localScale = Vector3.one * rippleScale;

                var rippleGraphic = rippleRing.GetComponent<Graphic>();
                if (rippleGraphic != null)
                {
                    Color rc = rippleGraphic.color;
                    rc.a = Mathf.Lerp(0.7f, 0f, progress);
                    rippleGraphic.color = rc;
                }
            }

            yield return null;
        }

        if (rippleRing != null) rippleRing.gameObject.SetActive(false);
        if (flashOverlay != null) flashOverlay.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Đặt lại trạng thái ban đầu của hiệu ứng.
    /// </summary>
    public void ResetEffect()
    {
        isTapped = false;
        transform.localScale = initialScale;
        if (canvasGroup != null) canvasGroup.alpha = idleMaxAlpha;
        if (rippleRing != null) rippleRing.gameObject.SetActive(false);
        if (flashOverlay != null) flashOverlay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Tạo âm thanh Pixel Art Tap Chime chuẩn âm thanh Retro 8-bit/16-bit.
    /// </summary>
    private AudioClip CreatePixelTapChime()
    {
        int sampleRate = 44100;
        float duration = 0.16f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            // Chime kép nốt G5 (784Hz) vút lên C6 (1046.5Hz)
            float freq = (t < 0.06f) ? 784f : 1046.5f;
            float env = Mathf.Exp(-t * 22f);
            // Sóng hài kết hợp sin và tam giác tạo âm sắc trong trẻo
            float wave = 0.7f * Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.PingPong(t * freq * 4f, 1f);
            data[i] = wave * env * 0.45f;
        }

        AudioClip clip = AudioClip.Create("PixelTapChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
