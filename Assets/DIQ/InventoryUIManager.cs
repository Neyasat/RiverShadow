using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventoryPanel; // Панель инвентаря (Panel)
    public GameObject itemSlotPrefab; // Префаб слота для предмета (может быть Text или Image)
    public Transform itemSlotContainer; // Контейнер для слотов предметов (Transform)

    private bool isInventoryOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false); // Скрываем инвентарь при старте игры
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) // Открытие/закрытие инвентаря по нажатию I
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UpdateInventoryUI();
        }
    }

    // Метод для обновления отображения предметов в UI
    public void UpdateInventoryUI()
    {
        // Очистка предыдущих слотов
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Получаем список всех предметов из InventoryManager
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            Dictionary<string, InventoryItem> items = inventoryManager.GetAllItems();

            // Создаем новый слот для каждого предмета
            foreach (KeyValuePair<string, InventoryItem> item in items)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);

                // Если используется Text
                Text itemText = itemSlot.GetComponent<Text>();
                if (itemText != null)
                {
                    itemText.text = item.Value.name;
                }

                // Если используется Image
                // Image itemImage = itemSlot.GetComponent<Image>();
                // if (itemImage != null && item.Value.icon != null)
                // {
                //     itemImage.sprite = item.Value.icon;
                // }
            }
        }
    }
}
