using UnityEngine;
using TMPro;

public class FlowFreeTerminal : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public GameObject flowFreeUI;

    [Header("Settings")]
    public string sceneToLoadOnComplete = "NextLevel";

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

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
    }

    // ---------------------------------------------
    //  SAIDA FORCADA
    // ---------------------------------------------
    public void ForceClose()
    {
        isGameActive = false;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(false);

        Time.timeScale = 1f;
        GameStateManager.Instance?.PopState();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ---------------------------------------------
    //  CONCLUSAO
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

        // - - ATUALIZAR OBJETIVO - -
        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnComplete);
    }
}
