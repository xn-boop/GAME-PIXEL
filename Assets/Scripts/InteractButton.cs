
using UnityEngine;

public class InteractButton : MonoBehaviour
{
    [SerializeField] private PlayerInteraction playerInteraction;

    public void OnInteractButtonPressed()
    {
        if (playerInteraction != null)
        {
            playerInteraction.Interact();
        }
        else
        {
            Debug.LogWarning(
                "Chưa gán PlayerInteraction cho InteractButton!"
            );
        }
    }
}
