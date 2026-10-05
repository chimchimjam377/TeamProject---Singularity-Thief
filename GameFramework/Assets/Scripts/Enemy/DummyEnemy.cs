using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    // =========================================================
    // Health
    // =========================================================

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;


    // =========================================================
    // Movement
    // =========================================================

    [Header("Movement")]
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float patrolSpeed = 2f;

    [SerializeField] private float chaseSpeed = 4f;


    // =========================================================
    // Patrol
    // =========================================================

    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 5f;

    private Vector2 patrolStartPosition;

    // 1 = 오른쪽
    // -1 = 왼쪽
    private int patrolDirection = 1;


    // =========================================================
    // Player Detection
    // =========================================================

    [Header("Player Detection")]
    [SerializeField] private Transform player;

    [SerializeField] private float detectionRange = 8f;

    [SerializeField] private float loseTargetRange = 12f;


    // =========================================================
    // Line Of Sight
    // =========================================================

    [Header("Line Of Sight")]
    [SerializeField] private bool requireLineOfSight = true;

    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private Transform eyePoint;


    // =========================================================
    // Visual
    // =========================================================

    [Header("Visual")]
    [SerializeField] private Transform visual;


    // =========================================================
    // State
    // =========================================================

    private bool isChasing;

    private bool isDead;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        patrolStartPosition = transform.position;
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (isDead)
            return;

        CheckPlayerDetection();
    }


    // =========================================================
    // FixedUpdate
    // =========================================================

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }


    // =========================================================
    // Player Detection
    // =========================================================

    private void CheckPlayerDetection()
    {
        if (player == null)
            return;


        float distance = Vector2.Distance(
            transform.position,
            player.position
        );


        // -----------------------------------------------------
        // 현재 추적 중
        // -----------------------------------------------------

        if (isChasing)
        {
            // 플레이어가 너무 멀어지면 추적 중단
            if (distance > loseTargetRange)
            {
                StopChasing();
            }

            return;
        }


        // -----------------------------------------------------
        // 플레이어가 감지 범위 밖
        // -----------------------------------------------------

        if (distance > detectionRange)
            return;


        // -----------------------------------------------------
        // 시야 확인을 사용하지 않는 경우
        // -----------------------------------------------------

        if (!requireLineOfSight)
        {
            StartChasing();
            return;
        }


        // -----------------------------------------------------
        // 플레이어가 실제로 보이는지 확인
        // -----------------------------------------------------

        if (CanSeePlayer())
        {
            StartChasing();
        }
    }


    // =========================================================
    // Line Of Sight
    // =========================================================

    private bool CanSeePlayer()
    {
        Vector2 startPosition;

        if (eyePoint != null)
        {
            startPosition = eyePoint.position;
        }
        else
        {
            startPosition = transform.position;
        }


        Vector2 direction =
            (Vector2)player.position - startPosition;


        float distance = direction.magnitude;


        RaycastHit2D hit = Physics2D.Raycast(
            startPosition,
            direction.normalized,
            distance,
            obstacleLayer
        );


        // 아무것도 맞지 않았다면
        // 플레이어까지 시야가 확보된 것
        return hit.collider == null;
    }


    // =========================================================
    // Start Chase
    // =========================================================

    private void StartChasing()
    {
        if (isChasing)
            return;


        isChasing = true;

        Debug.Log(
            "DummyEnemy : 플레이어 발견!"
        );
    }


    // =========================================================
    // Stop Chase
    // =========================================================

    private void StopChasing()
    {
        if (!isChasing)
            return;


        isChasing = false;


        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );


        Debug.Log(
            "DummyEnemy : 플레이어를 놓침."
        );
    }


    // =========================================================
    // Patrol
    // =========================================================

    private void Patrol()
    {
        float leftLimit =
            patrolStartPosition.x -
            patrolDistance;


        float rightLimit =
            patrolStartPosition.x +
            patrolDistance;


        // -----------------------------------------------------
        // 오른쪽 끝 도착
        // -----------------------------------------------------

        if (transform.position.x >= rightLimit)
        {
            patrolDirection = -1;
        }


        // -----------------------------------------------------
        // 왼쪽 끝 도착
        // -----------------------------------------------------

        if (transform.position.x <= leftLimit)
        {
            patrolDirection = 1;
        }


        // -----------------------------------------------------
        // 이동
        // -----------------------------------------------------

        rb.linearVelocity = new Vector2(
            patrolDirection * patrolSpeed,
            rb.linearVelocity.y
        );


        // -----------------------------------------------------
        // 스프라이트 방향 전환
        // -----------------------------------------------------

        FlipVisual();
    }


    // =========================================================
    // Chase
    // =========================================================

    private void ChasePlayer()
    {
        if (player == null)
        {
            StopChasing();
            return;
        }


        float direction =
            player.position.x -
            transform.position.x;


        // 플레이어와 거의 같은 위치
        if (Mathf.Abs(direction) < 0.05f)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            return;
        }


        // 플레이어 방향
        float moveDirection =
            Mathf.Sign(direction);


        // 이동
        rb.linearVelocity = new Vector2(
            moveDirection * chaseSpeed,
            rb.linearVelocity.y
        );


        // 스프라이트 방향 전환
        if (moveDirection > 0)
        {
            SetVisualDirection(1);
        }
        else
        {
            SetVisualDirection(-1);
        }
    }


    // =========================================================
    // Patrol Visual
    // =========================================================

    private void FlipVisual()
    {
        if (patrolDirection > 0)
        {
            SetVisualDirection(1);
        }
        else
        {
            SetVisualDirection(-1);
        }
    }


    // =========================================================
    // Set Visual Direction
    // =========================================================

    private void SetVisualDirection(int direction)
    {
        if (visual == null)
            return;


        Vector3 scale =
            visual.localScale;


        scale.x =
            Mathf.Abs(scale.x) *
            direction;


        visual.localScale =
            scale;
    }


    // =========================================================
    // Damage
    // =========================================================

    public void TakeDamage(
        int damage,
        GameObject attacker
    )
    {
        if (isDead)
            return;


        currentHealth -= damage;


        currentHealth =
            Mathf.Max(
                currentHealth,
                0
            );


        Debug.Log(
            $"Dummy Damage: {damage} / HP: {currentHealth}/{maxHealth}"
        );


        if (currentHealth <= 0)
        {
            Die();
        }
    }


    // =========================================================
    // Die
    // =========================================================

    private void Die()
    {
        isDead = true;


        rb.linearVelocity =
            Vector2.zero;


        Debug.Log(
            "Dummy Dead"
        );


        // 테스트용
        gameObject.SetActive(false);
    }


    // =========================================================
    // Gizmos
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // -----------------------------------------------------
        // 순찰 범위
        // -----------------------------------------------------

        Gizmos.color =
            Color.yellow;


        Vector3 startPosition;


        if (Application.isPlaying)
        {
            startPosition =
                patrolStartPosition;
        }
        else
        {
            startPosition =
                transform.position;
        }


        Vector3 left =
            startPosition +
            Vector3.left * patrolDistance;


        Vector3 right =
            startPosition +
            Vector3.right * patrolDistance;


        Gizmos.DrawLine(
            left,
            right
        );


        Gizmos.DrawWireSphere(
            left,
            0.2f
        );


        Gizmos.DrawWireSphere(
            right,
            0.2f
        );


        // -----------------------------------------------------
        // 감지 범위
        // -----------------------------------------------------

        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );


        // -----------------------------------------------------
        // 추적 해제 범위
        // -----------------------------------------------------

        Gizmos.color =
            Color.magenta;


        Gizmos.DrawWireSphere(
            transform.position,
            loseTargetRange
        );


        // -----------------------------------------------------
        // 시야
        // -----------------------------------------------------

        if (eyePoint != null &&
            player != null)
        {
            Gizmos.color =
                Color.cyan;


            Gizmos.DrawLine(
                eyePoint.position,
                player.position
            );
        }
    }
}