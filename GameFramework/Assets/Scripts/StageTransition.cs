
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTransition : MonoBehaviour
{
    // Singleton
    public static StageTransition Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private string nextSceneName;

    private bool isTransitioning = false;

    private void Awake()
    {
        // 싱글톤 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // 현재 인스턴스가 파괴될 때만 초기화
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        TransitionToScene(nextSceneName);
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("이동할 씬 이름이 설정되지 않았습니다.");
            return;
        }

        isTransitioning = true;

        SceneManager.LoadScene(sceneName);
    }
}

