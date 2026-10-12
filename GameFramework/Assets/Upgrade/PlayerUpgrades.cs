using System.Collections.Generic;
using UnityEngine;
using GameFramework.Gameplay; // InventoryManager 사용을 위한 네임스페이스

public enum BodyPart
{
    Head,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}

public class PlayerUpgrades : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeData
    {
        public BodyPart part;
        public int currentLevel = 0;
        public int maxLevel = 5;
        public int baseCost = 10; // 1번 누를때 10개씩 들어오니 기본 비용을 10으로 조정

        // 레벨에 따른 필요 재화 계산 (예: 10 -> 20 -> 30)
        public int GetUpgradeCost()
        {
            return baseCost + (currentLevel * baseCost);
        }
    }

    [Header("Upgrade Settings")]
    // Inventory_Demo에서 1번을 누를 때 들어오는 아이템 ID
    [SerializeField] private string currencyItemId = "Ba";
    [SerializeField] private List<UpgradeData> upgrades = new List<UpgradeData>();

    private void Awake()
    {
        if (upgrades.Count == 0)
        {
            upgrades.Add(new UpgradeData { part = BodyPart.Head });
            upgrades.Add(new UpgradeData { part = BodyPart.LeftArm });
            upgrades.Add(new UpgradeData { part = BodyPart.RightArm });
            upgrades.Add(new UpgradeData { part = BodyPart.LeftLeg });
            upgrades.Add(new UpgradeData { part = BodyPart.RightLeg });
        }
    }

    public UpgradeData GetUpgradeData(BodyPart part)
    {
        return upgrades.Find(u => u.part == part);
    }

    // 재화를 소모하여 실제 강화를 진행
    public bool TryUpgrade(BodyPart part)
    {
        UpgradeData data = GetUpgradeData(part);

        if (data == null || data.currentLevel >= data.maxLevel)
            return false;

        int cost = data.GetUpgradeCost();

        // InventoryManager를 통해 "Ba" 재화가 충분한지 확인
        if (InventoryManager.Instance.Has(currencyItemId, cost))
        {
            // 재화 소모 후 레벨 업
            InventoryManager.Instance.RemoveItem(currencyItemId, cost);
            data.currentLevel++;
            Debug.Log($"[{data.part}] 강화 성공! 현재 레벨: {data.currentLevel}");
            return true;
        }
        else
        {
            Debug.LogWarning($"재화가 부족합니다. 필요량: {cost}, 보유량: {InventoryManager.Instance.GetCount(currencyItemId)}");
            return false;
        }
    }
}