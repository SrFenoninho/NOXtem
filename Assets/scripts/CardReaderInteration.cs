using UnityEngine;
using UnityEngine.UI;

public class CardReaderInteraction : MonoBehaviour, IInteractable
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Door Settings")]

    public SimpleLockedDoor doorToUnlock;
    public string keyName = "Keycard";

    [Header("UI")]
    public Text messageText;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Glow Settings")]
    public bool enableGlow = true;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private bool isUnlocked = false;
    private GlowEmitter readerGlow;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        InitializeGlow();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void InitializeGlow()
    {
        if (!enableGlow) return;

        readerGlow = GetComponent<GlowEmitter>();
        if (readerGlow == null)
        {
            readerGlow = gameObject.AddComponent<GlowEmitter>();
            readerGlow.glowColor = Color.white;
            readerGlow.enableGlow = false;
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void OnKeyPickedUp(string keyName)
    {
        if (keyName == this.keyName && enableGlow && readerGlow != null)
            readerGlow.EnableGlow();
    }

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

            if (readerGlow != null && enableGlow)
            {
                readerGlow.DisableGlow();
            }

            if (messageText != null)
            {
                messageText.text = "Access Granted!";
                Invoke(nameof(ClearMessage), 2f);
            }
            if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
            {
                ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
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
        }
        else
        {
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}
