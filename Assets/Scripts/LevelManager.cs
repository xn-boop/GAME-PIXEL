using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Progress")]
    [SerializeField, Min(1)]
    private int totalSlots = 3;

    private int completedSlots = 0;
    private bool isLevelCompleted = false;

    // Cho UI và các hệ thống khác đọc tiến trình.
    public int CompletedSlots => completedSlots;
    public int TotalSlots => totalSlots;

    // Trả về true khi màn đã hoàn thành.
    public bool IsLevelCompleted => isLevelCompleted;

    // Sự kiện để kết nối UI, âm thanh và cảnh ký ức sau này.
    public event Action<int, int> OnProgressChanged;
    public event Action OnLevelCompleted;

    private void Start()
    {
        Debug.Log(
            "Bắt đầu màn 1. Tiến độ: " +
            completedSlots + "/" + totalSlots
        );

        OnProgressChanged?.Invoke(completedSlots, totalSlots);
    }

    public void RegisterCompletedSlot()
    {
        // Không tăng tiến độ nếu màn đã hoàn thành.
        if (isLevelCompleted)
        {
            return;
        }

        // Không cho tiến độ vượt quá tổng số ô.
        if (completedSlots >= totalSlots)
        {
            return;
        }

        completedSlots++;

        Debug.Log(
            "Tiến độ: " + completedSlots + "/" + totalSlots
        );

        // Cập nhật UI nếu có hệ thống đang lắng nghe.
        OnProgressChanged?.Invoke(completedSlots, totalSlots);

        // Chỉ hoàn thành màn đúng một lần.
        if (completedSlots >= totalSlots)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        if (isLevelCompleted)
        {
            return;
        }

        isLevelCompleted = true;

        Debug.Log("HOÀN THÀNH MÀN 1!");

        // Báo cho các hệ thống khác biết màn đã hoàn thành.
        OnLevelCompleted?.Invoke();
    }
}