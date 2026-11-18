
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class StoryUI : MonoBehaviour
{
    [Header("스토리 전체 UI")]
    [SerializeField] private GameObject storyCanvasRoot;
    [SerializeField] private GameObject visualStageRoot;

    [Header("배경")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("캐릭터 위치")]
    [SerializeField] private Transform leftCharacterRoot;
    [SerializeField] private Transform centerCharacterRoot;
    [SerializeField] private Transform rightCharacterRoot;

    [Header("CG 위치")]
    [SerializeField] private Transform cgRoot;

    [Header("대화 UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject nextIndicator;

    // =====================================
    // 타이핑 설정
    // =====================================

    [Header("타이핑 설정")]
    [SerializeField] private float charactersPerSecond = 35f;

    [SerializeField] private int charactersPerSound = 2;

    [SerializeField] private AudioSource typingAudioSource;

    [SerializeField] private AudioClip defaultTypingSound;

    [Range(0f, 1f)]
    [SerializeField] private float typingSoundVolume = 0.7f;

    private Coroutine typingCoroutine;

    public bool IsTyping => typingCoroutine != null;



    [Header("타이핑 피치 설정")]

    [Range(0.5f, 2f)]
    [SerializeField] private float typingPitch = 1f;

    [Range(0f, 0.5f)]
    [SerializeField] private float pitchVariation = 0.08f;

    // =====================================
    // 캐릭터 강조 설정
    // =====================================

    [Header("캐릭터 강조 설정")]
    [SerializeField]
    private Color nonSpeakerTint =
        new Color(0.4f, 0.4f, 0.4f, 1f);

    [SerializeField] private int normalSortingOrder = 10;

    [SerializeField] private int speakerSortingOrder = 100;

    // 캐릭터 런타임 정보
    private class CharacterRuntime
    {
        public GameObject prefab;
        public GameObject instance;

        public Animator animator;

        public SortingGroup sortingGroup;

        public SpriteRenderer[] renderers;

        public Color[] originalColors;
    }

    private readonly CharacterRuntime[] characters =
    {
        new CharacterRuntime(),
        new CharacterRuntime(),
        new CharacterRuntime()
    };

    private readonly bool[] characterVisibilityBeforeCG =
        new bool[3];

    private bool currentHasSpeaker;
    private StoryCharacterSlot currentSpeakerSlot;

    // =====================================
    // CG 정보
    // =====================================

    private GameObject currentCGPrefab;
    private GameObject currentCGInstance;
    private Animator currentCGAnimator;

    private bool hideCharactersForCG;
    private bool isCGShowing;

    private void Awake()
    {
        Close();
    }

    // =====================================
    // 화면 관리
    // =====================================

    public void Open()
    {
        StopTyping();

        storyCanvasRoot.SetActive(true);
        visualStageRoot.SetActive(true);

        ClearVisuals();

        dialogueRoot.SetActive(false);
    }

    public void Close()
    {
        StopTyping();

        if (storyCanvasRoot != null)
            storyCanvasRoot.SetActive(false);

        if (visualStageRoot != null)
            visualStageRoot.SetActive(false);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        SetNextIndicator(false);
    }

    private void ClearVisuals()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = null;
            backgroundRenderer.enabled = false;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].instance != null)
                characters[i].instance.SetActive(false);
        }

        if (currentCGInstance != null)
            currentCGInstance.SetActive(false);

        currentHasSpeaker = false;
        isCGShowing = false;
        hideCharactersForCG = false;
    }

    // =====================================
    // 배경
    // =====================================

    public void SetBackground(Sprite sprite)
    {
        if (backgroundRenderer == null)
            return;

        backgroundRenderer.sprite = sprite;
        backgroundRenderer.enabled = sprite != null;
    }

    // =====================================
    // 캐릭터 표시
    // =====================================

    public void ShowCharacter(
        StoryCharacterSlot slot,
        GameObject prefab,
        string animationStateName,
        bool flipHorizontal)
    {
        if (prefab == null)
        {
            Debug.LogWarning("캐릭터 프리팹이 없습니다.");
            return;
        }

        int index = (int)slot;

        Transform parent = GetCharacterRoot(slot);

        if (parent == null)
            return;

        CharacterRuntime runtime = characters[index];

        // 프리팹이 변경되었거나 인스턴스가 없는 경우
        if (runtime.prefab != prefab || runtime.instance == null)
        {
            if (runtime.instance != null)
                Destroy(runtime.instance);

            runtime.prefab = prefab;

            runtime.instance =
                Instantiate(prefab, parent, false);

            runtime.animator =
                runtime.instance.GetComponentInChildren<Animator>(true);

            // 캐릭터 전체를 하나의 렌더 그룹으로 관리
            runtime.sortingGroup =
                runtime.instance.GetComponent<SortingGroup>();

            if (runtime.sortingGroup == null)
            {
                runtime.sortingGroup =
                    runtime.instance.AddComponent<SortingGroup>();
            }

            // 캐릭터 내부 SpriteRenderer 수집
            runtime.renderers =
                runtime.instance.GetComponentsInChildren<SpriteRenderer>(true);

            runtime.originalColors =
                new Color[runtime.renderers.Length];

            for (int i = 0; i < runtime.renderers.Length; i++)
            {
                runtime.originalColors[i] =
                    runtime.renderers[i].color;
            }

            runtime.instance.transform.localPosition =
                Vector3.zero;

            runtime.instance.transform.localRotation =
                Quaternion.identity;
        }

        runtime.instance.SetActive(true);

        // 좌우 반전
        Vector3 scale = runtime.instance.transform.localScale;

        scale.x = Mathf.Abs(scale.x) *
            (flipHorizontal ? -1f : 1f);

        runtime.instance.transform.localScale = scale;

        // 애니메이션 재생
        PlayAnimation(
            runtime.animator,
            animationStateName,
            "캐릭터 " + slot
        );

        // 현재 대사 강조 상태 적용
        ApplyCharacterFocus(
            currentHasSpeaker,
            currentSpeakerSlot
        );
    }

    public void HideCharacter(StoryCharacterSlot slot)
    {
        int index = (int)slot;

        if (characters[index].instance != null)
            characters[index].instance.SetActive(false);
    }

    private Transform GetCharacterRoot(StoryCharacterSlot slot)
    {
        switch (slot)
        {
            case StoryCharacterSlot.Left:
                return leftCharacterRoot;

            case StoryCharacterSlot.Center:
                return centerCharacterRoot;

            case StoryCharacterSlot.Right:
                return rightCharacterRoot;
        }

        return null;
    }

    // =====================================
    // 말하는 캐릭터 강조
    // =====================================

    private void ApplyCharacterFocus(
        bool hasSpeaker,
        StoryCharacterSlot speakerSlot)
    {
        currentHasSpeaker = hasSpeaker;
        currentSpeakerSlot = speakerSlot;

        for (int i = 0; i < characters.Length; i++)
        {
            CharacterRuntime runtime = characters[i];

            if (runtime.instance == null)
                continue;

            bool isSpeaker =
                hasSpeaker && i == (int)speakerSlot;

            // 말하지 않는 캐릭터는 어둡게
            Color tint = isSpeaker || !hasSpeaker
                ? Color.white
                : nonSpeakerTint;

            // 원래 색상을 기준으로 밝기 적용
            for (int j = 0; j < runtime.renderers.Length; j++)
            {
                runtime.renderers[j].color =
                    runtime.originalColors[j] * tint;
            }

            // 말하는 캐릭터를 가장 앞쪽으로
            if (runtime.sortingGroup != null)
            {
                runtime.sortingGroup.sortingOrder =
                    isSpeaker
                    ? speakerSortingOrder
                    : normalSortingOrder + i;
            }
        }
    }

    // =====================================
    // CG 표시
    // =====================================

    public void ShowCG(
        GameObject prefab,
        string animationStateName,
        bool hideCharacters)
    {
        if (prefab == null)
        {
            Debug.LogWarning("CG 프리팹이 없습니다.");
            return;
        }

        if (cgRoot == null)
            return;

        if (currentCGPrefab != prefab || currentCGInstance == null)
        {
            if (currentCGInstance != null)
                Destroy(currentCGInstance);

            currentCGPrefab = prefab;

            currentCGInstance =
                Instantiate(prefab, cgRoot, false);

            currentCGAnimator =
                currentCGInstance.GetComponentInChildren<Animator>(true);

            currentCGInstance.transform.localPosition =
                Vector3.zero;

            currentCGInstance.transform.localRotation =
                Quaternion.identity;
        }

        currentCGInstance.SetActive(true);

        if (!isCGShowing)
        {
            hideCharactersForCG = hideCharacters;

            for (int i = 0; i < characters.Length; i++)
            {
                GameObject character = characters[i].instance;

                characterVisibilityBeforeCG[i] =
                    character != null && character.activeSelf;

                if (hideCharactersForCG && character != null)
                    character.SetActive(false);
            }
        }

        isCGShowing = true;

        PlayAnimation(
            currentCGAnimator,
            animationStateName,
            "CG"
        );
    }

    public void HideCG()
    {
        if (currentCGInstance != null)
            currentCGInstance.SetActive(false);

        if (hideCharactersForCG)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].instance != null)
                {
                    characters[i].instance.SetActive(
                        characterVisibilityBeforeCG[i]
                    );
                }
            }
        }

        hideCharactersForCG = false;
        isCGShowing = false;
    }

    // =====================================
    // 애니메이션 재생
    // =====================================

    private void PlayAnimation(
        Animator animator,
        string stateName,
        string targetName)
    {
        if (animator == null)
        {
            Debug.LogWarning(
                targetName + "에 Animator가 없습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(stateName))
            return;

        if (animator.runtimeAnimatorController == null)
            return;

        string fullStateName = stateName.Contains(".")
            ? stateName
            : "Base Layer." + stateName;

        int stateHash =
            Animator.StringToHash(fullStateName);

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                targetName + "의 Animator에서 '" +
                fullStateName + "' 상태를 찾을 수 없습니다."
            );

            return;
        }

        animator.Play(stateHash, 0, 0f);
    }

    // =====================================
    // 대사 출력
    // =====================================

    public void ShowDialogue(
        string speakerName,
        string dialogue,
        AudioClip typingSound,
        bool hasSpeakerCharacter,
        StoryCharacterSlot speakerSlot)
    {
        StopTyping();

        // 말하는 캐릭터 강조
        ApplyCharacterFocus(
            hasSpeakerCharacter,
            speakerSlot
        );

        dialogueRoot.SetActive(true);

        speakerNameText.text = speakerName;
        dialogueText.text = dialogue;

        dialogueText.ForceMeshUpdate();

        // 처음에는 모든 글자를 숨김
        dialogueText.maxVisibleCharacters = 0;

        SetNextIndicator(false);

        AudioClip selectedSound =
            typingSound != null
            ? typingSound
            : defaultTypingSound;

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        if (totalCharacters == 0)
        {
            SetNextIndicator(true);
            return;
        }

        typingCoroutine = StartCoroutine(
            TypewriterRoutine(
                selectedSound,
                totalCharacters
            )
        );
    }

    // =====================================
    // 타이핑 효과
    // =====================================

    private IEnumerator TypewriterRoutine(
    AudioClip typingSound,
    int totalCharacters)
    {
        float interval = 1f / Mathf.Max(1f, charactersPerSecond);

        int soundCharacterCount = 0;

        int soundInterval = Mathf.Max(1, charactersPerSound);

        for (int i = 0; i < totalCharacters; i++)
        {
            // 한 글자씩 출력
            dialogueText.maxVisibleCharacters = i + 1;

            char currentCharacter =
                dialogueText.textInfo.characterInfo[i].character;

            // 공백은 효과음 카운트에서 제외
            if (!char.IsWhiteSpace(currentCharacter))
            {
                soundCharacterCount++;

                if (soundCharacterCount >= soundInterval)
                {
                    PlayTypingSound(typingSound);
                    soundCharacterCount = 0;
                }
            }

            // 다음 글자까지 대기
            if (i < totalCharacters - 1)
            {
                yield return new WaitForSecondsRealtime(interval);
            }
        }

        typingCoroutine = null;

        SetNextIndicator(true);
    }

    private void PlayTypingSound(AudioClip clip)
    {
        if (clip == null || typingAudioSource == null)
            return;

        // 이전 효과음이 재생 중이면 중복 재생하지 않음
        if (typingAudioSource.isPlaying)
            return;

        float randomPitch = Random.Range(
            -pitchVariation,
            pitchVariation
        );

        typingAudioSource.pitch = Mathf.Clamp(
            typingPitch + randomPitch,
            0.5f,
            2f
        );

        typingAudioSource.PlayOneShot(
            clip,
            typingSoundVolume
        );
    }

    // 타이핑을 즉시 완료
    public void CompleteTyping()
    {
        if (!IsTyping)
            return;

        StopTyping();

        dialogueText.ForceMeshUpdate();

        dialogueText.maxVisibleCharacters =
            dialogueText.textInfo.characterCount;

        SetNextIndicator(true);
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void SetNextIndicator(bool visible)
    {
        if (nextIndicator != null)
            nextIndicator.SetActive(visible);
    }
}
