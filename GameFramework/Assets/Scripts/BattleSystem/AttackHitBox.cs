using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private BoxCollider2D hitboxCollider;

    private int damage;
    private GameObject attacker;

    private readonly HashSet<IDamageable> hitTargets = new();

    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();

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

    public void SetShape(
        float width,
        float height,
        float offsetX,
        float offsetY,
        int direction)
    {
        hitboxCollider.size = new Vector2(
            width,
            height
        );

        // 왼쪽 공격이면 X 위치 반전
        offsetX *= direction;

        hitboxCollider.offset = new Vector2(
            offsetX,
            offsetY
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxCollider.enabled)
            return;

        if (other.gameObject == attacker)
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        if (hitTargets.Contains(damageable))
            return;

        hitTargets.Add(damageable);

        damageable.TakeDamage(
            damage,
            attacker
        );
    }
}