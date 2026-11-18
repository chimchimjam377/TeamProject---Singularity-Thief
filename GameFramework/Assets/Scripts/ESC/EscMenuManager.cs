using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EscMenuManager : MonoBehaviour
{
    [Header("UI 패널 연결")]
    [SerializeField] private GameObject escMenuPanel;

    [Header("씬 이름 설정")]
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string titleSceneName = "Title"; // 타이틀 씬 이름 추가

    private bool isMenuOpen = false;

    private void Start()
    {
        if (escMenuPanel != null)
            escMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        escMenuPanel.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // 1. 거점(Hub) 되돌아가기 [이번 공략 실패 처리]
    public void OnClickReturnToHub()
    {
        Time.timeScale = 1f;

        // TODO: Run 시스템 공략 실패 처리 (예: 획득 재화 정산 후 Hub로 복귀)
        // RunManager.Instance.FailCurrentRun();

        SceneManager.LoadScene(hubSceneName);
    }

    // 2. 타이틀 씬으로 돌아가기 (기존 게임 나가기)
    public void OnClickReturnToTitle()
    {
        // 일시정지 해제 (필수)
        Time.timeScale = 1f;

        // 마우스 커서가 타이틀 화면에서 보이도록 유지
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // TODO: 진행 중인 Run 데이터나 인게임 전용 싱글톤 UI 정리
        // 예: Destroy(BattleUIManager.Instance.gameObject);

        SceneManager.LoadScene(titleSceneName);
    }
}