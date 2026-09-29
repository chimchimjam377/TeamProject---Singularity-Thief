using UnityEngine;

public class EvidenceObject : MonoBehaviour
{
    [Header("단서 정보")]
    public string evidenceName;

    [TextArea(3, 5)]
    public string evidenceDescription;

    [Header("단서 UI")]
    public EvidenceUI evidenceUI;

    [Header("조사 카메라")]
    public EvidenceInspectCamera inspectCamera;
    public Transform inspectPoint;

    // 단서 조사
    public void Interact()
    {
        Debug.Log("단서 발견: " + evidenceName);

        // 단서 이름 / 설명 표시
        if (evidenceUI != null)
        {
            evidenceUI.ShowEvidence(
                evidenceName,
                evidenceDescription
            );
        }

        // 조사 카메라 실행
        if (inspectCamera != null && inspectPoint != null)
        {
            inspectCamera.Inspect(
                inspectPoint,
                transform
            );
        }
        else
        {
            Debug.LogError(
                "Inspect Camera 또는 Inspect Point가 연결되지 않았습니다."
            );
        }
    }
}