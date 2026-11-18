using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "RunThemeData",
    menuName = "Game/Run/Theme Data"
)]
public class RunThemeData : ScriptableObject
{
    [Header("테마 정보")]
    [SerializeField] private string themeName;

    [Header("Stage 2 후보")]
    [SerializeField] private StageData[] stage2Candidates;

    [Header("Stage 3 후보")]
    [SerializeField] private StageData[] stage3Candidates;

    [Header("Stage 4 후보")]
    [SerializeField] private StageData[] stage4Candidates;

    [Header("Stage 5")]
    [SerializeField] private string shopSceneName;
    [SerializeField] private string bossSceneName;

    public string ThemeName => themeName;
    public string ShopSceneName => shopSceneName;
    public string BossSceneName => bossSceneName;

    // 스테이지 번호에 맞는 랜덤 후보 반환
    public StageData GetRandomStageData(int stageNumber)
    {
        StageData[] candidates = stageNumber switch
        {
            2 => stage2Candidates,
            3 => stage3Candidates,
            4 => stage4Candidates,
            _ => null
        };

        if (candidates == null || candidates.Length == 0)
        {
            Debug.LogError(
                $"{themeName}의 Stage {stageNumber} 후보가 없습니다."
            );

            return null;
        }

        // 비어 있지 않은 후보만 추출
        List<StageData> validCandidates = new List<StageData>();

        foreach (StageData data in candidates)
        {
            if (data != null &&
                !string.IsNullOrWhiteSpace(data.SceneName))
            {
                validCandidates.Add(data);
            }
        }

        if (validCandidates.Count == 0)
        {
            Debug.LogError(
                $"{themeName}의 Stage {stageNumber}에 유효한 씬 데이터가 없습니다."
            );

            return null;
        }

        int randomIndex = Random.Range(0, validCandidates.Count);

        return validCandidates[randomIndex];
    }
}