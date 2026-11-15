using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WallDetector wallDetector;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeDuration = 0.2f;

    [Header("Invincibility")]
    [SerializeField] private float invincibleStartTime = 0f;
    [SerializeField] private float invincibleEndTime = 0.15f;

    // 장비/상점 효과 등으로 닷지 자체를 막을 때 사용
    private bool dodgeEnabled = true;

    // 현재 닷지 충전이 남아있는지
    private bool canDodge = true;

    // 땅/벽에 새로 접촉했는지 확인용
    private bool wasGrounded;
    private bool wasTouchingWall;

    private float dodgeTimer;
    private Vector2 dodgeDirection;

    private float defaultGravityScale;

    public bool IsDodging =>
        playerState.Is(PlayerStateType.Dodge);

    public bool IsInvincible { get; private set; }

    public bool DodgeEnabled => dodgeEnabled;

    public bool CanDodgeCharge => canDodge;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (wallDetector == null)
            wallDetector = GetComponent<WallDetector>();

        if (rb != null)
        {
            defaultGravityScale = rb.gravityScale;
        }

        invincibleStartTime =
            Mathf.Max(0f, invincibleStartTime);

        invincibleEndTime =
            Mathf.Min(
                dodgeDuration,
                invincibleEndTime
            );
    }

    private void Start()
    {
        if (playerMovement != null)
        {
            wasGrounded =
                playerMovement.IsGrounded;
        }

        if (wallDetector != null)
        {
            wasTouchingWall =
                wallDetector.HasWall;
        }
    }

    private void Update()
    {
        CheckDodgeReset();

        HandleInput();
    }

    private void FixedUpdate()
    {
        if (!IsDodging)
            return;

        UpdateDodge();
    }

    // =========================================================
    // 닷지 사용 가능 / 불가능
    // =========================================================

    public void SetDodgeEnabled(bool enabled)
    {
        dodgeEnabled = enabled;

        // 닷지를 막는 순간 닷지 중이면 강제로 종료
        if (!dodgeEnabled && IsDodging)
        {
            EndDodge();
        }
    }

    // =========================================================
    // 닷지 충전 초기화
    // =========================================================

    private void CheckDodgeReset()
    {
        bool isGrounded =
            playerMovement != null &&
            playerMovement.IsGrounded;

        bool isTouchingWall =
            wallDetector != null &&
            wallDetector.HasWall;

        // 공중에 있다가 새로 땅에 착지했을 때
        if (isGrounded && !wasGrounded)
        {
            ResetDodge();
        }

        // 벽에 새로 접촉했을 때
        if (isTouchingWall && !wasTouchingWall)
        {
            ResetDodge();
        }

        wasGrounded = isGrounded;
        wasTouchingWall = isTouchingWall;
    }

    // 적 공격 적중 등 외부에서도 호출 가능
    public void ResetDodge()
    {
        canDodge = true;

        Debug.Log("닷지 충전 초기화");
    }

    // =========================================================
    // Input
    // =========================================================

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        // 장비 등의 효과로 닷지 자체가 비활성화된 상태
        if (!dodgeEnabled)
            return;

        // 닷지 충전이 없는 상태
        if (!canDodge)
            return;

        if (playerState == null)
            return;

        if (!playerState.CanDodge())
            return;

        StartDodge();
    }

    // =========================================================
    // Start Dodge
    // =========================================================

    private void StartDodge()
    {
        dodgeDirection =
            CalculateDodgeDirection();

        if (dodgeDirection == Vector2.zero)
            return;

        // 닷지 충전 1회 소비
        canDodge = false;

        playerState.SetState(
            PlayerStateType.Dodge
        );

        dodgeTimer = 0f;

        if (rb != null)
        {
            rb.gravityScale = 0f;

            float dodgeSpeed =
                dodgeDistance /
                Mathf.Max(dodgeDuration, 0.01f);

            rb.linearVelocity =
                dodgeDirection *
                dodgeSpeed;
        }

        IsInvincible = false;
    }

    // =========================================================
    // Dodge Direction
    // =========================================================

    private Vector2 CalculateDodgeDirection()
    {
        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning(
                "Main Camera가 존재하지 않습니다."
            );

            return Vector2.zero;
        }

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2 direction =
            mouseWorldPosition -
            transform.position;

        if (direction.sqrMagnitude <
            0.0001f)
        {
            return Vector2.zero;
        }

        direction.Normalize();

        // 지상에서는 아래 방향으로 닷지 불가
        if (playerMovement != null &&
            playerMovement.IsGrounded)
        {
            if (direction.y < 0f)
            {
                direction.y = 0f;

                if (direction.sqrMagnitude <
                    0.0001f)
                {
                    return Vector2.zero;
                }

                direction.Normalize();
            }
        }

        return direction;
    }

    // =========================================================
    // Dodge Update
    // =========================================================

    private void UpdateDodge()
    {
        dodgeTimer +=
            Time.fixedDeltaTime;

        UpdateInvincibility();

        if (dodgeTimer >= dodgeDuration)
        {
            EndDodge();
        }
    }

    // =========================================================
    // Invincibility
    // =========================================================

    private void UpdateInvincibility()
    {
        IsInvincible =
            dodgeTimer >=
            invincibleStartTime &&
            dodgeTimer <=
            invincibleEndTime;
    }

    // =========================================================
    // End Dodge
    // =========================================================

    private void EndDodge()
    {
        IsInvincible = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.gravityScale =
                defaultGravityScale;
        }

        if (playerState != null)
        {
            playerState.SetState(
                PlayerStateType.Normal
            );
        }

        dodgeTimer = 0f;
    }
}