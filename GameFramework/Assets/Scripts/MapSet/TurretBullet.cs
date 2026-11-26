using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 direction;
    private GameObject owner;

    public void Initialize(
        Vector2 newDirection,
        GameObject attacker
    )
    {
        direction = newDirection.normalized;
        owner = attacker;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 자기 포탑 무시
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

        // 일반 벽이나 바닥에 맞으면 삭제
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}