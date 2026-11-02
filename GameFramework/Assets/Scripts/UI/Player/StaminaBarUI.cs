using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class StaminaBarUI : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private PlayerStats playerStats;


    private void OnEnable()
    {
        if (playerStats == null)
            return;

        playerStats.OnStaminaChanged += UpdateUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStaminaChanged -= UpdateUI;
        }
    }

    private void RefreshUI()
    {
        if (playerStats == null)
            return;

        UpdateUI(
            playerStats.CurrentStamina,
            playerStats.MaxStamina
        );
    }

    private void UpdateUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = max;

            staminaSlider.SetValueWithoutNotify(current);
        }

        if (staminaText != null)
        {
            staminaText.text =
                $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}