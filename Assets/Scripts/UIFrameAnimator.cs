using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển hiệu ứng chuyển động khung hình (Frame-by-frame animation) cho UI Image.
/// Tự động phát chuỗi Sprite theo tốc độ khung hình (FPS) đã cấu hình mượt mà,
/// không gây giật lag (frame drop) và hỗ trợ chạy trên thời gian unscaled.
/// </summary>
[RequireComponent(typeof(Image))]
public class UIFrameAnimator : MonoBehaviour
{
    [Header("Frames Configuration")]
    [Tooltip("Danh sách các frame sprite chuyển động")]
    public Sprite[] frames;

    [Tooltip("Tốc độ khung hình (Frames Per Second). Mặc định 10 FPS tương ứng với chu kỳ chuyển động 100ms/frame")]
    [Range(0.5f, 60f)]
    public float framesPerSecond = 10f;

    [Tooltip("Lặp lại vô tận chuỗi khung hình")]
    public bool loop = true;

    [Tooltip("Chạy theo thời gian thực (không bị ảnh hưởng khi Pause hoặc Time.timeScale = 0)")]
    public bool unscaledTime = true;

    private Image targetImage;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    /// <summary>
    /// Số lượng khung hình hiện có.
    /// </summary>
    public int FrameCount => frames != null ? frames.Length : 0;

    /// <summary>
    /// Khung hình hiện tại đang hiển thị.
    /// </summary>
    public int CurrentFrameIndex => currentFrameIndex;

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

            // Nếu delta bị gián đoạn quá lâu (lag spike), giới hạn step để không trượt khung hình đột ngột
            if (step > frames.Length)
            {
                step = 1;
                timer = 0f;
            }

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
    /// Gán frame hiện tại lên UI Image, tối ưu chỉ gán khi có sự thay đổi Sprite.
    /// </summary>
    private void ApplyCurrentFrame()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null && frames != null && currentFrameIndex >= 0 && currentFrameIndex < frames.Length)
        {
            Sprite targetSprite = frames[currentFrameIndex];
            if (targetSprite != null && targetImage.sprite != targetSprite)
            {
                targetImage.sprite = targetSprite;
            }
        }
    }

    /// <summary>
    /// Đặt lại animation về frame đầu tiên.
    /// </summary>
    public void ResetAnimation()
    {
        currentFrameIndex = 0;
        timer = 0f;
        ApplyCurrentFrame();
    }

    /// <summary>
    /// Cập nhật bộ frames mới cho animator.
    /// </summary>
    public void SetFrames(Sprite[] newFrames, float newFps = 10f)
    {
        frames = newFrames;
        framesPerSecond = newFps;
        ResetAnimation();
    }
}
