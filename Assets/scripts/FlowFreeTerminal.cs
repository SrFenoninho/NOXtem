using UnityEngine;
using UnityEngine.UI;

public class FlowFreeTerminal : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public Text messageText;
    public GameObject flowFreeUI; // Assign the Flow Free minigame UI panel here

    [Header("Settings")]
    public float accessTime = 3f;
    public string sceneToLoadOnComplete = "NextLevel";

    private bool isAccessing = false;
    private bool isGameActive = false;
    private float accessTimer = 0f;

    public string GetInteractMessage()
    {
        if (isGameActive)
            return "Game in progress...";

        if (isAccessing)
            return $"Accessing... {Mathf.Ceil(accessTime - accessTimer)}s";

        return "Press E to access terminal";
    }

    public void Interact(GameObject player)
    {
        if (isGameActive) return;

        if (!isAccessing)
        {
            // Start accessing the terminal
            isAccessing = true;
            accessTimer = 0f;
        }
    }

    void Update()
    {
        // Handle the access timer
        if (isAccessing && !isGameActive)
        {
            accessTimer += Time.deltaTime;

            // Update the message text with the remaining time
            if (messageText != null)
            {
                messageText.text = $"Accessing... {Mathf.Ceil(accessTime - accessTimer)}s";
            }

            // Check if access time is complete 
            if (accessTimer >= accessTime)
            {
                OpenMinigame();
            }
        }
    }

    void OpenMinigame()
    {
        isAccessing = false;
        isGameActive = true;

        // Show the Flow Free minigame UI
        if (flowFreeUI != null)
        {
            flowFreeUI.SetActive(true);
        }

        // Pause the game while the minigame is active
        Time.timeScale = 0f;

        // Unlock the cursor for minigame interaction 
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
        {
            messageText.text = "";
        }

        Debug.Log("Flow Free minigame opened!");
    }

    // This method should be called by the Flow Free minigame when the player completes it
    public void OnGameComplete()
    {
        isGameActive = false;

        // Hide the Flow Free minigame UI
        if (flowFreeUI != null)
        {
            flowFreeUI.SetActive(false);
        }

        // Resume the game
        Time.timeScale = 1f;

        // Lock the cursor back for normal gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Game completed! Loading scene: " + sceneToLoadOnComplete);

        // Load the next scene or perform any other actions needed after completion
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnComplete);
    }
}