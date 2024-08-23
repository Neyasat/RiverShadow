using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    public string itemId; // ID предмета, который будет собран или использован
    public float interactionDistance = 3f; // Дистанция для взаимодействия

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithItem();
        }
    }

    void TryInteractWithItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            ItemInteraction itemInteraction = hit.collider.GetComponent<ItemInteraction>();

            if (itemInteraction != null && itemInteraction == this)
            {
                InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager != null)
                {
                    InventoryItem item = inventoryManager.GetItem(itemId);

                    if (item != null && !inventoryManager.HasItem(itemId))
                    {
                        // Проверяем, установлен ли requiredFlag для предмета
                        if (string.IsNullOrEmpty(item.requiredFlag) || DialogueManager.Instance.GetFlag(item.requiredFlag))
                        {
                            inventoryManager.AddItem(itemId);
                            Destroy(gameObject); // Удаляем предмет с карты после сбора
                        }
                        else
                        {
                            Debug.Log($"Невозможно собрать {item.name}. Требуется флаг {item.requiredFlag}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"Предмет {itemId} уже есть в инвентаре или не найден.");
                    }
                }
                else
                {
                    Debug.LogError("InventoryManager не найден на сцене.");
                }
            }
        }
    }
}
