using System.Collections;
using UnityEngine;

public class TutorialBoss : MonoBehaviour, IDamageable
{
    [Header("체력 및 상태 설정")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int dashDamage = 20;         // 돌진 데미지
    [SerializeField] private int laserDamage = 15;        // 레이저 초당/타격 데미지
    private int currentHealth;
    private bool isDead = false;

    [Header("기본 참조")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D bossCollider;
    [SerializeField] private LineRenderer laserLine;      // 레이저 연출용 LineRenderer

    [Header("돌진 패턴 설정")]
    [SerializeField] private float dashSpeed = 25f;       // 돌진 속도
    [SerializeField] private float dashDuration = 0.4f;   // 돌진 지속 시간
    [SerializeField] private float chargeTime = 1.0f;     // 준비(경고) 시간
    [SerializeField] private float cooldownTime = 2.0f;   // 패턴 후 딜레이 (딜 타임)

    [Header("2페이즈 연속 돌진 설정")]
    [SerializeField] private float secondDashDelay = 0.3f; // 1차 돌진 후 2차 돌진 전 재조준 대기시간

    [Header("2페이즈 레이저 패턴 설정")]
    [SerializeField] private float keepDistance = 8f;     // 플레이어와 유지할 후퇴 거리
    [SerializeField] private float retreatSpeed = 15f;    // 후퇴 속도
    [SerializeField] private float laserAimTime = 1.0f;   // 레이저 조준(경고) 시간
    [SerializeField] private float laserDuration = 0.8f;  // 레이저 유지(발사) 시간
    [SerializeField] private float laserMaxDistance = 20f;// 레이저 최대 사거리
    [SerializeField] private LayerMask laserHitMask;      // 레이저에 감지될 레이어 (Player, Wall 등)

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
        if (laserLine == null) laserLine = GetComponent<LineRenderer>();

        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        if (laserLine != null) laserLine.enabled = false;

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

        bool isPhase2 = currentHealth <= (maxHealth / 2);
        int dashCount = isPhase2 ? 2 : 1;

        // --- 1단계: 돌진 패턴 (1페이즈 1회, 2페이즈 2회) ---
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

        // --- 2단계: 2페이즈 전용 레이저 패턴 (거리 벌리기 후 레이저 발사) ---
        if (isPhase2 && !isDead && player != null)
        {
            yield return StartCoroutine(ExecuteLaserPattern());
        }

        // --- 3단계: 공격 완수 후 딜 타임(후딜레이) ---
        if (!isDead)
        {
            yield return new WaitForSeconds(1.5f);
        }

        isAttacking = false;
    }

    // 2페이즈 레이저 패턴 코루틴
    private IEnumerator ExecuteLaserPattern()
    {
        // 1. 플레이어와 거리 벌리기 (후퇴)
        LookAtPlayer();
        float currentDist = Vector2.Distance(transform.position, player.position);

        if (currentDist < keepDistance)
        {
            // 플레이어의 반대 방향으로 후퇴
            Vector2 retreatDir = (transform.position - player.position).normalized;
            // X축 위주로 후퇴하도록 조정
            retreatDir = new Vector2(Mathf.Sign(retreatDir.x), 0f).normalized;

            float retreatTimer = 0f;
            while (retreatTimer < 0.6f && Vector2.Distance(transform.position, player.position) < keepDistance && !isDead)
            {
                SetVelocity(retreatDir * retreatSpeed);
                retreatTimer += Time.deltaTime;
                yield return null;
            }
            StopHorizontalMovement();
        }

        if (isDead) yield break;

        // 2. 레이저 조준 단계 (붉은 가이드라인 표사)
        LookAtPlayer();
        spriteRenderer.color = warningColor;

        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.startWidth = 0.05f; // 경고용 얇은 선
            laserLine.endWidth = 0.05f;
        }

        float aimTimer = 0f;
        Vector2 laserDir = Vector2.zero;

        while (aimTimer < laserAimTime && !isDead)
        {
            LookAtPlayer();
            laserDir = (player.position - transform.position).normalized;

            // 조준 선 그리기
            if (laserLine != null)
            {
                laserLine.SetPosition(0, transform.position);
                laserLine.SetPosition(1, (Vector2)transform.position + laserDir * laserMaxDistance);
            }

            aimTimer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;

        // 3. 레이저 발사 (두꺼운 라인 및 데미지 판정)
        if (!isDead)
        {
            if (laserLine != null)
            {
                laserLine.startWidth = 0.4f; // 발사 시 두꺼워짐
                laserLine.endWidth = 0.4f;
            }

            float fireTimer = 0f;
            bool hitPlayerThisFrame = false;

            while (fireTimer < laserDuration && !isDead)
            {
                Vector2 startPos = transform.position;
                RaycastHit2D hit = Physics2D.Raycast(startPos, laserDir, laserMaxDistance, laserHitMask);

                Vector2 endPos = startPos + laserDir * laserMaxDistance;

                if (hit.collider != null)
                {
                    endPos = hit.point;

                    // 플레이어가 레이저에 맞았을 때 데미지 처리 (1회 또는 지속)
                    if (!hitPlayerThisFrame && hit.collider.CompareTag("Player"))
                    {
                        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                        {
                            damageable.TakeDamage(laserDamage, gameObject);
                            hitPlayerThisFrame = true; // 1회격출 처리 (지속 데미지를 원하면 조건 제거 가능)
                        }
                    }
                }

                if (laserLine != null)
                {
                    laserLine.SetPosition(0, startPos);
                    laserLine.SetPosition(1, endPos);
                }

                fireTimer += Time.deltaTime;
                yield return null;
            }
        }

        // 레이저 비활성화 및 정지
        if (laserLine != null) laserLine.enabled = false;
        StopHorizontalMovement();
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
        if (laserLine != null) laserLine.enabled = false;

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
}