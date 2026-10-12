using UnityEngine;

public class UpgradeNPC : MonoBehaviour
{
    [SerializeField] private UpgradeUIManager uiManager;
    private bool isPlayerInRange = false;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            uiManager.OpenUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // "F키를 눌러 강화하기" 같은 말풍선 UI를 띄울 수 있습니다.
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            uiManager.CloseUI(); // 멀어지면 UI 강제 닫기
        }
    }
}