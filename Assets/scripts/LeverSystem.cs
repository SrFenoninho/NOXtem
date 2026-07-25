using UnityEngine;

public class LeverSystem : MonoBehaviour
{
    // ---------------------------------------------
    //  SINGLETON
    // ---------------------------------------------
    public static LeverSystem Instance { get; private set; }

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Alavancas")]
    [Tooltip("Arrastar aqui os 3 objetos Lever da cena")]
    public Lever[] levers;

    [Header("Luzes da Sala")]
    [Tooltip("Luzes Unity que acendem quando as 3 alavancas estiverem ativas")]
    public Light[] roomLights;

    [Header("Audio")]
    public AudioClip powerOnSound;  // som quando a eletricidade e restaurada
    private AudioSource audioSource;

    [Header("UI")]
    public UnityEngine.UI.Text messageText;

    [Header("Atualizar Objetivo (Ao Completar Todas)")]
    public bool updateObjectiveOnRestore = false;
    [TextArea] public string nextObjectiveText = "Energia restaurada, segue em frente.";

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private int leversActivated = 0;
    private bool powerRestored = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        // Singleton simples - so deve existir um LeverSystem por cena
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

        // Garantir que as luzes da sala comecam desligadas
        SetRoomLights(false);
    }

    // ---------------------------------------------
    //  CHAMADO PELAS ALAVANCAS
    // ---------------------------------------------
    // Cada Lever.cs chama este metodo ao ser ativado
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

        // Debug.Log($"Alavanca ativada! {leversActivated}/{levers.Length}");

        // Verificar se todas estao ativas
        if (leversActivated >= levers.Length)
            RestorePower();
    }

    // ---------------------------------------------
    //  RESTAURAR ENERGIA
    // ---------------------------------------------
    void RestorePower()
    {
        powerRestored = true;

        // Som de energia a voltar
        if (powerOnSound != null && audioSource != null)
            audioSource.PlayOneShot(powerOnSound);

        // Acender luzes da sala
        SetRoomLights(true);

        // Avisar o DarknessManager para remover a escuridao
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

        // Debug.Log("Energia restaurada! Todas as alavancas ativas.");
    }

    // ---------------------------------------------
    //  AUXILIARES
    // ---------------------------------------------
    void SetRoomLights(bool on)
    {
        if (roomLights != null)
        {
            foreach (Light l in roomLights)
                if (l != null) l.enabled = on;
        }

        // Encontrar todas as lampadas CeilingLight no mapa e acende-las
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
