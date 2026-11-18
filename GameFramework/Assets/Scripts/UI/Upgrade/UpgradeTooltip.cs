using UnityEngine;
using TMPro;
using System.Text;

public class UpgradeTooltip : MonoBehaviour
{
    public static UpgradeTooltip Instance;

    [Header("툴팁 UI 연결")]
    [SerializeField] private TextMeshProUGUI partNameText;
    [SerializeField] private TextMeshProUGUI levelText;     // 💡 [추가됨] 레벨 텍스트 연결 변수
    [SerializeField] private TextMeshProUGUI statText;

    private void Awake()
    {
        Instance = this;
        ResetTooltip(); // 게임 시작 시 최초 1회만 비워둠
    }

    public void ShowTooltip(EquipmentPartData data)
    {
        if (data == null) return;

        partNameText.text = "장비 이름 : " + data.partName;

        // 💡 [추가됨] 현재 장비의 레벨을 가져와서 출력합니다.
        // (주의: data.level 변수명은 EquipmentPartData.cs에 작성된 변수명에 맞게 currentLevel 등으로 수정해야 할 수 있습니다.)
        levelText.text = "현재 단계 : Lv." + data.level;

        StringBuilder sb = new StringBuilder();

        if (data.healthPerLevel > 0) sb.Append($"최대 체력 +{data.healthPerLevel}\n");
        if (data.staminaPerLevel > 0) sb.Append($"최대 스태미나 +{data.staminaPerLevel}\n");
        if (data.critChancePerLevel > 0) sb.Append($"크리티컬 확률 +{data.critChancePerLevel * 100}%\n");
        if (data.critDamagePerLevel > 0) sb.Append($"크리티컬 데미지 +{data.critDamagePerLevel * 100}%\n");
        if (data.slashPerLevel > 0) sb.Append($"베기 피해 +{data.slashPerLevel * 100}%\n");
        if (data.piercePerLevel > 0) sb.Append($"찌르기 피해 +{data.piercePerLevel * 100}%\n");
        if (data.bluntPerLevel > 0) sb.Append($"타격 피해 +{data.bluntPerLevel * 100}%\n");

        statText.text = sb.ToString();
    }

    private void ResetTooltip()
    {
        if (partNameText != null) partNameText.text = "부위를 확인하세요";
        if (levelText != null) levelText.text = "현재 단계 : -"; // 💡 [추가됨] 초기화 텍스트
        if (statText != null) statText.text = "";
    }
}