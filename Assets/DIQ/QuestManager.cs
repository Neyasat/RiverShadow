using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public TextAsset jsonFile; // Ссылка на JSON файл с квестами
    private Dictionary<string, Quest> quests; // Словарь для хранения всех квестов

    void Start()
    {
        // Инициализация должна происходить после DialogueManager
        if (DialogueManager.Instance != null)
        {
            LoadQuests();
        }
        else
        {
            Debug.LogError("DialogueManager не инициализирован. QuestManager требует DialogueManager для работы.");
        }
    }

    void LoadQuests()
    {
        // Загружаем и парсим JSON файл с квестами
        QuestContainer questContainer = JsonUtility.FromJson<QuestContainer>(jsonFile.text);
        quests = new Dictionary<string, Quest>();
        foreach (var quest in questContainer.quests)
        {
            quests[quest.id] = quest; // Сохраняем каждый квест в словарь по его ID
        }
    }

    public void StartQuest(string questId)
    {
        if (quests.ContainsKey(questId))
        {
            var quest = quests[questId];
            bool canStart = true;

            // Проверяем все необходимые флаги для старта квеста
            foreach (var flag in quest.requiredFlags)
            {
                if (!DialogueManager.Instance.GetFlag(flag))
                {
                    canStart = false;
                    Debug.Log($"Необходимый флаг {flag} не установлен. Квест {quest.name} не может быть начат.");
                    break;
                }
            }

            if (canStart)
            {
                Debug.Log($"Квест {quest.name} начат.");
                // Здесь вы можете добавить логику для отображения квеста в журнале квестов и т.д.
            }
        }
        else
        {
            Debug.LogError($"Квест с ID {questId} не найден.");
        }
    }

    public void CompleteQuest(string questId)
    {
        if (quests.ContainsKey(questId))
        {
            var quest = quests[questId];

            // Устанавливаем все флаги, которые отмечают выполнение квеста
            foreach (var flag in quest.completionFlags)
            {
                DialogueManager.Instance.SetFlag(flag, true);
            }

            Debug.Log($"Квест {quest.name} выполнен.");
        }
        else
        {
            Debug.LogError($"Квест с ID {questId} не найден.");
        }
    }
}

[System.Serializable]
public class QuestContainer
{
    public Quest[] quests; // Массив всех квестов в JSON
}

[System.Serializable]
public class Quest
{
    public string id; // ID квеста
    public string name; // Название квеста
    public string description; // Описание квеста
    public string[] requiredFlags; // Необходимые флаги для старта квеста
    public string[] completionFlags; // Флаги, которые устанавливаются при выполнении квеста
}
