using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển nút chuyển đổi ngôn ngữ ở góc dưới bên phải màn hình Menu chính (EN | VN).
/// Cung cấp hiệu ứng thị giác làm nổi bật ngôn ngữ đang chọn, hiệu ứng nảy (punch scale) khi bấm,
/// âm thanh phản hồi và thông báo toast mượt mà.
/// </summary>
public class LanguageSelectorUI : MonoBehaviour
{
    [Header("UI Text References")]
    [Tooltip("Text hiển thị hoặc lớp phủ cho nhãn EN")]
    public Text enText;

    [Tooltip("Text hiển thị hoặc lớp phủ cho nhãn VN")]
    public Text vnText;

    [Tooltip("Con trỏ / Khung viền làm nổi bật ngôn ngữ đang chọn (tùy chọn)")]
    public RectTransform activeIndicator;

    [Header("Visual Colors")]
    public Color activeColor = new Color(1f, 0.95f, 0.65f, 1f); // Vàng sáng ấm
    public Color inactiveColor = new Color(0.75f, 0.8f, 0.85f, 0.45f); // Bạc mờ

    [Header("Toast Notification")]
    [Tooltip("GameObject hiển thị thông báo chuyển ngôn ngữ (nếu có)")]
    public GameObject toastPanel;
    public Text toastText;
    public CanvasGroup toastCanvasGroup;

    [Header("Audio")]
    public AudioClip clickSound;

    private Coroutine toastCoroutine;
    private Coroutine punchCoroutine;
    private Vector3 initialScale;
    private AudioSource audioSource;

    private void Awake()
    {
        initialScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Tự tạo âm thanh click UI dễ chịu nếu chưa gán
        if (clickSound == null)
        {
            clickSound = CreateClickChime();
        }

        EnsureVisualElements();
    }

