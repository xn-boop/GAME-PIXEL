using UnityEngine;

/// <summary>
/// Quản lý hiển thị Sprite và diễn hoạt bước đi tự nhiên cho nhân vật Hạ Ân theo chuẩn MOTION HA.
/// Đảm bảo nhân vật luôn hiển thị liên tục, giữ cố định 1 kích thước tuyệt đối không bị co giãn to nhỏ.
/// </summary>
public class PlayerVisualAnimator : MonoBehaviour
{
    public enum Direction
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3,
        UpRight = 4,
        DownRight = 5,
        UpLeft = 6,
        DownLeft = 7
    }

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Idle Sprites")]
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite idleRight;
    [SerializeField] private Sprite idleUpRight;
    [SerializeField] private Sprite idleDownRight;

    [Header("Walk Animations")]
    [SerializeField] private Sprite[] walkDownFrames;
    [SerializeField] private Sprite[] walkUpFrames;
    [SerializeField] private Sprite[] walkLeftFrames;
    [SerializeField] private Sprite[] walkRightFrames;
    [SerializeField] private Sprite[] walkUpRightFrames;
    [SerializeField] private Sprite[] walkDownRightFrames;

    [Header("Animation Settings")]
    [SerializeField] private float walkFps = 7.5f;
    [SerializeField] private float runFps = 12f;
    [SerializeField] private Direction initialDirection = Direction.Right;
    [SerializeField] private bool use4DirectionsOnly = true;

    private Direction currentDirection = Direction.Right;
    private float animationTimer = 0f;
    private int currentFrame = 0;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        currentDirection = initialDirection;
        UpdateIdleSprite();
    }

    private void Update()
    {
        if (playerMovement == null || spriteRenderer == null) return;

        Vector2 input = playerMovement.CurrentMoveInput;
        bool isMoving = playerMovement.IsGroundedMoving;
        bool isRunning = playerMovement.IsRunning;

        if (isMoving)
        {
            UpdateDirection(input);

            // Đồng bộ hoàn hảo tốc độ animation với tốc độ di chuyển thực tế trên sàn nhà
            float baseSpeed = Mathf.Max(playerMovement.MoveSpeed, 0.1f);
            float actualSpeed = playerMovement.CurrentSpeed;
            float speedRatio = Mathf.Clamp(actualSpeed / baseSpeed, 0.2f, 2.0f);

            float baseFps = isRunning ? runFps : walkFps;
            if (baseFps <= 0.1f) baseFps = 7.5f;

            float currentFps = baseFps * speedRatio;

            animationTimer += Time.deltaTime * currentFps;
            if (animationTimer >= 1f)
            {
                animationTimer -= 1f;
                currentFrame++;
            }

            ApplyWalkFrame();
        }
        else
        {
            animationTimer = 0f;
            currentFrame = 0;
            UpdateIdleSprite();
        }
    }

    private void UpdateDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return;

        float x = input.x;
        float y = input.y;
        float absX = Mathf.Abs(x);
        float absY = Mathf.Abs(y);

        if (!use4DirectionsOnly && absX > 0.45f && absY > 0.45f)
        {
            if (x > 0 && y > 0) currentDirection = Direction.UpRight;
            else if (x > 0 && y < 0) currentDirection = Direction.DownRight;
            else if (x < 0 && y > 0) currentDirection = Direction.UpLeft;
            else if (x < 0 && y < 0) currentDirection = Direction.DownLeft;
            return;
        }

        // 4 hướng chuẩn mực, dứt khoát, không giật lắc
        if (absX >= absY)
        {
            currentDirection = x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            currentDirection = y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private void ApplyWalkFrame()
    {
        Sprite[] activeFrames = null;
        bool flipX = false;

        switch (currentDirection)
        {
            case Direction.Down:
                activeFrames = walkDownFrames;
                break;
            case Direction.Up:
                activeFrames = walkUpFrames;
                break;
            case Direction.Left:
                activeFrames = walkLeftFrames;
                break;
            case Direction.Right:
                if (walkRightFrames != null && walkRightFrames.Length > 0)
                {
                    activeFrames = walkRightFrames;
                }
                else
                {
                    activeFrames = walkLeftFrames;
                    flipX = true;
                }
                break;
            case Direction.UpRight:
                if (walkUpRightFrames != null && walkUpRightFrames.Length > 0)
                {
                    activeFrames = walkUpRightFrames;
                }
                else if (walkRightFrames != null && walkRightFrames.Length > 0)
                {
                    activeFrames = walkRightFrames;
                }
                else
                {
                    activeFrames = walkUpFrames;
                }
                break;
            case Direction.DownRight:
                if (walkDownRightFrames != null && walkDownRightFrames.Length > 0)
                {
                    activeFrames = walkDownRightFrames;
                }
                else if (walkRightFrames != null && walkRightFrames.Length > 0)
                {
                    activeFrames = walkRightFrames;
                }
                else
                {
                    activeFrames = walkDownFrames;
                }
                break;
            case Direction.UpLeft:
                if (walkUpRightFrames != null && walkUpRightFrames.Length > 0)
                {
                    activeFrames = walkUpRightFrames;
                    flipX = true;
                }
                else if (walkLeftFrames != null && walkLeftFrames.Length > 0)
                {
                    activeFrames = walkLeftFrames;
                }
                else
                {
                    activeFrames = walkUpFrames;
                }
                break;
            case Direction.DownLeft:
                if (walkDownRightFrames != null && walkDownRightFrames.Length > 0)
                {
                    activeFrames = walkDownRightFrames;
                    flipX = true;
                }
                else if (walkLeftFrames != null && walkLeftFrames.Length > 0)
                {
                    activeFrames = walkLeftFrames;
                }
                else
                {
                    activeFrames = walkDownFrames;
                }
                break;
        }

        // Tuyệt đối không để sprite bị null gây hiện tượng biến mất rồi hiện
        if (activeFrames != null && activeFrames.Length > 0)
        {
            int index = currentFrame % activeFrames.Length;
            Sprite nextSprite = activeFrames[index];
            if (nextSprite != null)
            {
                spriteRenderer.sprite = nextSprite;
            }
            spriteRenderer.flipX = flipX;
        }
        else
        {
            UpdateIdleSprite();
        }
    }

    private void UpdateIdleSprite()
    {
        if (spriteRenderer == null) return;

        Sprite targetIdle = null;
        bool flipX = false;

        switch (currentDirection)
        {
            case Direction.Down:
                targetIdle = idleDown;
                break;
            case Direction.Up:
                targetIdle = idleUp;
                break;
            case Direction.Left:
                targetIdle = idleLeft;
                break;
            case Direction.Right:
                if (idleRight != null) targetIdle = idleRight;
                else if (idleLeft != null) { targetIdle = idleLeft; flipX = true; }
                break;
            case Direction.UpRight:
                if (idleUpRight != null) targetIdle = idleUpRight;
                else if (idleRight != null) targetIdle = idleRight;
                else targetIdle = idleUp;
                break;
            case Direction.DownRight:
                if (idleDownRight != null) targetIdle = idleDownRight;
                else if (idleRight != null) targetIdle = idleRight;
                else targetIdle = idleDown;
                break;
            case Direction.UpLeft:
                if (idleUpRight != null) { targetIdle = idleUpRight; flipX = true; }
                else if (idleLeft != null) targetIdle = idleLeft;
                else targetIdle = idleUp;
                break;
            case Direction.DownLeft:
                if (idleDownRight != null) { targetIdle = idleDownRight; flipX = true; }
                else if (idleLeft != null) targetIdle = idleLeft;
                else targetIdle = idleDown;
                break;
        }

        if (targetIdle != null)
        {
            spriteRenderer.sprite = targetIdle;
        }
        spriteRenderer.flipX = flipX;
    }

    public void SetDirection(Direction dir)
    {
        currentDirection = dir;
        UpdateIdleSprite();
    }
}
