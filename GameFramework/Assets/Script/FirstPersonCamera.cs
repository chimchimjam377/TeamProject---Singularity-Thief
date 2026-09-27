using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 120f;

    public Transform playerBody;

    [HideInInspector]
    public bool canLook = true;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canLook)
            return;

        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        // 위아래
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        // 좌우
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // 조사 종료 후 플레이어 시점으로 복구
    public void ResetCamera()
    {
        xRotation = transform.localEulerAngles.x;

        if (xRotation > 180f)
            xRotation -= 360f;

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        canLook = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}