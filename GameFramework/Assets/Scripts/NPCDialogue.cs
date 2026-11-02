using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("UI")]
    public GameObject speechBubble;
    public Text dialogueText;
    public GameObject interactText;

    [Header("Player")]
    public string playerTag = "Player";

    private GameObject player;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;

    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;

    private int currentLine = 0;

    [Header("Typing Settings")]
    public float typingSpeed = 0.04f;

    private Coroutine typingCoroutine;

    [Header("Camera")]
    public DialogueCamera dialogueCamera;

    // 플레이어의 물리 설정 저장
    private RigidbodyConstraints2D originalConstraints;
    private float originalGravity;

    private void Start()
    {
        speechBubble.SetActive(false);
        interactText.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange && !isDialogueActive)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        player = other.gameObject;

        playerRb = player.GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<PlayerMovement>();

        playerInRange = true;

        if (!isDialogueActive)
            interactText.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject != player)
            return;

        playerInRange = false;

        if (!isDialogueActive)
        {
            interactText.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        isDialogueActive = true;
        currentLine = 0;

        interactText.SetActive(false);
        speechBubble.SetActive(true);

        LockPlayer();

        if (dialogueCamera != null)
        {
            dialogueCamera.StartDialogueCamera(
                player.transform,
                transform
            );
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(
            TypeDialogue(dialogueLines[currentLine])
        );
    }

    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void NextDialogue()
    {
        // 대사가 출력 중이라면 즉시 완성
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueLines[currentLine];

            isTyping = false;
            typingCoroutine = null;

            return;
        }

        currentLine++;

        // 마지막 대사까지 출력했다면 종료
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowDialogue();
    }

    private void LockPlayer()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;
            originalGravity = playerRb.gravityScale;

            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = 0f;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void UnlockPlayer()
    {
        if (playerRb != null)
        {
            playerRb.constraints = originalConstraints;
            playerRb.gravityScale = originalGravity;
            playerRb.linearVelocity = Vector2.zero;
        }

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        speechBubble.SetActive(false);

        UnlockPlayer();

        if (dialogueCamera != null)
        {
            dialogueCamera.EndDialogueCamera();
        }

        if (playerInRange)
            interactText.SetActive(true);
    }
}