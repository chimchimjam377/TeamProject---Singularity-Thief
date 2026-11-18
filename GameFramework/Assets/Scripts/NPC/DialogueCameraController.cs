using System;
using System.Collections;
using UnityEngine;

public class DialogueCameraController : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("기존 플레이어 카메라 추적 스크립트")]
    [SerializeField] private Behaviour followBehaviour;

    [Header("줌 설정")]
    [Range(0.2f, 1f)]
    [SerializeField] private float zoomMultiplier = 0.65f;

    [SerializeField] private float transitionDuration = 0.4f;

    [Header("카메라 집중 위치")]
    [SerializeField]
    private Vector2 focusOffset =
        new Vector2(0f, 0.5f);

    private Vector3 originalPosition;
    private float originalOrthoSize;

    private bool isInDialogue;
    private bool followWasEnabled;

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public void BeginDialogue(
        Transform player,
        Transform npc)
    {
        if (targetCamera == null ||
            player == null ||
            npc == null)
            return;

        if (!isInDialogue)
        {
            originalPosition =
                targetCamera.transform.position;

            originalOrthoSize =
                targetCamera.orthographicSize;

            if (followBehaviour != null)
            {
                followWasEnabled =
                    followBehaviour.enabled;

                followBehaviour.enabled = false;
            }

            isInDialogue = true;
        }

        Vector3 midpoint =
            (player.position + npc.position) * 0.5f;

        Vector3 focusPosition =
            new Vector3(
                midpoint.x + focusOffset.x,
                midpoint.y + focusOffset.y,
                originalPosition.z
            );

        float targetSize =
            originalOrthoSize *
            zoomMultiplier;

        StartTransition(
            focusPosition,
            targetSize,
            null
        );
    }

    public void EndDialogue(
        Action onComplete = null)
    {
        if (targetCamera == null ||
            !isInDialogue)
        {
            onComplete?.Invoke();
            return;
        }

        StartTransition(
            originalPosition,
            originalOrthoSize,
            () =>
            {
                if (followBehaviour != null)
                {
                    followBehaviour.enabled =
                        followWasEnabled;
                }

                isInDialogue = false;

                onComplete?.Invoke();
            }
        );
    }

    private void StartTransition(
        Vector3 targetPosition,
        float targetSize,
        Action onComplete)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(
                transitionCoroutine
            );
        }

        transitionCoroutine =
            StartCoroutine(
                TransitionRoutine(
                    targetPosition,
                    targetSize,
                    onComplete
                )
            );
    }

    private IEnumerator TransitionRoutine(
        Vector3 targetPosition,
        float targetSize,
        Action onComplete)
    {
        Vector3 startPosition =
            targetCamera.transform.position;

        float startSize =
            targetCamera.orthographicSize;

        float elapsed = 0f;

        float duration =
            Mathf.Max(
                transitionDuration,
                0.01f
            );

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            targetCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            targetCamera.orthographicSize =
                Mathf.Lerp(
                    startSize,
                    targetSize,
                    smoothT
                );

            yield return null;
        }

        targetCamera.transform.position =
            targetPosition;

        targetCamera.orthographicSize =
            targetSize;

        transitionCoroutine = null;

        onComplete?.Invoke();
    }
}