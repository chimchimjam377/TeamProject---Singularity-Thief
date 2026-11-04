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

    // =========================================
    // 💡 [추가됨] 장비(신체 강화)로 얻는 보너스 스탯
    // =========================================
    private int equipHealthBonus = 0;
    private float equipStaminaBonus = 0f;
    private float equipCritChance = 0f;
    private float equipCritDamage = 0f;
    private float equipSlash = 0f;
    private float equipPierce = 0f;
    private float equipBlunt = 0f;

    // =========================================
    // 💡 [수정됨] 기본 스탯 + 장비 보너스를 합산하여 반환하도록 변경
    // =========================================
    public int MaxHealth => maxHealth + equipHealthBonus;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    public float MaxStamina => maxStamina + equipStaminaBonus;
    public float CurrentStamina => currentStamina;

    public float StaminaRatio => MaxStamina > 0f ? currentStamina / MaxStamina : 0f;

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

    // 크리티컬에도 장비 스탯 합산
    public float CriticalChanceBonus => criticalChanceBonus + equipCritChance;
    public float CriticalDamageBonus => criticalDamageBonus + equipCritDamage;

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

    // 공격 타입에도 장비 스탯 합산
    public float SlashDamageBonus => slashDamageBonus + equipSlash;
    public float PierceDamageBonus => pierceDamageBonus + equipPierce;
    public float BluntDamageBonus => bluntDamageBonus + equipBlunt;

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

    // =========================================
    // 💡 [핵심 추가됨] 에러가 났던 함수! 장비 스탯을 받아와서 저장합니다.
    // =========================================
    public void ApplyEquipmentBonuses(int hBonus, float sBonus, float ccBonus, float cdBonus, float slBonus, float piBonus, float blBonus)
    {
        equipHealthBonus = hBonus;
        equipStaminaBonus = sBonus;
        equipCritChance = ccBonus;
        equipCritDamage = cdBonus;
        equipSlash = slBonus;
        equipPierce = piBonus;
        equipBlunt = blBonus;

        // 최대치가 바뀌었을 때 현재 체력/스태미나가 최대치를 넘지 않도록 보정
        if (currentStamina > MaxStamina) currentStamina = MaxStamina;
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;

        NotifyStaminaChanged();
    }

    // =========================================
    // 아래는 기존 시스템 완벽 유지
    // =========================================
    private void UpdateStaminaRecovery()
    {
        if (isDead) return;
        if (currentStamina >= MaxStamina) return;

        if (staminaRecoveryTimer > 0f)
        {
            staminaRecoveryTimer -= Time.deltaTime;
            staminaRecoveryTimer = Mathf.Max(0f, staminaRecoveryTimer);
            return;
        }

        if (playerState != null && playerState.Is(PlayerStateType.Attack)) return;

        float previousStamina = currentStamina;

        currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, MaxStamina);

        if (!Mathf.Approximately(previousStamina, currentStamina))
        {
            NotifyStaminaChanged();
        }
    }

    public bool CanUseStamina(float amount)
    {
        if (isDead || amount < 0f) return false;
        return currentStamina >= amount;
    }

    public bool TryConsumeStamina(float amount)
    {
        if (!CanUseStamina(amount)) return false;
        if (amount <= 0f) return true;

        currentStamina -= amount;
        staminaRecoveryTimer = staminaRecoveryDelay;
        NotifyStaminaChanged();
        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (isDead || amount <= 0f) return;
        currentStamina = Mathf.Min(currentStamina + amount, MaxStamina);
        NotifyStaminaChanged();
    }

    public void IncreaseMaxStamina(float amount)
    {
        if (isDead || amount <= 0f) return;
        maxStamina += amount;
        NotifyStaminaChanged();
    }

    private void NotifyStaminaChanged()
    {
        OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
    }

    public int CalculateAttackDamage(AttackDefinition attack, out bool isCritical)
    {
        isCritical = false;
        if (attack == null) return 0;

        // 대문자로 시작하는 Property(합산된 스탯)를 사용하여 계산
        float typeBonus = GetAttackTypeDamageBonus(attack.Type);
        float finalDamage = attack.Damage * (1f + typeBonus);

        float finalCriticalChance = Mathf.Clamp01(attack.CriticalChance + CriticalChanceBonus);
        isCritical = UnityEngine.Random.value < finalCriticalChance;

        if (isCritical)
        {
            finalDamage *= 1f + CriticalDamageBonus;
        }

        return Mathf.Max(0, Mathf.RoundToInt(finalDamage));
    }

    private float GetAttackTypeDamageBonus(AttackType type)
    {
        switch (type)
        {
            case AttackType.Slash: return SlashDamageBonus;
            case AttackType.Pierce: return PierceDamageBonus;
            case AttackType.Blunt: return BluntDamageBonus;
            default: return 0f;
        }
    }

    public void AddCriticalChance(float amount) { criticalChanceBonus = Mathf.Clamp01(criticalChanceBonus + amount); }
    public void AddCriticalDamageBonus(float amount) { criticalDamageBonus = Mathf.Max(0f, criticalDamageBonus + amount); }

    public void AddAttackTypeDamageBonus(AttackType type, float amount)
    {
        switch (type)
        {
            case AttackType.Slash: slashDamageBonus = Mathf.Max(0f, slashDamageBonus + amount); break;
            case AttackType.Pierce: pierceDamageBonus = Mathf.Max(0f, pierceDamageBonus + amount); break;
            case AttackType.Blunt: bluntDamageBonus = Mathf.Max(0f, bluntDamageBonus + amount); break;
        }
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead || damage <= 0) return;
        if (playerDodge != null && playerDodge.IsInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (playerState != null) playerState.SetState(PlayerStateType.Dead);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, MaxHealth);
    }
}