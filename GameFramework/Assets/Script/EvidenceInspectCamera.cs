using UnityEngine;
using System.Collections;

public class EvidenceInspectCamera : MonoBehaviour
{
    [Header("조사 연출 시간")]
    public float inspectDuration = 2.0f;

    [Header("카메라 이동 시간")]
    public float moveDuration = 0.4f;

    [Header("참조")]
    public FirstPersonCamera firstPersonCamera;
    public PlayerMovement playerMovement;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool inspecting = false;

    void Start()
    {
        if (firstPersonCamera == null)
            firstPersonCamera = FindFirstObjectByType<FirstPersonCamera>();

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    public void Inspect(Transform inspectPoint, Transform evidenceTarget)
    {
        if (inspecting)
            return;

        if (inspectPoint == null || evidenceTarget == null)
        {
            Debug.LogError("InspectPoint 또는 EvidenceTarget이 없음");
            return;
        }

        inspecting = true;

        // 조사 시작 전에 현재 시점 저장
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // 플레이어 조작 잠금
        if (firstPersonCamera != null)
            firstPersonCamera.canLook = false;

        if (playerMovement != null)
            playerMovement.canMove = false;

        StartCoroutine(InspectRoutine(inspectPoint, evidenceTarget));
    }

    private IEnumerator InspectRoutine(
        Transform inspectPoint,
        Transform evidenceTarget)
    {
        // 조사 위치로 이동
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = inspectPoint.position;

        Vector3 direction =
            evidenceTarget.position - targetPosition;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        // 여기서 몇 초 동안 단서를 보여줌
        yield return new WaitForSeconds(inspectDuration);

        // 원래 시점으로 복귀
        yield return StartCoroutine(ReturnToOriginal());
    }

    private IEnumerator ReturnToOriginal()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    originalPosition,
                    t
                );

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    originalRotation,
                    t
                );

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // 플레이어 조작 다시 활성화
        if (playerMovement != null)
            playerMovement.canMove = true;

        if (firstPersonCamera != null)
            firstPersonCamera.canLook = true;

        inspecting = false;
    }

    public bool IsInspecting()
    {
        return inspecting;
    }
}