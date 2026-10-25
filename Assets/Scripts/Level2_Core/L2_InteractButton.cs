using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class L2_InteractButton : MonoBehaviour
{
    [SerializeField] private L2_PlayerController playerController;
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
            playerController = L2_PlayerController.Instance;
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
            playerController = L2_PlayerController.Instance;
        }

        if (playerController != null)
        {
            playerController.TriggerInteract();
        }
    }

    private void UpdateButtonVisual(L2_Interactable current)
    {
        bool hasInteractable = current != null && current.CanInteract;

        if (buttonImage != null)
        {
            buttonImage.color = hasInteractable ? activeColor : idleColor;
        }

        if (buttonText != null)
        {
            buttonText.color = hasInteractable ? Color.white : new Color(1f, 1f, 1f, 0.6f);
        }
    }
}
