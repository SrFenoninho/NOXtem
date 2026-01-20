using UnityEngine;
using UnityEngine.UI;

public class RadialMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject radialMenuUI;

    [Header("UI Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    [Header("Audio")]
    public AudioClip[] voiceLines;
    public AudioClip[] optionAudios;
    private AudioSource audioSource;

    [Header("Menu State")]
    public bool isMenuOpen = false;
    public string currentContext = "default";
    private bool isVoicelinePlaying = false;
    private bool isOptionAudioPlaying = false;

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
        button1.onClick.AddListener(ClickOption1);
        button2.onClick.AddListener(ClickOption2);
        button3.onClick.AddListener(ClickOption3);
        button4.onClick.AddListener(ClickOption4);

        radialMenuUI.SetActive(false);

        button1.GetComponentInChildren<Text>().text = option1;
        button2.GetComponentInChildren<Text>().text = option2;
        button3.GetComponentInChildren<Text>().text = option3;
        button4.GetComponentInChildren<Text>().text = option4;
    }

    void Update()
    {
        if (isVoicelinePlaying && !audioSource.isPlaying) // Opening voiceline finished - show UI
        {
            isVoicelinePlaying = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            radialMenuUI.SetActive(true);
        }

        if (isOptionAudioPlaying && !audioSource.isPlaying) // Option audio finished - close menu
        {
            isOptionAudioPlaying = false;
            FullCloseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isMenuOpen)
            {
                OpenMenu();
            }
            else if (!isVoicelinePlaying)
            {
                if (isOptionAudioPlaying)
                {
                    audioSource.Stop();
                    isOptionAudioPlaying = false;
                }
                FullCloseMenu();
            }
        }

        if (isMenuOpen && !isVoicelinePlaying && !isOptionAudioPlaying && radialMenuUI.activeSelf) // Keyboard selection only when UI is visible
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                SelectOption(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                SelectOption(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                SelectOption(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                SelectOption(4);
        }
    }

    void OpenMenu()
    {
        isMenuOpen = true;
        radialMenuUI.SetActive(false);

        if (voiceLines.Length > 0)
        {
            AudioClip randomVoice = voiceLines[Random.Range(0, voiceLines.Length)];
            audioSource.PlayOneShot(randomVoice);
            isVoicelinePlaying = true;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            radialMenuUI.SetActive(true);
        }

        Time.timeScale = 0;
        audioSource.pitch = 1f;
    }

    void SelectOption(int optionNumber)
    {
        radialMenuUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (optionAudios.Length >= optionNumber && optionAudios[optionNumber - 1] != null)
        {
            audioSource.PlayOneShot(optionAudios[optionNumber - 1]);
            isOptionAudioPlaying = true;
        }
        else
        {
            ExecuteOption(optionNumber);
            FullCloseMenu();
        }
    }

    void ExecuteOption(int optionNumber)
    {
        switch (currentContext)
        {
            case "default": HandleDefaultOption(optionNumber); break;
            case "door": HandleDoorOption(optionNumber); break;
            case "terminal": HandleTerminalOption(optionNumber); break;
        }
    }

    void FullCloseMenu()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        radialMenuUI.SetActive(false);
        isMenuOpen = false;
        isVoicelinePlaying = false;
        isOptionAudioPlaying = false;
        Time.timeScale = 1;
    }

    public void ClickOption1() { SelectOption(1); }
    public void ClickOption2() { SelectOption(2); }
    public void ClickOption3() { SelectOption(3); }
    public void ClickOption4() { SelectOption(4); }

    void HandleDefaultOption(int option)
    {
        switch (option)
        {
            case 1: Debug.Log("Default action 1"); break;
            case 2: Debug.Log("Default action 2"); break;
            case 3: Debug.Log("Default action 3"); break;
            case 4: Debug.Log("Default action 4"); break;
        }
    }

    void HandleDoorOption(int option)
    {
        switch (option)
        {
            case 1: Debug.Log("Try to open door"); break;
            case 2: Debug.Log("Examine door"); break;
            case 3: Debug.Log("Use key on door"); break;
            case 4: Debug.Log("Cancel"); break;
        }
    }

    void HandleTerminalOption(int option)
    {
        switch (option)
        {
            case 1: Debug.Log("Access system"); break;
            case 2: Debug.Log("Check logs"); break;
            case 3: Debug.Log("Shutdown"); break;
            case 4: Debug.Log("Cancel"); break;
        }
    }

    public void SetContext(string newContext, string opt1, string opt2, string opt3, string opt4)
    {
        currentContext = newContext;
        option1 = opt1;
        option2 = opt2;
        option3 = opt3;
        option4 = opt4;
    }
}