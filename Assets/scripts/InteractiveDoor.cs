using UnityEngine;
using UnityEngine.UI;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Door Settings")]
    public string requiredKeyName = "MainDoor";
    public bool isLocked = true;

    [Header("Teleport")]
    public Transform destination;

    [Header("Camera Angle After Teleport")]
    [Range(0f, 360f)]
    public float cameraHorizontalAngle = 0f;

    [Header("UI")]
    public Text messageText;

    [Header("audio")]
    public AudioClip doorLockedSound; // Som quando a porta esta trancada e tentamos abrir
    public AudioClip doorOpenSound;   // Som da porta a destrancar/abrir
    private AudioSource audioSource;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (doorOpenSound != null || doorLockedSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return isLocked
            ? $"Press E to open door (Requires {requiredKeyName} key)"
            : "Press E to open door";
    }

    public void Interact(GameObject player)
    {
        if (isLocked)
        {
            PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
            if (playerKeys != null && playerKeys.HasKey(requiredKeyName))
            {
                isLocked = false;
                OpenDoor(player);
            }
            else
            {
                if (messageText != null)
                {
                    messageText.text = $"You need a {requiredKeyName} key.";
                    Invoke(nameof(ClearMessage), 2f);
                }

                if (doorLockedSound != null && audioSource != null)
                    audioSource.PlayOneShot(doorLockedSound);
            }
        }
        else
        {
            OpenDoor(player);
        }
    }

    // ---------------------------------------------
    //  ABRIR PORTA / TELETRANSPORTE
    // ---------------------------------------------
    void OpenDoor(GameObject player)
    {
        if (destination == null) return;

        if (doorOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(doorOpenSound);

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
            controller.enabled = true;
        }
        else
        {
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
        }

        FPMove fpMove = player.GetComponent<FPMove>();
        if (fpMove != null)
        {
            var field = typeof(FPMove).GetField("xRotation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(fpMove, 0f);
        }

        if (messageText != null)
        {
            messageText.text = "Door opened";
            Invoke(nameof(ClearMessage), 2f);
        }

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}
