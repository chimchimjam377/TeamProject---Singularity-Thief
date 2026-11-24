using UnityEngine;

public class HubStartTrigger : MonoBehaviour
{
    [Header("UI Controller")]
    [SerializeField]
    private HubOperationUIController operationUI;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!HasPlayerTag(other.transform))
            return;

        if (operationUI == null)
        {
            Debug.LogError("HubOperationUIController가 연결되지 않았습니다.");
            return;
        }

        Debug.Log("안전 구역 이탈! 작전 시작 확인 요청");

        operationUI.ShowConfirmPopup();
    }

    private bool HasPlayerTag(Transform target)
    {
        while (target != null)
        {
            if (target.CompareTag("Player"))
                return true;

            target = target.parent;
        }

        return false;
    }
}