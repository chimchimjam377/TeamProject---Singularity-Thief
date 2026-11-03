using UnityEngine;

public class AirBomberEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bombPoint;
    [SerializeField] private GameObject bombPrefab;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;

    [Header("Bombing Movement")]
    [SerializeField] private float moveSpeed = 4f;

    // 플레이어보다 얼마나 높게 있을지
    [SerializeField] private float bombingHeight = 6f;

    // X축이 이 정도 가까워야 폭탄 투하
    [SerializeField] private float horizontalAttackRange = 0.8f;

    // 플레이어와 최소한 이 높이 이상 떨어져 있어야 공격
    [SerializeField] private float minimumDropHeight = 4f;

    [Header("Bomb Attack")]
    [SerializeField] private float firstBombDelay = 0.5f;
    [SerializeField] private float bombCooldown = 2f;

    [Header("Idle")]
    [SerializeField] private float idleMoveAmount = 1f;
    [SerializeField] private float idleMoveSpeed = 1f;

    private Transform player;

    private Vector3 startPosition;

    private float bombTimer;

    private bool detectedPlayer;

    private void Start()
    {
        startPosition = transform.position;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        bombTimer = firstBombDelay;
    }

    private void Update()
    {
        if (player == null)
        {
            IdleMovement();
            return;
        }

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distanceToPlayer <= detectionRange)
        {
            if (!detectedPlayer)
            {
                detectedPlayer = true;
                bombTimer = firstBombDelay;
            }

            BombingMovement();
        }
        else
        {
            detectedPlayer = false;

            bombTimer = firstBombDelay;

            ReturnToStart();
        }
    }

    private void BombingMovement()
    {
        // 항상 플레이어보다 높은 위치를 목표로 함
        Vector3 bombingPosition =
            new Vector3(
                player.position.x,
                player.position.y + bombingHeight,
                transform.position.z
            );

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                bombingPosition,
                moveSpeed * Time.deltaTime
            );

        float horizontalDistance =
            Mathf.Abs(
                transform.position.x -
                player.position.x
            );

        float verticalDistance =
            transform.position.y -
            player.position.y;

        // 완전히 머리 위 근처 + 충분히 높은 위치일 때만 공격
        if (
            horizontalDistance <= horizontalAttackRange &&
            verticalDistance >= minimumDropHeight
        )
        {
            BombAttack();
        }
        else
        {
            // 아직 머리 위에 도착하지 않았다면
            // 공격 타이머가 미리 줄어들지 않게 함
            bombTimer = Mathf.Max(
                bombTimer,
                firstBombDelay
            );
        }
    }

    private void BombAttack()
    {
        bombTimer -= Time.deltaTime;

        if (bombTimer > 0f)
            return;

        DropBomb();

        bombTimer = bombCooldown;
    }

    private void DropBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogError(
                "Bomb Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if (bombPoint == null)
        {
            Debug.LogError(
                "Bomb Point가 연결되지 않았습니다."
            );

            return;
        }

        GameObject bomb =
            Instantiate(
                bombPrefab,
                bombPoint.position,
                Quaternion.identity
            );

        EnemyBomb bombScript =
            bomb.GetComponent<EnemyBomb>();

        if (bombScript != null)
        {
            bombScript.Initialize(gameObject);
        }
    }

    private void ReturnToStart()
    {
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                startPosition,
                moveSpeed * Time.deltaTime
            );

        if (
            Vector2.Distance(
                transform.position,
                startPosition
            ) <= 0.05f
        )
        {
            IdleMovement();
        }
    }

    private void IdleMovement()
    {
        Vector3 targetPosition =
            startPosition;

        // 플레이어가 없을 때는 좌우로 조금씩 부유
        targetPosition.x +=
            Mathf.Sin(
                Time.time * idleMoveSpeed
            ) * idleMoveAmount;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * 0.5f * Time.deltaTime
            );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }
}