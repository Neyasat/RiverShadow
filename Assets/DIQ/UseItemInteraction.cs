using UnityEngine;

public class UseItemInteraction : MonoBehaviour
{
    public string requiredItemId; // ID ��������, ������� ����� ������������ �� ���� �������
    public float interactionDistance = 3f; // ��������� ��� ��������������

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // U - ������� ��� ������������� ��������
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
            // ���������, ��� ����� ������� �� ������ � ���� ��������
            UseItemInteraction useItemInteraction = hit.collider.GetComponent<UseItemInteraction>();

            if (useItemInteraction != null && useItemInteraction == this)
            {
                InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager != null)
                {
                    if (inventoryManager.HasItem(requiredItemId))
                    {
                        Debug.Log($"���������� ������� {requiredItemId} �� ������� {gameObject.name}");
                        inventoryManager.UseItem(requiredItemId);

                        // ����� ����� �������� ������, ������� ���������� ��� �������� ������������� ��������
                        // ��������, ������� �����, ������������ �������� � �.�.
                    }
                    else
                    {
                        Debug.Log($"� ��� ��� �������� {requiredItemId}.");
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
