using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public string itemID;
    public string itemName;
    public string itemType;
    public string itemDescription;
    public Sprite icon;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public InventoryItem(string id, string name, string type = "key", string description = "", Sprite icon = null)
    {
        itemID = id;
        itemName = name;
        itemType = type;
        itemDescription = description;
        this.icon = icon;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private List<InventoryItem> items = new List<InventoryItem>();

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

    public void AddItem(InventoryItem item)
    {
        if (HasItem(item.itemID)) return;
        items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(string itemID)
    {
        int index = items.FindIndex(i => i.itemID == itemID);
        if (index < 0) return;
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(string itemID)
    {
        return items.Exists(i => i.itemID == itemID);
    }

    public bool HasKey(string keyID) => HasItem(keyID);

    public List<InventoryItem> GetAllItems() => items;
}
