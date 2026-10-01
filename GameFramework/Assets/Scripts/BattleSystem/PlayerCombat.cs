using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D attackHitbox;

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

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // 공격 중이 아니라면 첫 공격
        if (!isAttacking)
        {
            StartAttack(0);
            return;
        }

        // 현재 공격 중 다음 공격 예약
        if (CanQueueNextAttack())
        {
            comboBuffered = true;
        }
    }

    private void StartAttack(int attackIndex)
    {
        if (comboData == null || comboData.Attacks.Count == 0)
            return;

        currentAttackIndex = attackIndex;

        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        isAttacking = true;
        comboBuffered = false;

        SetHitbox(false);

        animator.Play(attack.animatorStateName, 0, 0f);
    }

    private void UpdateAttack()
    {
        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        // 현재 공격 애니메이션이 아직 재생되지 않은 상태라면 대기
        if (!stateInfo.IsName(attack.animatorStateName))
            return;

        float normalizedTime = stateInfo.normalizedTime;

        // --------------------------------------------------
        // Hitbox ON
        // --------------------------------------------------
        if (!hitboxActive &&
            normalizedTime >= attack.hitboxStart &&
            normalizedTime < attack.hitboxEnd)
        {
            SetHitbox(true);
        }

        // --------------------------------------------------
        // Hitbox OFF
        // --------------------------------------------------
        if (hitboxActive &&
            normalizedTime >= attack.hitboxEnd)
        {
            SetHitbox(false);
        }

        // --------------------------------------------------
        // Animation 종료
        // --------------------------------------------------
        if (normalizedTime >= 1f)
        {
            SetHitbox(false);

            if (comboBuffered)
            {
                int nextIndex =
                    (currentAttackIndex + 1) % comboData.Attacks.Count;

                StartAttack(nextIndex);
            }
            else
            {
                EndAttack();
            }
        }
    }

    private bool CanQueueNextAttack()
    {
        AttackComboData.AttackStep attack =
            comboData.Attacks[currentAttackIndex];

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(attack.animatorStateName))
            return false;

        float normalizedTime = stateInfo.normalizedTime;

        return normalizedTime >= attack.comboInputStart &&
               normalizedTime <= attack.comboInputEnd;
    }

    private void EndAttack()
    {
        isAttacking = false;
        comboBuffered = false;
        currentAttackIndex = -1;

        SetHitbox(false);

        // 일단 Idle로 복귀
        animator.Play("Idle");
    }

    private void SetHitbox(bool active)
    {
        hitboxActive = active;

        if (attackHitbox != null)
        {
            attackHitbox.enabled = active;
        }
    }
}