using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private PlayerComboSequence comboSequence;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;

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

        if (comboSequence != null)
            comboSequence.Initialize();

        if (attackHitbox == null)
            Debug.LogError("AttackHitbox가 연결되지 않았습니다.", this);

        if (animator == null)
            Debug.LogError("Animator를 찾을 수 없습니다.", this);
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

        if (!isAttacking)
        {
            if (CanStartAttack() && comboSequence.Count > 0)
                StartAttack(0);

            return;
        }

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

        // 히트박스 활성 구간
        if (normalizedTime >= currentAttack.HitboxStart &&
            normalizedTime < currentAttack.HitboxEnd)
        {
            if (!hitboxActive)
            {
                attackHitbox.Activate(
                    currentAttack.Damage,
                    gameObject
                );

                hitboxActive = true;
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
        hitboxActive = active;

        if (attackHitbox == null)
            return;

        if (active && currentAttack != null)
        {
            attackHitbox.Activate(
                currentAttack.Damage,
                gameObject
            );
        }
        else
        {
            attackHitbox.Deactivate();
        }
    }

    // =====================================================
    // End Attack
    // =====================================================

    private void EndAttack()
    {
        SetHitbox(false);

        isAttacking = false;
        comboBuffered = false;
        currentSequenceIndex = -1;
        currentAttack = null;

        playerState.SetState(PlayerStateType.Normal);

        animator.Play("Idle");
    }
}