using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 3f;
    [SerializeField] private float dodgeDuration = 0.2f;

    [Header("Invincibility")]
    [SerializeField] private float invincibleStartTime = 0f;
    [SerializeField] private float invincibleEndTime = 0.15f;

    private float dodgeTimer;

    private Vector2 dodgeDirection;

    private float defaultGravityScale;

    public bool IsDodging => playerState.Is(PlayerStateType.Dodge);

    public bool IsInvincible { get; private set; }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        defaultGravityScale = rb.gravityScale;

        invincibleStartTime = Mathf.Max(0f, invincibleStartTime);
        invincibleEndTime = Mathf.Min(dodgeDuration, invincibleEndTime);
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (!IsDodging)
            return;

        UpdateDodge();
    }

    // =========================================================
    // Input
    // =========================================================

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(1))
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
        dodgeDirection = CalculateDodgeDirection();

        if (dodgeDirection == Vector2.zero)
            return;

        playerState.SetState(PlayerStateType.Dodge);

        dodgeTimer = 0f;

        rb.gravityScale = 0f;

        float dodgeSpeed = dodgeDistance / dodgeDuration;

        rb.linearVelocity = dodgeDirection * dodgeSpeed;

        IsInvincible = false;
    }

    // =========================================================
    // Dodge Direction
    // =========================================================

    private Vector2 CalculateDodgeDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera가 존재하지 않습니다.");
            return Vector2.zero;
        }

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction =
            mouseWorldPosition - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        direction.Normalize();

        // 지상에서는 아래 방향으로 닷지할 수 없음
        if (playerMovement.IsGrounded)
        {
            if (direction.y < 0f)
            {
                direction.y = 0f;
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
        dodgeTimer += Time.fixedDeltaTime;

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
            dodgeTimer >= invincibleStartTime &&
            dodgeTimer <= invincibleEndTime;
    }

    // =========================================================
    // End Dodge
    // =========================================================

    private void EndDodge()
    {
        IsInvincible = false;

        rb.linearVelocity = Vector2.zero;

        rb.gravityScale = defaultGravityScale;

        playerState.SetState(PlayerStateType.Normal);

        dodgeTimer = 0f;
    }
}