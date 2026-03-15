using UnityEngine;
using UnityEngine.UI;

public class FlowFreeTerminal : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("UI References")]
    public Text messageText;
    public GameObject flowFreeUI;

    [Header("Settings")]
    public string sceneToLoadOnComplete = "NextLevel";

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isGameActive = false;

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        if (isGameActive) return "Game in progress...";
        return "Press E to access terminal";
    }

    public void Interact(GameObject player)
    {
        if (isGameActive) return;
        OpenMinigame();
    }

    // ---------------------------------------------
    //  MINIJOGO
    // ---------------------------------------------
    void OpenMinigame()
    {
        isGameActive = true;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(true);

        Time.timeScale = 0f;
        GameStateManager.Instance?.PushState(GameState.Minigame);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
            messageText.text = "";

        Debug.Log("Mini-jogo Flow Free aberto!");
    }

    // ---------------------------------------------
    //  CONCLUSÃO
    // ---------------------------------------------
    public void OnGameComplete()
    {
        isGameActive = false;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(false);

        Time.timeScale = 1f;
        GameStateManager.Instance?.PopState();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Puzzle concluído! A carregar: " + sceneToLoadOnComplete);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnComplete);
    }
}