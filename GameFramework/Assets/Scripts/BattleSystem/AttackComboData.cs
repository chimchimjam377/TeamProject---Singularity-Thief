using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AttackComboData",
    menuName = "Game/Combat/Attack Combo Data")]
public class AttackComboData : ScriptableObject
{
    [System.Serializable]
    public class AttackStep
    {
        [Header("Animation")]
        public string animatorStateName;

        [Header("Hitbox Timing")]
        [Range(0f, 1f)]
        public float hitboxStart = 0.25f;

        [Range(0f, 1f)]
        public float hitboxEnd = 0.5f;

        [Header("Next Combo Input")]
        [Range(0f, 1f)]
        public float comboInputStart = 0.4f;

        [Range(0f, 1f)]
        public float comboInputEnd = 0.9f;

        [Header("Damage")]
        public int damage = 10;

        [Header("Hitbox Shape")]
        public AnimationCurve hitboxWidth =
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 1f)
            );

        public AnimationCurve hitboxHeight =
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 1f)
            );

        public AnimationCurve hitboxOffsetX =
            new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(1f, 0.5f)
            );

        public AnimationCurve hitboxOffsetY =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 0f)
            );

        public void ClampValues()
        {
            hitboxStart = Mathf.Clamp01(hitboxStart);
            hitboxEnd = Mathf.Clamp01(hitboxEnd);

            comboInputStart = Mathf.Clamp01(comboInputStart);
            comboInputEnd = Mathf.Clamp01(comboInputEnd);

            if (hitboxEnd < hitboxStart)
                hitboxEnd = hitboxStart;

            if (comboInputEnd < comboInputStart)
                comboInputEnd = comboInputStart;
        }
    }

    [SerializeField]
    private List<AttackStep> attacks = new();

    public IReadOnlyList<AttackStep> Attacks => attacks;

    private void OnValidate()
    {
        foreach (AttackStep attack in attacks)
        {
            attack.ClampValues();
        }
    }
}