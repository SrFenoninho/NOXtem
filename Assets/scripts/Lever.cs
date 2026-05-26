using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Lever Settings")]
    public string leverID = "Lever_A";

    [Header("Animacao")]
    [Tooltip("Transform da parte visual que roda ao ativar (ex: o braco da alavanca)")]
    public Transform leverArm;
    public Vector3 activatedRotation = new Vector3(-60f, 0f, 0f);
    public float animationSpeed = 5f;

    [Header("Audio")]
    public AudioClip leverSound;
    private AudioSource audioSource;

    [Header("UI")]
    public Text messageText;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Glow Settings")]
    public bool enableGlow = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isActivated = false;
    private Quaternion targetRotation;
    private Quaternion defaultRotation;
    private GlowEmitter leverGlow;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (leverArm != null)
        {
            defaultRotation = leverArm.localRotation;
            targetRotation = defaultRotation;
        }

        if (enableGlow)
        {
            leverGlow = GetComponent<GlowEmitter>();
            if (leverGlow == null)
            {
                leverGlow = gameObject.AddComponent<GlowEmitter>();
                leverGlow.glowColor = Color.white;
            }
        }
    }

    void Update()
    {
        if (leverArm != null)
            leverArm.localRotation = Quaternion.Lerp(
                leverArm.localRotation, targetRotation, Time.deltaTime * animationSpeed);
    }

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return isActivated
            ? "Generator already active"
            : "Press E to activate generator";
    }

    public void Interact(GameObject player)
    {
        if (isActivated) return;
        Activate();
    }

    // ---------------------------------------------
    //  ATIVACAO
    // ---------------------------------------------
    void Activate()
    {
        isActivated = true;

        if (leverArm != null)
            targetRotation = Quaternion.Euler(activatedRotation);

        if (leverSound != null && audioSource != null)
            audioSource.PlayOneShot(leverSound);

        if (leverGlow != null)
            leverGlow.DisableGlow();

        if (LeverSystem.Instance != null)
            LeverSystem.Instance.OnLeverActivated();
        
        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        Debug.Log($"Alavanca {leverID} ativada!");
    }
}