using UnityEngine;
using UnityEngine.UI;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public string requiredKeyName = "MainDoor";
    public bool isLocked = true;

    [Header("Teleport")]
    public Transform destination;

    [Header("UI")]
    public Text messageText;

    [Header("Audio")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Add AudioSource automatically if needed
        if (audioSource == null && doorOpenSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

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

            // Check if player has required key
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

                Debug.Log("Door is locked! Need: " + requiredKeyName);
            }
        }
        else
        {
            OpenDoor(player);
        }
    }

    void OpenDoor(GameObject player)
    {
        if (destination == null)
        {
            Debug.LogError("No destination set for door!");
            return;
        }

        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        // Temporarily disable CharacterController before teleporting
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = destination.position;
            player.transform.rotation = destination.rotation;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = destination.position;
            player.transform.rotation = destination.rotation;
        }

        if (messageText != null)
        {
            messageText.text = "Door opened";
            Invoke(nameof(ClearMessage), 2f);
        }

        Debug.Log("Player teleported to: " + destination.name);
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}