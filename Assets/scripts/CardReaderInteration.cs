using UnityEngine;
using UnityEngine.UI;

public class CardReaderInteraction : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public SimpleLockedDoor doorToUnlock;
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
        if (doorToUnlock != null)
        {
            doorToUnlock.Unlock();
            Debug.Log("Door unlocked by card reader!");
        }
        else
        {
            Debug.LogWarning("No door assigned to card reader!");
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}