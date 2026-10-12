using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Movement Lock")]
    [SerializeField] private bool isMovementLocked = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 joystickInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "PlayerMovement cần có Rigidbody2D trên Player!"
            );
        }
    }

    private void Update()
    {
        // Khi bị khóa, xóa input và không nhận điều khiển mới.
        if (isMovementLocked)
        {
            moveInput = Vector2.zero;
            joystickInput = Vector2.zero;
            return;
        }

        // Đọc input bàn phím mỗi frame.
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 keyboardInput = new Vector2(moveX, moveY);

        // Ưu tiên bàn phím khi người chơi đang nhấn phím.
        if (keyboardInput.sqrMagnitude > 0f)
        {
            moveInput = keyboardInput.normalized;
        }
        else
        {
            // Không nhấn phím thì dùng joystick.
            moveInput = joystickInput;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Khóa cả vận tốc để nhân vật dừng ngay.
        if (isMovementLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    // Joystick gọi hàm này khi người chơi kéo cần.
    public void SetMoveInput(Vector2 input)
    {
        if (isMovementLocked) return;

        joystickInput = Vector2.ClampMagnitude(input, 1f);
    }

    // Joystick gọi hàm này khi người chơi thả cần.
    public void StopMovement()
    {
        joystickInput = Vector2.zero;
    }

    // Cho phép script khác khóa hoặc mở khóa di chuyển.
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
}