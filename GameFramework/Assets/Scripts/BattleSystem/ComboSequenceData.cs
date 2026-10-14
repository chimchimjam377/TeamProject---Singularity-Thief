using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DefaultComboSequence",
    menuName = "Game/Combat/Combo Sequence Data")]
public class ComboSequenceData : ScriptableObject
{
    [SerializeField]
    private List<AttackDefinition> defaultAttacks = new();

    public IReadOnlyList<AttackDefinition> DefaultAttacks
        => defaultAttacks;
}