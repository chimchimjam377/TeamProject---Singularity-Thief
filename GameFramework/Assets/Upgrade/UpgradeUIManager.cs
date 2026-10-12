using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class UpgradeUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private PlayerUpgrades playerUpgrades;

    [Header("Center Info Panel")]
    [SerializeField] private TextMeshProUGUI selectedPartNameText; // TMP용 변수
    [SerializeField] private TextMeshProUGUI currentLevelText;     // TMP용 변수
    [SerializeField] private TextMeshProUGUI costText;             // TMP용 변수
    [SerializeField] private Button doUpgradeButton;

    private BodyPart currentSelectedPart;

    private void Start()
    {
        // uiPanel.SetActive(false); // 테스트를 위해 잠시 주석 처리하거나 지우셔도 됩니다.
        doUpgradeButton.onClick.AddListener(OnDoUpgradeButtonClicked);
    }

    public void OpenUI()
    {
        uiPanel.SetActive(true);
        SelectBodyPart((int)BodyPart.Head);
    }

    public void CloseUI()
    {
        uiPanel.SetActive(false);
    }

    public void SelectBodyPart(int partIndex)
    {
        currentSelectedPart = (BodyPart)partIndex;
        UpdateCenterPanel();
    }

    private void UpdateCenterPanel()
    {
        var data = playerUpgrades.GetUpgradeData(currentSelectedPart);

        selectedPartNameText.text = GetPartNameInKorean(currentSelectedPart);

        if (data.currentLevel >= data.maxLevel)
        {
            currentLevelText.text = $"Lv. MAX";
            costText.text = "강화 완료";
            doUpgradeButton.interactable = false;
        }
        else
        {
            currentLevelText.text = $"Lv. {data.currentLevel}";
            costText.text = $"필요한 재화: {data.GetUpgradeCost()}";
            doUpgradeButton.interactable = true;
        }
    }

    private void OnDoUpgradeButtonClicked()
    {
        if (playerUpgrades.TryUpgrade(currentSelectedPart))
        {
            UpdateCenterPanel();
        }
    }

    private string GetPartNameInKorean(BodyPart part)
    {
        return part switch
        {
            BodyPart.Head => "머리",
            BodyPart.LeftArm => "왼팔",
            BodyPart.RightArm => "오른팔",
            BodyPart.LeftLeg => "왼다리",
            BodyPart.RightLeg => "오른다리",
            _ => "알 수 없음"
        };
    }
    private void Update()
    {
        // UI 패널이 켜져 있을 때 ESC 키를 누르면 닫힘
        if (uiPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }
}