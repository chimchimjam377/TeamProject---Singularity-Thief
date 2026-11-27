using UnityEngine;

public class BossHomingBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f, turnSpeed = 150f, lifeTime = 6f, hitRadius = 0.55f;
    [SerializeField] private int damage = 12;
    private PlayerStats player;
    private Vector2 direction;
    public void Initialize(PlayerStats target)
    {
        player = target;
        direction = Vector2.down;
        Destroy(gameObject, lifeTime);
    }
    private void Update()
    {
        if (!player || player.IsDead) { Destroy(gameObject); return; }
        Vector2 target = (Vector2)player.transform.position - (Vector2)transform.position;
        float angle = Vector2.SignedAngle(direction, target);
        direction = Quaternion.Euler(0, 0, Mathf.Clamp(angle, -turnSpeed * Time.deltaTime, turnSpeed * Time.deltaTime)) * direction;
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, player.transform.position) <= hitRadius)
        { player.TakeDamage(damage, gameObject); Destroy(gameObject); }
    }
}
