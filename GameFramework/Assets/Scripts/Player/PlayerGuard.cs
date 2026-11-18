using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [Header("Guard")]
    [SerializeField] private KeyCode guardKey = KeyCode.LeftShift;

    [Tooltip("몇 도 범위까지 정면 공격으로 인정할지")]
    [Range(0f, 180f)]
    [SerializeField] private float guardAngle = 90f;

    private Vector2 guardDirection = Vector2.right;

    public bool IsGuarding { get; private set; }

    private void Update()
    {
        HandleGuardInput();

        if (IsGuarding)
        {
            UpdateGuardDirection();
        }
    }

    private void HandleGuardInput()
    {
        // Shift 누르고 있는 동안 가드
        IsGuarding = Input.GetKey(guardKey);
    }

    private void UpdateGuardDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        Vector2 direction =
            mouseWorldPosition -
            transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        guardDirection = direction.normalized;
    }

    // 공격자의 위치를 받아서 방어 가능한 방향인지 확인
    public bool CanBlockAttack(Vector2 attackerPosition)
    {
        if (!IsGuarding)
            return false;

        Vector2 attackDirection =
            (
                attackerPosition -
                (Vector2)transform.position
            ).normalized;

        float angle =
            Vector2.Angle(
                guardDirection,
                attackDirection
            );

        return angle <= guardAngle * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            transform.position,
            (Vector2)transform.position +
            guardDirection * 2f
        );
    }
}