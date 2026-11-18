using UnityEngine;

[CreateAssetMenu(
    fileName = "StageData",
    menuName = "Game/Run/Stage Data"
)]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    [SerializeField] private string stageDisplayName;

    [Header("씬 이름")]
    [SerializeField] private string sceneName;

    public string StageDisplayName => stageDisplayName;
    public string SceneName => sceneName;
}