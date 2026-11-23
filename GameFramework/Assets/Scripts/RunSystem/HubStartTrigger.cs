using UnityEngine;

public class HubStartTrigger : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어 태그 검사
        if (!HasPlayerTag(other.transform))
            return;

        if (RunManager.Instance == null)
            return;

        Debug.Log("거점 이탈! 새로운 RUN 시작!");

        RunManager.Instance.StartRun();
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
