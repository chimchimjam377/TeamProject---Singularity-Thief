using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;

    [SerializeField] private float lifeTime = 5f;

    public void Initialize(
        Vector3 direction,
        float speed,
        float damage)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position +=
            direction *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}