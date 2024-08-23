using UnityEngine;

public class UseItemInteraction : MonoBehaviour
{
    public string requiredItemId; // ID предмета, который можно использовать на этом объекте
    public float interactionDistance = 3f; // Дистанция для взаимодействия

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // U - клавиша для использования предмета
        {
            TryUseItem();
        }
    }

    void TryUseItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Проверяем, что игрок смотрит на объект с этим скриптом
            UseItemInteraction useItemInteraction = hit.collider.GetComponent<UseItemInteraction>();

            if (useItemInteraction != null && useItemInteraction == this)
            {
                InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager != null)
                {
                    if (inventoryManager.HasItem(requiredItemId))
                    {
                        Debug.Log($"Используем предмет {requiredItemId} на объекте {gameObject.name}");
                        inventoryManager.UseItem(requiredItemId);

                        // Здесь можно добавить логику, которая выполнится при успешном использовании предмета
                        // Например, открыть дверь, активировать механизм и т.д.
                    }
                    else
                    {
                        Debug.Log($"У вас нет предмета {requiredItemId}.");
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
