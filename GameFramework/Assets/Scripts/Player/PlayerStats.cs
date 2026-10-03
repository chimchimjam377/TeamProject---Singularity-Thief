using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    private PlayerDodge playerDodge;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        playerDodge = GetComponent<PlayerDodge>();
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
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
        Debug.Log("Player Dead");
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}