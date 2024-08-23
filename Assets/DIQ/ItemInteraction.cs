using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    public string itemId; // ID ��������, ������� ����� ������ ��� �����������
    public float interactionDistance = 3f; // ��������� ��� ��������������

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
                        // ���������, ���������� �� requiredFlag ��� ��������
                        if (string.IsNullOrEmpty(item.requiredFlag) || DialogueManager.Instance.GetFlag(item.requiredFlag))
                        {
                            inventoryManager.AddItem(itemId);
                            Destroy(gameObject); // ������� ������� � ����� ����� �����
                        }
                        else
                        {
                            Debug.Log($"���������� ������� {item.name}. ��������� ���� {item.requiredFlag}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"������� {itemId} ��� ���� � ��������� ��� �� ������.");
                    }
                }
                else
                {
                    Debug.LogError("InventoryManager �� ������ �� �����.");
                }
            }
        }
    }
}
