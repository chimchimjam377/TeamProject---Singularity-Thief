using UnityEngine;

public class FollowKey : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float followSpeed = 6f;

    [SerializeField]
    private Vector2 followOffset =
        new Vector2(-1f, 1f);

    private Transform player;

    private bool collected;

    private void Update()
    {
        if (!collected || player == null)
            return;

        Vector3 targetPosition =
            player.position +
            (Vector3)followOffset;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // √≥¿Ω «√∑π¿ÃæÓ∞° ø≠ºË »πµÊ
        if (!collected && other.CompareTag("Player"))
        {
            player = other.transform;
            collected = true;

            Debug.Log("ø≠ºË »πµÊ!");

            return;
        }

        // »πµÊ«— ªÛ≈¬ø°º≠ ø≠ºË ∫Æ ¡¢√À
        if (collected)
        {
            KeyWall wall =
                other.GetComponent<KeyWall>();

            if (wall != null)
            {
                wall.Unlock();

                Destroy(gameObject);
            }
        }
    }
}