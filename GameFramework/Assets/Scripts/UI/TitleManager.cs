using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
