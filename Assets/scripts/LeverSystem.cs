using UnityEngine;

public class LeverSystem : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static LeverSystem Instance { get; private set; }

    [Header("Alavancas")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("Arrastar aqui os 3 objetos Lever da cena")]
    public Lever[] levers;

    [Header("Luzes da Sala")]
    [Tooltip("Luzes Unity que acendem quando as 3 alavancas estiverem ativas")]
    public Light[] roomLights;

    [Header("Audio")]
    public AudioClip powerOnSound;

    private AudioSource audioSource;

    [Header("UI")]
    public UnityEngine.UI.Text messageText;

    [Header("Atualizar Objetivo (Ao Completar Todas)")]
    public bool updateObjectiveOnRestore = false;
    [TextArea] public string nextObjectiveText = "Energia restaurada, segue em frente.";

    private int leversActivated = 0;
    private bool powerRestored = false;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetRoomLights(false);
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void OnLeverActivated()
    {
        if (powerRestored) return;

        leversActivated++;

        int remaining = levers.Length - leversActivated;

        if (messageText != null)
        {
            messageText.text = remaining > 0
                ? $"Generators activated: {leversActivated}/{levers.Length}"
                : "";
            if (remaining > 0)
                Invoke(nameof(ClearMessage), 3f);
        }

        if (leversActivated >= levers.Length)
            RestorePower();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void RestorePower()
    {
        powerRestored = true;

        if (powerOnSound != null && audioSource != null)
            audioSource.PlayOneShot(powerOnSound);

        SetRoomLights(true);

        if (DarknessManager.Instance != null)
            DarknessManager.Instance.OnPowerRestored();

        if (messageText != null)
        {
            messageText.text = "Power restored!";
            Invoke(nameof(ClearMessage), 4f);
        }
        if (updateObjectiveOnRestore && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

    }

    void SetRoomLights(bool on)
    {
        if (roomLights != null)
        {
            foreach (Light l in roomLights)
                if (l != null) l.enabled = on;
        }

        CeilingLight[] ceilingLights = Object.FindFirstObjectByType<CeilingLight>() != null 
            ? Object.FindObjectsByType<CeilingLight>(FindObjectsSortMode.None) 
            : new CeilingLight[0];

        foreach (CeilingLight cLight in ceilingLights)
        {
            if (cLight != null)
            {
                if (on) cLight.TurnOn();
                else cLight.TurnOff();
            }
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }

    public bool IsPowerRestored() => powerRestored;
}
