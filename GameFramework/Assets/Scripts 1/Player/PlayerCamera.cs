using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private PlayerInput playerInput;

    [Header("Camera")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 0f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionOffset = 0.1f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Collision Smooth")]
    [SerializeField] private float cameraMoveSpeed = 15f;

    private float yaw;
    private float pitch;

    private Vector3 currentCameraPosition;

    private void Start()
    {
        Vector3 rotation = transform.eulerAngles;

        yaw = rotation.y;
        pitch = rotation.x;

        currentCameraPosition = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandlePosition();
    }

    private void HandleRotation()
    {
        Vector2 lookInput = playerInput.LookInput;

        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );
    }

    private void HandlePosition()
    {
        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 targetPosition =
            target.position +
            Vector3.up * height;

        Vector3 desiredOffset =
            rotation * Vector3.back * distance;

        Vector3 desiredPosition =
            targetPosition + desiredOffset;

        Vector3 finalPosition =
            CalculateCollisionPosition(
                targetPosition,
                desiredPosition
            );

        currentCameraPosition = Vector3.Lerp(
            currentCameraPosition,
            finalPosition,
            cameraMoveSpeed * Time.deltaTime
        );

        transform.position = currentCameraPosition;

        transform.rotation = rotation;
    }

    private Vector3 CalculateCollisionPosition(
        Vector3 targetPosition,
        Vector3 desiredPosition)
    {
        Vector3 direction =
            desiredPosition - targetPosition;

        float desiredDistance =
            direction.magnitude;

        direction.Normalize();

        if (Physics.SphereCast(
            targetPosition,
            collisionRadius,
            direction,
            out RaycastHit hit,
            desiredDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            float safeDistance =
                hit.distance - collisionOffset;

            safeDistance = Mathf.Max(
                safeDistance,
                0.1f
            );

            return targetPosition +
                   direction * safeDistance;
        }

        return desiredPosition;
    }
}