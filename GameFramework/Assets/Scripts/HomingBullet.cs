using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;

    [SerializeField] private float turnSpeed = 4f;

    [Header("Damage")]
    [SerializeField] private int damage = 15;

    [SerializeField] private float lifeTime = 6f;

    private Transform target;

    private GameObject owner;

    private Vector2 direction;

    public void Initialize(
        Transform newTarget,
        GameObject attacker
    )
    {
        target = newTarget;
        owner = attacker;

        if (target != null)
        {
            direction =
                ((Vector2)target.position -
                 (Vector2)transform.position)
                .normalized;
        }

        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void Update()
    {
        if (target != null)
        {
            Vector2 targetDirection =
                ((Vector2)target.position -
                 (Vector2)transform.position)
                .normalized;

            direction = Vector2.Lerp(
                direction,
                targetDirection,
                turnSpeed * Time.deltaTime
            ).normalized;
        }

        transform.position +=
            (Vector3)(
                direction *
                speed *
                Time.deltaTime
            );
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (other.gameObject == owner)
            return;

        PlayerStats player =
            other.GetComponentInParent<PlayerStats>();

        if (player != null)
        {
            player.TakeDamage(
                damage,
                owner
            );

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}