using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tạo hiệu ứng nhấp nháy ánh sáng (Alpha Blink) kết hợp nhịp thở phóng to/thu nhỏ nhẹ (Scale Pulse)
/// cho nút "TAP TO START", giúp nút bấm sống động và kích thích thao tác chạm của người chơi.
/// </summary>
public class MenuBlinkEffect : MonoBehaviour
{
    [Header("Blink / Transparency")]
    [Tooltip("CanvasGroup điều khiển độ trong suốt (nếu chưa có sẽ tự động thêm)")]
    public CanvasGroup canvasGroup;

    [Tooltip("Tốc độ chu kỳ nhấp nháy và thở")]
    public float speed = 2.5f;

    [Range(0f, 1f)]
    public float minAlpha = 0.4f;

    [Range(0f, 1f)]
    public float maxAlpha = 1.0f;

    [Header("Scale Pulse / Breathing")]
    [Tooltip("Bật/Tắt hiệu ứng nhịp thở co giãn nhẹ cho nút")]
    public bool enableScalePulse = true;

    [Tooltip("Tỉ lệ co lại nhỏ nhất")]
    public float minScale = 0.97f;

    [Tooltip("Tỉ lệ dãn ra lớn nhất")]
    public float maxScale = 1.03f;

    [Header("Timing")]
    [Tooltip("Sử dụng thời gian thực không bị ảnh hưởng bởi Time.timeScale")]
    public bool unscaledTime = true;

    private Vector3 initialScale;

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
    }

    private void Update()
    {
        float timeValue = unscaledTime ? Time.unscaledTime : Time.time;
        float t = (Mathf.Sin(timeValue * speed) + 1f) * 0.5f;

        // Cập nhật độ trong suốt Alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        }

        // Cập nhật nhịp thở Scale
        if (enableScalePulse)
        {
            float scaleMultiplier = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = initialScale * scaleMultiplier;
        }
    }

    private void OnDisable()
    {
        transform.localScale = initialScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = maxAlpha;
        }
    }
}
