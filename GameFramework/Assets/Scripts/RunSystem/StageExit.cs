using UnityEngine;

public class StageExit : MonoBehaviour
{
    private bool isTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered)
            return;

        // 플레이어 태그 확인
        if (!HasPlayerTag(other.transform))
            return;

        Debug.Log("플레이어가 출구에 진입했습니다!");

        // RunManager 확인
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager를 찾을 수 없습니다.");
            return;
        }

        // 런 진행 상태 확인
        if (!RunManager.Instance.IsRunActive ||
            RunManager.Instance.CurrentPhase != RunPhase.Combat)
        {
            Debug.LogWarning("현재 전투 스테이지를 진행 중인 상태가 아닙니다.");
            return;
        }

        isTriggered = true;

        // 다음 스테이지로 이동
        RunManager.Instance.OnCombatStageCleared();
    }

    // 현재 Collider와 부모 오브젝트까지 태그 검사
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