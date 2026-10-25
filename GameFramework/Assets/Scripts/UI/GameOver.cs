using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // 게임 오버가 발생하기 전 씬 이름 저장
    private static string previousSceneName;

    [Header("Scene Settings")]
    [SerializeField] private string titleSceneName = "Title";

    // 플레이어가 죽었을 때 호출하는 함수
    public static void LoadGameOver()
    {
        // 현재 씬이 GameOver라면 중복 실행 방지
        if (SceneManager.GetActiveScene().name == "GameOver")
            return;

        // 죽었던 씬 이름 저장
        previousSceneName = SceneManager.GetActiveScene().name;

        // 시간 정지 상태 해제
        Time.timeScale = 1f;

        // 공용 GameOver 씬으로 이동
        SceneManager.LoadScene("GameOver");
    }

    // 다시 하기 버튼
    public void Retry()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(previousSceneName))
        {
            Debug.LogWarning("다시 불러올 씬 이름이 저장되지 않았습니다.");
            return;
        }

        // 죽었던 씬을 처음부터 다시 로드
        SceneManager.LoadScene(previousSceneName);
    }

    // 타이틀 화면 버튼
    public void GoToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(titleSceneName);
    }
}