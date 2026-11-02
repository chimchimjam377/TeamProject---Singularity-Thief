using UnityEngine;
using System;
public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("체력")]
    [SerializeField] private int maxHealth = 100;

    [Header("스태미나")]
    [Min(0f)]
    [SerializeField] private float maxStamina = 100f;

    [Min(0f)]
    [SerializeField] private float staminaRecoveryPerSecond = 20f;

    [Min(0f)]
    [SerializeField] private float staminaRecoveryDelay = 1.5f;

    private int currentHealth;
    private float currentStamina;
    private float staminaRecoveryTimer;

    private PlayerDodge playerDodge;
    private PlayerState playerState;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;


    public bool IsDead => isDead;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;

    // Ui에서 스태미나 비율을 표시할 때 사용
    public float StaminaRatio => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    public event Action<float, float> OnStaminaChanged;

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

        currentStamina = maxStamina;

        playerDodge = GetComponent<PlayerDodge>();
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateStaminaRecovery();
    }

    private void UpdateStaminaRecovery()
    {
        if (isDead)
            return;

        if (currentStamina >= maxStamina)
            return;

        // 스태미나 회복 대기 시간
        if (staminaRecoveryTimer > 0f)
        {
            staminaRecoveryTimer -= Time.deltaTime;
            staminaRecoveryTimer = Mathf.Max(
                0f,
                staminaRecoveryTimer
            );

            return;
        }

        // 공격 중에는 회복하지 않는다.
        // 대기 시간은 공격 중에도 계속 감소한다.
        if (playerState != null &&
            playerState.Is(PlayerStateType.Attack))
        {
            return;
        }

        float previousStamina = currentStamina;

        currentStamina += staminaRecoveryPerSecond * Time.deltaTime;

        currentStamina = Mathf.Min(
            currentStamina,
            maxStamina
        );

        if (!Mathf.Approximately(previousStamina, currentStamina))
        {
            NotifyStaminaChanged();
        }
    }

    // =========================================
    // Stamina Management
    // =========================================

    // 스태미나가 충분한지 확인
    public bool CanUseStamina(float amount)
    {
        if (isDead || amount < 0f)
            return false;

        return currentStamina >= amount;
    }



    public bool TryConsumeStamina(float amount)
    {
        if (!CanUseStamina(amount))
            return false;

        if (amount <= 0f)
            return true;

        currentStamina -= amount;

        staminaRecoveryTimer = staminaRecoveryDelay;

        NotifyStaminaChanged();

        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentStamina = Mathf.Min(
            currentStamina + amount,
            maxStamina
        );

        NotifyStaminaChanged();
    }

    public void IncreaseMaxStamina(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        maxStamina += amount;

        NotifyStaminaChanged();
    }

    private void NotifyStaminaChanged()
    {
        OnStaminaChanged?.Invoke(
            currentStamina,
            maxStamina
        );
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
        isCritical = UnityEngine.Random.value < finalCriticalChance;

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