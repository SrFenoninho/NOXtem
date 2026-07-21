using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Key Settings")]
    public string keyName = "Door";
    public string displayName = "";
    public string keyDescription = "";

    [Header("UI")]
    public Text messageText;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Audio")]
    public AudioClip pickupSound;

    [Header("Glow Settings")]
    public bool enableGlow = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool alreadyPickedUp = false;
    private GlowEmitter keyGlow;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (enableGlow)
        {
            keyGlow = GetComponent<GlowEmitter>();
            if (keyGlow == null)
            {
                keyGlow = gameObject.AddComponent<GlowEmitter>();
                keyGlow.glowColor = Color.white;
            }
        }
    }

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return $"Press E to pick up {(string.IsNullOrEmpty(displayName) ? keyName : displayName)} key.";
    }

    public void Interact(GameObject player)
    {
        if (alreadyPickedUp) return;

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        string name = string.IsNullOrEmpty(displayName) ? keyName : displayName;
        InventoryItem item = new InventoryItem(keyName, name, "key", keyDescription);
        InventoryManager.Instance?.AddItem(item);

        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
        if (playerKeys != null)
            playerKeys.AddKey(keyName);

        alreadyPickedUp = true;

        if (messageText != null)
        {
            messageText.text = $"You picked up: {name}.";
            Invoke(nameof(ClearMessage), 2f);
        }

        NotifyDoorsAboutKey(keyName);

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        GetComponent<Collider>().enabled = false;
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        if (keyGlow != null)
            keyGlow.DisableGlow();
            
        RotateObject rotator = GetComponent<RotateObject>();
        if (rotator != null)
            rotator.StopEffects();

        // 100% Garantido que desaparece dos olhos do jogador atirando a chave para fora do mapa instantaneamente
        transform.position = new Vector3(transform.position.x, transform.position.y - 1000f, transform.position.z);

        Destroy(gameObject, 2.1f);
    }

    // ---------------------------------------------
    //  GLOW
    // ---------------------------------------------
    void NotifyDoorsAboutKey(string keyName)
    {
        foreach (InteractiveDoor door in FindObjectsByType<InteractiveDoor>(FindObjectsSortMode.None))
            door.OnKeyPickedUp(keyName);

        foreach (LockedDoor door in FindObjectsByType<LockedDoor>(FindObjectsSortMode.None))
            door.OnKeyPickedUp(keyName);

        foreach (CardReaderInteraction reader in FindObjectsByType<CardReaderInteraction>(FindObjectsSortMode.None))
            reader.OnKeyPickedUp(keyName);
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