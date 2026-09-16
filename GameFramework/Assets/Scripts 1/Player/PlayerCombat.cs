using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerStats playerStats;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private float hitboxStartTime = 0.08f;
    [SerializeField] private float hitboxDuration = 0.15f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform attackOrigin;

    [SerializeField] private float projectileSpeed = 15f;

    [Header("Hitbox")]
    [SerializeField] private PlayerAttackHitbox attackHitbox;

    private float lastAttackTime;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (attackHitbox != null)
        {
            attackHitbox.Initialize(this);
            attackHitbox.DisableHitbox();
        }
    }

    private void Update()
    {
        if (!playerInput.AttackPressed)
            return;

        TryAttack();
    }

    private void TryAttack()
    {
        if (IsAttacking)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        playerInput.ConsumeAttack();

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;

        // 공격 시작
        Debug.Log("Sword Attack");

        yield return new WaitForSeconds(hitboxStartTime);

        // 검 Hitbox 활성화
        if (attackHitbox != null)
        {
            attackHitbox.EnableHitbox();
        }

        yield return new WaitForSeconds(hitboxDuration);

        // 검 Hitbox 종료
        if (attackHitbox != null)
        {
            attackHitbox.DisableHitbox();
        }

        // 탄환 발사
        FireProjectile();

        float remainingTime =
            attackDuration -
            hitboxStartTime -
            hitboxDuration;

        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        IsAttacking = false;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null)
            return;

        if (attackOrigin == null)
            return;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                attackOrigin.position,
                attackOrigin.rotation
            );

        PlayerProjectile bullet =
            projectile.GetComponent<PlayerProjectile>();

        if (bullet != null)
        {
            bullet.Initialize(
                attackOrigin.forward,
                projectileSpeed,
                playerStats.BulletDamage
            );
        }
    }

    public float GetSwordDamage()
    {
        return playerStats.SwordDamage;
    }
}