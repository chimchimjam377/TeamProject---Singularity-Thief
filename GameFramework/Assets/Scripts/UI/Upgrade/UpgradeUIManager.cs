using UnityEngine;
using TMPro;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject uiPanel;
    public PlayerUpgrades playerUpgrades;

    [Header("Center Info Panel")]
    public TextMeshProUGUI selectedPartNameText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI costText;

    private BodyPart currentSelectedPart;

    // 유니티 버튼 OnClick에서 숫자를 넘겨받음 (0:Head, 1:LeftArm, 2:RightArm, 3:LeftLeg, 4:RightLeg)
    public void OnSelectBodyPart(int partIndex)
    {
        currentSelectedPart = (BodyPart)partIndex;
        UpdateUI();
    }

    public void UpdateUI()
    {
        PlayerUpgrades.UpgradeData data = playerUpgrades.GetUpgradeData(currentSelectedPart);
        EquipmentPartData soData = playerUpgrades.library.GetPartData(currentSelectedPart);

        if (data == null || soData == null) return;

        selectedPartNameText.text = $"부위 이름: {soData.partName}";
        currentLevelText.text = $"Lv: {data.currentLevel}";

        if (data.currentLevel >= data.maxLevel)
        {
            costText.text = "최대 레벨 도달";
        }
        else
        {
            costText.text = $"필요한 재화(Ba): {data.GetUpgradeCost()}";
        }
    }

    // 중앙의 '강화' 버튼 이벤트
    public void OnClickDoUpgrade()
    {
        bool isSuccess = playerUpgrades.TryUpgrade(currentSelectedPart);

        if (isSuccess)
        {
            UpdateUI(); // 재화가 소모되고 렙업했으니 텍스트 갱신
        }
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
    }
    public void OpenPanel()
    {
        uiPanel.SetActive(true);
    }
}