using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    private PlayerDodge playerDodge;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private bool isDead;
    private PlayerState playerState;

    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        playerDodge = GetComponent<PlayerDodge>();
        playerState = GetComponent<PlayerState>();
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isDead || damage <= 0)
             return;

        // 닷지 무적
        if (playerDodge != null && playerDodge.IsInvincible)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"Player Damage: {damage} / HP: {currentHealth}/{maxHealth}"
        );

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