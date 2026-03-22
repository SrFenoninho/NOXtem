using UnityEngine;
using System.Collections.Generic;

// ---------------------------------------------
//  ESTRUTURA DE UM ITEM DO INVENTARIO
// ---------------------------------------------
[System.Serializable]
public class InventoryItem
{
    public string itemID;           // ID unico (ex: "Door_A")
    public string itemName;         // nome visivel no inventario
    public string itemType;         // "key", "note", "tool", etc.
    public string itemDescription;  // descricao opcional
    public Sprite icon;             // icone opcional (pode ficar null)

    public InventoryItem(string id, string name, string type = "key", string description = "", Sprite icon = null)
    {
        itemID = id;
        itemName = name;
        itemType = type;
        itemDescription = description;
        this.icon = icon;
    }
}

// ---------------------------------------------
//  INVENTARIO GLOBAL
// ---------------------------------------------
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private List<InventoryItem> items = new List<InventoryItem>();

    // Evento disparado sempre que o inventario muda
    // A InventoryUI subscreve isto para se atualizar automaticamente
    public System.Action OnInventoryChanged;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // ---------------------------------------------
    //  ADICIONAR / REMOVER
    // ---------------------------------------------
    public void AddItem(InventoryItem item)
    {
        if (HasItem(item.itemID)) return;
        items.Add(item);
        OnInventoryChanged?.Invoke();
        Debug.Log($"Inventario: adicionado '{item.itemName}'");
    }

    public void RemoveItem(string itemID)
    {
        int index = items.FindIndex(i => i.itemID == itemID);
        if (index < 0) return;
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }

    // ---------------------------------------------
    //  CONSULTA
    // ---------------------------------------------
    public bool HasItem(string itemID)
    {
        return items.Exists(i => i.itemID == itemID);
    }

    // Compatibilidade com PlayerKeys usado nas portas
    public bool HasKey(string keyID) => HasItem(keyID);

    public List<InventoryItem> GetAllItems() => items;
}