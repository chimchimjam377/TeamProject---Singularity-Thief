using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    private BoxCollider2D hitboxCollider;
    private int damage;
    private GameObject attacker;

    [SerializeField] private LayerMask targetLayer;
    private readonly HashSet<IDamageable> hitTargets = new();

    private void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
        hitboxCollider.isTrigger = true;
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

    public void SetShape(float width, float height, float offsetX, float offsetY, int direction)
    {
        hitboxCollider.size = new Vector2(width, height);
        offsetX *= direction;
        hitboxCollider.offset = new Vector2(offsetX, offsetY);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxCollider.enabled) return;
        if (other.gameObject == attacker) return;
        if (targetLayer != 0 && ((1 << other.gameObject.layer) & targetLayer) == 0) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;
        if (hitTargets.Contains(damageable)) return;

        hitTargets.Add(damageable);
        damageable.TakeDamage(damage, attacker);
    }
}