
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        JourneyOfMemories.Core.Managers.GameStateManager.Instance?.AcquirePlayerControlLock("Pause");
    }

    public void ResumeGame()
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        JourneyOfMemories.Core.Managers.GameStateManager.Instance?.ReleasePlayerControlLock("Pause");
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
}
