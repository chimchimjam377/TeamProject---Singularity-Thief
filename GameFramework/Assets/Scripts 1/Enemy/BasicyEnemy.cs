using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("기본 경계(청각) 범위")]
    public float baseAlertRadius = 3f;
    
    [Tooltip("Raycast 시야 거리")]
    public float visionDistance = 5f;
    
    [Tooltip("감지할 레이어 (Player 지정)")]
    public LayerMask playerLayer;

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;

    private Transform playerTransform;
    private Player playerScript;
    private bool isPlayerDetected = false;

    private void Update()
    {
        DetectPlayer();

        if (isPlayerDetected && playerTransform != null)
        {
            // 플레이어 방향으로 천천히 이동
            MoveTowardsPlayer();
        }
    }

    private void DetectPlayer()
    {
        isPlayerDetected = false;

        // 1. Raycast 방식 (시야 감지) - 적이 바라보는 방향(right) 기준
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, visionDistance, playerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            isPlayerDetected = true;
            playerTransform = hit.transform;
            return; // Raycast로 이미 찾았다면 탐색 종료
        }

        // 2. OverlapCircle 방식 (경계/소리 감지)
        // 플레이어를 찾기 위해 싱글톤이나 Tag로 참조
        if (playerScript == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerScript = playerObj.GetComponent<Player>();
            }
        }

        // 플레이어의 소리 파워에 따라 경계 범위를 동적으로 확장
        float currentSoundPower = (playerScript != null) ? playerScript.soundPower : 0f;
        float currentAlertRadius = baseAlertRadius + currentSoundPower;

        Collider2D overlapCollider = Physics2D.OverlapCircle(transform.position, currentAlertRadius, playerLayer);
        if (overlapCollider != null && overlapCollider.CompareTag("Player"))
        {
            isPlayerDetected = true;
            playerTransform = overlapCollider.transform;
        }
    }

    private void MoveTowardsPlayer()
    {
        // 플레이어 쪽으로 천천히 다가가는 로직
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

        // 바라보는 방향 전환 (선택 사항)
        if (direction.x != 0)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Scene 뷰에서 감지 범위를 시각적으로 확인
    private void OnDrawGizmosSelected()
    {
        // Raycast 시야 선
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * visionDistance);

        // OverlapCircle 기본 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseAlertRadius);

        // 플레이어가 할당되어 있다면 소리 적용 범위도 표시
        if (playerScript != null)
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, baseAlertRadius + playerScript.soundPower);
        }
    }
}