
using UnityEngine;

namespace GameFramework.Enemy
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyVision : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private Transform visual;

        [Header("Vision Settings")]
        [SerializeField] private string playerTag = "Player";

        [Range(0f, 360f)]
        [SerializeField] private float viewAngle = 120f;

        private CircleCollider2D detectionRange;

        private void Awake()
        {
            detectionRange = GetComponent<CircleCollider2D>();

            if (brain == null)
                brain = GetComponentInParent<EnemyBrain>();

            if (brain == null)
            {
                Debug.LogError(
                    "EnemyBrain을 찾을 수 없습니다.",
                    this
                );
            }

            if (!detectionRange.isTrigger)
            {
                Debug.LogError(
                    "DetectionRange의 Is Trigger를 활성화해주세요.",
                    this
                );
            }

            if (visual == null)
            {
                Debug.LogError(
                    "Visual이 연결되지 않았습니다.",
                    this
                );
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (brain == null || visual == null)
                return;

            Transform player = ResolvePlayer(other);

            if (player == null)
                return;

            // 시야각 검사
            if (!IsWithinViewAngle(player.position))
                return;

            // 플레이어 발견 정보 전달
            brain.ReportVisualTarget(player);
        }

        // 플레이어 오브젝트 확인
        private Transform ResolvePlayer(Collider2D other)
        {
            Transform root = other.transform.root;

            // 플레이어 루트에 Player 태그가 있는 경우
            if (root.CompareTag(playerTag))
                return root;

            // 콜라이더 자체에 Player 태그가 있는 경우
            if (other.CompareTag(playerTag))
                return other.transform;

            return null;
        }

        // 시야각 검사
        private bool IsWithinViewAngle(Vector2 targetPosition)
        {
            Vector2 direction =
                targetPosition - (Vector2)transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                return true;

            direction.Normalize();

            // Visual의 X 스케일로 바라보는 방향 확인
            Vector2 facingDirection =
                visual.localScale.x >= 0f
                ? Vector2.right
                : Vector2.left;

            float dot = Vector2.Dot(
                facingDirection,
                direction
            );

            float threshold = Mathf.Cos(
                viewAngle * 0.5f * Mathf.Deg2Rad
            );

            return dot >= threshold;
        }

        private void OnDrawGizmosSelected()
        {
            CircleCollider2D circle =
                GetComponent<CircleCollider2D>();

            if (circle == null)
                return;

            float scale = Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.y
            );

            float radius = circle.radius * scale;

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.TransformPoint(circle.offset),
                radius
            );

            if (visual == null)
                return;

            Vector2 facing =
                visual.localScale.x >= 0f
                ? Vector2.right
                : Vector2.left;

            Vector2 left = Quaternion.Euler(
                0f, 0f, viewAngle * 0.5f
            ) * facing;

            Vector2 right = Quaternion.Euler(
                0f, 0f, -viewAngle * 0.5f
            ) * facing;

            Vector3 origin = transform.position;

            Gizmos.DrawLine(
                origin,
                origin + (Vector3)(left * radius)
            );

            Gizmos.DrawLine(
                origin,
                origin + (Vector3)(right * radius)
            );
        }
    }
}
