using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Lever Settings")]
    public string leverID = "Lever_A";  // identificador único desta alavanca

    [Header("Animação")]
    [Tooltip("Transform da parte visual que roda ao ativar (ex: o braço da alavanca)")]
    public Transform leverArm;
    public Vector3 activatedRotation = new Vector3(-60f, 0f, 0f); // rotação quando ativa
    public float animationSpeed = 5f;

    [Header("Audio")]
    public AudioClip leverSound;
    private AudioSource audioSource;

    [Header("UI")]
    public Text messageText;

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
            targetRotation  = defaultRotation;
        }
    }

    void Update()
    {
        // Animar suavemente o braço da alavanca para a rotação alvo
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
    //  ATIVAÇÃO
    // ---------------------------------------------
    void Activate()
    {
        isActivated = true;

        // Animar o braço da alavanca
        if (leverArm != null)
            targetRotation = Quaternion.Euler(activatedRotation);

        // Som da alavanca
        if (leverSound != null && audioSource != null)
            audioSource.PlayOneShot(leverSound);

        // Notificar o sistema central
        if (LeverSystem.Instance != null)
            LeverSystem.Instance.OnLeverActivated();

        Debug.Log($"Alavanca {leverID} ativada!");
    }
}
