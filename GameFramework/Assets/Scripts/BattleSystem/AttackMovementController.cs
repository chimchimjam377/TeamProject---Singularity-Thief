
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AttackMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Collider2D bodyCollider;

    private AttackDefinition currentAttack;

    private int attackDirection = 1;
    private float normalizedTime;

    private Vector2 movementStartPosition;
    private Vector2 movementEndPosition;

    private bool segmentStarted;
    private bool movementCompleted;
    private bool isDriving;
    private bool finishRequested;
    private bool finishStepApplied;
    private bool controlsVertical;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (playerMovement == null)
        {
            Debug.LogError(
                "AttackMovementController: PlayerMovement가 없습니다.",
                this
            );
        }

        if (bodyCollider == null)
        {
            Debug.LogWarning(
                "플레이어의 Collider2D를 찾지 못했습니다. " +
                "적 접근 거리 계산에 콜라이더 크기가 반영되지 않습니다.",
                this
            );
        }
    }

    // =====================================================
    // 공격 시작
    // =====================================================

    public void BeginAttack(
        AttackDefinition attack,
        int direction)
    {
        CancelAttackMovement();

        currentAttack = attack;
        attackDirection = direction >= 0 ? 1 : -1;

        normalizedTime = 0f;

        segmentStarted = false;
        movementCompleted = false;
        finishRequested = false;
        finishStepApplied = false;

        movementStartPosition = rb.position;
        movementEndPosition = rb.position;
        controlsVertical = false;
    }

    // =====================================================
    // 애니메이션 진행도 전달
    // =====================================================

    public void UpdateAttackProgress(float progress)
    {
        if (currentAttack == null || movementCompleted)
            return;

        normalizedTime = Mathf.Clamp01(progress);

        if (currentAttack.MovementType == AttackMovementType.None)
        {
            movementCompleted = true;
            return;
        }

        // 이동 시작과 종료 시점이 같으면 이동하지 않는다.
        if (currentAttack.MovementEnd <=
            currentAttack.MovementStart + 0.0001f)
        {
            movementCompleted = true;
            return;
        }

        // 이동 구간에 처음 진입한 순간 목적지를 계산한다.
        if (!segmentStarted &&
            normalizedTime >= currentAttack.MovementStart)
        {
            StartMovementSegment();
        }

        if (!isDriving)
            return;

        if (normalizedTime >= currentAttack.MovementEnd)
        {
            finishRequested = true;
        }
    }

    // =====================================================
    // 이동 구간 시작
    // =====================================================

    private void StartMovementSegment()
    {
        segmentStarted = true;
        movementStartPosition = rb.position;

        controlsVertical =
            currentAttack.MovementType ==
                AttackMovementType.NearestTarget &&
            currentAttack.TargetMovementMode ==
                AttackTargetMovementMode.TowardsTarget;

        if (currentAttack.MovementType ==
            AttackMovementType.FixedDistance)
        {
            movementEndPosition =
                movementStartPosition +
                Vector2.right *
                attackDirection *
                currentAttack.MoveDistance;
        }
        else if (currentAttack.MovementType ==
                 AttackMovementType.NearestTarget)
        {
            Collider2D target = FindNearestTarget();

            if (target == null)
            {
                movementCompleted = true;
                return;
            }

            if (!TryCalculateTargetDestination(
                    target,
                    out movementEndPosition))
            {
                // 이미 충분히 가까운 적은 향해 이동하지 않는다.
                movementCompleted = true;
                return;
            }
        }
        else
        {
            movementCompleted = true;
            return;
        }

        if (playerMovement == null)
        {
            movementCompleted = true;
            return;
        }

        isDriving = true;

        // PlayerMovement가 일반 이동으로 속도를 덮어쓰지 않게 한다.
        playerMovement.SetAttackMovementActive(true);
    }

    // =====================================================
    // 가장 가까운 적 탐색
    // =====================================================

    private Collider2D FindNearestTarget()
    {
        float radius = currentAttack.TargetSearchRadius;

        if (radius <= 0f)
            return null;

        Collider2D[] targets = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            currentAttack.TargetLayers
        );

        Collider2D nearestTarget = null;

        float nearestDistance = float.MaxValue;

        foreach (Collider2D candidate in targets)
        {
            if (candidate == null)
                continue;

            // 플레이어 자신의 콜라이더는 제외한다.
            if (candidate.transform.root == transform.root)
                continue;

            Vector2 closestPoint =
                candidate.ClosestPoint(rb.position);

            float distance =
                (closestPoint - rb.position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = candidate;
            }
        }

        return nearestTarget;
    }

    // =====================================================
    // 목표 위치 계산
    // =====================================================

    private bool TryCalculateTargetDestination(
        Collider2D target,
        out Vector2 destination)
    {
        destination = movementStartPosition;

        Bounds targetBounds = target.bounds;

        Vector2 start = movementStartPosition;

        float ownHalfWidth = bodyCollider != null
            ? bodyCollider.bounds.extents.x
            : 0f;

        if (currentAttack.TargetMovementMode ==
            AttackTargetMovementMode.HorizontalOnly)
        {
            float differenceX =
                targetBounds.center.x - start.x;

            if (Mathf.Abs(differenceX) < 0.001f)
                return false;

            float direction = Mathf.Sign(differenceX);

            float gap = Mathf.Abs(differenceX)
                        - targetBounds.extents.x
                        - ownHalfWidth;

            float stopDistance =
                currentAttack.TargetStopDistance;

            // 이미 원하는 거리 안에 있다면 접근하지 않는다.
            if (gap <= stopDistance)
                return false;

            // 적의 콜라이더 경계에서 우리 콜라이더 크기와
            // 설정한 정지 거리를 추가로 확보한다.
            float destinationX =
                targetBounds.center.x
                - direction * (
                    targetBounds.extents.x
                    + ownHalfWidth
                    + stopDistance
                );

            destination = new Vector2(
                destinationX,
                start.y
            );

            return true;
        }

        // 적을 향하는 2D 이동
        Vector2 targetPoint = target.ClosestPoint(start);

        Vector2 toTarget = targetPoint - start;

        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= 0.001f)
            return false;

        Vector2 directionToTarget =
            toTarget / distanceToTarget;

        float playerRadius = 0f;

        if (bodyCollider != null)
        {
            Bounds ownBounds = bodyCollider.bounds;

            // 이동 방향을 기준으로 플레이어 콜라이더의
            // 반지름에 해당하는 거리를 계산한다.
            playerRadius =
                Mathf.Abs(directionToTarget.x) *
                ownBounds.extents.x
                +
                Mathf.Abs(directionToTarget.y) *
                ownBounds.extents.y;
        }

        float desiredGap =
            currentAttack.TargetStopDistance + playerRadius;

        if (distanceToTarget <= desiredGap)
            return false;

        destination =
            targetPoint - directionToTarget * desiredGap;

        return true;
    }

    // =====================================================
    // 실제 물리 이동
    // =====================================================

    private void FixedUpdate()
    {
        if (!isDriving)
            return;

        // 이동 종료 지점까지 마지막 물리 이동을 수행한 뒤
        // 다음 물리 프레임에서 이동 제어를 해제한다.
        if (finishStepApplied)
        {
            StopDrivingMovement();
            movementCompleted = true;
            return;
        }

        float progress = Mathf.InverseLerp(
            currentAttack.MovementStart,
            currentAttack.MovementEnd,
            normalizedTime
        );

        float curvedProgress = EvaluateMovementCurve(progress);

        Vector2 desiredPosition = Vector2.LerpUnclamped(
            movementStartPosition,
            movementEndPosition,
            curvedProgress
        );

        Vector2 difference = desiredPosition - rb.position;

        float fixedDeltaTime = Time.fixedDeltaTime;

        if (controlsVertical)
        {
            // 적의 위치를 향하는 2D 이동
            rb.linearVelocity = difference / fixedDeltaTime;
        }
        else
        {
            // 수평 돌진은 수직 속도와 중력을 유지한다.
            float targetVelocityX =
                difference.x / fixedDeltaTime;

            rb.linearVelocity = new Vector2(
                targetVelocityX,
                rb.linearVelocity.y
            );
        }

        if (finishRequested)
        {
            finishStepApplied = true;
        }
    }

    private float EvaluateMovementCurve(float progress)
    {
        AnimationCurve curve = currentAttack.MovementCurve;

        if (curve == null || curve.length == 0)
            return progress;

        float startValue = curve.Evaluate(0f);
        float endValue = curve.Evaluate(1f);
        float currentValue = curve.Evaluate(progress);

        float range = endValue - startValue;

        if (Mathf.Abs(range) < 0.0001f)
            return progress;

        return Mathf.Clamp01(
            (currentValue - startValue) / range
        );
    }

    // =====================================================
    // 이동 종료 및 취소
    // =====================================================

    private void StopDrivingMovement()
    {
        if (!isDriving)
            return;

        isDriving = false;

        if (playerMovement != null)
            playerMovement.SetAttackMovementActive(false);

        if (controlsVertical)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }
    }

    public void CancelAttackMovement()
    {
        // 실제로 이동 제어 중일 때만 이동 속도를 정리한다.
        if (isDriving)
            StopDrivingMovement();

        currentAttack = null;

        segmentStarted = false;
        movementCompleted = true;
        finishRequested = false;
        finishStepApplied = false;
        controlsVertical = false;
    }

    private void OnDisable()
    {
        CancelAttackMovement();
    }
}
