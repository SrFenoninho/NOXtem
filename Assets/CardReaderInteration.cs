using UnityEngine;
using UnityEngine.UI;

public class CardReaderInteraction : MonoBehaviour
{
    [Header("UI Message")]
    public Text messageText;

    [Header("Door Settings")]
    public GameObject GateDoor1;
    public GameObject GateDoor2;
    public string keyName = "Keycard";

    private bool playerInside = false;
    private PlayerKeys playerKeys;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerKeys = other.GetComponent<PlayerKeys>();

            messageText.text = "Press E to use card reader";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            messageText.text = "";
            playerKeys = null;
        }
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            TryAccess();
        }
    }

    void TryAccess()
    {
        if (playerKeys == null) return;

        if (playerKeys.HasKey(keyName))
        {
            UnlockDoor();
            messageText.text = "Access Granted!";
        }
        else
        {
            messageText.text = $"You need a {keyName}";
        }

        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }
    void UnlockDoor()
    {
        if (GateDoor1 != null)
        {
            GateDoor1.SetActive(false);
            // this makes the door disappear, is temporary! In a future, probably, I make an animation or other thing
        }
        if (GateDoor2 != null)
        {
            GateDoor2.SetActive(false);
        }
    }
    void ClearMessage()
    {
        if (!playerInside)
            messageText.text = "";
    }
}
