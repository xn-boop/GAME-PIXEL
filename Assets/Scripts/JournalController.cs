using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JourneyOfMemories.Core.Data;

public class JournalController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private Button continueButton;

    [Header("Optional")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private MapId mapId = MapId.Map1_Kitchen;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        else
        {
            Debug.LogWarning(
                "JournalController: Chưa gán ContinueButton!"
            );
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }

    private void OnContinueClicked()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = "Ký ức đầu tiên đã được lưu.";
            statusText.gameObject.SetActive(true);
        }

        Debug.Log("Người chơi đã hoàn thành Ký ức #1.");

        // Thông báo cho Core Logic xác nhận trang nhật ký
        if (JourneyOfMemories.Core.Managers.GameStateManager.Instance != null)
        {
            JourneyOfMemories.Core.Managers.GameStateManager.Instance.ConfirmJournal(mapId);
        }
    }
}
