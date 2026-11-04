using System.Collections.Generic;
using UnityEngine;
using GameFramework.Gameplay;

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
        public int baseCost = 10;

        public int GetUpgradeCost()
        {
            return baseCost + (currentLevel * baseCost);
        }
    }

    [Header("스탯 적용을 위한 SO 및 스크립트 연결")]
    public EquipmentLibrary library; // 스크립터블 오브젝트 라이브러리
    public PlayerStats playerStats;  // 플레이어 스탯 스크립트

    [Header("Upgrade Settings")]
    [SerializeField] private string currencyItemId = "Ba";
    [SerializeField] private List<UpgradeData> upgrades = new List<UpgradeData>();

    private void Awake()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();

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

    public bool TryUpgrade(BodyPart part)
    {
        UpgradeData data = GetUpgradeData(part);

        if (data == null || data.currentLevel >= data.maxLevel)
            return false;

        int cost = data.GetUpgradeCost();

        // InventoryManager를 통해 "Ba" 재화가 충분한지 확인
        if (InventoryManager.Instance.Has(currencyItemId, cost))
        {
            InventoryManager.Instance.RemoveItem(currencyItemId, cost);
            data.currentLevel++;
            Debug.Log($"[{data.part}] 강화 성공! 현재 레벨: {data.currentLevel}");

            // 💡 [추가됨] 강화 성공 시 SO 데이터를 읽어와서 스탯 갱신
            ApplyAllBonusesToPlayer();

            return true;
        }
        else
        {
            Debug.LogWarning($"재화가 부족합니다. 필요량: {cost}, 보유량: {InventoryManager.Instance.GetCount(currencyItemId)}");
            return false;
        }
    }

    // 💡 [추가됨] 모든 부위의 현재 레벨과 SO 스탯 증가량을 곱해서 합산
    private void ApplyAllBonusesToPlayer()
    {
        if (library == null || playerStats == null) return;

        int totalHp = 0;
        float totalSp = 0, totalCritC = 0, totalCritD = 0, totalSlash = 0, totalPierce = 0, totalBlunt = 0;

        foreach (var upgrade in upgrades)
        {
            // 라이브러리에서 이 부위(Head, LeftArm 등)에 해당하는 SO 데이터를 가져옴
            EquipmentPartData data = library.GetPartData(upgrade.part);

            if (data != null && upgrade.currentLevel > 0)
            {
                int level = upgrade.currentLevel;
                totalHp += data.healthPerLevel * level;
                totalSp += data.staminaPerLevel * level;
                totalCritC += data.critChancePerLevel * level;
                totalCritD += data.critDamagePerLevel * level;
                totalSlash += data.slashPerLevel * level;
                totalPierce += data.piercePerLevel * level;
                totalBlunt += data.bluntPerLevel * level;
            }
        }

        // 최종 합산된 스탯을 PlayerStats에 쏴줌
        playerStats.ApplyEquipmentBonuses(totalHp, totalSp, totalCritC, totalCritD, totalSlash, totalPierce, totalBlunt);
    }
}