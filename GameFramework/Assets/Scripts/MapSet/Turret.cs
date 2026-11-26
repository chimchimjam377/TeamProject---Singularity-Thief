using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject straightBulletPrefab;
    [SerializeField] private GameObject homingBulletPrefab;

    [Header("Attack Timing")]
    [SerializeField] private float attackCooldown = 2f;

    [Header("Burst Fire")]
    [SerializeField] private int burstCount = 5;
    [SerializeField] private float burstInterval = 0.2f;

    private Transform player;
    private bool playerInside;

    private Coroutine attackRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.transform;
        playerInside = true;

        if (attackRoutine == null)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        player = null;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        while (playerInside && player != null)
        {
            // 0 = 직사
            // 1 = 추적
            // 2 = 연사
            int randomPattern = Random.Range(0, 3);

            switch (randomPattern)
            {
                case 0:
                    Debug.Log("포탑 패턴: 직사");
                    FireStraightBullet();
                    break;

                case 1:
                    Debug.Log("포탑 패턴: 추적탄");
                    FireHomingBullet();
                    break;

                case 2:
                    Debug.Log("포탑 패턴: 연사");
                    yield return StartCoroutine(BurstFire());
                    break;
            }

            // 다음 공격 전 대기
            yield return new WaitForSeconds(attackCooldown);
        }

        attackRoutine = null;
    }

    // =========================
    // 직사탄
    // =========================

    private void FireStraightBullet()
    {
        if (player == null)
            return;

        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)firePoint.position)
            .normalized;

        GameObject bullet =
            Instantiate(
                straightBulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

        TurretBullet bulletScript =
            bullet.GetComponent<TurretBullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(
                direction,
                gameObject
            );
        }
    }

    // =========================
    // 추적탄
    // =========================

    private void FireHomingBullet()
    {
        if (player == null)
            return;

        GameObject bullet =
            Instantiate(
                homingBulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

        HomingBullet bulletScript =
            bullet.GetComponent<HomingBullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(
                player,
                gameObject
            );
        }
    }

    // =========================
    // 연사
    // =========================

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (!playerInside || player == null)
                yield break;

            FireStraightBullet();

            yield return new WaitForSeconds(
                burstInterval
            );
        }
    }
}