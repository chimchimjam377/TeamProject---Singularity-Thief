using UnityEngine;

public class EnemyBomb : MonoBehaviour
{
    [Header("Bomb")]
    [SerializeField] private int damage = 20;

    [SerializeField] private float lifeTime = 8f;

    private GameObject owner;

    public void Initialize(GameObject attacker)
    {
        owner = attacker;

        // 폭탄과 공중 적의 Collider가 서로 충돌하지 않게 함
        Collider2D bombCollider =
            GetComponent<Collider2D>();

        Collider2D[] ownerColliders =
            owner.GetComponentsInChildren<Collider2D>();

        if (bombCollider != null)
        {
            foreach (Collider2D ownerCollider in ownerColliders)
            {
                Physics2D.IgnoreCollision(
                    bombCollider,
                    ownerCollider
                );
            }
        }
    }

    private void Start()
    {
        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        // 자신을 떨어뜨린 적은 무시
        if (
            owner != null &&
            other.transform.IsChildOf(owner.transform)
        )
        {
            return;
        }

        PlayerStats player =
            other.GetComponentInParent<PlayerStats>();

        if (player != null)
        {
            player.TakeDamage(
                damage,
                owner != null
                    ? owner
                    : gameObject
            );

            Destroy(gameObject);

            return;
        }

        // Trigger가 아닌 바닥이나 벽에 닿으면 제거
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}