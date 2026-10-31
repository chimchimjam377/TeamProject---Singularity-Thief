using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    private PlayerDodge playerDodge;
    private PlayerState playerState;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    [Header("Detection")]
    public float SoundPower = 10f;

    [Header("Critical Stats")]

    [Tooltip("모든 공격에 적용되는 추가 크리티컬 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float criticalChanceBonus = 0f;

    [Tooltip("크리티컬 발생 시 추가 피해량. 0.5 = 50%")]
    [Min(0f)]
    [SerializeField] private float criticalDamageBonus = 0.5f;

    public float CriticalChanceBonus => criticalChanceBonus;
    public float CriticalDamageBonus => criticalDamageBonus;


    // =========================================
    // Attack Type Damage Bonuses
    // =========================================

    [Header("Attack Type Damage Bonus")]

    [Tooltip("베기 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField] private float slashDamageBonus = 0f;

    [Tooltip("찌르기 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField] private float pierceDamageBonus = 0f;

    [Tooltip("타격 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField] private float bluntDamageBonus = 0f;

    public float SlashDamageBonus => slashDamageBonus;
    public float PierceDamageBonus => pierceDamageBonus;
    public float BluntDamageBonus => bluntDamageBonus;

    private void Awake()
    {
        currentHealth = maxHealth;

        playerDodge = GetComponent<PlayerDodge>();
        playerState = GetComponent<PlayerState>();
    }


    // =========================================
    // Attack Damage Calculation
    // =========================================

    public int CalculateAttackDamage(
    AttackDefinition attack,
    out bool isCritical)
    {
        isCritical = false;

        if (attack == null)
            return 0;

        // 1. 공격 유형별 피해량 증가
        float typeBonus = GetAttackTypeDamageBonus(attack.Type);

        float finalDamage =
            attack.Damage * (1f + typeBonus);

        // 2. 공격 고유 확률 + 플레이어 추가 확률
        float finalCriticalChance = Mathf.Clamp01(
            attack.CriticalChance + criticalChanceBonus
        );

        // 3. 크리티컬 판정
        isCritical = Random.value < finalCriticalChance;

        // 4. 크리티컬 추가 피해
        if (isCritical)
        {
            finalDamage *= 1f + criticalDamageBonus;
        }

        return Mathf.Max(
            0,
            Mathf.RoundToInt(finalDamage)
        );
    }


    // =========================================
    // Get Attack Type Bonus
    // =========================================

    private float GetAttackTypeDamageBonus(AttackType type)
    {
        switch (type)
        {
            case AttackType.Slash:
                return slashDamageBonus;

            case AttackType.Pierce:
                return pierceDamageBonus;

            case AttackType.Blunt:
                return bluntDamageBonus;

            default:
                return 0f;
        }
    }


    // =========================================
    // Runtime Stat Upgrades
    // =========================================

    // 크리티컬 확률 증가
    // amount = 0.05f라면 5%p 증가
    public void AddCriticalChance(float amount)
    {
        criticalChanceBonus = Mathf.Clamp01(
        criticalChanceBonus + amount
         );
    }

    // 크리티컬 추가 피해 증가
    // amount = 0.25f라면 추가 피해율 25%p 증가
    public void AddCriticalDamageBonus(float amount)
    {
        criticalDamageBonus = Mathf.Max(
            0f,
            criticalDamageBonus + amount
        );
    }

    // 특정 공격 유형의 피해 증가
    // amount = 0.1f라면 해당 유형의 피해 증가율 10%p 추가
    public void AddAttackTypeDamageBonus(
        AttackType type,
        float amount)
    {
        switch (type)
        {
            case AttackType.Slash:
                slashDamageBonus = Mathf.Max(
                    0f,
                    slashDamageBonus + amount
                );
                break;

            case AttackType.Pierce:
                pierceDamageBonus = Mathf.Max(
                    0f,
                    pierceDamageBonus + amount
                );
                break;

            case AttackType.Blunt:
                bluntDamageBonus = Mathf.Max(
                    0f,
                    bluntDamageBonus + amount
                );
                break;
        }
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead || damage <= 0)
            return;

        // 닷지 무적 판정 유지
        if (playerDodge != null && playerDodge.IsInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player Damage: {damage} / HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (playerState != null)
            playerState.SetState(PlayerStateType.Dead);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log("Player Dead");
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}