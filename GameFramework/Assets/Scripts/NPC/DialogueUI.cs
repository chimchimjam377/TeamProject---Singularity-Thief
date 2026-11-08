using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("대화 UI")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("텍스트")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("초상화")]
    [SerializeField] private Image portraitImage;

    private NPCProfile currentProfile;
    private int currentLineIndex;

    private Action onCompleted;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        dialoguePanel.SetActive(false);
        IsOpen = false;
    }

    public void BeginDialogue(
        NPCProfile profile,
        Action completed)
    {
        if (profile == null)
            return;

        currentProfile = profile;
        onCompleted = completed;

        currentLineIndex = 0;

        if (profile.DialogueLines == null ||
            profile.DialogueLines.Length == 0)
        {
            Debug.LogWarning(
                $"{profile.NPCName}의 대사가 없습니다."
            );

            EndDialogue();
            return;
        }

        IsOpen = true;

        dialoguePanel.SetActive(true);

        speakerNameText.text = profile.NPCName;

        portraitImage.sprite =
            profile.PortraitSprite;

        portraitImage.enabled =
            profile.PortraitSprite != null;

        ShowCurrentLine();
    }

    public void Advance()
    {
        if (!IsOpen)
            return;

        currentLineIndex++;

        if (currentLineIndex >=
            currentProfile.DialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text =
            currentProfile.DialogueLines[
                currentLineIndex
            ];
    }

    private void EndDialogue()
    {
        IsOpen = false;

        dialoguePanel.SetActive(false);

        currentProfile = null;

        Action callback = onCompleted;
        onCompleted = null;

        callback?.Invoke();
    }
}