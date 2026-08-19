using UnityEngine;

public class RemoverLever : MonoBehaviour, IInteractable
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("A��o Principal")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("Arrasta para aqui o objeto da hierarquia que queres apagar")]

    public GameObject objectToRemove;

    [Header("Anima��o da Alavanca")]
    public Transform leverArm;

    public Vector3 activatedRotation = new Vector3(-60f, 0f, 0f);
    public float animationSpeed = 5f;

    [Header("Audio")]
    public AudioClip leverSound;
    private AudioSource audioSource;

    [Header("UI & Objetivo")]
    public string interactMessage = "Press E to pull lever";
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    private bool isActivated = false;
    private Quaternion targetRotation;
    private Quaternion defaultRotation;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && leverSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (leverArm != null)
        {
            defaultRotation = leverArm.localRotation;
            targetRotation = defaultRotation;
        }
    }

    void Update()
    {
        if (leverArm != null)
        {
            leverArm.localRotation = Quaternion.Lerp(
                leverArm.localRotation, targetRotation, Time.deltaTime * animationSpeed);
        }
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        if (isActivated) return "";
        return interactMessage;
    }

    public void Interact(GameObject player)
    {
        if (isActivated) return;
        Activate();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Activate()
    {
        isActivated = true;

        if (leverArm != null)
            targetRotation = Quaternion.Euler(activatedRotation);

        if (leverSound != null && audioSource != null)
            audioSource.PlayOneShot(leverSound);

        if (objectToRemove != null)
        {
            Destroy(objectToRemove);
        }

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }
    }
}
