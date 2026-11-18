using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewStory",
    menuName = "Game/Story/Story Data"
)]
public class StoryData : ScriptableObject
{
    [Header("스토리 명령 목록")]
    [SerializeField]
    private List<StoryStep> steps = new List<StoryStep>();

    public IReadOnlyList<StoryStep> Steps => steps;
}