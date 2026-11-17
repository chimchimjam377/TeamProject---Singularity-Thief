using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("씬 이름 설정")]
    public string baseSceneName = "BaseScene";   // 거점 씬 이름
    public string firstBattleScene = "Stage1";   // 첫 전투 스테이지 씬 이름

    [Header("사망 후 복귀 대기 시간")]
    public float respawnDelay = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 호출되어 플레이어 위치를 스폰 포인트로 이동
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        if (spawnPoint != null && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.transform.position = spawnPoint.transform.position;
        }
    }

    // 거점에서 전투 시작할 때 호출
    public void StartBattle()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetStats();
        }
        SceneManager.LoadScene(firstBattleScene);
    }

    // 플레이어 사망 시 PlayerStats.Die()에서 호출됨
    public void OnPlayerDeath()
    {
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // 사망 애니메이션 등을 보여주기 위해 잠시 대기
        yield return new WaitForSeconds(respawnDelay);

        // 스탯 초기화 및 플레이어 활성화 복구
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetStats();
        }

        // 거점 씬으로 복귀
        SceneManager.LoadScene(baseSceneName);
    }
}