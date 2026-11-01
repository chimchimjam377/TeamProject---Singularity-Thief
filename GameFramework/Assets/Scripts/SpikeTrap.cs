using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spikeVisual;

    [Header("Movement")]
    [SerializeField] private float spikeHeight = 1f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Timing")]
    [SerializeField] private float activationDelay = 0.2f;
    [SerializeField] private float activeDuration = 0.5f;
    [SerializeField] private float cooldown = 1f;

    [Header("Damage")]
    [SerializeField] private int damage = 20;

    private Vector3 hiddenPosition;
    private Vector3 activePosition;

    private bool isActive;
    private bool canActivate = true;

    private Transform player;

    private void Awake()
    {
        hiddenPosition = spikeVisual.localPosition;

        activePosition =
            hiddenPosition + Vector3.up * spikeHeight;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        player = other.transform;

        if (canActivate)
        {
            StartCoroutine(SpikeRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (player == other.transform)
        {
            player = null;
        }
    }

    private IEnumerator SpikeRoutine()
    {
        canActivate = false;

        // 밟은 뒤 약간의 지연
        yield return new WaitForSeconds(
            activationDelay
        );

        // 가시 올라오기
        isActive = true;

        yield return StartCoroutine(
            MoveSpike(
                hiddenPosition,
                activePosition
            )
        );

        // 올라온 순간 플레이어가 아직 위에 있으면 데미지
        DamagePlayer();

        // 잠깐 유지
        yield return new WaitForSeconds(
            activeDuration
        );

        // 가시 내려가기
        yield return StartCoroutine(
            MoveSpike(
                activePosition,
                hiddenPosition
            )
        );

        isActive = false;

        // 쿨타임
        yield return new WaitForSeconds(
            cooldown
        );

        canActivate = true;
    }

    private IEnumerator MoveSpike(
        Vector3 start,
        Vector3 end
    )
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            spikeVisual.localPosition =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }

        spikeVisual.localPosition = end;
    }

    private void DamagePlayer()
    {
        if (player == null)
            return;

        PlayerStats playerStats =
            player.GetComponentInParent<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.TakeDamage(
                damage,
                gameObject
            );
        }
    }
}