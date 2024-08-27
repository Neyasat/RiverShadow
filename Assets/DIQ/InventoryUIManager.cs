using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventoryPanel; // ������ ��������� (Panel)
    public GameObject itemSlotPrefab; // ������ ����� ��� �������� (����� ���� Text ��� Image)
    public Transform itemSlotContainer; // ��������� ��� ������ ��������� (Transform)

    private bool isInventoryOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false); // �������� ��������� ��� ������ ����
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) // ��������/�������� ��������� �� ������� I
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

    // ����� ��� ���������� ����������� ��������� � UI
    public void UpdateInventoryUI()
    {
        // ������� ���������� ������
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // �������� ������ ���� ��������� �� InventoryManager
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            Dictionary<string, InventoryItem> items = inventoryManager.GetAllItems();

            // ������� ����� ���� ��� ������� ��������
            foreach (KeyValuePair<string, InventoryItem> item in items)
            {
                GameObject itemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);

                // ���� ������������ Text
                Text itemText = itemSlot.GetComponent<Text>();
                if (itemText != null)
                {
                    itemText.text = item.Value.name;
                }

                // ���� ������������ Image
                // Image itemImage = itemSlot.GetComponent<Image>();
                // if (itemImage != null && item.Value.icon != null)
                // {
                //     itemImage.sprite = item.Value.icon;
                // }
            }
        }
    }
}
