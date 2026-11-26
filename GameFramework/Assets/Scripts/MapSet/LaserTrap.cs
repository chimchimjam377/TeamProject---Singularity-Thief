using System.Collections;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Aiming")]
    [SerializeField] private float aimDuration = 1.2f;
    [SerializeField] private float blinkInterval = 0.15f;
    [SerializeField] private float warningWidth = 0.04f;
    [SerializeField] private Color warningColor = Color.yellow;

    [Header("Fire")]
    [SerializeField] private float laserDistance = 15f;
    [SerializeField] private float fireDuration = 0.4f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private float fireWidth = 0.15f;
    [SerializeField] private Color fireColor = Color.red;
    [SerializeField] private int damage = 20;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayer;

    private Transform currentTarget;

    private bool playerInside;
    private Coroutine laserRoutine;

    private void Awake()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public void PlayerEntered(Transform player)
    {
        currentTarget = player;
        playerInside = true;

        if (laserRoutine == null)
        {
            laserRoutine = StartCoroutine(LaserRoutine());
        }
    }

    public void PlayerExited(Transform player)
    {
        if (currentTarget != player)
            return;

        currentTarget = null;
        playerInside = false;

        if (laserRoutine != null)
        {
            StopCoroutine(laserRoutine);
            laserRoutine = null;
        }

        lineRenderer.enabled = false;
    }

    private IEnumerator LaserRoutine()
    {
        while (playerInside && currentTarget != null)
        {
            Vector2 lockedDirection = Vector2.down;

            // -----------------------------
            // 1. 조준 단계
            // -----------------------------

            float timer = 0f;
            float blinkTimer = 0f;

            bool visible = true;

            SetLineStyle(
                warningWidth,
                warningColor
            );

            while (
                timer < aimDuration &&
                playerInside &&
                currentTarget != null
            )
            {
                Vector2 start = firePoint.position;

                Vector2 direction =
                    ((Vector2)currentTarget.position - start)
                    .normalized;

                if (direction != Vector2.zero)
                {
                    lockedDirection = direction;
                }

                Vector2 end =
                    start +
                    lockedDirection * laserDistance;

                lineRenderer.SetPosition(
                    0,
                    start
                );

                lineRenderer.SetPosition(
                    1,
                    end
                );

                blinkTimer += Time.deltaTime;

                if (blinkTimer >= blinkInterval)
                {
                    visible = !visible;

                    lineRenderer.enabled = visible;

                    blinkTimer = 0f;
                }

                timer += Time.deltaTime;

                yield return null;
            }

            if (!playerInside || currentTarget == null)
                break;

            // -----------------------------
            // 2. 방향 고정 후 발사
            // -----------------------------

            yield return StartCoroutine(
                FireLaser(lockedDirection)
            );

            // -----------------------------
            // 3. 레이저 끄기
            // -----------------------------

            lineRenderer.enabled = false;

            // -----------------------------
            // 4. 다음 공격까지 대기
            // -----------------------------

            yield return new WaitForSeconds(
                fireInterval
            );
        }

        lineRenderer.enabled = false;
        laserRoutine = null;
    }

    private IEnumerator FireLaser(
        Vector2 direction
    )
    {
        SetLineStyle(
            fireWidth,
            fireColor
        );

        Vector2 start = firePoint.position;

        RaycastHit2D hit =
            Physics2D.Raycast(
                start,
                direction,
                laserDistance,
                hitLayer
            );

        Vector2 end =
            start +
            direction * laserDistance;

        if (hit.collider != null)
        {
            end = hit.point;

            PlayerStats player =
                hit.collider
                .GetComponentInParent<PlayerStats>();

            if (player != null)
            {
                player.TakeDamage(
                    damage,
                    gameObject
                );
            }
        }

        lineRenderer.SetPosition(
            0,
            start
        );

        lineRenderer.SetPosition(
            1,
            end
        );

        lineRenderer.enabled = true;

        yield return new WaitForSeconds(
            fireDuration
        );
    }

    private void SetLineStyle(
        float width,
        Color color
    )
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}