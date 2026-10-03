using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private AttackHitbox attackHitbox;

    [Header("Attack Data")]
    [SerializeField] private AttackComboData comboData;

    private int currentAttackIndex = -1;

    private bool isAttacking;
    private bool comboBuffered;
    private bool hitboxActive;

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

        if (playerState.Is(PlayerStateType.Dodge))
            return false;

        if (playerState.Is(PlayerStateType.WallClimb))
            return false;

        if (playerState.Is(PlayerStateType.Hit))
            return false;

        if (playerState.Is(PlayerStateType.Dead))
            return false;

        return true;
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

        isAttacking = true;
        comboBuffered = false;

        SetHitbox(false);

        playerState.SetState(PlayerStateType.Attack);

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

        if (!stateInfo.IsName(attack.animatorStateName))
            return;

        float normalizedTime =
            stateInfo.normalizedTime;

        // Hitbox ON
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

        // Hitbox OFF
        if (hitboxActive &&
            normalizedTime >= attack.hitboxEnd)
        {
            SetHitbox(false);
        }

        // 다음 공격 입력
        if (!comboBuffered &&
            normalizedTime >= attack.comboInputStart &&
            normalizedTime <= attack.comboInputEnd)
        {
            // 입력은 Update에서 이미 들어왔을 수 있으므로
            // 별도의 입력 처리를 위해 여기서는 상태만 유지
        }

        // 애니메이션 종료
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

        if (!stateInfo.IsName(attack.animatorStateName))
            return;

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