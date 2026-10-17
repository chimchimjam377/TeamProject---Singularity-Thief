using UnityEngine;
using GameFramework.Gameplay; // InventoryManager를 쓰기 위한 네임스페이스

[RequireComponent(typeof(CircleCollider2D))]
public class DroppedItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private string itemId = "Ba"; // 인벤토리에 들어갈 아이템 ID
    [SerializeField] private int amount = 5;       // 한 번에 획득할 개수

    private void Awake()
    {
        // 안전장치: 동그라미 콜라이더가 무조건 Trigger(통과 가능) 상태가 되도록 설정
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 닿은 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // 인벤토리 매니저가 존재하는지 확인 후 아이템 추가
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemId, amount);
                Debug.Log($"[아이템 획득] {itemId} {amount}개가 인벤토리에 추가됨!");
            }
            else
            {
                Debug.LogWarning("InventoryManager를 찾을 수 없습니다.");
            }

            // 플레이어가 먹었으므로 맵에서 삭제(Destroy)
            Destroy(gameObject);
        }
    }
}