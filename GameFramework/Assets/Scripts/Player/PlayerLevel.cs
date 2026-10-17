using UnityEngine;
using GameFramework.Gameplay; // InventoryManager 접근용

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevel = 10;

    [Header("Level Up Requirements")]
    [SerializeField] private string reqItem1 = "Ba";
    [SerializeField] private string reqItem2 = "Ka";

    // 레벨 1 -> 2로 갈 때 필요한 기본 개수
    [SerializeField] private int baseCost1 = 10;
    [SerializeField] private int baseCost2 = 10;

    // 매 프레임 확인하면 낭비이므로 0.5초마다 인벤토리를 확인하도록 타이머 설정
    private float checkTimer = 0f;
    private float checkInterval = 0.5f;

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        // 만렙이면 더 이상 검사하지 않음
        if (currentLevel >= maxLevel) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckLevelUp();
        }
    }

    private void CheckLevelUp()
    {
        if (InventoryManager.Instance == null) return;

        // 현재 레벨에 비례해서 필요량이 늘어남 (예: 1렙엔 10개, 2렙엔 20개...)
        int required1 = baseCost1 * currentLevel;
        int required2 = baseCost2 * currentLevel;

        // 두 아이템이 모두 필요량 이상 인벤토리에 있는지 확인
        if (InventoryManager.Instance.Has(reqItem1, required1) &&
            InventoryManager.Instance.Has(reqItem2, required2))
        {
            // 조건이 충족되면 아이템을 차감 (소모)
            InventoryManager.Instance.RemoveItem(reqItem1, required1);
            InventoryManager.Instance.RemoveItem(reqItem2, required2);

            // 레벨 상승
            currentLevel++;
            Debug.Log($"플레이어 레벨업! 현재 레벨: {currentLevel}");

            OnLevelUp();
        }
    }

    // 레벨업 시 발생하는 효과 처리
    private void OnLevelUp()
    {
        // 1. 시각적 효과 (임시)
        // Instantiate(levelUpEffectPrefab, transform.position, Quaternion.identity);

        // 2. 레벨업 시 체력 100% 회복
        if (playerStats != null)
        {
            playerStats.Heal(playerStats.MaxHealth);
            Debug.Log("레벨업으로 인해 체력이 전부 회복되었습니다!");
        }

        // 3. 만약 최대 체력을 늘리고 싶다면 PlayerStats 스크립트에 MaxHealth를 늘리는 public 함수를 만들어 여기서 호출하면 됩니다.
    }
}