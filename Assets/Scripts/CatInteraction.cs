using System;
using UnityEngine;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;
using JourneyOfMemories.Core.UI;

/// <summary>
/// Quản lý tương tác với chú mèo trong Map 1.
/// Chú mèo có dấu hỏi nổi trên đầu khi chưa được hỏi;
/// khi tương tác sẽ mở câu hỏi mở khóa "Q_Cat".
/// Trả lời đúng sẽ đánh dấu hoàn thành mini-game mèo, ẩn dấu hỏi và mở gợi ý đầu tiên.
/// </summary>
public class CatInteraction : MonoBehaviour
{
    [Header("Cat Settings")]
    [SerializeField] private string quizId = "Q_Cat";
    [SerializeField] private GameObject questionMarkObject;
    [SerializeField] private float bobbingSpeed = 3f;
    [SerializeField] private float bobbingHeight = 0.12f;

    private Vector3 initialQuestionMarkLocalPos;
    private bool isSolved = false;

    public bool IsSolved => isSolved;
    public string QuizId => quizId;

    public event Action OnCatQuizCompleted;

    private void Start()
    {
        if (questionMarkObject != null)
        {
            initialQuestionMarkLocalPos = questionMarkObject.transform.localPosition;
        }

        // Kiểm tra trạng thái đã giải trước đó từ GameStateManager hoặc QuizManager
        if (GameStateManager.Instance != null && GameStateManager.Instance.M1_MiniDone)
        {
            SetCatSolved(false);
        }
        else if (QuizManager.Instance != null && QuizManager.Instance.IsQuizSolved(quizId))
        {
            SetCatSolved(true);
        }
    }

    private void Update()
    {
        // Hiệu ứng nhấp nhô nhẹ cho dấu hỏi trên đầu mèo
        if (!isSolved && questionMarkObject != null && questionMarkObject.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            questionMarkObject.transform.localPosition = initialQuestionMarkLocalPos + new Vector3(0f, yOffset, 0f);
        }
    }

    /// <summary>
    /// Được gọi khi người chơi nhấn nút tương tác ở gần mèo.
    /// </summary>
    public void Interact()
    {
        if (isSolved || (GameStateManager.Instance != null && GameStateManager.Instance.M1_MiniDone))
        {
            Debug.Log("[CatInteraction] Meo meo~ Chú mèo dụi đầu vào bạn một cách vui vẻ.");
            return;
        }

        if (QuizUIController.Instance != null)
        {
            QuizUIController.Instance.OpenQuiz(quizId);
        }
        else
        {
            Debug.LogWarning("[CatInteraction] QuizUIController.Instance chưa sẵn sàng!");
        }
    }

    /// <summary>
    /// Đánh dấu mèo đã được trả lời đúng câu hỏi mở khóa.
    /// </summary>
    public void SetCatSolved(bool notifyGameState = true)
    {
        isSolved = true;

        if (questionMarkObject != null)
        {
            questionMarkObject.SetActive(false);
        }

        if (notifyGameState && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetMiniGameDone(1, true);
        }

        OnCatQuizCompleted?.Invoke();
    }
}
