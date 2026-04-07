using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Lever Settings")]
    public string leverID = "Lever_A";  // identificador unico desta alavanca

    [Header("Animacao")]
    [Tooltip("Transform da parte visual que roda ao ativar (ex: o braco da alavanca)")]
    public Transform leverArm;
    public Vector3 activatedRotation = new Vector3(-60f, 0f, 0f); // rotacao quando ativa
    public float animationSpeed = 5f;

    [Header("Audio")]
    public AudioClip leverSound;
    private AudioSource audioSource;

    [Header("UI")]
    public Text messageText;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isActivated = false;
    private Quaternion targetRotation;
    private Quaternion defaultRotation;

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
    }

    void Update()
    {
        // Animar suavemente o braco da alavanca para a rotacao alvo
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
    //  ATIVAcaO
    // ---------------------------------------------
    void Activate()
    {
        isActivated = true;

        // Animar o braco da alavanca
        if (leverArm != null)
            targetRotation = Quaternion.Euler(activatedRotation);

        // Som da alavanca
        if (leverSound != null && audioSource != null)
            audioSource.PlayOneShot(leverSound);

        // Notificar o sistema central
        if (LeverSystem.Instance != null)
            LeverSystem.Instance.OnLeverActivated();
        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        Debug.Log($"Alavanca {leverID} ativada!");
    }
}
