using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map0
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Map0_PlayerController : MonoBehaviour
    {
        public static Map0_PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4.5f;

        [Header("Component References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        private Vector2 keyboardInput = Vector2.zero;
        private Vector2 joystickInput = Vector2.zero;
        private Vector2 moveInput = Vector2.zero;
        private bool isMoving = false;
        private bool isMovementLocked = false;

        private bool hasIsMovingParam = false;
        private string movingParamName = null;

        // Interaction Cooldown / Debounce
        private float interactCooldownTimer = 0f;

        private readonly List<Map0_InteractZone> nearbyInteractables = new List<Map0_InteractZone>();

        public Vector2 MoveDirection => moveInput;
        public bool IsMoving => isMoving;
        public bool IsMovementLocked => isMovementLocked;
        public bool IsInteractionOnCooldown => interactCooldownTimer > 0f;
        public Map0_InteractZone CurrentInteractable => nearbyInteractables.Count > 0 ? nearbyInteractables[0] : null;

        public event Action<Map0_InteractZone> OnInteractableChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            InitAnimatorParameters();
        }

        private void Start()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            InitAnimatorParameters();
        }

        private void InitAnimatorParameters()
        {
            if (animator == null) return;
            hasIsMovingParam = false;
            foreach (var p in animator.parameters)
            {
                if (string.Equals(p.name, "isMoving", StringComparison.OrdinalIgnoreCase))
                {
                    hasIsMovingParam = true;
                    movingParamName = p.name;
                    break;
                }
            }
        }

        private void SetMovingAnimation(bool moving)
        {
            if (!hasIsMovingParam || animator == null) return;
            try
            {
                animator.SetBool(movingParamName, moving);
            }
            catch
            {
                // Suppress if parameter missing
            }
        }

        private void Update()
        {
            // Update debounce timer
            if (interactCooldownTimer > 0f)
            {
                interactCooldownTimer -= Time.deltaTime;
            }

            // If dialogue is active, player movement and interact listening are disabled (UIManager handles dialogue input exclusively)
            if (Map0_UIManager.Instance != null && Map0_UIManager.Instance.IsDialogueActive)
            {
                moveInput = Vector2.zero;
                isMoving = false;
                SetMovingAnimation(false);
                return;
            }

            if (isMovementLocked)
            {
                moveInput = Vector2.zero;
                isMoving = false;
                SetMovingAnimation(false);
                return;
            }

            // Keyboard input (WASD / Arrows)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            keyboardInput = new Vector2(h, v);

            if (keyboardInput.sqrMagnitude > 0.01f)
            {
                moveInput = keyboardInput.normalized;
            }
            else
            {
                moveInput = joystickInput;
            }

            isMoving = moveInput.sqrMagnitude > 0.01f;

            // Flip sprite based on horizontal direction
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
            }

            SetMovingAnimation(isMoving);

            // Interact key: [E] or [Space] (only when dialogue is NOT active and not on cooldown)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TriggerInteract();
            }
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            if (isMovementLocked || (Map0_UIManager.Instance != null && Map0_UIManager.Instance.IsDialogueActive))
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
                if (rb != null) rb.linearVelocity = Vector2.zero;
                SetMovingAnimation(false);
            }
        }

        public void StartInteractionCooldown(float duration = 0.3f)
        {
            interactCooldownTimer = Mathf.Max(interactCooldownTimer, duration);
        }

        public void TeleportTo(Vector3 targetPos)
        {
            transform.position = targetPos;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void RegisterInteractable(Map0_InteractZone interactable)
        {
            if (interactable != null && !nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
                UpdateInteractPrompt();
                OnInteractableChanged?.Invoke(CurrentInteractable);
            }
        }

        public void UnregisterInteractable(Map0_InteractZone interactable)
        {
            if (interactable != null && nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Remove(interactable);
                UpdateInteractPrompt();
                OnInteractableChanged?.Invoke(CurrentInteractable);
            }
        }

        public void UpdateInteractPrompt()
        {
            if (Map0_UIManager.Instance == null) return;

            var current = CurrentInteractable;
            if (current != null && current.CanInteract && !IsInteractionOnCooldown)
            {
                Map0_UIManager.Instance.SetInteractPrompt(true, current.PromptText);
            }
            else
            {
                Map0_UIManager.Instance.SetInteractPrompt(false);
            }
        }

        public void TriggerInteract()
        {
            // Debounce / Cooldown protection
            if (interactCooldownTimer > 0f) return;
            if (Map0_UIManager.Instance != null && Map0_UIManager.Instance.IsDialogueActive) return;
            if (isMovementLocked) return;

            var current = CurrentInteractable;
            if (current != null && current.CanInteract)
            {
                current.Interact();
            }
        }
    }
}
