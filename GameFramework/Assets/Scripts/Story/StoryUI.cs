
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryUI : MonoBehaviour
{
    [Header("화면")]
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

    // 캐릭터 런타임 데이터
    private readonly GameObject[] characterPrefabs = new GameObject[3];
    private readonly GameObject[] characterInstances = new GameObject[3];
    private readonly Animator[] characterAnimators = new Animator[3];

    // CG 런타임 데이터
    private GameObject currentCGPrefab;
    private GameObject currentCGInstance;
    private Animator currentCGAnimator;

    private readonly bool[] characterVisibilityBeforeCG = new bool[3];

    private bool hideCharactersForCG;
    private bool isCGShowing;

    private void Awake()
    {
        Close();
    }

    // =========================
    // 화면 관리
    // =========================

    public void Open()
    {
        storyCanvasRoot.SetActive(true);
        visualStageRoot.SetActive(true);

        ClearVisuals();

        dialogueRoot.SetActive(false);
    }

    public void Close()
    {
        storyCanvasRoot.SetActive(false);
        visualStageRoot.SetActive(false);

        dialogueRoot.SetActive(false);

        if (nextIndicator != null)
            nextIndicator.SetActive(false);
    }

    private void ClearVisuals()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = null;
            backgroundRenderer.enabled = false;
        }

        for (int i = 0; i < characterInstances.Length; i++)
        {
            if (characterInstances[i] != null)
                characterInstances[i].SetActive(false);
        }

        if (currentCGInstance != null)
            currentCGInstance.SetActive(false);

        isCGShowing = false;
        hideCharactersForCG = false;
    }

    // =========================
    // 배경
    // =========================

    public void SetBackground(Sprite sprite)
    {
        if (backgroundRenderer == null)
            return;

        backgroundRenderer.sprite = sprite;
        backgroundRenderer.enabled = sprite != null;
    }

    // =========================
    // 캐릭터
    // =========================

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

        // 다른 캐릭터라면 프리팹 교체
        if (characterPrefabs[index] != prefab ||
            characterInstances[index] == null)
        {
            if (characterInstances[index] != null)
            {
                characterInstances[index].SetActive(false);
                Destroy(characterInstances[index]);
            }

            characterPrefabs[index] = prefab;

            characterInstances[index] =
                Instantiate(prefab, parent, false);

            characterAnimators[index] =
                characterInstances[index].GetComponentInChildren<Animator>(true);

            characterInstances[index].transform.localPosition =
                Vector3.zero;

            characterInstances[index].transform.localRotation =
                Quaternion.identity;
        }

        GameObject character = characterInstances[index];

        character.SetActive(true);

        // 좌우 반전
        Vector3 scale = character.transform.localScale;

        scale.x = Mathf.Abs(scale.x) *
            (flipHorizontal ? -1f : 1f);

        character.transform.localScale = scale;

        // 애니메이션 재생
        PlayAnimation(
            characterAnimators[index],
            animationStateName,
            "캐릭터 " + slot
        );
    }

    public void HideCharacter(StoryCharacterSlot slot)
    {
        int index = (int)slot;

        if (characterInstances[index] != null)
            characterInstances[index].SetActive(false);
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

    // =========================
    // CG
    // =========================

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

        // 다른 CG라면 교체
        if (currentCGPrefab != prefab ||
            currentCGInstance == null)
        {
            if (currentCGInstance != null)
            {
                currentCGInstance.SetActive(false);
                Destroy(currentCGInstance);
            }

            currentCGPrefab = prefab;

            currentCGInstance =
                Instantiate(prefab, cgRoot, false);

            currentCGAnimator =
                currentCGInstance.GetComponentInChildren<Animator>(true);

            currentCGInstance.transform.localPosition = Vector3.zero;
            currentCGInstance.transform.localRotation = Quaternion.identity;
        }

        currentCGInstance.SetActive(true);

        if (!isCGShowing)
        {
            hideCharactersForCG = hideCharacters;

            // CG 출력 전 캐릭터 상태 저장
            for (int i = 0; i < characterInstances.Length; i++)
            {
                GameObject character = characterInstances[i];

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
            for (int i = 0; i < characterInstances.Length; i++)
            {
                if (characterInstances[i] != null)
                {
                    characterInstances[i].SetActive(
                        characterVisibilityBeforeCG[i]
                    );
                }
            }
        }

        hideCharactersForCG = false;
        isCGShowing = false;
    }

    // =========================
    // 애니메이션 재생
    // =========================

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
        {
            Debug.LogWarning(
                targetName + "에 Animator Controller가 없습니다."
            );

            return;
        }

        // Inspector에서 Happy만 입력해도 작동하도록 처리
        string fullStateName = stateName.Contains(".")
            ? stateName
            : "Base Layer." + stateName;

        int stateHash = Animator.StringToHash(fullStateName);

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

    // =========================
    // 대사
    // =========================

    public void ShowDialogue(
        string speakerName,
        string dialogue)
    {
        dialogueRoot.SetActive(true);

        speakerNameText.text = speakerName;
        dialogueText.text = dialogue;

        if (nextIndicator != null)
            nextIndicator.SetActive(true);
    }
}
