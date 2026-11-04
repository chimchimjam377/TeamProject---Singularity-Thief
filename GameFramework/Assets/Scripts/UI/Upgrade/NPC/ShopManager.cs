using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ShopManager : MonoBehaviour
{
    [Header("데이터 및 프리팹")]
    public EquipmentLibrary shopLibrary;
    public GameObject equipmentSlotPrefab;

    // 💡 강화 비용과 현재 레벨을 가져오기 위해 플레이어 업그레이드 스크립트 연결
    public PlayerUpgrades playerUpgrades;

    [Header("중앙 리스트 UI")]
    public Transform listContainer;

    [Header("좌측 상세 패널 UI")]
    public GameObject detailPanel;
    // 아이콘과 설명 이미지 변수는 데이터에서 삭제되었으므로 여기에서도 제거했습니다.
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailCostText;
    public TextMeshProUGUI detailStatText;

    private void Start()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    public void OnClickBodyPartButton(int typeIndex)
    {
        // 💡 EquipType 대신 BodyPart 열거형 사용
        BodyPart selectedType = (BodyPart)typeIndex;

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (EquipmentPartData equipData in shopLibrary.allEquipments)
        {
            // 💡 equipType 대신 bodyPart로 비교
            if (equipData.bodyPart == selectedType)
            {
                GameObject slotGO = Instantiate(equipmentSlotPrefab, listContainer);
                EquipmentSlotUI slotUI = slotGO.GetComponent<EquipmentSlotUI>();

                if (slotUI != null)
                {
                    slotUI.InitSlot(equipData, this);
                }
            }
        }
    }

    public void ShowDetailInfo(EquipmentPartData data)
    {
        detailPanel.SetActive(true);
        detailNameText.text = data.partName;

        // 💡 PlayerUpgrades 스크립트에서 진짜 '현재 레벨'과 '필요 재화'를 가져와서 출력
        if (playerUpgrades != null)
        {
            var upgradeData = playerUpgrades.GetUpgradeData(data.bodyPart);
            if (upgradeData != null)
            {
                detailCostText.text = $"현재 Lv: {upgradeData.currentLevel}\n강화 필요 재화(Ba): {upgradeData.GetUpgradeCost()}";
            }
        }
        else
        {
            detailCostText.text = "PlayerUpgrades 스크립트를 인스펙터에 연결해주세요!";
        }

        detailStatText.text = GenerateStatText(data);
    }

    private string GenerateStatText(EquipmentPartData data)
    {
        StringBuilder sb = new StringBuilder();

        if (data.healthPerLevel != 0) sb.Append($"HP {(data.healthPerLevel > 0 ? "+" : "")}{data.healthPerLevel} ");
        if (data.staminaPerLevel != 0) sb.Append($"SP {(data.staminaPerLevel > 0 ? "+" : "")}{data.staminaPerLevel} ");
        if (data.critChancePerLevel != 0) sb.Append($"CRP {(data.critChancePerLevel > 0 ? "+" : "")}{data.critChancePerLevel * 100}% ");
        if (data.critDamagePerLevel != 0) sb.Append($"CRD {(data.critDamagePerLevel > 0 ? "+" : "")}{data.critDamagePerLevel * 100}% ");
        if (data.slashPerLevel != 0) sb.Append($"베기 {(data.slashPerLevel > 0 ? "+" : "")}{data.slashPerLevel * 100}% ");
        if (data.piercePerLevel != 0) sb.Append($"찌르기 {(data.piercePerLevel > 0 ? "+" : "")}{data.piercePerLevel * 100}% ");
        if (data.bluntPerLevel != 0) sb.Append($"타격 {(data.bluntPerLevel > 0 ? "+" : "")}{data.bluntPerLevel * 100}% ");

        if (sb.Length == 0) return "스탯 변화 없음";

        return sb.ToString().TrimEnd();
    }
}