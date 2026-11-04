using UnityEngine;

[CreateAssetMenu(fileName = "NewBodyPartData", menuName = "Upgrade/Body Part Data")]
public class EquipmentPartData : ScriptableObject
{
    [Header("신체 부위 설정 (여기서 머리, 팔, 다리 선택)")]
    // 💡 핵심: EquipType이 아니라 PlayerUpgrades에 있는 BodyPart를 사용합니다!
    public BodyPart bodyPart;

    [Header("UI 표시 정보")]
    public string partName;     // 예: 머리, 왼팔

    [Header("1레벨업 당 스탯 증가량")]
    [Tooltip("체력 증가량")] public int healthPerLevel;
    [Tooltip("스태미나 증가량")] public float staminaPerLevel;

    [Space(10)]
    [Tooltip("크리티컬 확률 (0.01 = 1%)")] public float critChancePerLevel;
    [Tooltip("크리티컬 데미지 (0.1 = 10%)")] public float critDamagePerLevel;

    [Space(10)]
    [Tooltip("베기 데미지 증가")] public float slashPerLevel;
    [Tooltip("찌르기 데미지 증가")] public float piercePerLevel;
    [Tooltip("타격 데미지 증가")] public float bluntPerLevel;
}