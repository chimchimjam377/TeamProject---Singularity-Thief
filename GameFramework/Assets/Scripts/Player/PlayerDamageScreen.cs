using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Image damagePanel;

    [Header("Opacity")]
    [SerializeField, Range(0f, 1f)]
    private float maxOpacity = 0.65f;

    [SerializeField, Min(0.1f)]
    private float intensityPower = 1.2f;

    [Header("Transition")]
    [SerializeField, Min(0.1f)]
    private float transitionSpeed = 5f;

    private float currentOpacity;

    private void Start()
    {
        if (playerStats == null || damagePanel == null)
        {
            Debug.LogError(
                "PlayerDamageScreen: Player Stats와 Damage Panel을 연결하세요.",
                this
            );

            enabled = false;
            return;
        }

        damagePanel.raycastTarget = false;

        currentOpacity = GetTargetOpacity();
        ApplyOpacity();
    }

    private void Update()
    {
        if (playerStats == null || damagePanel == null)
            return;

        float targetOpacity = GetTargetOpacity();

        float blend = 1f - Mathf.Exp(
            -Mathf.Max(0.1f, transitionSpeed) * Time.unscaledDeltaTime
        );

        currentOpacity = Mathf.Lerp(
            currentOpacity,
            targetOpacity,
            blend
        );

        ApplyOpacity();
    }

    private float GetTargetOpacity()
    {
        float healthRatio = Mathf.Clamp01(
            (float)playerStats.CurrentHealth /
            Mathf.Max(1, playerStats.MaxHealth)
        );

        float lostHealthRatio = 1f - healthRatio;

        return Mathf.Pow(
            lostHealthRatio,
            Mathf.Max(0.1f, intensityPower)
        ) * Mathf.Clamp01(maxOpacity);
    }

    private void ApplyOpacity()
    {
        Color color = damagePanel.color;
        color.a = currentOpacity;
        damagePanel.color = color;
    }
}