using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class L2_PlayerController : MonoBehaviour
{
    public static L2_PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.5f;

    [Header("Component References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Visual & Animation References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector2 keyboardInput = Vector2.zero;
    private Vector2 joystickInput = Vector2.zero;
    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;
    private bool isMovementLocked = false;

    private readonly List<L2_Interactable> nearbyInteractables = new List<L2_Interactable>();

    // Public properties for future 8-directional Animator integration
    public Vector2 MoveDirection => moveInput;
    public bool IsMoving => isMoving;
    public bool IsMovementLocked => isMovementLocked;
    public L2_Interactable CurrentInteractable => nearbyInteractables.Count > 0 ? nearbyInteractables[0] : null;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;

    public event Action<L2_Interactable> OnInteractableChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (isMovementLocked)
        {
            moveInput = Vector2.zero;
            isMoving = false;
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
            return;
        }

        // Keyboard input (WASD / Arrows)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        keyboardInput = new Vector2(horizontal, vertical);

        if (keyboardInput.sqrMagnitude > 0.01f)
        {
            moveInput = keyboardInput.normalized;
        }
        else
        {
            moveInput = joystickInput;
        }

        isMoving = moveInput.sqrMagnitude > 0.01f;

        // Flip sprite based on horizontal movement
        if (spriteRenderer != null)
        {
            if (moveInput.x > 0.05f)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput.x < -0.05f)
            {
                spriteRenderer.flipX = true;
            }
            // When stopped or moving only vertically (|moveInput.x| <= 0.05f),
            // DO NOT modify spriteRenderer.flipX to keep facing direction during Idle
        }

        // Update animation flag
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        // Interaction keypress (E or Space)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            TriggerInteract();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (isMovementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void SetJoystickInput(Vector2 input)
    {
        joystickInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;
        if (locked)
        {
            moveInput = Vector2.zero;
            joystickInput = Vector2.zero;
            keyboardInput = Vector2.zero;
            isMoving = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    public void RegisterInteractable(L2_Interactable interactable)
    {
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            OnInteractableChanged?.Invoke(CurrentInteractable);
        }
    }

    public void UnregisterInteractable(L2_Interactable interactable)
    {
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
            OnInteractableChanged?.Invoke(CurrentInteractable);
        }
    }

    public void TriggerInteract()
    {
        // Don't interact if Quiz Dialog is open or movement is locked
        if (L2_QuizManager.Instance != null && L2_QuizManager.Instance.IsQuizOpen)
        {
            return;
        }

        // Clean nulls from nearby list
        nearbyInteractables.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy || !item.CanInteract);

        if (CurrentInteractable != null && CurrentInteractable.CanInteract)
        {
            CurrentInteractable.Interact();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
