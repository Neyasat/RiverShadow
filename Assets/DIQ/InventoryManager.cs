using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // ������� ��� �������� ���� ��������� �� JSON
    private Dictionary<string, InventoryItem> itemDatabase;

    // ������� ��� �������� ��������� ���������
    private Dictionary<string, InventoryItem> inventory;

    // ������ �� JSON ���� � ����������
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
            Debug.LogError("DialogueManager �� ���������������. InventoryManager ������� DialogueManager ��� ������.");
        }
    }

    // ����� ��� �������� ��������� �� JSON �����
    void LoadItemsFromJson()
    {
        itemDatabase = new Dictionary<string, InventoryItem>();
        ItemContainer itemContainer = JsonUtility.FromJson<ItemContainer>(jsonFile.text);

        foreach (var item in itemContainer.items)
        {
            itemDatabase.Add(item.id, item);
        }
    }

    // ������������� ���������
    void InitializeInventory()
    {
        inventory = new Dictionary<string, InventoryItem>();
    }

    // ����� ��� ���������� �������� � ���������
    public void AddItem(string itemId)
    {
        if (!inventory.ContainsKey(itemId) && itemDatabase.ContainsKey(itemId))
        {
            InventoryItem newItem = itemDatabase[itemId];
            inventory.Add(itemId, newItem);

            // ������������� ���� � DialogueManager, ��� ������� ������
            DialogueManager.Instance.SetFlag(newItem.acquiredFlag, true);

            Debug.Log($"������� {newItem.name} �������� � ���������.");
        }
        else
        {
            Debug.Log($"������� {itemId} ��� ��������� � ��������� ��� �� ������ � ���� ������.");
        }
    }

    // ����� ��� �������� �������� �� ���������
    public void RemoveItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            inventory.Remove(itemId);

            // ������������� ���� � DialogueManager, ��� ������� ��� ������ ��� �����������
            DialogueManager.Instance.SetFlag(item.acquiredFlag, false);

            Debug.Log($"������� {item.name} ������ �� ���������.");
        }
        else
        {
            Debug.Log($"������� {itemId} �� ������ � ���������.");
        }
    }

    // ����� ��� �������� ������� �������� � ���������
    public bool HasItem(string itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    // ����� ��� ��������� �������� �� ��� ID
    public InventoryItem GetItem(string itemId)
    {
        if (itemDatabase.ContainsKey(itemId))
        {
            return itemDatabase[itemId];
        }
        Debug.LogError($"������� � ID {itemId} �� ������ � ���� ������.");
        return null;
    }

    // ����� ��� ������������� ��������
    public void UseItem(string itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            InventoryItem item = inventory[itemId];
            Debug.Log($"����������� ������� {item.name}");

            // ����� �� ������ �������� ������, ������� ����� ��������� ��� ������������� ��������

            // ����� ������������� ��������, ������� ��� �� ���������
            RemoveItem(itemId);
        }
        else
        {
            Debug.LogError($"������� {itemId} ����������� � ���������.");
        }
    }
}

// ����� ��� ������������� �������� ���������
[System.Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public string requiredFlag;
    public string acquiredFlag;
}

// ����� ��� ���������� ��������� �� JSON
[System.Serializable]
public class ItemContainer
{
    public InventoryItem[] items;
}
