using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private PlayerComboSequence comboSequence;
    [SerializeField] private AttackMovementController attackMovementController;
    [SerializeField] private PlayerStats playerStats;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;



    [Header("Animation Settings")]
    [SerializeField] private string idleAnimatorStateName = "Idle";

    [SerializeField, Min(0.1f)]
    private float attackStateEntryTimeout = 0.75f;

    // 현재 공격 애니메이션 상태 확인
    private int expectedAttackStateHash;
    private float attackStartTime;
    private bool attackStateEntered;


    private AttackDefinition currentAttack;



    private int currentSequenceIndex = -1;
    private int attackDirection = 1;

    private bool isAttacking;
    private bool comboBuffered;
    private bool hitboxActive;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (comboSequence == null)
            comboSequence = GetComponent<PlayerComboSequence>();

        if (attackMovementController == null)
            attackMovementController =
                GetComponent<AttackMovementController>();

        if (comboSequence != null)
            comboSequence.Initialize();

        if (attackHitbox == null)
            Debug.LogError("AttackHitbox가 연결되지 않았습니다.", this);

        if (animator == null)
            Debug.LogError("Animator를 찾을 수 없습니다.", this);

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        
    }

    private void Update()
    {
        HandleInput();

        if (isAttacking)
            UpdateAttack();
    }

    // =====================================================
    // Input
    // =====================================================

    private void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // UI 위에서 클릭했다면 전투 입력으로 처리하지 않는다.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (comboSequence == null)
            return;

        // 공격 중이 아니라면 첫 번째 공격부터 시작한다.
        if (!isAttacking)
        {
            if (CanStartAttack() && comboSequence.Count > 0)
            {
                StartAttack(0);
            }

            return;
        }

        // 공격 중이라면 기존 콤보 입력 예약을 유지한다.
        TryQueueNextAttack();
    }

    private bool CanStartAttack()
    {
        return playerState != null &&
               playerState.Is(PlayerStateType.Normal);
    }

    // =====================================================
    // Direction
    // =====================================================

    private void UpdateAttackDirection()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        attackDirection =
            mouseWorldPosition.x >= transform.position.x ? 1 : -1;

        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;

        scale.x = Mathf.Abs(scale.x) * attackDirection;

        visualRoot.localScale = scale;
    }

    // =====================================================
    // Start Attack
    // =====================================================


    private bool StartAttack(int sequenceIndex)
    {
        if (comboSequence == null ||
            comboSequence.Count == 0)
            return false;

        if (sequenceIndex < 0 ||
            sequenceIndex >= comboSequence.Count)
            return false;

        if (animator == null || playerStats == null)
            return false;

        AttackDefinition attack =
            comboSequence.GetAttackAt(sequenceIndex);

        if (attack == null)
            return false;

        // 공격 시작 전에 스태미나 확인 및 소모
        if (!playerStats.TryConsumeStamina(attack.StaminaCost))
        {
            Debug.Log("스태미나 부족! 공격을 시작할 수 없습니다.");
            return false;
        }

        currentSequenceIndex = sequenceIndex;
        currentAttack = attack;

        UpdateAttackDirection();

        isAttacking = true;
        comboBuffered = false;

        // 애니메이션 상태 확인 초기화
        string stateName = currentAttack.AnimatorStateName;

        int lastDot = stateName.LastIndexOf('.');

        string shortStateName = lastDot >= 0
            ? stateName.Substring(lastDot + 1)
            : stateName;

        expectedAttackStateHash =
            Animator.StringToHash(shortStateName);

        attackStartTime = Time.time;
        attackStateEntered = false;

        SetHitbox(false);

        // 공격 이동 시작
        if (attackMovementController != null)
        {
            attackMovementController.BeginAttack(
                currentAttack,
                attackDirection
            );
        }

        playerState.SetState(PlayerStateType.Attack);

        animator.Play(
            currentAttack.AnimatorStateName,
            0,
            0f
        );

        return true;
    }


    // =====================================================
    // Attack Update
    // =====================================================


    private void UpdateAttack()
    {
        if (currentAttack == null || animator == null)
        {
            EndAttack();
            return;
        }

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        // 현재 재생 중인 상태가 공격 애니메이션인지 확인
        bool isExpectedState =
            stateInfo.shortNameHash == expectedAttackStateHash ||
            stateInfo.IsName(currentAttack.AnimatorStateName);

        if (!isExpectedState)
        {
            // 공격 애니메이션으로 전환 중인지 확인
            if (animator.IsInTransition(0))
            {
                AnimatorStateInfo nextState =
                    animator.GetNextAnimatorStateInfo(0);

                bool nextIsAttack =
                    nextState.shortNameHash == expectedAttackStateHash ||
                    nextState.IsName(currentAttack.AnimatorStateName);

                if (nextIsAttack)
                    return;
            }

            // 아직 공격 애니메이션에 진입하지 못한 경우
            if (!attackStateEntered)
            {
                if (Time.time - attackStartTime >= attackStateEntryTimeout)
                {
                    Debug.LogError(
                        $"공격 애니메이션을 찾지 못했습니다: " +
                        $"{currentAttack.AnimatorStateName}",
                        this
                    );

                    EndAttack();
                }

                return;
            }

            // 공격 애니메이션이 다른 상태로 빠져나갔다면 종료
            EndAttack();
            return;
        }

        attackStateEntered = true;

        float normalizedTime = stateInfo.normalizedTime;

        // 공격 이동 업데이트
        if (attackMovementController != null)
        {
            attackMovementController.UpdateAttackProgress(
                normalizedTime
            );
        }

        // 히트박스 활성 구간
        if (normalizedTime >= currentAttack.HitboxStart &&
            normalizedTime < currentAttack.HitboxEnd)
        {
            if (!hitboxActive)
            {
                ActivateCurrentHitbox();
            }

            UpdateHitboxShape(normalizedTime);
        }
        else if (hitboxActive &&
                 normalizedTime >= currentAttack.HitboxEnd)
        {
            SetHitbox(false);
        }

        // 공격 애니메이션 종료
        if (normalizedTime >= 1f)
        {
            SetHitbox(false);

            if (comboBuffered && comboSequence.Count > 0)
            {
                int nextIndex =
                    (currentSequenceIndex + 1) % comboSequence.Count;

                // 다음 공격의 스태미나가 부족하면 콤보 종료
                if (!StartAttack(nextIndex))
                {
                    EndAttack();
                }
            }
            else
            {
                EndAttack();
            }
        }
    }


    // =====================================================
    // Hitbox Shape
    // =====================================================

    private void UpdateHitboxShape(float normalizedTime)
    {
        float progress = Mathf.InverseLerp(
            currentAttack.HitboxStart,
            currentAttack.HitboxEnd,
            normalizedTime
        );

        float width =
            currentAttack.HitboxWidth.Evaluate(progress);

        float height =
            currentAttack.HitboxHeight.Evaluate(progress);

        float offsetX =
            currentAttack.HitboxOffsetX.Evaluate(progress);

        float offsetY =
            currentAttack.HitboxOffsetY.Evaluate(progress);

        attackHitbox.SetShape(
            width,
            height,
            offsetX,
            offsetY,
            attackDirection
        );
    }

    // =====================================================
    // Combo Buffer
    // =====================================================

    private void TryQueueNextAttack()
    {
        if (currentAttack == null)
            return;

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(currentAttack.AnimatorStateName))
            return;

        float normalizedTime = stateInfo.normalizedTime;

        if (normalizedTime >= currentAttack.ComboInputStart &&
            normalizedTime <= currentAttack.ComboInputEnd)
        {
            comboBuffered = true;
        }
    }

    // =====================================================
    // Hitbox
    // =====================================================

    private void SetHitbox(bool active)
    {
        if (!active)
        {
            hitboxActive = false;

            if (attackHitbox != null)
                attackHitbox.Deactivate();

            return;
        }

        if (!hitboxActive)
        {
            ActivateCurrentHitbox();
        }
    }


    private void ActivateCurrentHitbox()
    {
        if (currentAttack == null || attackHitbox == null)
            return;

        int finalDamage = currentAttack.Damage;
        bool isCritical = false;

        if (playerStats != null)
        {
            finalDamage = playerStats.CalculateAttackDamage(
                currentAttack,
                out isCritical
            );
        }

        attackHitbox.Activate(finalDamage, gameObject);

        hitboxActive = true;

        if (isCritical)
        {
            Debug.Log(
                $"CRITICAL! {currentAttack.DisplayName} / " +
                $"Damage: {finalDamage}"
            );
        }
    }

    // =====================================================
    // End Attack
    // =====================================================

    private void EndAttack()
    {
        if (!isAttacking)
            return;

        // 공격 이동 해제
        if (attackMovementController != null)
        {
            attackMovementController.CancelAttackMovement();
        }

        // 히트박스 비활성화
        SetHitbox(false);

        isAttacking = false;
        comboBuffered = false;

        currentSequenceIndex = -1;
        currentAttack = null;

        attackStateEntered = false;

        // 플레이어 상태 복구
        if (playerState != null)
        {
            playerState.SetState(PlayerStateType.Normal);
        }

        // Idle 애니메이션으로 복귀
        if (animator != null)
        {
            animator.Play(
                idleAnimatorStateName,
                0,
                0f
            );
        }
    }

    private void OnDisable()
    {
        if (attackMovementController != null)
            attackMovementController.CancelAttackMovement();
    }

}