using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"Dummy Damage: {damage} / HP: {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Dummy Dead");

        // 테스트용으로 비활성화
        gameObject.SetActive(false);
    }
}