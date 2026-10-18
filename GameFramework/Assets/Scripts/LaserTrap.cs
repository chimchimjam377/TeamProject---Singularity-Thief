using System.Collections;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Settings")]
    [SerializeField] private float laserDistance = 15f;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float laserVisibleTime = 0.2f;
    [SerializeField] private int damage = 20;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayer;

    private bool playerInside;
    private Coroutine laserCoroutine;

    private void Awake()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (laserCoroutine == null)
        {
            laserCoroutine = StartCoroutine(LaserRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private IEnumerator LaserRoutine()
    {
        while (playerInside)
        {
            FireLaser();

            yield return new WaitForSeconds(laserVisibleTime);

            lineRenderer.enabled = false;

            float waitTime = fireInterval - laserVisibleTime;

            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }

        laserCoroutine = null;
    }

    private void FireLaser()
    {
        Vector2 start = firePoint.position;
        Vector2 direction = firePoint.right;

        RaycastHit2D hit = Physics2D.Raycast(
            start,
            direction,
            laserDistance,
            hitLayer
        );

        Vector2 end = start + direction * laserDistance;

        if (hit.collider != null)
        {
            end = hit.point;

            PlayerStats player =
                hit.collider.GetComponentInParent<PlayerStats>();

            if (player != null)
            {
                // 네 PlayerStats 형식에 맞춤
                player.TakeDamage(damage, gameObject);
            }
        }

        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}