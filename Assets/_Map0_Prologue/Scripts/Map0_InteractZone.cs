using UnityEngine;

namespace Game.Map0
{
    public enum Map0_ZoneType
    {
        WallCalendar,
        FamilyPhoto,
        TimeCapsule,
        KitchenDoor,
        YellowCat,
        Custom
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class Map0_InteractZone : MonoBehaviour
    {
        [Header("Zone Type & Display")]
        [SerializeField] private Map0_ZoneType zoneType = Map0_ZoneType.Custom;
        [SerializeField] private string objectDisplayName = "Điểm Tương Tác";
        [SerializeField] private string promptText = "[E] Tương tác";
        [SerializeField] private GameObject interactIndicator;
        [SerializeField] private bool canInteract = true;

        [Header("Visual Tuning")]
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 0.4f, 0.75f);

        private bool isPlayerInRange = false;

        public Map0_ZoneType ZoneType => zoneType;
        public string ObjectDisplayName => objectDisplayName;
        public string PromptText => promptText;
        
        public bool CanInteract => canInteract 
            && gameObject.activeInHierarchy 
            && (Map0_UIManager.Instance == null || !Map0_UIManager.Instance.IsDialogueActive)
            && (Map0_PlayerController.Instance == null || !Map0_PlayerController.Instance.IsInteractionOnCooldown);

        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            if (interactIndicator == null)
            {
                var ind = transform.Find("Interact_Indicator");
                if (ind != null) interactIndicator = ind.gameObject;
            }

            if (interactIndicator != null)
            {
                interactIndicator.SetActive(false);
            }
        }

        public void SetupZone(Map0_ZoneType type, string displayName, string prompt)
        {
            zoneType = type;
            objectDisplayName = displayName;
            promptText = prompt;
        }

        public void SetCanInteract(bool value)
        {
            canInteract = value;
            if (!canInteract)
            {
                ShowIndicator(false);
            }
            else if (isPlayerInRange)
            {
                ShowIndicator(true);
            }
        }

        public virtual void Interact()
        {
            if (!CanInteract) return;

            // Extra debounce check
            if (Map0_UIManager.Instance != null && Map0_UIManager.Instance.IsDialogueActive) return;
            if (Map0_PlayerController.Instance != null && Map0_PlayerController.Instance.IsInteractionOnCooldown) return;

            if (Map0_PrologueManager.Instance != null)
            {
                Map0_PrologueManager.Instance.OnZoneInteracted(this);
            }
        }

        public void ShowIndicator(bool show)
        {
            if (interactIndicator != null)
            {
                interactIndicator.SetActive(show && CanInteract);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract) return;

            var player = other.GetComponent<Map0_PlayerController>() ?? other.GetComponentInParent<Map0_PlayerController>();
            if (player == null && other.CompareTag("Player"))
            {
                player = Map0_PlayerController.Instance;
            }

            if (player != null)
            {
                isPlayerInRange = true;
                ShowIndicator(true);
                player.RegisterInteractable(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponent<Map0_PlayerController>() ?? other.GetComponentInParent<Map0_PlayerController>();
            if (player == null && other.CompareTag("Player"))
            {
                player = Map0_PlayerController.Instance;
            }

            if (player != null)
            {
                isPlayerInRange = false;
                ShowIndicator(false);
                player.UnregisterInteractable(this);
            }
        }

        private void OnDisable()
        {
            if (isPlayerInRange && Map0_PlayerController.Instance != null)
            {
                Map0_PlayerController.Instance.UnregisterInteractable(this);
            }
            isPlayerInRange = false;
            ShowIndicator(false);
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;

            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.15f);
            Gizmos.DrawCube(col.offset, col.size);
        }
    }
}
