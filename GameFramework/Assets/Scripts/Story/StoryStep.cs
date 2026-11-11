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

    [Header("캐릭터 표시")]
    [SerializeField] private StoryCharacterSlot characterSlot;
    [SerializeField] private Sprite characterSprite;
    [SerializeField] private bool flipHorizontal;

    [Header("CG 표시")]
    [SerializeField] private Sprite cgSprite;

    [SerializeField]
    private bool hideCharactersWhenShowingCG = true;

    [Header("대사")]
    [SerializeField] private string speakerName;

    [TextArea(3, 6)]
    [SerializeField] private string dialogueText;

    public StoryStepType StepType => stepType;

    public Sprite BackgroundSprite => backgroundSprite;

    public StoryCharacterSlot CharacterSlot => characterSlot;
    public Sprite CharacterSprite => characterSprite;
    public bool FlipHorizontal => flipHorizontal;

    public Sprite CGSprite => cgSprite;
    public bool HideCharactersWhenShowingCG =>
        hideCharactersWhenShowingCG;

    public string SpeakerName => speakerName;
    public string DialogueText => dialogueText;
}