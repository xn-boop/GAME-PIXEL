using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển hiệu ứng chuyển động khung hình (Frame-by-frame animation) cho UI Image.
/// Tự động phát chuỗi Sprite theo tốc độ khung hình (FPS) đã cấu hình.
/// </summary>
[RequireComponent(typeof(Image))]
public class UIFrameAnimator : MonoBehaviour
{
    [Header("Frames Configuration")]
    [Tooltip("Danh sách các frame sprite chuyển động")]
    public Sprite[] frames;

    [Tooltip("Tốc độ khung hình (Frames Per Second). Mặc định 4.545 FPS = 220ms/frame tương ứng với bản thiết kế mẫu")]
    [Range(0.5f, 60f)]
    public float framesPerSecond = 4.545f;

    [Tooltip("Lặp lại vô tận")]
    public bool loop = true;

    [Tooltip("Chạy theo thời gian thực (không bị ảnh hưởng khi Pause/Time.timeScale = 0)")]
    public bool unscaledTime = true;

    private Image targetImage;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        timer = 0f;
        currentFrameIndex = 0;
        ApplyCurrentFrame();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f) return;

        float delta = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timer += delta;

        float frameDuration = 1f / framesPerSecond;
        if (timer >= frameDuration)
        {
            int step = (int)(timer / frameDuration);
            timer -= step * frameDuration;

            if (loop)
            {
                currentFrameIndex = (currentFrameIndex + step) % frames.Length;
            }
            else
            {
                currentFrameIndex = Mathf.Min(currentFrameIndex + step, frames.Length - 1);
            }

            ApplyCurrentFrame();
        }
    }

    /// <summary>
    /// Gán frame hiện tại lên UI Image
    /// </summary>
    private void ApplyCurrentFrame()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null && frames != null && currentFrameIndex >= 0 && currentFrameIndex < frames.Length)
        {
            if (frames[currentFrameIndex] != null)
            {
                targetImage.sprite = frames[currentFrameIndex];
            }
        }
    }

    /// <summary>
    /// Đặt lại animation về frame đầu tiên
    /// </summary>
    public void ResetAnimation()
    {
        currentFrameIndex = 0;
        timer = 0f;
        ApplyCurrentFrame();
    }
}
