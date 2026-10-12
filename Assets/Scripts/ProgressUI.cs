using TMPro;
using UnityEngine;

public class ProgressUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text progressText;

    private void OnEnable()
    {
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager != null)
        {
            levelManager.OnProgressChanged += UpdateProgress;
            levelManager.OnLevelCompleted += ShowCompleted;
        }
    }

    private void Start()
    {
        if (levelManager == null || progressText == null)
        {
            Debug.LogError(
                "ProgressUI chưa được gán LevelManager hoặc ProgressText!"
            );
            return;
        }

        UpdateProgress(
            levelManager.CompletedSlots,
            levelManager.TotalSlots
        );
    }

    private void OnDisable()
    {
        if (levelManager != null)
        {
            levelManager.OnProgressChanged -= UpdateProgress;
            levelManager.OnLevelCompleted -= ShowCompleted;
        }
    }

    private void UpdateProgress(int completed, int total)
    {
        if (progressText == null)
            return;

        progressText.text = $"MÓN ĂN: {completed}/{total}";
    }

    private void ShowCompleted()
    {
        if (progressText == null)
            return;

        progressText.text = "HOÀN THÀNH: 3/3!";
    }
}