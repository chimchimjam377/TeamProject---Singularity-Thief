using System;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private StoryUI storyUI;

    [Header("스토리 데이터")]
    [SerializeField] private StoryData storyData;

    [Header("자동 재생")]
    [SerializeField] private bool playOnSceneStart = true;

    private StoryData currentStory;

    private int currentStepIndex;

    private bool isPlaying;
    private bool waitingForInput;

    private int startedFrame;

    private Action onStoryFinished;

    public bool IsPlaying => isPlaying;

    private void Start()
    {
        if (playOnSceneStart)
        {
            PlayAssignedStory();
        }
    }

    


    private void Update()
    {
        if (!isPlaying || !waitingForInput)
            return;

        // 스토리 시작과 같은 프레임의 입력은 무시
        if (Time.frameCount == startedFrame)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceStory();
        }
    }


    public void PlayAssignedStory()
    {
        if (storyData == null)
        {
            Debug.LogWarning("StoryData가 연결되지 않았습니다.");
            return;
        }

        PlayStory(storyData);
    }

    // 스토리 시작
    public void PlayStory(
        StoryData story,
        Action onFinished = null)
    {
        if (story == null)
        {
            Debug.LogWarning("StoryData가 없습니다.");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("이미 스토리가 재생 중입니다.");
            return;
        }

        currentStory = story;

        currentStepIndex = 0;

        isPlaying = true;
        waitingForInput = false;

        startedFrame = Time.frameCount;

        onStoryFinished = onFinished;

        storyUI.Open();

        ExecuteNextStep();
    }

    // 다음 대사
    public void AdvanceStory()
    {
        if (!isPlaying || !waitingForInput)
            return;

        // 아직 타이핑 중이라면 글자를 전부 출력
        if (storyUI.IsTyping)
        {
            storyUI.CompleteTyping();
            return;
        }

        // 타이핑이 끝난 상태라면 다음 명령 실행
        waitingForInput = false;

        ExecuteNextStep();
    }

    // 다음 명령 실행
    private void ExecuteNextStep()
    {
        while (currentStepIndex < currentStory.Steps.Count)
        {
            StoryStep step =
                currentStory.Steps[currentStepIndex];

            currentStepIndex++;

            switch (step.StepType)
            {
                case StoryStepType.ChangeBackground:

                    storyUI.SetBackground(
                        step.BackgroundSprite
                    );

                    break;

                case StoryStepType.ShowCharacter:

                    storyUI.ShowCharacter(
                        step.CharacterSlot,
                        step.CharacterPrefab,
                        step.CharacterAnimationStateName,
                        step.FlipHorizontal
                    );

                    break;

                case StoryStepType.HideCharacter:

                    storyUI.HideCharacter(
                        step.CharacterSlot
                    );

                    break;

                case StoryStepType.ShowCG:

                    storyUI.ShowCG(
                        step.CGPrefab,
                        step.CGAnimationStateName,
                        step.HideCharactersWhenShowingCG
                    );

                    break;

                case StoryStepType.HideCG:

                    storyUI.HideCG();

                    break;

                case StoryStepType.Dialogue:

                    storyUI.ShowDialogue(
                        step.SpeakerName,
                        step.DialogueText,
                        step.TypingSound,
                        step.HasSpeakerCharacter,
                        step.SpeakerSlot
                    );

                    waitingForInput = true;

                    return;

                case StoryStepType.EndStory:
                    EndStory(step.NextSceneName);
                    return;
            }
        }

        // 더 이상 실행할 명령이 없으면 종료
        FinishStory();
    }

    // 스토리 종료
    private void FinishStory()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        waitingForInput = false;

        storyUI.Close();

        currentStory = null;

        Action callback = onStoryFinished;
        onStoryFinished = null;

        callback?.Invoke();
    }

    // 강제 종료
    public void StopStory()
    {
        FinishStory();
    }



    private void EndStory(string nextSceneName)
    {
        isPlaying = false;
        waitingForInput = false;

        // 스토리 UI 종료
        storyUI.Close();

        // 다음 씬이 지정되어 있다면 이동
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
