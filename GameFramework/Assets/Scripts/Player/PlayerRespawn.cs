using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("리스폰 설정 (자동 탐색됨)")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1.0f;

    [Header("카메라 설정 (자동 탐색됨)")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, -10f);

    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;
    private Vector3 initialScale;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        // 1. 인스펙터가 비어있다면 씬에서 SpawnPoint 자동 탐색
        FindSceneReferences();

        // 2. 초기 스폰 위치 저장
        if (spawnPoint != null)
        {
            initialSpawnPosition = spawnPoint.position;
            initialSpawnRotation = spawnPoint.rotation;
        }
        else
        {
            initialSpawnPosition = transform.position;
            initialSpawnRotation = transform.rotation;
        }
    }

    private void Start()
    {
        // 3. 프리팹이 새로 생성(리스폰)되었을 때 스폰 지점과 카메라를 즉시 맞춤
        SnapToSpawnAndCamera();
    }

    // 씬에 있는 오브젝트들을 이름/태그로 자동 탐색하여 인스펙터 빈칸을 채움
    private void FindSceneReferences()
    {
        if (spawnPoint == null)
        {
            // 먼저 "SpawnPoint" 태그로 찾고, 없으면 이름으로 찾음
            GameObject spObj = null;
            try { spObj = GameObject.FindGameObjectWithTag("SpawnPoint"); } catch { }
            
            if (spObj == null)
                spObj = GameObject.Find("SpawnPoint");

            if (spObj != null)
                spawnPoint = spObj.transform;
        }

        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    // 세이브 포인트 통과 시 스폰 지점 갱신
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        if (newSpawnPoint != null)
        {
            spawnPoint = newSpawnPoint;
            initialSpawnPosition = newSpawnPoint.position;
            initialSpawnRotation = newSpawnPoint.rotation;
        }
    }

    // 사망 시 호출 (오브젝트를 파괴하지 않고 재사용할 때)
    public void TriggerRespawn()
    {
        Invoke(nameof(RespawnNow), respawnDelay);
    }

    public void RespawnNow()
    {
        FindSceneReferences(); // 혹시 끊어진 참조가 있다면 다시 연결
        SnapToSpawnAndCamera();
        gameObject.SetActive(true);
    }

    // 플레이어 위치, 물리 상태, 카메라 시점을 초기 상태로 동기화
    private void SnapToSpawnAndCamera()
    {
        Vector3 targetPos = spawnPoint != null ? spawnPoint.position : initialSpawnPosition;
        Quaternion targetRot = spawnPoint != null ? spawnPoint.rotation : initialSpawnRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.localScale = initialScale;

        if (mainCamera != null)
        {
            mainCamera.position = new Vector3(
                targetPos.x + cameraOffset.x,
                targetPos.y + cameraOffset.y,
                cameraOffset.z
            );
        }
    }
}