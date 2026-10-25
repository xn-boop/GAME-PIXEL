using UnityEngine;

/// <summary>
/// Quản lý di chuyển đi bộ tự do 4 hướng (trái, phải, lên, xuống) trên mặt sàn phòng bếp Map 1.
/// Chân nhân vật luôn bám sàn tự nhiên, không nhảy, không bay lên, không xuyên qua tường/nội thất.
/// Khi thả phím hoặc thả joystick thì lập tức đứng yên, không trôi trượt quán tính.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings (4 Hướng: Trái, Phải, Lên, Xuống)")]
    [SerializeField] private float moveSpeed = 2.4f;

    [Header("Run Settings")]
    [SerializeField] private float runMultiplier = 1.5f;

    [Header("Physics Settings")]
    [SerializeField] private float gravityScale = 0f; // Top-down 2D: không dùng trọng lực rơi tự do trục Y

    [Header("Movement Lock")]
    [SerializeField] private bool isMovementLocked = false;

    [Header("Scale Lock")]
    [SerializeField] private Vector3 fixedScale = new Vector3(0.55f, 0.55f, 1f);
    [SerializeField] private bool lockScale = true;

    [Header("Ground Check & Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.90f);
    [SerializeField] private float groundCheckRadius = 0.20f;

    private Rigidbody2D rb;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 joystickInput = Vector2.zero;
    private bool isGrounded = true;

    public float MoveSpeed => moveSpeed;
    public float RunMultiplier => runMultiplier;
    public Vector2 CurrentMoveInput => moveInput;
    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : (moveInput.magnitude * moveSpeed);
    public bool IsMoving => !isMovementLocked && moveInput.sqrMagnitude > 0.001f;
    public bool IsGrounded => isGrounded;
    public bool IsGroundedMoving => !isMovementLocked && CurrentSpeed > 0.05f;
    public bool IsRunning => IsMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = gravityScale; // Top-down 2D: trọng lực = 0, nhân vật bám trên mặt sàn phòng
            // Khôi phục di chuyển 4 hướng tự do: giải phóng hoàn toàn trục X và Y, chỉ khóa xoay trục Z
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Chống xuyên qua tường và sàn
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Khử giật khung hình
        }
        else
        {
            Debug.LogError("PlayerMovement cần có Rigidbody2D trên Player!");
        }

        // Tự động gán groundLayer nếu chưa được gán trong Inspector
        if (groundLayer.value == 0)
        {
            int gLayer = LayerMask.NameToLayer("Ground");
            if (gLayer != -1)
            {
                groundLayer = 1 << gLayer;
            }
        }

        if (lockScale)
        {
            transform.localScale = fixedScale;
        }
    }

    private void LateUpdate()
    {
        // Khóa cố định 1 kích thước tuyệt đối, ngăn ngừa mọi hiện tượng scale to nhỏ liên tục
        if (lockScale && transform.localScale != fixedScale)
        {
            transform.localScale = fixedScale;
        }
    }

    private void Update()
    {
        PerformGroundCheck();

        // Khi bị khóa, xóa input và đứng yên
        if (isMovementLocked)
        {
            moveInput = Vector2.zero;
            joystickInput = Vector2.zero;
            return;
        }

        // ĐỌC INPUT 4 HƯỚNG: Trái (-X), Phải (+X), Lên (+Y), Xuống (-Y)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 keyboardInput = new Vector2(moveX, moveY);

        if (keyboardInput.sqrMagnitude > 0.01f)
        {
            // Chuẩn hóa vector khi đi chéo để tốc độ đồng đều mọi hướng
            moveInput = keyboardInput.normalized;
        }
        else if (joystickInput.sqrMagnitude > 0.01f)
        {
            moveInput = Vector2.ClampMagnitude(joystickInput, 1f);
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Khi bị khóa di chuyển, dừng ngay lập tức
        if (isMovementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Di chuyển 4 hướng mượt mà; khi thả phím (moveInput == 0) thì đứng yên lập tức
        if (moveInput.sqrMagnitude > 0.001f)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = moveSpeed * (isRunning ? runMultiplier : 1f);
            rb.linearVelocity = moveInput * currentSpeed;
        }
        else
        {
            // Thả phím: vận tốc lập tức về 0, nhân vật đứng yên vững chãi bám sàn, không trôi trượt
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void PerformGroundCheck()
    {
        Vector2 feetPosition = (Vector2)transform.position + groundCheckOffset;

        int mask = groundLayer.value;
        if (mask == 0)
        {
            int gLayer = LayerMask.NameToLayer("Ground");
            if (gLayer != -1) mask = 1 << gLayer;
        }

        if (mask != 0)
        {
            Collider2D groundCol = Physics2D.OverlapCircle(feetPosition, groundCheckRadius, mask);
            isGrounded = groundCol != null;
        }
        else
        {
            Collider2D groundCol = Physics2D.OverlapCircle(feetPosition, groundCheckRadius);
            isGrounded = groundCol != null && groundCol.gameObject != gameObject;
        }
    }

    // Joystick gọi hàm này khi người chơi kéo cần
    public void SetMoveInput(Vector2 input)
    {
        if (isMovementLocked) return;
        joystickInput = input;
        moveInput = input.sqrMagnitude > 0.001f ? input.normalized : Vector2.zero;
    }

    // Joystick gọi hàm này khi người chơi thả cần hoặc thả phím
    public void StopMovement()
    {
        joystickInput = Vector2.zero;
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Cho phép script khác khóa hoặc mở khóa di chuyển
    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            joystickInput = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // Hiển thị trực quan Gizmos trong Scene view
    private void OnDrawGizmosSelected()
    {
        Vector2 feetPos = (Vector2)transform.position + groundCheckOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feetPos, groundCheckRadius);
        Gizmos.DrawLine(feetPos, feetPos + Vector2.down * 0.15f);

        var col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere((Vector2)transform.position + col.offset, col.radius);
        }
    }
}
