using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();
    }

    private void Update()
    {
        if (playerStats == null) return;

        if (healthText != null)
            healthText.text = $"{playerStats.CurrentHealth} / {playerStats.MaxHealth}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerStats.MaxHealth;
            healthSlider.value = playerStats.CurrentHealth;
        }
    }
}