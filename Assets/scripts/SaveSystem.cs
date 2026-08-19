using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static bool carregarSaveAoIniciar = false;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public static void GuardarProgresso()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);

        PlayerPrefs.Save();
    }

    public static void AplicarSaveAoPlayer(GameObject player)
    {
        carregarSaveAoIniciar = false;
    }

    public static void LimparSaveProgresso()
    {
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("PlayerKeys");
        PlayerPrefs.Save();
    }
}
