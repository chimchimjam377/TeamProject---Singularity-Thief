using UnityEngine;
using UnityEngine.SceneManagement;

public class StageExit : MonoBehaviour
{
    [Header("다음 스테이지 후보")]
    [SerializeField] private string[] nextSceneNames;

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        LoadRandomStage();
    }

    private void LoadRandomStage()
    {
        if (nextSceneNames == null ||
            nextSceneNames.Length == 0)
        {
            Debug.LogWarning(
                "다음 스테이지가 설정되지 않았습니다."
            );

            return;
        }

        isLoading = true;

        int randomIndex =
            Random.Range(
                0,
                nextSceneNames.Length
            );

        string selectedScene =
            nextSceneNames[randomIndex];

        Debug.Log(
            "다음 스테이지: " +
            selectedScene
        );

        SceneManager.LoadScene(
            selectedScene
        );
    }
}