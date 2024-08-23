using UnityEngine;

public class QuestInteraction : MonoBehaviour
{
    public string questId; // ID квеста, который будет запускаться при взаимодействии
    public float interactionDistance = 3f; // Дистанция для взаимодействия

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryStartQuest();
        }
    }

    void TryStartQuest()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            QuestInteraction questInteraction = hit.collider.GetComponent<QuestInteraction>();

            if (questInteraction != null && questInteraction == this)
            {
                QuestManager questManager = FindObjectOfType<QuestManager>();
                if (questManager != null)
                {
                    questManager.StartQuest(questId);
                }
                else
                {
                    Debug.LogError("QuestManager не найден на сцене.");
                }
            }
        }
    }
}
