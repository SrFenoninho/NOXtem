using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ---------------------------------------------
    //  BOToES DO MENU PRINCIPAL
    // ---------------------------------------------
    public void PlayGame()
    {
        SceneManager.LoadScene("Floor1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
