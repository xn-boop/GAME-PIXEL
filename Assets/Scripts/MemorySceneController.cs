
using System.Collections;
using UnityEngine;

public class MemorySceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject memoryPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Memory Settings")]
    [SerializeField, Min(0f)] private float memoryDuration = 5f;

    private bool sequenceStarted = false;

    private void Awake()
    {
        // Ban đầu ẩn cả hai giao diện.
        if (memoryPanel != null)
            memoryPanel.SetActive(false);

        if (journalPanel != null)
            journalPanel.SetActive(false);

        if (playerInteraction == null)
        {
            playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        }
    }

    private void OnEnable()
    {
        if (JourneyOfMemories.Core.Managers.GameStateManager.Instance != null)
            JourneyOfMemories.Core.Managers.GameStateManager.Instance.OnMemoryTriggered += OnCoreMemoryTriggered;
    }

    private void OnDisable()
    {
        if (JourneyOfMemories.Core.Managers.GameStateManager.Instance != null)
            JourneyOfMemories.Core.Managers.GameStateManager.Instance.OnMemoryTriggered -= OnCoreMemoryTriggered;
    }

    private void OnCoreMemoryTriggered(JourneyOfMemories.Core.Data.MapId mapId)
    {
        StartMemorySequence();
    }

    private void Start()
    {
        if (memoryPanel == null || journalPanel == null)
        {
            Debug.LogError(
                "MemorySceneController: Chưa gán MemoryPanel hoặc JournalPanel!"
            );
            return;
        }

        var core = JourneyOfMemories.Core.Managers.GameStateManager.Instance;
        if (core != null && core.CurrentMapState == JourneyOfMemories.Core.Data.MapState.MemoryPending)
            StartMemorySequence();
    }

    private void StartMemorySequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;
        StartCoroutine(PlayMemorySequence());
    }

    private IEnumerator PlayMemorySequence()
    {
        Debug.Log("Bắt đầu cảnh ký ức!");

        // Hiện cảnh ký ức.
        memoryPanel.SetActive(true);
        journalPanel.SetActive(false);

        yield return new WaitForSeconds(memoryDuration);

        // Chuyển sang trang nhật ký.
        memoryPanel.SetActive(false);
        journalPanel.SetActive(true);

        Debug.Log("Đã mở trang nhật ký số 1!");
        JourneyOfMemories.Core.Managers.GameStateManager.Instance?.NotifyMemoryFinished();
    }
}
