using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Map3
{
    [RequireComponent(typeof(Button))]
    public class Map3_InteractButton : MonoBehaviour
    {
        [SerializeField] private Map3_PlayerController playerController;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI buttonText;

        private Button button;

        private readonly Color activeColor = new Color(1f, 0.76f, 0.03f, 1f); // Warm Gold (#FFC107)
        private readonly Color idleColor = new Color(1f, 1f, 1f, 0.5f); // 50% translucent white

        private void Awake()
        {
            button = GetComponent<Button>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(OnClickInteract);
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = Map3_PlayerController.Instance;
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
                playerController = Map3_PlayerController.Instance;
            }

            if (playerController != null)
            {
                playerController.TriggerInteract();
            }
        }

        private void UpdateButtonVisual(Map3_Interactable current)
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
}
