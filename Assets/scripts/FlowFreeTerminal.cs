using UnityEngine;
using TMPro;

public class FlowFreeTerminal : MonoBehaviour, IInteractable
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("UI References")]

    public TextMeshProUGUI messageText;
    public GameObject flowFreeUI;

    [Header("Settings")]
    public string sceneToLoadOnComplete = "NextLevel";

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Glow Settings")]
    public bool enableGlow = true;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private bool isGameActive = false;
    private GlowEmitter terminalGlow;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        InitializeGlow();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void InitializeGlow()
    {
        if (!enableGlow) return;

        terminalGlow = GetComponent<GlowEmitter>();
        if (terminalGlow == null)
        {
            terminalGlow = gameObject.AddComponent<GlowEmitter>();
            terminalGlow.glowColor = Color.white;
            terminalGlow.enableGlow = false;
        }

        terminalGlow.EnableGlow();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
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

    void OpenMinigame()
    {
        isGameActive = true;

        if (terminalGlow != null && enableGlow)
        {
            terminalGlow.DisableGlow();
        }

        if (flowFreeUI != null)
            flowFreeUI.SetActive(true);

        Time.timeScale = 0f;
        GameStateManager.Instance?.PushState(GameState.Minigame);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
            messageText.text = "";
    }

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

    public void OnGameComplete()
    {
        isGameActive = false;

        if (flowFreeUI != null)
            flowFreeUI.SetActive(false);

        Time.timeScale = 1f;
        GameStateManager.Instance?.PopState();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        LoadingManager.Carregar(sceneToLoadOnComplete);
    }
}
