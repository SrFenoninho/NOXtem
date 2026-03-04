using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ---------------------------------------------
    //  BOTÕES DO MENU PRINCIPAL
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
