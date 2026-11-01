using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map3
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Map3_PlayerController : MonoBehaviour
    {
        public static Map3_PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.5f;

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

        private readonly List<Map3_Interactable> nearbyInteractables = new List<Map3_Interactable>();

        public Vector2 MoveDirection => moveInput;
        public bool IsMoving => isMoving;
        public bool IsMovementLocked => isMovementLocked;
        public Map3_Interactable CurrentInteractable => nearbyInteractables.Count > 0 ? nearbyInteractables[0] : null;

        public event Action<Map3_Interactable> OnInteractableChanged;

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
                // Safe catch to suppress warning if parameter is missing
            }
        }

        private void Update()
        {
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

            // Interact key
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
                if (rb != null) rb.linearVelocity = Vector2.zero;
                SetMovingAnimation(false);
            }
        }

        public void TeleportTo(Vector3 targetPos)
        {
            transform.position = targetPos;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void RegisterInteractable(Map3_Interactable interactable)
        {
            if (interactable != null && !nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
                UpdateInteractPrompt();
                OnInteractableChanged?.Invoke(CurrentInteractable);
            }
        }

        public void UnregisterInteractable(Map3_Interactable interactable)
        {
            if (interactable != null && nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Remove(interactable);
                UpdateInteractPrompt();
                OnInteractableChanged?.Invoke(CurrentInteractable);
            }
        }

        private void UpdateInteractPrompt()
        {
            nearbyInteractables.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy || !item.CanInteract);
            if (CurrentInteractable != null && CurrentInteractable.CanInteract)
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.SetInteractPrompt(true, CurrentInteractable.PromptText);
                }
            }
            else
            {
                if (Map3_UIManager.Instance != null)
                {
                    Map3_UIManager.Instance.SetInteractPrompt(false);
                }
            }
        }

        public void TriggerInteract()
        {
            if (Map3_QuizManager.Instance != null && Map3_QuizManager.Instance.IsQuizOpen)
            {
                return;
            }

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
}
