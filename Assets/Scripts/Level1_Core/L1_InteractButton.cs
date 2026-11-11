using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class L1_InteractButton : MonoBehaviour
{
    [SerializeField] private L1_PlayerController playerController;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMPro.TextMeshProUGUI buttonText;

    private Button button;

    private readonly Color activeColor = new Color(1f, 0.76f, 0.03f, 1f); // Warm Gold/Amber (#FFC107)
    private readonly Color idleColor = new Color(1f, 1f, 1f, 0.5f); // 50% translucent white

    private void Awake()
    {
        button = GetComponent<Button>();
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        button.onClick.AddListener(OnClickInteract);
    }

    private void Start()
    {
        if (playerController == null)
        {
            playerController = L1_PlayerController.Instance;
        }

        if (playerController != null)
        {
            playerController.OnInteractableChanged += UpdateButtonVisual;
        }

        UpdateButtonVisual(playerController != null ? playerController.CurrentInteractable : null);
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnInteractableChanged -= UpdateButtonVisual;
        }
    }

    private void OnClickInteract()
    {
        if (playerController == null)
        {
            playerController = L1_PlayerController.Instance;
        }

        if (playerController != null)
        {
            playerController.TriggerInteract();
        }
    }

    private void UpdateButtonVisual(L1_Interactable interactable)
    {
        bool hasTarget = interactable != null && interactable.CanInteract;

        if (buttonImage != null)
        {
            buttonImage.color = hasTarget ? activeColor : idleColor;
        }

        if (button != null)
        {
            button.interactable = hasTarget;
        }
    }
}