    /// <summary>
    /// Đảm bảo các thành phần thị giác (Active Indicator, Overlay Text, Toast) luôn sẵn sàng ngay cả khi chưa gán qua Inspector.
    /// </summary>
    private void EnsureVisualElements()
    {
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Tạo Khung viền / Indicator phát sáng làm nổi bật ngôn ngữ được chọn
        if (activeIndicator == null)
        {
            Transform indTrans = transform.Find("ActiveIndicator");
            if (indTrans == null)
            {
                var indObj = new GameObject("ActiveIndicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                indObj.transform.SetParent(transform, false);
                indObj.transform.SetAsFirstSibling();

                var indRect = indObj.GetComponent<RectTransform>();
                indRect.anchorMin = new Vector2(0.5f, 0.5f);
                indRect.anchorMax = new Vector2(0.5f, 0.5f);
                indRect.sizeDelta = new Vector2(64f, 32f);
                indRect.anchoredPosition = new Vector2(40f, 0f);

                var indImg = indObj.GetComponent<Image>();
                indImg.color = new Color(1f, 0.88f, 0.35f, 0.28f);
                indImg.raycastTarget = false;
                activeIndicator = indRect;
            }
            else
            {
                activeIndicator = indTrans.GetComponent<RectTransform>();
            }
        }

        // 2. Tạo nhãn EN
        if (enText == null)
        {
            Transform enTrans = transform.Find("EN_Overlay");
            if (enTrans == null)
            {
                var enObj = new GameObject("EN_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                enObj.transform.SetParent(transform, false);

                var enRect = enObj.GetComponent<RectTransform>();
                enRect.anchorMin = new Vector2(0.5f, 0.5f);
                enRect.anchorMax = new Vector2(0.5f, 0.5f);
                enRect.sizeDelta = new Vector2(55f, 36f);
                enRect.anchoredPosition = new Vector2(-40f, 0f);

                var txt = enObj.GetComponent<Text>();
                txt.font = font;
                txt.fontSize = 24;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.text = "EN";
                txt.color = inactiveColor;
                txt.raycastTarget = false;
                enText = txt;
            }
            else
            {
                enText = enTrans.GetComponent<Text>();
            }
        }

        // 3. Tạo nhãn VN
        if (vnText == null)
        {
            Transform vnTrans = transform.Find("VN_Overlay");
            if (vnTrans == null)
            {
                var vnObj = new GameObject("VN_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                vnObj.transform.SetParent(transform, false);

                var vnRect = vnObj.GetComponent<RectTransform>();
                vnRect.anchorMin = new Vector2(0.5f, 0.5f);
                vnRect.anchorMax = new Vector2(0.5f, 0.5f);
                vnRect.sizeDelta = new Vector2(55f, 36f);
                vnRect.anchoredPosition = new Vector2(40f, 0f);

                var txt = vnObj.GetComponent<Text>();
                txt.font = font;
                txt.fontSize = 24;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.text = "VN";
                txt.color = activeColor;
                txt.raycastTarget = false;
                vnText = txt;
            }
            else
            {
                vnText = vnTrans.GetComponent<Text>();
            }
        }

        // 4. Tạo Toast Notification Panel trên Canvas nếu chưa có
        if (toastPanel == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Transform tTrans = parentCanvas.transform.Find("ToastNotification");
                if (tTrans == null)
                {
                    var tObj = new GameObject("ToastNotification", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                    tObj.transform.SetParent(parentCanvas.transform, false);

                    var tRect = tObj.GetComponent<RectTransform>();
                    tRect.anchorMin = new Vector2(0.5f, 0.05f);
                    tRect.anchorMax = new Vector2(0.5f, 0.05f);
                    tRect.pivot = new Vector2(0.5f, 0.5f);
                    tRect.sizeDelta = new Vector2(360f, 42f);
                    tRect.anchoredPosition = Vector2.zero;

                    var tImg = tObj.GetComponent<Image>();
                    tImg.color = new Color(0.08f, 0.1f, 0.15f, 0.90f);
                    tImg.raycastTarget = false;

                    var cg = tObj.GetComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;

                    var textChild = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    textChild.transform.SetParent(tObj.transform, false);
                    var childRect = textChild.GetComponent<RectTransform>();
                    childRect.anchorMin = Vector2.zero;
                    childRect.anchorMax = Vector2.one;
                    childRect.sizeDelta = Vector2.zero;

                    var tt = textChild.GetComponent<Text>();
                    tt.font = font;
                    tt.fontSize = 18;
                    tt.alignment = TextAnchor.MiddleCenter;
                    tt.color = new Color(1f, 0.92f, 0.7f, 1f);
                    tt.text = "Ngôn ngữ: Tiếng Việt (VN)";
                    tt.raycastTarget = false;

                    toastPanel = tObj;
                    toastText = tt;
                    toastCanvasGroup = cg;
                    tObj.SetActive(false);
                }
                else
                {
                    toastPanel = tTrans.gameObject;
                    toastCanvasGroup = tTrans.GetComponent<CanvasGroup>();
                    toastText = tTrans.GetComponentInChildren<Text>();
                }
            }
        }
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += OnLanguageChangedHandler;
        RefreshUI(LocalizationManager.CurrentLanguage, false);
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChangedHandler;
    }

    private void Start()
    {
        RefreshUI(LocalizationManager.CurrentLanguage, false);
    }

    /// <summary>
    /// Chuyển đổi ngôn ngữ qua lại khi nhấp vào nút.
    /// </summary>
    public void OnLanguageButtonClicked()
    {
        LocalizationManager.ToggleLanguage();
        PlayFeedback();
    }

    /// <summary>
    /// Chọn trực tiếp Tiếng Anh.
    /// </summary>
    public void SelectEnglish()
    {
        if (LocalizationManager.CurrentLanguage != GameLanguage.English)
        {
            LocalizationManager.CurrentLanguage = GameLanguage.English;
            PlayFeedback();
        }
        else
        {
            PlayPunchAnimation();
        }
    }

    /// <summary>
    /// Chọn trực tiếp Tiếng Việt.
    /// </summary>
    public void SelectVietnamese()
    {
        if (LocalizationManager.CurrentLanguage != GameLanguage.Vietnamese)
        {
            LocalizationManager.CurrentLanguage = GameLanguage.Vietnamese;
            PlayFeedback();
        }
        else
        {
            PlayPunchAnimation();
        }
    }

    private void PlayFeedback()
    {
        PlayPunchAnimation();

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, 0.7f);
        }

        ShowToast(LocalizationManager.CurrentLanguage == GameLanguage.Vietnamese
            ? "Ngôn ngữ: Tiếng Việt (VN)"
            : "Language: English (EN)");
    }

    private void OnLanguageChangedHandler(GameLanguage newLang)
    {
        RefreshUI(newLang, true);
    }

    /// <summary>
    /// Cập nhật hiển thị màu sắc và vị trí indicator theo ngôn ngữ.
    /// </summary>
    public void RefreshUI(GameLanguage lang, bool animated)
    {
        bool isVi = (lang == GameLanguage.Vietnamese);

        if (vnText != null)
        {
            vnText.color = isVi ? activeColor : inactiveColor;
            vnText.fontStyle = isVi ? FontStyle.Bold : FontStyle.Normal;
        }

        if (enText != null)
        {
            enText.color = !isVi ? activeColor : inactiveColor;
            enText.fontStyle = !isVi ? FontStyle.Bold : FontStyle.Normal;
        }

        if (activeIndicator != null)
        {
            Vector2 targetPos = isVi 
                ? (vnText != null ? (Vector2)vnText.rectTransform.anchoredPosition : new Vector2(40f, 0f))
                : (enText != null ? (Vector2)enText.rectTransform.anchoredPosition : new Vector2(-40f, 0f));

            if (animated && gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateIndicator(targetPos));
            }
            else
            {
                activeIndicator.anchoredPosition = targetPos;
            }
        }
    }

