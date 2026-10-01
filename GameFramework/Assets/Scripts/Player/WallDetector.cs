using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Check")]
    [SerializeField] private Transform leftCheck;
    [SerializeField] private Transform rightCheck;

    [SerializeField] private float checkRadius = 0.08f;

    [Header("Layer")]
    [SerializeField] private LayerMask climbableWallLayer;

    public bool IsLeftWall { get; private set; }
    public bool IsRightWall { get; private set; }

    public bool HasWall => IsLeftWall || IsRightWall;

    private void Update()
    {
        CheckWalls();
    }

    private void CheckWalls()
    {
        IsLeftWall = CheckWall(leftCheck);
        IsRightWall = CheckWall(rightCheck);
    }

    private bool CheckWall(Transform checkPoint)
    {
        if (checkPoint == null)
            return false;

        return Physics2D.OverlapCircle(
            checkPoint.position,
            checkRadius,
            climbableWallLayer
        ) != null;
    }

    // 방향에 해당하는 벽이 있는지 확인
    public bool HasWallOnDirection(int direction)
    {
        if (direction > 0)
            return IsRightWall;

        if (direction < 0)
            return IsLeftWall;

        return false;
    }

    // 현재 상황에서 사용할 벽 방향 반환
    // -1 = 왼쪽 벽
    // +1 = 오른쪽 벽
    public int GetWallDirection(float preferredDirection)
    {
        // 입력 방향에 벽이 있으면 그쪽 우선
        if (preferredDirection > 0f && IsRightWall)
            return 1;

        if (preferredDirection < 0f && IsLeftWall)
            return -1;

        // 한쪽에만 벽이 있다면 그쪽 사용
        if (IsLeftWall && !IsRightWall)
            return -1;

        if (IsRightWall && !IsLeftWall)
            return 1;

        // 양쪽 모두 벽이면 지정할 수 없음
        return 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (leftCheck != null)
        {
            Gizmos.DrawWireSphere(
                leftCheck.position,
                checkRadius
            );
        }

        if (rightCheck != null)
        {
            Gizmos.DrawWireSphere(
                rightCheck.position,
                checkRadius
            );
        }
    }
}