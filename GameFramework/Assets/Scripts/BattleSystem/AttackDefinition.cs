
using UnityEngine;

public enum AttackType
{
    Slash,  // 베기
    Pierce, // 찌르기
    Blunt   // 타격
}

public enum AttackMovementType
{
    None,           // 이동하지 않음
    FixedDistance,  // 바라보는 방향으로 고정 거리 이동
    NearestTarget   // 가까운 적에게 접근
}

public enum AttackTargetMovementMode
{
    HorizontalOnly, // 좌우 방향으로만 접근
    TowardsTarget   // 적의 위치를 향해 2D 방향으로 접근
}

[CreateAssetMenu(
    fileName = "AttackDefinition",
    menuName = "Game/Combat/Attack Definition")]
public class AttackDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string attackId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Animation")]
    [SerializeField] private string animatorStateName;

    [Header("Damage")]
    [Min(0)]
    [SerializeField] private int damage = 10;

    [Header("Attack Type")]
    [SerializeField] private AttackType attackType = AttackType.Slash;

    [Header("Hitbox Timing")]
    [Range(0f, 1f)]
    [SerializeField] private float hitboxStart = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float hitboxEnd = 0.5f;

    [Header("Combo Input Timing")]
    [Range(0f, 1f)]
    [SerializeField] private float comboInputStart = 0.4f;

    [Range(0f, 1f)]
    [SerializeField] private float comboInputEnd = 0.9f;

    [Header("Hitbox Shape Curves")]
    [SerializeField]
    private AnimationCurve hitboxWidth =
        AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [SerializeField]
    private AnimationCurve hitboxHeight =
        AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [SerializeField]
    private AnimationCurve hitboxOffsetX =
        AnimationCurve.Linear(0f, 0.5f, 1f, 0.5f);

    [SerializeField]
    private AnimationCurve hitboxOffsetY =
        AnimationCurve.Linear(0f, 0f, 1f, 0f);


    [Header("Critical")]
    [Tooltip("이 공격의 기본 크리티컬 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float criticalChance = 0.05f;

    // =========================================
    // 공격 이동 설정
    // =========================================

    [Header("Attack Movement")]
    [SerializeField]
    private AttackMovementType movementType =
        AttackMovementType.None;

    [Tooltip("애니메이션 진행도 기준 이동 시작 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float movementStart = 0.2f;

    [Tooltip("애니메이션 진행도 기준 이동 종료 시점")]
    [Range(0f, 1f)]
    [SerializeField] private float movementEnd = 0.6f;

    [Tooltip("이동 구간 내 이동 진행도")]
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Tooltip("고정 거리 이동의 총 이동 거리")]
    [Min(0f)]
    [SerializeField] private float moveDistance = 2.5f;

    // =========================================
    // 적 추적 이동 설정
    // =========================================

    [Header("Target Movement")]
    [SerializeField]
    private AttackTargetMovementMode targetMovementMode =
        AttackTargetMovementMode.HorizontalOnly;

    [Tooltip("이 거리 안에서 가장 가까운 적을 탐색")]
    [Min(0f)]
    [SerializeField] private float targetSearchRadius = 5f;

    [Tooltip("이 거리만큼 적과 간격을 두고 멈춤")]
    [Min(0f)]
    [SerializeField] private float targetStopDistance = 1f;

    [Tooltip("적 탐색에 사용할 레이어")]
    [SerializeField] private LayerMask targetLayers = ~0;

    // =========================================
    // 기존 프로퍼티
    // =========================================

    public string AttackId => attackId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public string AnimatorStateName => animatorStateName;
    public int Damage => damage;

    public AttackType Type => attackType;

    public float HitboxStart => hitboxStart;
    public float HitboxEnd => hitboxEnd;

    public float ComboInputStart => comboInputStart;
    public float ComboInputEnd => comboInputEnd;

    public AnimationCurve HitboxWidth => hitboxWidth;
    public AnimationCurve HitboxHeight => hitboxHeight;
    public AnimationCurve HitboxOffsetX => hitboxOffsetX;
    public AnimationCurve HitboxOffsetY => hitboxOffsetY;

    public float CriticalChance => criticalChance;

    // =========================================
    // 이동 설정 프로퍼티
    // =========================================

    public AttackMovementType MovementType => movementType;

    public float MovementStart => movementStart;
    public float MovementEnd => movementEnd;

    public AnimationCurve MovementCurve => movementCurve;

    public float MoveDistance => moveDistance;

    public AttackTargetMovementMode TargetMovementMode =>
        targetMovementMode;

    public float TargetSearchRadius => targetSearchRadius;
    public float TargetStopDistance => targetStopDistance;

    public LayerMask TargetLayers => targetLayers;

    // =========================================
    // Inspector 값 검증
    // =========================================

    private void OnValidate()
    {
        hitboxStart = Mathf.Clamp01(hitboxStart);
        hitboxEnd = Mathf.Clamp(hitboxEnd, hitboxStart, 1f);

        comboInputStart = Mathf.Clamp01(comboInputStart);
        comboInputEnd = Mathf.Clamp(
            comboInputEnd,
            comboInputStart,
            1f
        );

        movementStart = Mathf.Clamp01(movementStart);
        movementEnd = Mathf.Clamp(
            movementEnd,
            movementStart,
            1f
        );

        damage = Mathf.Max(0, damage);
        moveDistance = Mathf.Max(0f, moveDistance);

        criticalChance = Mathf.Clamp01(criticalChance);

        targetSearchRadius = Mathf.Max(
            0f, targetSearchRadius
        );

        targetStopDistance = Mathf.Max(
            0f, targetStopDistance
        );
    }
}
