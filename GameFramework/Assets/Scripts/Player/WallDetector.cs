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

    public bool HasWallOnDirection(float direction)
    {
        if (direction > 0f)
            return IsRightWall;

        if (direction < 0f)
            return IsLeftWall;

        return false;
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