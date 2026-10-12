
using UnityEngine;

public class Level1MusicStarter : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevel1Music();
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy AudioManager trong Scene!");
        }
    }
}
