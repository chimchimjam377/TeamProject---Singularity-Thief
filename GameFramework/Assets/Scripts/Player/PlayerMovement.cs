using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private WallDetector wallDetector;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumpCount = 2;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Climb")]
    [SerializeField] private float wallClimbSpeed = 4f;

    private float moveInput;
    private float verticalInput;

    private int jumpCount;

    private bool isGrounded;
    private bool isWallClimbing;

    private float defaultGravityScale;

    public bool IsGrounded => isGrounded;
    public bool IsWallClimbing => isWallClimbing;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (wallDetector == null)
            wallDetector = GetComponent<WallDetector>();

        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        ReadInput();

        CheckGround();
        HandleWallClimbState();
        HandleJump();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleWallClimb();
    }

    // =========================================================
    // Input
    // =========================================================

    private void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    // =========================================================
    // Ground
    // =========================================================

    private void CheckGround()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // 착지했을 때 점프 횟수 초기화
        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0;
        }

        // 지상에서는 벽타기 종료
        if (isGrounded && isWallClimbing)
        {
            StopWallClimb();
        }
    }

    // =========================================================
    // Movement
    // =========================================================

    private void HandleMovement()
    {
        if (isWallClimbing)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            return;
        }

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // =========================================================
    // Jump
    // =========================================================

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        // 벽타기 중 점프
        if (isWallClimbing)
        {
            StopWallClimb();

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            return;
        }

        // 더블 점프 횟수 초과
        if (jumpCount >= maxJumpCount)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpCount++;
    }

    // =========================================================
    // Wall Climb State
    // =========================================================

    private void HandleWallClimbState()
    {
        // 지상에서는 벽타기 불가능
        if (isGrounded)
            return;

        // 이미 벽타기 중이라면 계속 유지 가능한지 검사
        if (isWallClimbing)
        {
            bool stillCanClimb =
                wallDetector.HasWallOnDirection(moveInput);

            if (!stillCanClimb)
            {
                StopWallClimb();
            }

            return;
        }

        // 벽타기 시작 조건
        bool hasWall =
            wallDetector.HasWallOnDirection(moveInput);

        if (!hasWall)
            return;

        StartWallClimb();
    }

    private void StartWallClimb()
    {
        isWallClimbing = true;

        rb.gravityScale = 0f;

        rb.linearVelocity = Vector2.zero;
    }

    private void StopWallClimb()
    {
        isWallClimbing = false;

        rb.gravityScale = defaultGravityScale;
    }

    // =========================================================
    // Wall Climb Movement
    // =========================================================

    private void HandleWallClimb()
    {
        if (!isWallClimbing)
            return;

        rb.linearVelocity = new Vector2(
            0f,
            verticalInput * wallClimbSpeed
        );
    }

    // =========================================================
    // Gizmos
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}