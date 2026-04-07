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
    public AudioClip pickupSound;      // O som a tocar ao apanhar

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool alreadyPickedUp = false;

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
            // Toca o som na posicao atual do objeto, evitando que o som corte quando o objeto for destruido
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        string name = string.IsNullOrEmpty(displayName) ? keyName : displayName;
        InventoryItem item = new InventoryItem(keyName, name, "key", keyDescription);
        InventoryManager.Instance?.AddItem(item);
        Debug.Log("InventoryManager instance: " + (InventoryManager.Instance == null ? "NULL" : "OK"));

        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
        if (playerKeys != null)
            playerKeys.AddKey(keyName);

        alreadyPickedUp = true;

        if (messageText != null)
        {
            messageText.text = $"Apanhaste a chave: {name}.";
            Invoke(nameof(ClearMessage), 2f);
        }

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        // Desativar e destruir o objeto
        GetComponent<Collider>().enabled = false;
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;
        Destroy(gameObject, 2.1f);
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
