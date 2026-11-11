
using System;
using UnityEngine;

public enum StoryStepType
{
    ChangeBackground,
    ShowCharacter,
    HideCharacter,
    ShowCG,
    HideCG,
    Dialogue,
    EndStory
}

public enum StoryCharacterSlot
{
    Left,
    Center,
    Right
}

[Serializable]
public class StoryStep
{
    [Header("명령 종류")]
    [SerializeField] private StoryStepType stepType;

    [Header("배경 변경")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("캐릭터")]
    [SerializeField] private StoryCharacterSlot characterSlot;

    [SerializeField] private GameObject characterPrefab;

    [SerializeField] private string characterAnimationStateName;

    [SerializeField] private bool flipHorizontal;

    [Header("CG")]
    [SerializeField] private GameObject cgPrefab;

    [SerializeField] private string cgAnimationStateName;

    [SerializeField]
    private bool hideCharactersWhenShowingCG = true;

    [Header("대사")]
    [SerializeField] private string speakerName;

    [TextArea(3, 6)]
    [SerializeField] private string dialogueText;

    public StoryStepType StepType => stepType;

    public Sprite BackgroundSprite => backgroundSprite;

    public StoryCharacterSlot CharacterSlot => characterSlot;
    public GameObject CharacterPrefab => characterPrefab;
    public string CharacterAnimationStateName =>
        characterAnimationStateName;

    public bool FlipHorizontal => flipHorizontal;

    public GameObject CGPrefab => cgPrefab;
    public string CGAnimationStateName => cgAnimationStateName;

    public bool HideCharactersWhenShowingCG =>
        hideCharactersWhenShowingCG;

    public string SpeakerName => speakerName;
    public string DialogueText => dialogueText;
}
