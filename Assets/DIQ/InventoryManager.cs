using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Словарь для хранения всех предметов из JSON
    private Dictionary<string, InventoryItem> itemDatabase;

    // Словарь для хранения предметов инвентаря
    private Dictionary<string, InventoryItem> inventory;

    // Ссылка на JSON файл с предметами
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

    // Метод для загрузки предметов из JSON файла
    void LoadItemsFromJson()
    {
        itemDatabase = new Dictionary<string, InventoryItem>();
        ItemContainer itemContainer = JsonUtility.FromJson<ItemContainer>(jsonFile.text);

        foreach (var item in itemContainer.items)
        {
            itemDatabase.Add(item.id, item);
        }
    }

    // Инициализация инвентаря
    void InitializeInventory()
    {
        inventory = new Dictionary<string, InventoryItem>();
    }

    // Метод для добавления предмета в инвентарь
    public void AddItem(string itemId)
    {
        if (!inventory.ContainsKey(itemId) && itemDatabase.ContainsKey(itemId))
        {
            InventoryItem newItem = itemDatabase[itemId];
            inventory.Add(itemId, newItem);

            // Устанавливаем флаг в DialogueManager, что предмет собран
            DialogueManager.Instance.SetFlag(newItem.acquiredFlag, true);

            Debug.Log($"Предмет {newItem.name} добавлен в инвентарь.");
        }
        else
        {
            Debug.Log($"Предмет {itemId} уже находится в инвентаре или не найден в базе данных.");
        }
    }

    // Метод для удаления предмета из инвентаря
    public void RemoveItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            inventory.Remove(itemId);

            // Устанавливаем флаг в DialogueManager, что предмет был удален или использован
            DialogueManager.Instance.SetFlag(item.acquiredFlag, false);

            Debug.Log($"Предмет {item.name} удален из инвентаря.");
        }
        else
        {
            Debug.Log($"Предмет {itemId} не найден в инвентаре.");
        }
    }

    // Метод для проверки наличия предмета в инвентаре
    public bool HasItem(string itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    // Метод для получения предмета по его ID
    public InventoryItem GetItem(string itemId)
    {
        if (itemDatabase.ContainsKey(itemId))
        {
            return itemDatabase[itemId];
        }
        Debug.LogError($"Предмет с ID {itemId} не найден в базе данных.");
        return null;
    }

    // Метод для использования предмета
    public void UseItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            Debug.Log($"Использован предмет {item.name}");

            // Здесь вы можете добавить логику, которая будет выполнена при использовании предмета

            // После использования предмета, удалите его из инвентаря
            RemoveItem(itemId);
        }
        else
        {
            Debug.LogError($"Предмет {itemId} отсутствует в инвентаре.");
        }
    }
}

// Класс для представления предмета инвентаря
[System.Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public string requiredFlag;
    public string acquiredFlag;
}

// Класс для контейнера предметов из JSON
[System.Serializable]
public class ItemContainer
{
    public InventoryItem[] items;
}
