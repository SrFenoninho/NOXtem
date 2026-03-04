using UnityEngine;
using UnityEngine.SceneManagement;

public class TPHackAndSlash : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Tooltip("Nome da cena da fase Hack & Slash a carregar")]
    public string sceneToLoad;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    // Teletransporta o jogador para a cena de combate ao entrar na zona
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            SceneManager.LoadScene(sceneToLoad);
    }
}
