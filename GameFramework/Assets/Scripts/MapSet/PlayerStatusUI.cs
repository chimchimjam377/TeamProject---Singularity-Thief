using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Health")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Stamina")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text staminaText;

    private void Update()
    {
        if (playerStats == null)
            return;

        UpdateHealthUI();
        UpdateStaminaUI();
    }

    private void UpdateHealthUI()
    {
        int currentHealth =
            playerStats.CurrentHealth;

        int maxHealth =
            playerStats.MaxHealth;

        if (healthSlider != null)
        {
            healthSlider.value =
                maxHealth > 0
                    ? (float)currentHealth / maxHealth
                    : 0f;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{currentHealth} / {maxHealth}";
        }
    }

    private void UpdateStaminaUI()
    {
        float currentStamina =
            playerStats.CurrentStamina;

        float maxStamina =
            playerStats.MaxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.value =
                maxStamina > 0f
                    ? currentStamina / maxStamina
                    : 0f;
        }

        if (staminaText != null)
        {
            staminaText.text =
                $"{Mathf.CeilToInt(currentStamina)} / " +
                $"{Mathf.CeilToInt(maxStamina)}";
        }
    }
}