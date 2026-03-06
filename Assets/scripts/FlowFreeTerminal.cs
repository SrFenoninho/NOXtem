using UnityEngine;
using UnityEngine.UI;

public class FlowFreeTerminal : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("UI References")]
    public Text messageText;
    public GameObject flowFreeUI; // painel do mini-jogo a mostrar/esconder

    [Header("Settings")]
    public float accessTime = 3f;                       // tempo de "hacking" antes de abrir o mini-jogo
    public string sceneToLoadOnComplete = "NextLevel";  // cena a carregar após concluir o puzzle

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isAccessing = false;   // temporizador de acesso em curso
    private bool isGameActive = false;  // mini-jogo aberto
    private float accessTimer = 0f;

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        if (isGameActive) return "Game in progress...";
        if (isAccessing) return $"Accessing... {Mathf.Ceil(accessTime - accessTimer)}s";
        return "Press E to access terminal";
    }

    public void Interact(GameObject player)
    {
        if (isGameActive) return;

        if (!isAccessing)
        {
            // Iniciar o temporizador de acesso
            isAccessing = true;
            accessTimer = 0f;
        }
    }

    // ---------------------------------------------
    //  TEMPORIZADOR DE ACESSO
    // ---------------------------------------------
    void Update()
    {
        if (!isAccessing || isGameActive) return;

        accessTimer += Time.deltaTime;

        if (messageText != null)
            messageText.text = $"Accessing... {Mathf.Ceil(accessTime - accessTimer)}s";

        if (accessTimer >= accessTime)
            OpenMinigame();
    }

    // ---------------------------------------------
    //  ABRIR / FECHAR MINI-JOGO
    // ---------------------------------------------
    void OpenMinigame()
    {
        isAccessing = false;
        isGameActive = true;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(true);

        // Pausar o jogo enquanto o mini-jogo está ativo
        Time.timeScale = 0f;
        GameStateManager.Instance?.PushState(GameState.Minigame);

        // Desbloquear o cursor para interação com a UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
            messageText.text = "";

        Debug.Log("Mini-jogo Flow Free aberto!");
    }

    // Chamado pelo FlowFreeGame quando o jogador conclui o puzzle
    public void OnGameComplete()
    {
        isGameActive = false;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(false);

        // Retomar o jogo
        Time.timeScale = 1f;
        GameStateManager.Instance?.PopState();

        // Voltar a bloquear o cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Puzzle concluído! A carregar: " + sceneToLoadOnComplete);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnComplete);
    }
}