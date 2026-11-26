using UnityEngine;
using System.Collections;

public class DialogueCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;

    public float zoomSize = 3.5f;
    public float zoomSpeed = 3f;

    private float originalSize;
    private Vector3 originalPosition;

    private Coroutine cameraCoroutine;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        originalSize = mainCamera.orthographicSize;
        originalPosition = transform.position;
    }

    public void StartDialogueCamera(Transform player, Transform npc)
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        Vector3 focusPosition = (player.position + npc.position) / 2f;

        focusPosition.z = originalPosition.z;

        originalPosition = transform.position;
        originalSize = mainCamera.orthographicSize;

        cameraCoroutine = StartCoroutine(
            MoveCamera(focusPosition, zoomSize)
        );
    }

    public void EndDialogueCamera()
    {
        if (cameraCoroutine != null)
            StopCoroutine(cameraCoroutine);

        cameraCoroutine = StartCoroutine(
            MoveCamera(originalPosition, originalSize)
        );
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, float targetSize)
    {
        while (
            Vector3.Distance(transform.position, targetPosition) > 0.01f ||
            Mathf.Abs(mainCamera.orthographicSize - targetSize) > 0.01f
        )
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * zoomSpeed
            );

            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetSize,
                Time.deltaTime * zoomSpeed
            );

            yield return null;
        }

        transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;

        cameraCoroutine = null;
    }
}