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

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

    [SerializeField] private PlayerStats playerStats;

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

    private void StartAttack(int sequenceIndex)
    {
        if (comboSequence == null || comboSequence.Count == 0)
            return;

        if (sequenceIndex < 0 || sequenceIndex >= comboSequence.Count)
            return;

        AttackDefinition attack =
            comboSequence.GetAttackAt(sequenceIndex);

        if (attack == null)
            return;

        currentSequenceIndex = sequenceIndex;
        currentAttack = attack;

        UpdateAttackDirection();

        // 현재 공격의 이동 설정 전달
        if (attackMovementController != null)
        {
            attackMovementController.BeginAttack(
                currentAttack,
                attackDirection
            );
        }


        isAttacking = true;
        comboBuffered = false;

        SetHitbox(false);

        playerState.SetState(PlayerStateType.Attack);

        animator.Play(
            currentAttack.AnimatorStateName,
            0,
            0f
        );
    }

    // =====================================================
    // Attack Update
    // =====================================================

    private void UpdateAttack()
    {
        if (currentAttack == null)
            return;

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(currentAttack.AnimatorStateName))
            return;

        float normalizedTime = stateInfo.normalizedTime;

        // 공격 애니메이션에 맞춰 공격 이동 갱신
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

        // 애니메이션 종료
        if (normalizedTime >= 1f)
        {
            SetHitbox(false);

            if (comboBuffered && comboSequence.Count > 0)
            {
                int nextIndex =
                    (currentSequenceIndex + 1) % comboSequence.Count;

                StartAttack(nextIndex);
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
        if (attackMovementController != null)
            attackMovementController.CancelAttackMovement();

        SetHitbox(false);

        isAttacking = false;
        comboBuffered = false;
        currentSequenceIndex = -1;
        currentAttack = null;

        playerState.SetState(PlayerStateType.Normal);

        animator.Play("Idle");
    }

    private void OnDisable()
    {
        if (attackMovementController != null)
            attackMovementController.CancelAttackMovement();
    }

}