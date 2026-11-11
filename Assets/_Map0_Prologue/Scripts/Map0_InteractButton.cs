using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Map0
{
    public class Map0_InteractButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Map0_PlayerController playerController;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI buttonText;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.18f, 0.70f, 0.35f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.12f, 0.50f, 0.25f, 1f);

        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonImage != null) buttonImage.color = normalColor;
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = Map0_PlayerController.Instance;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = initialScale * 0.92f;
            if (buttonImage != null) buttonImage.color = pressedColor;

            // If dialogue is active, tapping the interact button advances dialogue!
            if (Map0_UIManager.Instance != null && Map0_UIManager.Instance.IsDialogueActive)
            {
                Map0_UIManager.Instance.AdvanceDialogue();
                return;
            }

            if (playerController != null)
            {
                playerController.TriggerInteract();
            }
            else if (Map0_PlayerController.Instance != null)
            {
                Map0_PlayerController.Instance.TriggerInteract();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = initialScale;
            if (buttonImage != null) buttonImage.color = normalColor;
        }

        public void SetButtonText(string text)
        {
            if (buttonText != null) buttonText.text = text;
        }
    }
}
