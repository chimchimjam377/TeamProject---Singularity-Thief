using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryUI : MonoBehaviour
{
    [Header("스토리 전체 UI")]
    [SerializeField] private GameObject storyRoot;

    [Header("배경")]
    [SerializeField] private Image backgroundImage;

    [Header("캐릭터")]
    [SerializeField] private GameObject characterRoot;

    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image centerCharacter;
    [SerializeField] private Image rightCharacter;

    [Header("CG")]
    [SerializeField] private Image cgImage;

    [Header("대화 UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    private bool hideCharactersForCG;

    private void Awake()
    {
        Close();
    }

    public void Open()
    {
        storyRoot.SetActive(true);

        backgroundImage.sprite = null;
        backgroundImage.gameObject.SetActive(false);

        cgImage.sprite = null;
        cgImage.gameObject.SetActive(false);

        characterRoot.SetActive(true);

        leftCharacter.gameObject.SetActive(false);
        centerCharacter.gameObject.SetActive(false);
        rightCharacter.gameObject.SetActive(false);

        dialogueRoot.SetActive(false);

        hideCharactersForCG = false;
    }

    public void Close()
    {
        storyRoot.SetActive(false);
    }

    // 배경 변경
    public void SetBackground(Sprite sprite)
    {
        backgroundImage.sprite = sprite;
        backgroundImage.gameObject.SetActive(sprite != null);
    }

    // 캐릭터 표시
    public void ShowCharacter(
        StoryCharacterSlot slot,
        Sprite sprite,
        bool flipHorizontal)
    {
        Image target = GetCharacterImage(slot);

        if (target == null)
            return;

        target.sprite = sprite;

        if (sprite == null)
        {
            target.gameObject.SetActive(false);
            return;
        }

        target.preserveAspect = true;
        target.gameObject.SetActive(true);

        Vector3 scale = target.rectTransform.localScale;

        scale.x = Mathf.Abs(scale.x) *
            (flipHorizontal ? -1f : 1f);

        target.rectTransform.localScale = scale;
    }

    // 캐릭터 숨기기
    public void HideCharacter(StoryCharacterSlot slot)
    {
        Image target = GetCharacterImage(slot);

        if (target != null)
            target.gameObject.SetActive(false);
    }

    private Image GetCharacterImage(StoryCharacterSlot slot)
    {
        switch (slot)
        {
            case StoryCharacterSlot.Left:
                return leftCharacter;

            case StoryCharacterSlot.Center:
                return centerCharacter;

            case StoryCharacterSlot.Right:
                return rightCharacter;
        }

        return null;
    }

    // CG 표시
    public void ShowCG(Sprite sprite, bool hideCharacters)
    {
        if (sprite == null)
            return;

        hideCharactersForCG = hideCharacters;

        cgImage.sprite = sprite;
        cgImage.preserveAspect = true;
        cgImage.gameObject.SetActive(true);

        if (hideCharactersForCG)
        {
            characterRoot.SetActive(false);
        }
    }

    // CG 숨기기
    public void HideCG()
    {
        cgImage.gameObject.SetActive(false);

        if (hideCharactersForCG)
        {
            characterRoot.SetActive(true);
        }

        hideCharactersForCG = false;
    }

    // 대사 표시
    public void ShowDialogue(
        string speakerName,
        string dialogue)
    {
        dialogueRoot.SetActive(true);

        speakerNameText.text = speakerName;
        dialogueText.text = dialogue;
    }
}
