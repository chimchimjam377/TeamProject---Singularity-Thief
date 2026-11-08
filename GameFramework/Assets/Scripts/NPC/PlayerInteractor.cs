using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("탐색 설정")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionRadius = 1.8f;
    [SerializeField] private LayerMask npcLayer;

    [Header("입력")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    [SerializeField] private NPCInteractionUI interactionUI;
    [SerializeField] private DialogueUI dialogueUI;

    [Header("카메라")]
    [SerializeField]
    private DialogueCameraController dialogueCamera;

    [Header("플레이어 조작")]
    [SerializeField]
    private Behaviour[] playerControlsToDisable;

    private NPCInteractable currentTarget;

    private bool controlsLocked;
    private bool[] previousControlStates;

    private void Update()
    {
        // 대화 중
        if (dialogueUI != null &&
            dialogueUI.IsOpen)
        {
            if (Input.GetKeyDown(interactKey) ||
                Input.GetKeyDown(KeyCode.Return))
            {
                dialogueUI.Advance();
            }

            return;
        }

        NPCInteractable nearestNPC =
            FindNearestNPC();

        if (nearestNPC != currentTarget)
        {
            currentTarget = nearestNPC;

            if (currentTarget != null)
            {
                interactionUI.Show(
                    currentTarget
                );
            }
            else
            {
                interactionUI.Hide();
            }
        }

        if (currentTarget != null &&
            Input.GetKeyDown(interactKey))
        {
            StartConversation(
                currentTarget
            );
        }
    }

    private NPCInteractable FindNearestNPC()
    {
        Transform origin =
            interactionOrigin != null
            ? interactionOrigin
            : transform;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                origin.position,
                interactionRadius,
                npcLayer
            );

        NPCInteractable nearestNPC = null;

        float nearestDistance =
            float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            NPCInteractable npc =
                hit.GetComponentInParent<
                    NPCInteractable
                >();

            if (npc == null ||
                npc.Profile == null)
                continue;

            float distance =
                (
                    npc.transform.position -
                    origin.position
                ).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestNPC = npc;
            }
        }

        return nearestNPC;
    }

    private void StartConversation(
        NPCInteractable npc)
    {
        currentTarget = null;

        interactionUI.Hide();

        SetPlayerControls(false);

        if (dialogueCamera != null)
        {
            dialogueCamera.BeginDialogue(
                transform,
                npc.FocusPoint
            );
        }

        dialogueUI.BeginDialogue(
            npc.Profile,
            OnDialogueFinished
        );
    }

    private void OnDialogueFinished()
    {
        if (dialogueCamera != null)
        {
            dialogueCamera.EndDialogue(
                () => SetPlayerControls(true)
            );
        }
        else
        {
            SetPlayerControls(true);
        }
    }

    private void SetPlayerControls(
        bool enabled)
    {
        if (!enabled)
        {
            if (controlsLocked)
                return;

            previousControlStates =
                new bool[
                    playerControlsToDisable.Length
                ];

            for (
                int i = 0;
                i < playerControlsToDisable.Length;
                i++)
            {
                Behaviour control =
                    playerControlsToDisable[i];

                if (control == null)
                    continue;

                previousControlStates[i] =
                    control.enabled;

                control.enabled = false;
            }

            controlsLocked = true;
        }
        else
        {
            if (!controlsLocked)
                return;

            for (
                int i = 0;
                i < playerControlsToDisable.Length;
                i++)
            {
                Behaviour control =
                    playerControlsToDisable[i];

                if (control == null)
                    continue;

                control.enabled =
                    previousControlStates[i];
            }

            controlsLocked = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin =
            interactionOrigin != null
            ? interactionOrigin
            : transform;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            origin.position,
            interactionRadius
        );
    }
}