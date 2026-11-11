using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class L1_Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    [SerializeField] protected GameObject interactIndicator;
    [SerializeField] protected string objectDisplayName = "Đối tượng";

    protected bool isPlayerInRange = false;

    public string ObjectDisplayName => objectDisplayName;
    public virtual bool CanInteract => true;

    protected virtual void Awake()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
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

        L1_PlayerController player = other.GetComponent<L1_PlayerController>();
        if (player != null)
        {
            isPlayerInRange = true;
            ShowIndicator(true);
            player.RegisterInteractable(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        L1_PlayerController player = other.GetComponent<L1_PlayerController>();
        if (player != null)
        {
            isPlayerInRange = false;
            ShowIndicator(false);
            player.UnregisterInteractable(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (isPlayerInRange && L1_PlayerController.Instance != null)
        {
            L1_PlayerController.Instance.UnregisterInteractable(this);
        }
        isPlayerInRange = false;
        ShowIndicator(false);
    }
}
