using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private Dictionary<string, InventoryItem> itemDatabase;
    private Dictionary<string, InventoryItem> inventory;

    public TextAsset jsonFile;

    void Start()
    {
        if (DialogueManager.Instance != null)
        {
            LoadItemsFromJson();
            InitializeInventory();
        }
        else
        {
            Debug.LogError("DialogueManager не инициализирован. InventoryManager требует DialogueManager для работы.");
        }
    }

    void LoadItemsFromJson()
    {
        itemDatabase = new Dictionary<string, InventoryItem>();
        ItemContainer itemContainer = JsonUtility.FromJson<ItemContainer>(jsonFile.text);

        foreach (var item in itemContainer.items)
        {
            itemDatabase.Add(item.id, item);
        }
    }

    void InitializeInventory()
    {
        inventory = new Dictionary<string, InventoryItem>();
    }

    public void AddItem(string itemId)
    {
        if (!inventory.ContainsKey(itemId) && itemDatabase.ContainsKey(itemId))
        {
            InventoryItem newItem = itemDatabase[itemId];
            inventory.Add(itemId, newItem);

            DialogueManager.Instance.SetFlag(newItem.acquiredFlag, true);

            Debug.Log($"Предмет {newItem.name} добавлен в инвентарь.");
        }
        else
        {
            Debug.Log($"Предмет {itemId} уже находится в инвентаре или не найден в базе данных.");
        }
    }

    public void RemoveItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            inventory.Remove(itemId);

            DialogueManager.Instance.SetFlag(item.acquiredFlag, false);

            Debug.Log($"Предмет {item.name} удален из инвентаря.");
        }
        else
        {
            Debug.Log($"Предмет {itemId} не найден в инвентаре.");
        }
    }

    public bool HasItem(string itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    public InventoryItem GetItem(string itemId)
    {
        if (itemDatabase.ContainsKey(itemId))
        {
            return itemDatabase[itemId];
        }
        Debug.LogError($"Предмет с ID {itemId} не найден в базе данных.");
        return null;
    }

    // Новый метод для получения всех предметов в инвентаре
    public Dictionary<string, InventoryItem> GetAllItems()
    {
        return inventory;
    }

    public void UseItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            Debug.Log($"Использован предмет {item.name}");

            RemoveItem(itemId);
        }
        else
        {
            Debug.LogError($"Предмет {itemId} отсутствует в инвентаре.");
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public string requiredFlag;
    public string acquiredFlag;
}

[System.Serializable]
public class ItemContainer
{
    public InventoryItem[] items;
}
