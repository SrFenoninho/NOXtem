using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Key Settings")]
    public string keyName = "Door";         // ID unico desta chave (usado nas portas)
    public string displayName = "";         // nome visivel no inventario (se vazio usa keyName)
    public string keyDescription = "";      // descricao opcional

    [Header("UI")]
    public Text messageText;

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

        // - - Adicionar ao inventario global - -
        string name = string.IsNullOrEmpty(displayName) ? keyName : displayName;
        InventoryItem item = new InventoryItem(keyName, name, "key", keyDescription);
        InventoryManager.Instance?.AddItem(item);
        Debug.Log("InventoryManager instance: " + (InventoryManager.Instance == null ? "NULL" : "OK"));

        // - - Compatibilidade com PlayerKeys (usado nas portas) - -
        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
        if (playerKeys != null)
            playerKeys.AddKey(keyName);

        alreadyPickedUp = true;

        if (messageText != null)
        {
            messageText.text = $"Apanhaste a chave: {name}.";
            Invoke(nameof(ClearMessage), 2f);
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