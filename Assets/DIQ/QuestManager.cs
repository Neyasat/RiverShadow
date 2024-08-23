using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public TextAsset jsonFile; // ������ �� JSON ���� � ��������
    private Dictionary<string, Quest> quests; // ������� ��� �������� ���� �������

    void Start()
    {
        // ������������� ������ ����������� ����� DialogueManager
        if (DialogueManager.Instance != null)
        {
            LoadQuests();
        }
        else
        {
            Debug.LogError("DialogueManager �� ���������������. QuestManager ������� DialogueManager ��� ������.");
        }
    }

    void LoadQuests()
    {
        // ��������� � ������ JSON ���� � ��������
        QuestContainer questContainer = JsonUtility.FromJson<QuestContainer>(jsonFile.text);
        quests = new Dictionary<string, Quest>();
        foreach (var quest in questContainer.quests)
        {
            quests[quest.id] = quest; // ��������� ������ ����� � ������� �� ��� ID
        }
    }

    public void StartQuest(string questId)
    {
        if (quests.ContainsKey(questId))
        {
            var quest = quests[questId];
            bool canStart = true;

            // ��������� ��� ����������� ����� ��� ������ ������
            foreach (var flag in quest.requiredFlags)
            {
                if (!DialogueManager.Instance.GetFlag(flag))
                {
                    canStart = false;
                    Debug.Log($"����������� ���� {flag} �� ����������. ����� {quest.name} �� ����� ���� �����.");
                    break;
                }
            }

            if (canStart)
            {
                Debug.Log($"����� {quest.name} �����.");
                // ����� �� ������ �������� ������ ��� ����������� ������ � ������� ������� � �.�.
            }
        }
        else
        {
            Debug.LogError($"����� � ID {questId} �� ������.");
        }
    }

    public void CompleteQuest(string questId)
    {
        if (quests.ContainsKey(questId))
        {
            var quest = quests[questId];

            // ������������� ��� �����, ������� �������� ���������� ������
            foreach (var flag in quest.completionFlags)
            {
                DialogueManager.Instance.SetFlag(flag, true);
            }

            Debug.Log($"����� {quest.name} ��������.");
        }
        else
        {
            Debug.LogError($"����� � ID {questId} �� ������.");
        }
    }
}

[System.Serializable]
public class QuestContainer
{
    public Quest[] quests; // ������ ���� ������� � JSON
}

[System.Serializable]
public class Quest
{
    public string id; // ID ������
    public string name; // �������� ������
    public string description; // �������� ������
    public string[] requiredFlags; // ����������� ����� ��� ������ ������
    public string[] completionFlags; // �����, ������� ��������������� ��� ���������� ������
}
