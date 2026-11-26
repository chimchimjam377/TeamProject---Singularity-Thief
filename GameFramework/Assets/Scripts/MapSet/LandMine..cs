using System.Collections;
using UnityEngine;

public class LandMine : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Warning")]
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool activated;

    private void Awake()
    {
        // Inspector에 직접 안 넣어도 자동으로 찾음
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 작동한 지뢰면 무시
        if (activated)
            return;

        // 플레이어가 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        activated = true;

        StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        float timer = 0f;

        // 경고 시간 동안 깜빡임
        while (timer < warningTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            yield return new WaitForSeconds(
                blinkInterval
            );

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            yield return new WaitForSeconds(
                blinkInterval
            );

            timer += blinkInterval * 2f;
        }

        // 폭발 직전 확실히 보이게
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius,
                playerLayer
            );

        foreach (Collider2D hit in hits)
        {
            PlayerStats player =
                hit.GetComponentInParent<PlayerStats>();

            if (player != null)
            {
                player.TakeDamage(
                    damage,
                    gameObject
                );

                // 플레이어 한 명이면 한 번만 데미지
                break;
            }
        }

        Debug.Log("지뢰 폭발!");

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}