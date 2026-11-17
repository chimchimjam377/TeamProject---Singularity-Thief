using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("리스폰 설정")]
    [SerializeField] private float respawnDelay = 1.5f; // 죽고 나서 다시 살아날 때까지의 시간

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 플레이어가 죽을 때 이 함수를 호출합니다.
    public void RequestRespawn(GameObject playerObj)
    {
        StartCoroutine(RespawnRoutine(playerObj));
    }

    private IEnumerator RespawnRoutine(GameObject playerObj)
    {
        // 1. 플레이어 오브젝트를 파괴하지 않고 화면에서만 숨김
        playerObj.SetActive(false);

        // 2. 지정된 시간만큼 대기 (플레이어가 꺼져있어도 매니저는 살아있으므로 정상 작동)
        yield return new WaitForSeconds(respawnDelay);

        // 3. 플레이어 위치 및 카메라 초기화 후 다시 활성화
        PlayerRespawn respawn = playerObj.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.RespawnNow(); // 내부에서 위치 이동 + 카메라 동기화 + SetActive(true) 실행
        }
        else
        {
            playerObj.transform.position = transform.position;
            playerObj.SetActive(true);
        }
    }
}