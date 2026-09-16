using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraTransform;


[Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;

    [Header("Ground")]
    [SerializeField] private float groundedGravity = -2f;

    private CharacterController controller;

    private float verticalVelocity;

    public bool IsGrounded => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleJump();
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector2 input = playerInput.MoveInput;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * input.y +
            right * input.x;

        moveDirection = Vector3.ClampMagnitude(
            moveDirection,
            1f
        );

        Vector3 velocity =
            moveDirection * moveSpeed;

        velocity.y = verticalVelocity;

        controller.Move(
            velocity * Time.deltaTime
        );
    }

    private void HandleRotation()
    {
        Vector3 cameraForward = cameraTransform.forward;

        // 위/아래 기울기는 회전에 사용하지 않는다.
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.01f)
            return;

        cameraForward.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(cameraForward);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleJump()
    {
        if (!playerInput.JumpPressed)
            return;

        if (!controller.isGrounded)
            return;

        // v = sqrt(h * -2g)
        verticalVelocity =
            Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

        playerInput.ConsumeJump();
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            // 바닥에 붙어 있도록 약간의 음수 속도 적용
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedGravity;
            }

            return;
        }

        // 기본 중력
        verticalVelocity += gravity * Time.deltaTime;

        // 내려가는 중이라면 추가 중력
        if (verticalVelocity < 0f)
        {
            verticalVelocity +=
                gravity *
                (fallGravityMultiplier - 1f) *
                Time.deltaTime;
        }
    }


}
