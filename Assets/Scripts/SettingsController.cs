
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public GameObject pausePanel;

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void BackToPause()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
}
