using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider healthSlider; // 체력 게이지 바
    [SerializeField] private TextMeshProUGUI healthText; // 체력 숫자 텍스트

    private void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void Update()
    {
        if (playerStats == null) return;

        // 텍스트 업데이트 (예: 80 / 100)
        if (healthText != null)
        {
            healthText.text = $"{playerStats.CurrentHealth} / {playerStats.MaxHealth}";
        }

        // 슬라이더 바 업데이트
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerStats.MaxHealth;
            healthSlider.value = playerStats.CurrentHealth;
        }
    }
}