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

    // 가드
    private PlayerGuard playerGuard;

    private bool isDead;


    // =========================================
    // 장비 보너스 스탯
    // =========================================

    private int equipHealthBonus = 0;
    private float equipStaminaBonus = 0f;

    private float equipCritChance = 0f;
    private float equipCritDamage = 0f;

    private float equipSlash = 0f;
    private float equipPierce = 0f;
    private float equipBlunt = 0f;


    // =========================================
    // 기본 스탯 + 장비 보너스
    // =========================================

    public int MaxHealth =>
        maxHealth + equipHealthBonus;

    public int CurrentHealth =>
        currentHealth;

    public bool IsDead =>
        isDead;

    public float MaxStamina =>
        maxStamina + equipStaminaBonus;

    public float CurrentStamina =>
        currentStamina;

    public float StaminaRatio =>
        MaxStamina > 0f
            ? currentStamina / MaxStamina
            : 0f;


    public event Action<float, float> OnStaminaChanged;


    [Header("Detection")]
    public float SoundPower = 10f;


    // =========================================
    // Critical
    // =========================================

    [Header("Critical Stats")]

    [Tooltip("모든 공격에 적용되는 추가 크리티컬 확률")]
    [Range(0f, 1f)]
    [SerializeField]
    private float criticalChanceBonus = 0f;

    [Tooltip("크리티컬 발생 시 추가 피해량. 0.5 = 50%")]
    [Min(0f)]
    [SerializeField]
    private float criticalDamageBonus = 0.5f;


    public float CriticalChanceBonus =>
        criticalChanceBonus +
        equipCritChance;

    public float CriticalDamageBonus =>
        criticalDamageBonus +
        equipCritDamage;


    // =========================================
    // Attack Type Damage Bonuses
    // =========================================

    [Header("Attack Type Damage Bonus")]

    [Tooltip("베기 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField]
    private float slashDamageBonus = 0f;

    [Tooltip("찌르기 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField]
    private float pierceDamageBonus = 0f;

    [Tooltip("타격 피해 증가. 0.2 = 20%")]
    [Min(0f)]
    [SerializeField]
    private float bluntDamageBonus = 0f;


    public float SlashDamageBonus =>
        slashDamageBonus +
        equipSlash;

    public float PierceDamageBonus =>
        pierceDamageBonus +
        equipPierce;

    public float BluntDamageBonus =>
        bluntDamageBonus +
        equipBlunt;


    // =========================================
    // Awake
    // =========================================

    private void Awake()
    {
        currentHealth = MaxHealth;
        currentStamina = MaxStamina;

        playerDodge =
            GetComponent<PlayerDodge>();

        playerState =
            GetComponent<PlayerState>();

        // 가드 자동 연결
        playerGuard =
            GetComponent<PlayerGuard>();
    }


    private void Update()
    {
        UpdateStaminaRecovery();
    }


    // =========================================
    // Equipment
    // =========================================

    public void ApplyEquipmentBonuses(
        int hBonus,
        float sBonus,
        float ccBonus,
        float cdBonus,
        float slBonus,
        float piBonus,
        float blBonus)
    {
        equipHealthBonus = hBonus;
        equipStaminaBonus = sBonus;

        equipCritChance = ccBonus;
        equipCritDamage = cdBonus;

        equipSlash = slBonus;
        equipPierce = piBonus;
        equipBlunt = blBonus;


        if (currentStamina > MaxStamina)
        {
            currentStamina =
                MaxStamina;
        }

        if (currentHealth > MaxHealth)
        {
            currentHealth =
                MaxHealth;
        }

        NotifyStaminaChanged();
    }


    // =========================================
    // Stamina Recovery
    // =========================================

    private void UpdateStaminaRecovery()
    {
        if (isDead)
            return;

        if (currentStamina >= MaxStamina)
            return;


        if (staminaRecoveryTimer > 0f)
        {
            staminaRecoveryTimer -=
                Time.deltaTime;

            staminaRecoveryTimer =
                Mathf.Max(
                    0f,
                    staminaRecoveryTimer
                );

            return;
        }


        if (playerState != null &&
            playerState.Is(PlayerStateType.Attack))
        {
            return;
        }


        float previousStamina =
            currentStamina;


        currentStamina +=
            staminaRecoveryPerSecond *
            Time.deltaTime;


        currentStamina =
            Mathf.Min(
                currentStamina,
                MaxStamina
            );


        if (!Mathf.Approximately(
                previousStamina,
                currentStamina))
        {
            NotifyStaminaChanged();
        }
    }


    // =========================================
    // Stamina
    // =========================================

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

        staminaRecoveryTimer =
            staminaRecoveryDelay;

        NotifyStaminaChanged();

        return true;
    }


    public void RestoreStamina(float amount)
    {
        if (isDead || amount <= 0f)
            return;


        currentStamina =
            Mathf.Min(
                currentStamina + amount,
                MaxStamina
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
            MaxStamina
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


        float typeBonus =
            GetAttackTypeDamageBonus(
                attack.Type
            );


        float finalDamage =
            attack.Damage *
            (1f + typeBonus);


        float finalCriticalChance =
            Mathf.Clamp01(
                attack.CriticalChance +
                CriticalChanceBonus
            );


        isCritical =
            UnityEngine.Random.value <
            finalCriticalChance;


        if (isCritical)
        {
            finalDamage *=
                1f +
                CriticalDamageBonus;
        }


        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                finalDamage
            )
        );
    }


    private float GetAttackTypeDamageBonus(
        AttackType type)
    {
        switch (type)
        {
            case AttackType.Slash:
                return SlashDamageBonus;

            case AttackType.Pierce:
                return PierceDamageBonus;

            case AttackType.Blunt:
                return BluntDamageBonus;

            default:
                return 0f;
        }
    }


    public void AddCriticalChance(float amount)
    {
        criticalChanceBonus =
            Mathf.Clamp01(
                criticalChanceBonus +
                amount
            );
    }


    public void AddCriticalDamageBonus(
        float amount)
    {
        criticalDamageBonus =
            Mathf.Max(
                0f,
                criticalDamageBonus +
                amount
            );
    }


    public void AddAttackTypeDamageBonus(
        AttackType type,
        float amount)
    {
        switch (type)
        {
            case AttackType.Slash:

                slashDamageBonus =
                    Mathf.Max(
                        0f,
                        slashDamageBonus +
                        amount
                    );

                break;


            case AttackType.Pierce:

                pierceDamageBonus =
                    Mathf.Max(
                        0f,
                        pierceDamageBonus +
                        amount
                    );

                break;


            case AttackType.Blunt:

                bluntDamageBonus =
                    Mathf.Max(
                        0f,
                        bluntDamageBonus +
                        amount
                    );

                break;
        }
    }


    // =========================================
    // Damage
    // =========================================

    public void TakeDamage(
        int damage,
        GameObject attacker)
    {
        if (isDead || damage <= 0)
            return;


        // -------------------------
        // 1. 닷지 무적
        // -------------------------

        if (playerDodge != null &&
            playerDodge.IsInvincible)
        {
            return;
        }


        // -------------------------
        // 2. 방향 가드
        // -------------------------

        if (playerGuard != null &&
            attacker != null)
        {
            bool blocked =
                playerGuard.CanBlockAttack(
                    attacker.transform.position
                );


            if (blocked)
            {
                Debug.Log(
                    "공격 방어 성공!"
                );

                return;
            }
        }


        // -------------------------
        // 3. 실제 데미지
        // -------------------------

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(
                currentHealth,
                0
            );


        Debug.Log(
            $"Player Damage: {damage} / " +
            $"HP: {currentHealth}/{MaxHealth}"
        );


        if (currentHealth <= 0)
        {
            Die();
        }
    }


    // =========================================
    // Death
    // =========================================

    private void Die()
    {
        if (isDead)
            return;


        isDead = true;


        if (playerState != null)
        {
            playerState.SetState(
                PlayerStateType.Dead
            );
        }


        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }


        gameObject.SetActive(false);
    }


    // =========================================
    // Heal
    // =========================================

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;


        currentHealth += amount;

        currentHealth =
            Mathf.Min(
                currentHealth,
                MaxHealth
            );
    }
}