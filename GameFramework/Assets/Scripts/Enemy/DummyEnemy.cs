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
    private int patrolDirection = 1;

    // =========================================================
    // Player Detection
    // =========================================================
    [Header("Player Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 8f;
    // 추적 해제 범위(loseTargetRange)는 무한 추적을 위해 사용하지 않게 됨

    // =========================================================
    // Line Of Sight
    // =========================================================
    [Header("Line Of Sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform eyePoint;

    // =========================================================
    // Attack (새로 추가됨)
    // =========================================================
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;       // 공격 사거리
    [SerializeField] private int attackDamage = 10;          // 공격 데미지
    [SerializeField] private float attackCooldown = 2f;      // 공격 쿨타임
    [SerializeField] private float attackDuration = 0.5f;    // 공격(정지) 지속 시간
    [SerializeField] private Transform attackPoint;          // 히트박스 중심점 (자식 오브젝트로 생성 필요)
    [SerializeField] private float attackRadius = 0.5f;      // 히트박스 반경
    [SerializeField] private LayerMask playerLayer;          // 플레이어 레이어 (Player 지정 필요)

    private float lastAttackTime = -999f;
    private float attackTimer;

    // =========================================================
    // Visual & Drop
    // =========================================================
    [Header("Visual")]
    [SerializeField] private Transform visual;

    [Header("Drop Item")]
    [SerializeField] private GameObject dropItemPrefab;

    // =========================================================
    // State
    // =========================================================
    private bool isChasing;
    private bool isAttacking;
    private bool isDead;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        patrolStartPosition = transform.position;
    }

    private void Update()
    {
        if (isDead) return;

        CheckPlayerDetection();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // 공격 중일 때는 이동 불가
        if (isAttacking)
        {
            UpdateAttack();
            return;
        }

        if (isChasing)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // 공격 사거리 안이고, 쿨타임이 지났다면 공격 시작
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttack();
            }
            else
            {
                // 공격 사거리를 벗어나면 다시 추적
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    // =========================================================
    // Player Detection (무한 추적으로 수정됨)
    // =========================================================
    private void CheckPlayerDetection()
    {
        if (player == null) return;

        // 한 번 추적을 시작했다면 더 이상 감지 범위를 검사하지 않음 (무한 추적)
        if (isChasing) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectionRange) return;

        if (!requireLineOfSight)
        {
            StartChasing();
            return;
        }

        if (CanSeePlayer())
        {
            StartChasing();
        }
    }

    private bool CanSeePlayer()
    {
        Vector2 startPosition = eyePoint != null ? eyePoint.position : transform.position;
        Vector2 direction = (Vector2)player.position - startPosition;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction.normalized, distance, obstacleLayer);
        return hit.collider == null;
    }

    private void StartChasing()
    {
        if (isChasing) return;

        isChasing = true;
        Debug.Log("DummyEnemy : 플레이어 발견! (끝까지 추적합니다)");
    }

    // =========================================================
    // Attack Actions (새로 추가됨)
    // =========================================================
    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;

        // 그 자리에 멈춤
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 플레이어가 바라보는 방향에 히트박스 판정
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hitPlayer != null)
        {
            IDamageable damageable = hitPlayer.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, gameObject);
            }
        }
    }

    private void UpdateAttack()
    {
        attackTimer -= Time.fixedDeltaTime;
        if (attackTimer <= 0f)
        {
            isAttacking = false;
            lastAttackTime = Time.time;
        }
    }

    // =========================================================
    // Patrol & Chase Movement
    // =========================================================
    private void Patrol()
    {
        float leftLimit = patrolStartPosition.x - patrolDistance;
        float rightLimit = patrolStartPosition.x + patrolDistance;

        if (transform.position.x >= rightLimit) patrolDirection = -1;
        if (transform.position.x <= leftLimit) patrolDirection = 1;

        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);
        FlipVisual();
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;

        // 너무 가까우면 이동 멈춤
        if (Mathf.Abs(direction) < 0.05f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float moveDirection = Mathf.Sign(direction);
        rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);

        if (moveDirection > 0) SetVisualDirection(1);
        else SetVisualDirection(-1);
    }

    private void FlipVisual()
    {
        if (patrolDirection > 0) SetVisualDirection(1);
        else SetVisualDirection(-1);
    }

    private void SetVisualDirection(int direction)
    {
        if (visual == null) return;

        Vector3 scale = visual.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        visual.localScale = scale;
    }

    // =========================================================
    // Damage & Die
    // =========================================================
    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Dummy Damage: {damage} / HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Dummy Dead");

        if (dropItemPrefab != null)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }

    // =========================================================
    // Gizmos (공격 히트박스 그리기 추가)
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        // 순찰
        Gizmos.color = Color.yellow;
        Vector3 startPosition = Application.isPlaying ? (Vector3)patrolStartPosition : transform.position;
        Vector3 left = startPosition + Vector3.left * patrolDistance;
        Vector3 right = startPosition + Vector3.right * patrolDistance;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);

        // 감지
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 시야
        if (eyePoint != null && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(eyePoint.position, player.position);
        }

        // 공격 사거리
        Gizmos.color = new Color(1f, 0.5f, 0f); // 주황색
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 히트박스 영역 표시
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}