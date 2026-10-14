using UnityEngine;

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

    public string AttackId => attackId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public string AnimatorStateName => animatorStateName;
    public int Damage => damage;

    public float HitboxStart => hitboxStart;
    public float HitboxEnd => hitboxEnd;

    public float ComboInputStart => comboInputStart;
    public float ComboInputEnd => comboInputEnd;

    public AnimationCurve HitboxWidth => hitboxWidth;
    public AnimationCurve HitboxHeight => hitboxHeight;
    public AnimationCurve HitboxOffsetX => hitboxOffsetX;
    public AnimationCurve HitboxOffsetY => hitboxOffsetY;

    private void OnValidate()
    {
        hitboxStart = Mathf.Clamp01(hitboxStart);
        hitboxEnd = Mathf.Clamp(
            hitboxEnd, hitboxStart, 1f);

        comboInputStart = Mathf.Clamp01(comboInputStart);
        comboInputEnd = Mathf.Clamp(
            comboInputEnd, comboInputStart, 1f);

        damage = Mathf.Max(0, damage);
    }
}