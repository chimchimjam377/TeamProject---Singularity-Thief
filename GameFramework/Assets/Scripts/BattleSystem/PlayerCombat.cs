using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private AttackHitbox attackHitbox;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

    [Header("Attack Data")]
    [SerializeField] private AttackComboData comboData;

    private int currentAttackIndex = -1;

    private bool isAttacking;
    private bool comboBuffered;
    private bool hitboxActive;

    // 1 = 오른쪽
    // -1 = 왼쪽
    private int attackDirection = 1;

    private void Update()
    {
        HandleInput();

        if (isAttacking)
        {
            UpdateAttack();
        }
    }

    // =========================================================
    // Input
    // =========================================================

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (!isAttacking)
        {
            if (CanStartAttack())
            {
                StartAttack(0);
            }

            return;
        }

        TryQueueNextAttack();
    }

    private bool CanStartAttack()
    {
        if (isAttacking)
            return false;

        return playerState.CanAttack();
    }

    // =========================================================
    // Direction
    // =========================================================

    private void UpdateAttackDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        attackDirection =
            mouseWorldPosition.x >= transform.position.x
                ? 1
                : -1;

        UpdateVisualDirection();
    }

    private void UpdateVisualDirection()
    {
        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;

        scale.x =
            Mathf.Abs(scale.x) * attackDirection;

        visualRoot.localScale = scale;
    }

    // =========================================================
    // Start Attack
    // =========================================================

    private void StartAttack(int attackIndex)
    {
        if (comboData == null)
            return;

        if (comboData.Attacks.Count == 0)
            return;

        currentAttackIndex = attackIndex;

        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        // 공격 시작 시 마우스 방향 확인
        UpdateAttackDirection();

        isAttacking = true;
        comboBuffered = false;

        SetHitbox(false);

        playerState.SetState(
            PlayerStateType.Attack
        );

        animator.Play(
            attack.animatorStateName,
            0,
            0f
        );
    }

    // =========================================================
    // Attack Update
    // =========================================================

    private void UpdateAttack()
    {
        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(
            attack.animatorStateName))
        {
            return;
        }

        float normalizedTime =
            stateInfo.normalizedTime;

        // -----------------------------------------
        // Hitbox ON
        // -----------------------------------------

        if (!hitboxActive &&
            normalizedTime >= attack.hitboxStart &&
            normalizedTime < attack.hitboxEnd)
        {
            attackHitbox.Activate(
                attack.damage,
                gameObject
            );

            hitboxActive = true;
        }

        // -----------------------------------------
        // Hitbox Shape Update
        // -----------------------------------------

        if (hitboxActive)
        {
            UpdateHitboxShape(
                attack,
                normalizedTime
            );
        }

        // -----------------------------------------
        // Hitbox OFF
        // -----------------------------------------

        if (hitboxActive &&
            normalizedTime >= attack.hitboxEnd)
        {
            SetHitbox(false);
        }

        // -----------------------------------------
        // Animation End
        // -----------------------------------------

        if (normalizedTime >= 1f)
        {
            SetHitbox(false);

            if (comboBuffered)
            {
                int nextAttack =
                    (currentAttackIndex + 1)
                    % comboData.Attacks.Count;

                StartAttack(nextAttack);
            }
            else
            {
                EndAttack();
            }
        }
    }

    // =========================================================
    // Hitbox Shape
    // =========================================================

    private void UpdateHitboxShape(
        AttackComboData.AttackStep attack,
        float normalizedTime)
    {
        float width =
            attack.hitboxWidth.Evaluate(
                normalizedTime
            );

        float height =
            attack.hitboxHeight.Evaluate(
                normalizedTime
            );

        float offsetX =
            attack.hitboxOffsetX.Evaluate(
                normalizedTime
            );

        float offsetY =
            attack.hitboxOffsetY.Evaluate(
                normalizedTime
            );

        attackHitbox.SetShape(
            width,
            height,
            offsetX,
            offsetY,
            attackDirection
        );
    }

    // =========================================================
    // Combo
    // =========================================================

    private void TryQueueNextAttack()
    {
        if (currentAttackIndex < 0)
            return;

        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(
            attack.animatorStateName))
        {
            return;
        }

        float normalizedTime =
            stateInfo.normalizedTime;

        if (normalizedTime >= attack.comboInputStart &&
            normalizedTime <= attack.comboInputEnd)
        {
            comboBuffered = true;
        }
    }

    // =========================================================
    // Hitbox
    // =========================================================

    private void SetHitbox(bool active)
    {
        hitboxActive = active;

        if (attackHitbox == null)
            return;

        if (active)
        {
            AttackComboData.AttackStep attack =
                comboData.Attacks[currentAttackIndex];

            attackHitbox.Activate(
                attack.damage,
                gameObject
            );
        }
        else
        {
            attackHitbox.Deactivate();
        }
    }

    // =========================================================
    // End
    // =========================================================

    private void EndAttack()
    {
        SetHitbox(false);

        isAttacking = false;
        comboBuffered = false;
        currentAttackIndex = -1;

        playerState.SetState(
            PlayerStateType.Normal
        );

        animator.Play("Idle");
    }
}