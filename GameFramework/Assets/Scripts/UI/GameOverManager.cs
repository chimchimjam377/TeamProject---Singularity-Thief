using UnityEngine;
using UnityEngine.SceneManagement; // 씬(화면) 재시작을 위해 필요

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject gameOverPanel; // 게임 오버 창 전체

    private bool isGameOver = false;

    private void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        // 시작할 때는 게임 오버 창을 무조건 숨김
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        // 플레이어의 체력이 다해 IsDead가 true가 되면 실행
        if (playerStats != null && playerStats.IsDead)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // (선택) 적이나 화면이 더 이상 움직이지 않도록 게임 시간 정지
        // Time.timeScale = 0f; 
    }

    // [재시작] 버튼에 연결할 함수
    public void RestartGame()
    {
        Time.timeScale = 1f; // 시간 정지 해제

        // 현재 열려있는 씬(맵)을 처음부터 다시 불러오기
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // [종료] 버튼에 연결할 함수
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("게임 종료!");
        Application.Quit(); // 실제 빌드된 게임에서 창 닫기
    }
}