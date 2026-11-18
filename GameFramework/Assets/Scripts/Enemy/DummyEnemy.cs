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

    // =========================================================
    // Line Of Sight
    // =========================================================
    [Header("Line Of Sight")]
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform eyePoint;

    // =========================================================
    // Attack
    // =========================================================
    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;       // 공격 사거리
    [SerializeField] private int attackDamage = 10;          // 공격 데미지
    [SerializeField] private float attackCooldown = 2f;      // 공격 쿨타임
    [SerializeField] private float attackDuration = 0.5f;    // 공격(정지) 지속 시간
    [SerializeField] private Transform attackPoint;          // 히트박스 중심점
    [SerializeField] private float attackRadius = 0.8f;      // 히트박스 반경 (기존 0.5 -> 0.8로 소폭 상향)
    [SerializeField] private LayerMask playerLayer;          // 플레이어 레이어

    private float lastAttackTime = -999f;
    private float attackTimer;
    private float initialAttackPointX; // [추가] 초기 공격 포인트 X좌표 저장

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

        // [추가] 인스펙터에서 Player를 안 넣었더라도 태그로 자동 탐색
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // [추가] AttackPoint의 초기 X 위치 기억
        if (attackPoint != null)
        {
            initialAttackPointX = Mathf.Abs(attackPoint.localPosition.x);
        }

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
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.position);

            // 공격 사거리 안이고, 쿨타임이 지났다면 공격 시작
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttack();
            }
            else
            {
                ChasePlayer();
            }
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
        if (player == null) return;
        if (isChasing) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange) return;

        if (!requireLineOfSight || CanSeePlayer())
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
    // Attack Actions (보강됨)
    // =========================================================
    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;

        // 그 자리에 멈춤
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 공격 시 플레이어 쪽을 바라보도록 방향 보정
        if (player != null)
        {
            int dir = (player.position.x >= transform.position.x) ? 1 : -1;
            SetVisualDirection(dir);
        }

        // 1차 판정: AttackPoint 기반 히트박스 판정
        Vector2 hitOrigin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D hitPlayer = Physics2D.OverlapCircle(hitOrigin, attackRadius, playerLayer);

        if (hitPlayer != null)
        {
            ApplyDamageToTarget(hitPlayer.gameObject);
            return;
        }

        // 2차 판정(안전장치): 히트박스나 레이어 설정 오차로 빗나갔을 경우, 사거리 내 플레이어 직접 타격
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.2f)
        {
            ApplyDamageToTarget(player.gameObject);
        }
        else
        {
            Debug.LogWarning("DummyEnemy : 공격을 시도했으나 사거리 내에 플레이어가 감지되지 않았습니다.");
        }
    }

    // [추가] 대상(자식/부모 포함)에서 IDamageable을 찾아 데미지를 전달하는 통합 함수
    private void ApplyDamageToTarget(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null) damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null) damageable = target.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage, gameObject);
            Debug.Log($"DummyEnemy : {target.name}에게 {attackDamage} 데미지 적중!");
        }
        else
        {
            Debug.LogError($"DummyEnemy : {target.name} 오브젝트에 IDamageable을 상속받은 스크립트가 없습니다!");
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

        if (Mathf.Abs(direction) < 0.05f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float moveDirection = Mathf.Sign(direction);
        rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);

        SetVisualDirection(moveDirection > 0 ? 1 : -1);
    }

    private void FlipVisual()
    {
        SetVisualDirection(patrolDirection > 0 ? 1 : -1);
    }

    private void SetVisualDirection(int direction)
    {
        if (visual != null)
        {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            visual.localScale = scale;
        }

        // [추가] AttackPoint가 Visual의 자식이 아니더라도 몬스터 방향에 맞춰 위치 반전
        if (attackPoint != null && (visual == null || !attackPoint.IsChildOf(visual)))
        {
            Vector3 pointPos = attackPoint.localPosition;
            pointPos.x = initialAttackPointX * direction;
            attackPoint.localPosition = pointPos;
        }
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
    // Gizmos
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 startPosition = Application.isPlaying ? (Vector3)patrolStartPosition : transform.position;
        Vector3 left = startPosition + Vector3.left * patrolDistance;
        Vector3 right = startPosition + Vector3.right * patrolDistance;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (eyePoint != null && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(eyePoint.position, player.position);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}