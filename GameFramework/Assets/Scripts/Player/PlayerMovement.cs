using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private WallDetector wallDetector;
    [SerializeField] private Transform visual;

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

    [Header("State")]
    [SerializeField] private PlayerState playerState;

    private float moveInput;
    private float verticalInput;

    private int jumpCount;

    private bool isGrounded;
    private bool isWallClimbing;

    // 현재 붙어 있는 벽
    // -1 = 왼쪽
    // +1 = 오른쪽
    private int currentWallDirection;

    private float defaultGravityScale;

    public bool IsGrounded => isGrounded;
    public bool IsWallClimbing => isWallClimbing;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (wallDetector == null)
            wallDetector = GetComponent<WallDetector>();

        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        // Visual 자동 찾기
        if (visual == null)
            visual = transform.Find("Visual");

        defaultGravityScale = rb.gravityScale;
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        ReadInput();

        CheckGround();
        HandleWallClimbState();
        HandleJump();
    }


    // =========================================================
    // FixedUpdate
    // =========================================================

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
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            jumpCount = 0;
        }

        // 중요!
        // 여기서 WallClimbing을 강제로 종료하지 않는다.
        //
        // 벽 + 바닥 모서리에서는
        // Grounded = true
        // Wall = true
        // 가 동시에 될 수 있기 때문이다.
    }


    // =========================================================
    // Movement
    // =========================================================

    private void HandleMovement()
    {
        // 닷지 중에는 일반 이동이 PlayerDodge가 담당한다.
        if (playerState.Is(PlayerStateType.Dodge))
            return;


        // =====================================================
        // 플레이어 방향 전환
        // =====================================================

        if (moveInput > 0)
        {
            // 오른쪽
            visual.localScale = new Vector3(
                1f,
                1f,
                1f
            );
        }
        else if (moveInput < 0)
        {
            // 왼쪽
            visual.localScale = new Vector3(
                -1f,
                1f,
                1f
            );
        }


        // =====================================================
        // Wall Climb
        // =====================================================

        if (isWallClimbing)
        {
            // 벽에 붙어 있는 동안 좌우 이동 정지
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            return;
        }


        // =====================================================
        // 일반 이동
        // =====================================================

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
        if (playerState.Is(PlayerStateType.Dodge))
            return;

        if (!Input.GetKeyDown(KeyCode.Space))
            return;


        // =====================================================
        // 벽타기 중 점프
        // =====================================================

        if (isWallClimbing)
        {
            StopWallClimb();

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            // 공중에서 다시 더블 점프할 수 있도록
            jumpCount = 0;

            return;
        }


        // =====================================================
        // 더블 점프 횟수 초과
        // =====================================================

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
        // =====================================================
        // 이미 벽타기 중
        // =====================================================

        if (isWallClimbing)
        {
            // 현재 붙어 있는 벽이 사라짐
            if (!wallDetector.HasWallOnDirection(currentWallDirection))
            {
                StopWallClimb();
                return;
            }


            // 벽에서 반대쪽으로 입력하면 벽에서 떨어짐
            if (moveInput * currentWallDirection < -0.1f)
            {
                StopWallClimb();
                return;
            }

            return;
        }


        // =====================================================
        // 벽타기 시작
        // =====================================================

        int wallDirection =
            wallDetector.GetWallDirection(moveInput);

        if (wallDirection == 0)
            return;


        // 지상에서는 W/S 입력으로 벽타기 시작
        if (isGrounded)
        {
            if (verticalInput <= 0.1f)
                return;
        }


        // 공중이면 자동으로 벽에 붙음
        StartWallClimb(wallDirection);
    }


    // =========================================================
    // Start Wall Climb
    // =========================================================

    private void StartWallClimb(int wallDirection)
    {
        isWallClimbing = true;

        currentWallDirection =
            wallDetector.GetWallDirection(moveInput);

        rb.gravityScale = 0f;

        rb.linearVelocity = Vector2.zero;

        playerState.SetState(PlayerStateType.WallClimb);
    }


    // =========================================================
    // Stop Wall Climb
    // =========================================================

    private void StopWallClimb()
    {
        isWallClimbing = false;

        currentWallDirection = 0;

        rb.gravityScale = defaultGravityScale;

        if (playerState.Is(PlayerStateType.WallClimb))
        {
            playerState.SetState(PlayerStateType.Normal);
        }
    }


    // =========================================================
    // Wall Climb Movement
    // =========================================================

    private void HandleWallClimb()
    {
        if (!isWallClimbing)
            return;

        if (playerState.Is(PlayerStateType.Dodge))
            return;


        rb.linearVelocity = new Vector2(
            0f,
            verticalInput * wallClimbSpeed
        );
    }


    // =========================================================
    // Force Stop Wall Climb
    // =========================================================

    public void ForceStopWallClimb()
    {
        if (!isWallClimbing)
            return;

        StopWallClimb();
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