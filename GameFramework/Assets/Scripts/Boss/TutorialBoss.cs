using System.Collections;
using UnityEngine;

public class TutorialBoss : MonoBehaviour, IDamageable
{
    [Header("체력 및 상태 설정")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int dashDamage = 20;     // 돌진 데미지
    [SerializeField] private int slamDamage = 35;     // 내려찍기 충격파 데미지
    private int currentHealth;
    private bool isDead = false;

    [Header("기본 참조")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D bossCollider;

    [Header("돌진 패턴 설정")]
    [SerializeField] private float dashSpeed = 25f;       // 돌진 속도
    [SerializeField] private float dashDuration = 0.4f;   // 돌진 지속 시간
    [SerializeField] private float chargeTime = 1.0f;     // 준비(경고) 시간
    [SerializeField] private float cooldownTime = 2.0f;   // 패턴 후 딜레이 (딜 타임)

    [Header("2페이즈 연속 돌진 설정")]
    [SerializeField] private float secondDashDelay = 0.3f; // 1차 돌진 후 2차 돌진 전 재조준 대기시간

    [Header("내려찍기 패턴 설정")]
    [SerializeField] private float jumpForce = 12f;       // 내려찍기 직전 공중 점프 힘
    [SerializeField] private float slamSpeed = 30f;       // 바닥 강하 기본 속도
    [SerializeField] private float slamRadius = 3f;       // 내려찍기 충격파 범위 반지름
    [SerializeField] private LayerMask playerLayerMask;   // 플레이어 레이어 감지용

    [Header("시각 효과 (산나비 느낌)")]
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color hitColor = Color.white;
    private Color originalColor;

    private bool isAttacking = false;
    private Vector2 dashDirection;

    private int bossLayer;
    private int playerLayer;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (bossCollider == null) bossCollider = GetComponent<Collider2D>();

        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        bossLayer = gameObject.layer;
        if (player != null) playerLayer = player.gameObject.layer;

        StartCoroutine(BossPatternLoop());
    }

    private IEnumerator BossPatternLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(cooldownTime);

            if (!isAttacking && player != null && !isDead)
            {
                yield return StartCoroutine(ExecuteBossAttackSequence());
            }
        }
    }

    private IEnumerator ExecuteBossAttackSequence()
    {
        isAttacking = true;

        // 1단계: 돌진 패턴 (체력 50% 이하 시 2회 연속 돌진)
        bool isPhase2 = currentHealth <= (maxHealth / 2);
        int dashCount = isPhase2 ? 2 : 1;

        for (int i = 0; i < dashCount; i++)
        {
            if (isDead) yield break;

            LookAtPlayer();
            dashDirection = (player.position - transform.position).normalized;

            StopHorizontalMovement();
            spriteRenderer.color = warningColor;

            float currentChargeTime = (i == 0) ? chargeTime : chargeTime * 0.5f;
            yield return new WaitForSeconds(currentChargeTime);

            if (!isDead)
            {
                spriteRenderer.color = originalColor;
                SetPlayerCollision(false);

                float timer = 0f;
                while (timer < dashDuration && !isDead)
                {
                    SetVelocity(dashDirection * dashSpeed);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            StopHorizontalMovement();
            SetPlayerCollision(true);

            if (dashCount > 1 && i < dashCount - 1 && !isDead)
            {
                yield return new WaitForSeconds(secondDashDelay);
            }
        }

        // 2단계: 바닥 내려찍기 패턴
        if (!isDead && player != null)
        {
            yield return StartCoroutine(ExecuteGroundSlamPattern());
        }

        // 3단계: 패턴 완료 후 딜 타임
        if (!isDead)
        {
            yield return new WaitForSeconds(1.5f);
        }

        isAttacking = false;
    }

    // 내려찍기 패턴 코루틴 (속도 저하 방지 적용)
    private IEnumerator ExecuteGroundSlamPattern()
    {
        LookAtPlayer();
        spriteRenderer.color = warningColor;

        // 1. 공중 선점프
        SetVelocity(new Vector2(0f, jumpForce));
        yield return new WaitForSeconds(0.15f);

        // 2. 공중 정지 및 위치 조준
        SetVelocity(Vector2.zero);

        float targetX = player.position.x;
        float alignTimer = 0f;
        while (alignTimer < 0.08f && !isDead)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 20f);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            alignTimer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;

        // 3. 수직 초고속 급강하
        bool hitGround = false;
        float currentSlamSpeed = slamSpeed;

        // ★ 플레이어와 충돌을 무시하여 마찰로 인한 속도 저하 방지
        SetPlayerCollision(false);

        while (!hitGround && !isDead)
        {
            currentSlamSpeed += Time.deltaTime * 100f; // 매 프레임 가속
            SetVelocity(Vector2.down * currentSlamSpeed);

            // 속도가 빠르므로 Raycast 길이를 넉넉하게 지정하여 바닥 감지
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2.0f);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                hitGround = true;
            }

            yield return null;
        }

        // 4. 착지 및 원상 복구
        StopHorizontalMovement();
        SetVelocity(Vector2.zero);

        // ★ 착지 완료 후 플레이어와의 충돌 관계 복원
        SetPlayerCollision(true);

        // 충격파 범위 데미지 판정
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, slamRadius, playerLayerMask);
        foreach (Collider2D p in hitPlayers)
        {
            if (p.CompareTag("Player") && p.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(slamDamage, gameObject);
            }
        }
    }

    private void SetPlayerCollision(bool enableCollision)
    {
        if (playerLayer != 0 && bossLayer != 0)
        {
            Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, !enableCollision);
        }
    }

    private void LookAtPlayer()
    {
        if (player == null || isDead) return;

        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    #region IDamageable 구현
    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"보스가 {damage}의 데미지를 입었습니다! 남은 체력: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashHitEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    #endregion

    private IEnumerator FlashHitEffect()
    {
        if (!isAttacking)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        SetPlayerCollision(true);

        Debug.Log("튜토리얼 보스가 처치되었습니다!");
        Destroy(gameObject, 0.5f);
    }

    private void SetVelocity(Vector2 velocity)
    {
        if (rb == null) return;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
            rb.velocity = velocity;
#endif
    }

    private void StopHorizontalMovement()
    {
        if (rb == null) return;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
#else
            rb.velocity = new Vector2(0f, rb.velocity.y);
#endif
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttacking && collision.gameObject.CompareTag("Wall"))
        {
            StopHorizontalMovement();
        }

        if (isAttacking && collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(dashDamage, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}