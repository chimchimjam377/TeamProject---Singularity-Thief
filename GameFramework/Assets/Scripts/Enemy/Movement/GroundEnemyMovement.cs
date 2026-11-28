
using UnityEngine;

namespace GameFramework.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GroundEnemyMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private EnemyAnimationController animationController;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float stoppingDistance = 0.08f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.12f;

        // 현재 목적지
        private float targetX;
        private bool hasDestination;

        public bool IsGrounded { get; private set; }
        public bool HasDestination => hasDestination;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (visual == null)
                Debug.LogError("Visual이 연결되지 않았습니다.", this);

            if (groundCheck == null)
                Debug.LogError("GroundCheck가 연결되지 않았습니다.", this);
        }

        private void FixedUpdate()
        {
            CheckGround();

            if (!hasDestination)
            {
                SetHorizontalVelocity(0f);
                return;
            }

            // 현재 위치와 목적지 사이의 거리
            float distanceX = targetX - rb.position.x;

            // 목적지에 도착했는지 확인
            float arrivalThreshold = Mathf.Max(
                stoppingDistance,
                moveSpeed * Time.fixedDeltaTime
            );

            if (Mathf.Abs(distanceX) <= arrivalThreshold)
            {
                hasDestination = false;
                SetHorizontalVelocity(0f);
                return;
            }

            // 이동 방향 결정
            float direction = Mathf.Sign(distanceX);

            // 이동
            SetHorizontalVelocity(direction * moveSpeed);

            // 바라보는 방향 변경
            UpdateFacing(direction);
        }

        private void Update()
        {
            UpdateAnimation();
        }

        // 외부에서 이동 목적지를 전달
        public void MoveTo(Vector2 destination)
        {
            targetX = destination.x;
            hasDestination = true;
        }

        // 이동 중단
        public void Stop()
        {
            hasDestination = false;
        }

        // 수평 속도 변경
        private void SetHorizontalVelocity(float velocityX)
        {
            rb.linearVelocity = new Vector2(
                velocityX,
                rb.linearVelocity.y
            );
        }

        // 지면 확인
        private void CheckGround()
        {
            if (groundCheck == null)
                return;

            IsGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        // 바라보는 방향 변경
        private void UpdateFacing(float direction)
        {
            if (visual == null)
                return;

            Vector3 scale = visual.localScale;

            scale.x = Mathf.Abs(scale.x) * direction;

            visual.localScale = scale;
        }

        // 애니메이션 제어
        private void UpdateAnimation()
        {
            if (animationController == null || !IsGrounded)
                return;

            if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
            {
                animationController.PlayWalk();
            }
            else
            {
                animationController.PlayIdle();
            }
        }

        // Scene 뷰에서 감지 범위 표시
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
}
