using UnityEngine;
using UnityEngine.UI;

public class CardReaderInteraction : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Door Settings")]
    public SimpleLockedDoor doorToUnlock;   // porta que este leitor controla
    public string keyName = "Keycard";      // ID do cartão necessário

    [Header("UI")]
    public Text messageText;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isUnlocked = false;

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return isUnlocked ? "Already unlocked" : "Press E to use card reader";
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

    // ---------------------------------------------
    //  DESBLOQUEIO
    // ---------------------------------------------
    void UnlockDoor()
    {
        if (doorToUnlock != null)
        {
            doorToUnlock.Unlock();
            Debug.Log("Porta destrancada pelo leitor de cartão!");
        }
        else
        {
            Debug.LogWarning("Nenhuma porta atribuída ao leitor de cartão!");
        }
    }

    // ---------------------------------------------
    //  UI
    // ---------------------------------------------
    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}
