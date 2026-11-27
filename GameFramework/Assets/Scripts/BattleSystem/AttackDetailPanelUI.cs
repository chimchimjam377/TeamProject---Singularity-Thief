
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackDetailPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Preview")]
    [SerializeField] private Image previewImage;

    [Header("Basic Information")]
    [SerializeField] private TMP_Text attackNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text attackTypeText;

    [Header("Combat Information")]
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text criticalChanceText;
    [SerializeField] private TMP_Text criticalDamageText;
    [SerializeField] private TMP_Text staminaCostText;

    [Header("Movement Information")]
    [SerializeField] private TMP_Text movementText;

    private void Awake()
    {
        Clear();
    }

    // ========================================
    // 공격 정보 표시
    // ========================================

    public void Show(AttackDefinition attack)
    {
        if (attack == null)
        {
            Clear();
            return;
        }

        // 이름
        SetText(
            attackNameText,
            attack.DisplayName
        );

        // 설명
        SetText(
            descriptionText,
            string.IsNullOrWhiteSpace(attack.Description)
                ? "등록된 공격 설명이 없습니다."
                : attack.Description
        );

        // 이미지
        if (previewImage != null)
        {
            Sprite image = attack.PreviewImage != null
                ? attack.PreviewImage
                : attack.Icon;

            previewImage.sprite = image;
            previewImage.enabled = image != null;
        }

        // 공격 유형
        SetText(
            attackTypeText,
            GetAttackTypeName(attack.Type)
        );

        // 피해량 및 크리티컬
        if (playerStats != null)
        {
            int normalDamage =
                playerStats.GetNormalAttackDamage(attack);

            int criticalDamage =
                playerStats.GetCriticalAttackDamage(attack);

            float criticalChance =
                playerStats.GetFinalCriticalChance(attack);

            float criticalMultiplier =
                (1f + playerStats.CriticalDamageBonus) * 100f;

            SetText(
                damageText,
                $"일반 피해량 : {normalDamage}"
            );

            SetText(
                criticalChanceText,
                $"크리티컬 확률 : {criticalChance * 100f:0.#}%"
            );

            SetText(
                criticalDamageText,
                $"크리티컬 피해량 : {criticalDamage} ({criticalMultiplier:0.#}%)"
            );
        }
        else
        {
            SetText(
                damageText,
                $"기본 피해량 : {attack.Damage}"
            );

            SetText(
                criticalChanceText,
                "크리티컬 확률 : 플레이어 스탯 미연결"
            );

            SetText(
                criticalDamageText,
                "크리티컬 피해량 : 플레이어 스탯 미연결"
            );
        }

        // 스태미나
        SetText(
            staminaCostText,
            $"스태미나 소모 : {attack.StaminaCost:0.##}"
        );

        // 공격 이동 방식
        SetText(
            movementText,
            GetMovementDescription(attack)
        );
    }

    // ========================================
    // 공격 유형 이름
    // ========================================

    private string GetAttackTypeName(AttackType type)
    {
        switch (type)
        {
            case AttackType.Slash:
                return "공격 유형 : 참격";

            case AttackType.Pierce:
                return "공격 유형 : 찌르기";

            case AttackType.Blunt:
                return "공격 유형 : 타격";

            default:
                return "공격 유형 : 없음";
        }
    }

    // ========================================
    // 이동 정보
    // ========================================

    private string GetMovementDescription(
        AttackDefinition attack)
    {
        switch (attack.MovementType)
        {
            case AttackMovementType.FixedDistance:
                return $"이동 : 돌진 ({attack.MoveDistance:0.##})";

            case AttackMovementType.NearestTarget:
                return
                    $"이동 : 적 접근\n" +
                    $"탐색 범위 : {attack.TargetSearchRadius:0.##}\n" +
                    $"정지 거리 : {attack.TargetStopDistance:0.##}";

            default:
                return "이동 : 없음";
        }
    }

    // ========================================
    // 정보 초기화
    // ========================================

    public void Clear()
    {
        SetText(attackNameText, "공격을 선택하세요.");
        SetText(descriptionText, "");
        SetText(attackTypeText, "");
        SetText(damageText, "");
        SetText(criticalChanceText, "");
        SetText(criticalDamageText, "");
        SetText(staminaCostText, "");
        SetText(movementText, "");

        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
        }
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}
