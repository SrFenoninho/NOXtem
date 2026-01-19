using UnityEngine;

public class RadialMenuManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip[] voiceLines;
    private AudioSource audioSource;

    [Header("Menu State")]
    public bool isMenuOpen = false;
    public string currentContext = "default";

    [Header("Current Options")]
    public string option1 = "Option 1";
    public string option2 = "Option 2";
    public string option3 = "Option 3";
    public string option4 = "Option 4";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isMenuOpen)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }

        if (isMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectOption(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectOption(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SelectOption(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                SelectOption(4);
            }
        }
    }

    void OpenMenu()
    {
        isMenuOpen = true;

        if (voiceLines.Length > 0)
        {
            AudioClip randomVoice = voiceLines[Random.Range(0, voiceLines.Length)];
            audioSource.PlayOneShot(randomVoice);
            Debug.Log("Playing voiceline: " + randomVoice.name);
        }

        Debug.Log("=== MENU OPENED ===");
        Debug.Log("Context: " + currentContext);
        Debug.Log("[1] " + option1);
        Debug.Log("[2] " + option2);
        Debug.Log("[3] " + option3);
        Debug.Log("[4] " + option4);

        Time.timeScale = 0; 
    }

    void CloseMenu()
    {
        isMenuOpen = false;
        Debug.Log("Menu closed");
        Time.timeScale = 1;
    }

    void SelectOption(int optionNumber)
    {
        Debug.Log("Selected option " + optionNumber);

        switch (currentContext)
        {
            case "default":
                HandleDefaultOption(optionNumber);
                break;
            case "door":
                HandleDoorOption(optionNumber);
                break;
            case "terminal":
                HandleTerminalOption(optionNumber);
                break;
        }

        CloseMenu();
    }

    void HandleDefaultOption(int option)
    {
        switch (option)
        {
            case 1:
                Debug.Log("Default action 1");
                break;
            case 2:
                Debug.Log("Default action 2");
                break;
            case 3:
                Debug.Log("Default action 3");
                break;
            case 4:
                Debug.Log("Default action 4");
                break;
        }
    }

    void HandleDoorOption(int option)
    {
        switch (option)
        {
            case 1:
                Debug.Log("Try to open door");
                break;
            case 2:
                Debug.Log("Examine door");
                break;
            case 3:
                Debug.Log("Use key on door");
                break;
            case 4:
                Debug.Log("Cancel");
                break;
        }
    }

    void HandleTerminalOption(int option)
    {
        switch (option)
        {
            case 1:
                Debug.Log("Access system");
                break;
            case 2:
                Debug.Log("Check logs");
                break;
            case 3:
                Debug.Log("Shutdown");
                break;
            case 4:
                Debug.Log("Cancel");
                break;
        }
    }

    public void SetContext(string newContext, string opt1, string opt2, string opt3, string opt4)
    {
        currentContext = newContext;
        option1 = opt1;
        option2 = opt2;
        option3 = opt3;
        option4 = opt4;

        Debug.Log("Context changed to: " + newContext);
    }
}