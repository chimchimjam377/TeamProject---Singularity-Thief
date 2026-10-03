using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private Collider2D hitboxCollider;

    private int damage;

    private GameObject attacker;

    private readonly HashSet<IDamageable> hitTargets = new();

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();

        hitboxCollider.enabled = false;
    }

    public void Activate(int damage, GameObject attacker)
    {
        this.damage = damage;
        this.attacker = attacker;

        hitTargets.Clear();

        hitboxCollider.enabled = true;
    }

    public void Deactivate()
    {
        hitboxCollider.enabled = false;

        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxCollider.enabled)
            return;

        // 자기 자신은 무시
        if (other.gameObject == attacker)
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        // 같은 공격에서 같은 대상에게 여러 번 데미지 방지
        if (hitTargets.Contains(damageable))
            return;

        hitTargets.Add(damageable);

        damageable.TakeDamage(
            damage,
            attacker
        );
    }
}