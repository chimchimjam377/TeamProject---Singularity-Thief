using System.Collections;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 0.5f;

    [Header("Invincibility Window")]
    [SerializeField] private float invincibleStartTime = 0.05f;
    [SerializeField] private float invincibleEndTime = 0.25f;

    [Header("Reflect Window")]
    [SerializeField] private float reflectStartTime = 0.12f;
    [SerializeField] private float reflectEndTime = 0.20f;

    [Header("Colliders")]
    [SerializeField] private Collider dodgeCollider;
    [SerializeField] private Collider reflectCollider;

    private CharacterController controller;

    private float lastDodgeTime;

    public bool IsDodging { get; private set; }
    public bool IsInvincible { get; private set; }
    public bool CanReflect { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        SetDodgeCollider(false);
        SetReflectCollider(false);
    }

    private void Update()
    {
        if (!playerInput.DodgePressed)
            return;

        TryDodge();
    }

    private void TryDodge()
    {
        if (IsDodging)
            return;

        if (Time.time <
            lastDodgeTime + dodgeCooldown)
            return;

        lastDodgeTime = Time.time;

        playerInput.ConsumeDodge();

        Vector2 input =
            playerInput.MoveInput;

        Vector3 direction;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 forward =
                Camera.main.transform.forward;

            Vector3 right =
                Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            direction =
                forward * input.y +
                right * input.x;

            direction.Normalize();
        }
        else
        {
            direction =
                transform.forward;
        }

        StartCoroutine(
            DodgeRoutine(direction)
        );
    }

    private IEnumerator DodgeRoutine(
        Vector3 direction)
    {
        IsDodging = true;

        float timer = 0f;

        while (timer < dodgeDuration)
        {
            UpdateDodgeState(timer);

            controller.Move(
                direction *
                dodgeSpeed *
                Time.deltaTime
            );

            timer += Time.deltaTime;

            yield return null;
        }

        // 닷지 종료 시 전부 비활성화
        IsInvincible = false;
        CanReflect = false;

        SetDodgeCollider(false);
        SetReflectCollider(false);

        IsDodging = false;
    }

    private void UpdateDodgeState(float timer)
    {
        bool invincible =
            timer >= invincibleStartTime &&
            timer < invincibleEndTime;

        bool reflect =
            timer >= reflectStartTime &&
            timer < reflectEndTime;

        SetInvincible(invincible);
        SetReflect(reflect);
    }

    private void SetInvincible(bool active)
    {
        if (IsInvincible == active)
            return;

        IsInvincible = active;

        SetDodgeCollider(active);
    }

    private void SetReflect(bool active)
    {
        if (CanReflect == active)
            return;

        CanReflect = active;

        SetReflectCollider(active);
    }

    private void SetDodgeCollider(bool active)
    {
        if (dodgeCollider != null)
        {
            dodgeCollider.enabled = active;
        }
    }

    private void SetReflectCollider(bool active)
    {
        if (reflectCollider != null)
        {
            reflectCollider.enabled = active;
        }
    }
}