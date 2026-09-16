using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Combat")]
    [SerializeField] private float swordDamage = 30f;
    [SerializeField] private float bulletDamage = 20f;

    private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public float SwordDamage => swordDamage;
    public float BulletDamage => bulletDamage;

    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"Player Damage: {damage} / HP: {currentHealth}");

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(
            currentHealth,
            maxHealth
        );
    }

    private void Die()
    {
        Debug.Log("Player Dead");
    }
}