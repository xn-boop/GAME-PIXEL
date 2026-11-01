using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Map3
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Map3_Interactable : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string objectDisplayName = "Đối tượng";
        [SerializeField] protected string promptText = "[E] Tương tác";
        [SerializeField] protected GameObject interactIndicator;
        [SerializeField] protected Color gizmoColor = new Color(0.2f, 0.9f, 0.4f, 0.8f);

        protected bool isPlayerInRange = false;

        public string ObjectDisplayName => objectDisplayName;
        public string PromptText => promptText;
        public virtual bool CanInteract => true;

        protected virtual void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            if (interactIndicator == null)
            {
                Transform ind = transform.Find("Interact_Indicator");
                if (ind != null)
                {
                    interactIndicator = ind.gameObject;
                }
            }

            if (interactIndicator != null)
            {
                interactIndicator.SetActive(false);
            }
        }

        public abstract void Interact();

        public virtual void ShowIndicator(bool show)
        {
            if (interactIndicator != null)
            {
                interactIndicator.SetActive(show && CanInteract);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanInteract) return;

            var player = other.GetComponent<Map3_PlayerController>() ?? other.GetComponentInParent<Map3_PlayerController>();
            if (player == null && other.CompareTag("Player"))
            {
                player = Map3_PlayerController.Instance;
            }

            if (player != null)
            {
                isPlayerInRange = true;
                ShowIndicator(true);
                player.RegisterInteractable(this);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponent<Map3_PlayerController>() ?? other.GetComponentInParent<Map3_PlayerController>();
            if (player == null && other.CompareTag("Player"))
            {
                player = Map3_PlayerController.Instance;
            }

            if (player != null)
            {
                isPlayerInRange = false;
                ShowIndicator(false);
                player.UnregisterInteractable(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (isPlayerInRange && Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.UnregisterInteractable(this);
            }
            isPlayerInRange = false;
            ShowIndicator(false);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            Vector3 center = transform.position;
            Vector3 size = Vector3.one;

            if (col != null)
            {
                center = transform.TransformPoint(col.offset);
                size = Vector3.Scale(col.size, transform.lossyScale);
            }

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(center, size);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = gizmoColor;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            Handles.Label(center + Vector3.up * (size.y * 0.5f + 0.35f), $"[{objectDisplayName}]", style);
        }
#endif
    }
}
