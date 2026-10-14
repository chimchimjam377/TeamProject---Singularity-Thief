using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AttackLibrary",
    menuName = "Game/Combat/Attack Library")]
public class AttackLibrary : ScriptableObject
{
    [SerializeField]
    private List<AttackDefinition> attacks = new();

    public IReadOnlyList<AttackDefinition> Attacks => attacks;

    public AttackDefinition GetAttack(string attackId)
    {
        return attacks.Find(
            attack => attack != null &&
                      attack.AttackId == attackId
        );
    }


}