    private IEnumerator AnimateIndicator(Vector2 targetPos)
    {
        if (activeIndicator == null) yield break;

        Vector2 startPos = activeIndicator.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            activeIndicator.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        activeIndicator.anchoredPosition = targetPos;
    }

    private void PlayPunchAnimation()
    {
        if (!gameObject.activeInHierarchy) return;

        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }
        punchCoroutine = StartCoroutine(PunchScaleCoroutine());
    }

    private IEnumerator PunchScaleCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Nhịp punch: Co lại 0.9 -> Bật nảy 1.12 -> Quay lại 1.0
            float scale;
            if (t < 0.35f)
            {
                scale = Mathf.Lerp(1.0f, 0.90f, t / 0.35f);
            }
            else if (t < 0.75f)
            {
                scale = Mathf.Lerp(0.90f, 1.10f, (t - 0.35f) / 0.40f);
            }
            else
            {
                scale = Mathf.Lerp(1.10f, 1.0f, (t - 0.75f) / 0.25f);
            }

            transform.localScale = initialScale * scale;
            yield return null;
        }

        transform.localScale = initialScale;
        punchCoroutine = null;
    }

    private void ShowToast(string message)
    {
        if (toastPanel == null && toastText == null) return;

        if (toastCoroutine != null)
        {
            StopCoroutine(toastCoroutine);
        }
        toastCoroutine = StartCoroutine(ToastCoroutine(message));
    }

    private IEnumerator ToastCoroutine(string message)
    {
        if (toastText != null) toastText.text = message;
        if (toastPanel != null) toastPanel.SetActive(true);

        if (toastCanvasGroup != null)
        {
            // Fade in
            float fadeTime = 0.18f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                toastCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
                yield return null;
            }
            toastCanvasGroup.alpha = 1f;

            // Chờ đọc
            yield return new WaitForSecondsRealtime(1.2f);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                yield return null;
            }
            toastCanvasGroup.alpha = 0f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(1.5f);
        }

        if (toastPanel != null) toastPanel.SetActive(false);
        toastCoroutine = null;
    }

    /// <summary>
    /// Tạo âm thanh chimes phong cách Pixel Art bằng code thuần.
    /// </summary>
    private AudioClip CreateClickChime()
    {
        int sampleRate = 44100;
        float duration = 0.08f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(1046.5f, 1318.5f, t / duration); // Nốt C6 lên E6
            float env = Mathf.Exp(-t * 35f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.35f;
        }

        AudioClip clip = AudioClip.Create("LangClickChime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
