using UnityEngine;
using UnityEngine.UI;

public class CardReaderInteraction : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject GateDoor1;
    public GameObject GateDoor2;
    public string keyName = "Keycard";

    [Header("UI")]
    public Text messageText;

    private bool isUnlocked = false;

    public string GetInteractMessage()
    {
        if (isUnlocked)
            return "Already unlocked";

        return "Press E to use card reader";
    }
    public void Interact(GameObject player)
    {
        if (isUnlocked) return;

        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();

        if (playerKeys == null) return;

        if (playerKeys.HasKey(keyName))
        {
            UnlockDoor();
            isUnlocked = true;

            if (messageText != null)
            {
                messageText.text = "Access Granted!";
                Invoke(nameof(ClearMessage), 2f);
            }
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = $"You need a {keyName}";
                Invoke(nameof(ClearMessage), 2f);
            }
        }
    }
    void UnlockDoor()
    {
        if (GateDoor1 != null)
            GateDoor1.SetActive(false);

        if (GateDoor2 != null)
            GateDoor2.SetActive(false);
    }
    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}