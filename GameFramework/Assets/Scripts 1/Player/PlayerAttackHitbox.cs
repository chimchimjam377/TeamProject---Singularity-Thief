using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    private PlayerCombat owner;

    private readonly HashSet<GameObject> hitObjects = new();

    public void Initialize(PlayerCombat combat)
    {
        owner = combat;
    }

    public void EnableHitbox()
    {
        hitObjects.Clear();
        gameObject.SetActive(true);
    }

    public void DisableHitbox()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner == null)
            return;

        GameObject target = other.gameObject;

        if (hitObjects.Contains(target))
            return;

        hitObjects.Add(target);

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(
                owner.GetSwordDamage()
            );
        }
    }
}