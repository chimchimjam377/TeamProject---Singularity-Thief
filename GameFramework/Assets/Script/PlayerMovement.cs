using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("중력")]
    public float gravity = -20f;

    [Header("이동 가능 여부")]
    public bool canMove = true;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 이동이 막혀 있으면 이동하지 않음
        if (!canMove)
        {
            ApplyGravity();
            return;
        }

        Move();
        ApplyGravity();
    }

    // WASD 이동
    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        move.y = 0f;
        move.Normalize();

        controller.Move(
            move * moveSpeed * Time.deltaTime
        );
    }

    // 중력
    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(
            velocity * Time.deltaTime
        );
    }
}