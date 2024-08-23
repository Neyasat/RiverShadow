using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Singleton instance
    public static DialogueManager Instance { get; private set; }

    // ������� ��� �������� ���� ������
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    // ���������� ��� UI � ������
    public TextAsset jsonFile; // ������ �� JSON ���� � ���������
    public GameObject dialogueUI; // ������ �� ������ UI ��� �������
    public TMP_Text dialogueText; // TMP_Text ������� ��� ����������� ������ �������
    public TMP_Text option1Text; // TMP_Text ������� ��� ������� �������� ������
    public TMP_Text option2Text; // TMP_Text ������� ��� ������� �������� ������
    public TMP_Text option3Text; // TMP_Text ������� ��� �������� �������� ������
    public TMP_Text characterNameText; // TMP_Text ������� ��� ����������� ����� ���������

    private string currentDialogId; // ������� ID �������
    private Dictionary<string, Dialogue> dialogues; // ������� ��� �������� ���� ��������

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��������� ������ ��� �������� ����� �������
        }
        else
        {
            Destroy(gameObject); // ���������� ������������� ����������
        }
    }

    void Start()
    {
        LoadDialogues(); // ��������� ������� �� JSON �����
        HideDialogueUI(); // �������� UI ������� ��� ������
    }

    void LoadDialogues()
    {
        // ��������� � ������ JSON ���� � ���������
        DialogueContainer dialogueContainer = JsonUtility.FromJson<DialogueContainer>(jsonFile.text);
        dialogues = new Dictionary<string, Dialogue>();
        foreach (var dialogue in dialogueContainer.dialogs)
        {
            dialogues[dialogue.id] = dialogue; // ��������� ������ ������ � ������� �� ��� ID
        }
    }

    public void StartDialogue(string dialogId)
    {
        currentDialogId = dialogId;
        ShowCurrentDialogue();
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true); // ���������� UI �������
        }
    }

    void ShowCurrentDialogue()
    {
        if (dialogues.ContainsKey(currentDialogId))
        {
            var dialogue = dialogues[currentDialogId];
            dialogueText.text = dialogue.text;

            // ���������� ��� ���������
            if (characterNameText != null)
            {
                characterNameText.text = dialogue.character;
            }

            option1Text.text = dialogue.responses.Length > 0 && CheckRequiredFlag(dialogue.responses[0]) ? dialogue.responses[0].text : "";
            option2Text.text = dialogue.responses.Length > 1 && CheckRequiredFlag(dialogue.responses[1]) ? dialogue.responses[1].text : "";
            option3Text.text = dialogue.responses.Length > 2 && CheckRequiredFlag(dialogue.responses[2]) ? dialogue.responses[2].text : "";

            // ��������� ��� ����������� ��������� ��������� � ����������� �� ������� ������
            option1Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option1Text.text));
            option2Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option2Text.text));
            option3Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option3Text.text));
        }
        else
        {
            Debug.LogError("������ �� ������: " + currentDialogId);
        }
    }

    bool CheckRequiredFlag(Response response)
    {
        if (string.IsNullOrEmpty(response.requiredFlag))
        {
            return true; // ���� ���� �� ������, ����� ��������
        }
        return flags.ContainsKey(response.requiredFlag) && flags[response.requiredFlag];
    }

    public void ChooseResponse(int responseIndex)
    {
        if (dialogues.ContainsKey(currentDialogId))
        {
            var dialogue = dialogues[currentDialogId];
            if (responseIndex >= 0 && responseIndex < dialogue.responses.Length)
            {
                var response = dialogue.responses[responseIndex];
                if (CheckRequiredFlag(response))
                {
                    flags[response.flag] = true; // ���������� ����� ���������� ������

                    Debug.Log("���� ����������: " + response.flag);
                    Debug.Log("�������� ����� " + response.flag + ": " + flags[response.flag]);

                    StartDialogue(response.nextDialog); // ������� � ���������� �������
                }
                else
                {
                    Debug.LogError("��������� ���� �����������: " + response.requiredFlag);
                }
            }
            else
            {
                Debug.LogError("�������� ������ ������");
            }
        }
    }

    public void EndDialogue()
    {
        Debug.Log("������ ��������.");
        currentDialogId = null; // �������� ������� ID �������
        HideDialogueUI(); // �������� UI �������
    }

    private void HideDialogueUI()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false); // �������� UI �������
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChooseResponse(0); // ����� ������� ������
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChooseResponse(1); // ����� ������� ������
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChooseResponse(2); // ����� �������� ������
        }
    }

    // ������ ��� ������ � �������

    public bool GetFlag(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            return flags[flagName];
        }
        return false; // ���� ���� �� ������, ������� false ��� ���������� ��-�������
    }

    public void SetFlag(string flagName, bool value)
    {
        if (flags.ContainsKey(flagName))
        {
            flags[flagName] = value;
        }
        else
        {
            flags.Add(flagName, value);
        }
    }

    // ����� ��� ��������� ���� ������
    public Dictionary<string, bool> GetAllFlags()
    {
        return flags;
    }
}

[System.Serializable]
public class DialogueContainer
{
    public Dialogue[] dialogs; // ������ ���� �������� � JSON
}

[System.Serializable]
public class Dialogue
{
    public string id; // ID �������
    public string character; // ��� ���������
    public string text; // ����� �������
    public Response[] responses; // ������ ��������� �������
}

[System.Serializable]
public class Response
{
    public string text; // ����� ������
    public string nextDialog; // ID ���������� �������
    public string flag; // ����, ������� ��������������� ��� ������ ����� ������
    public string requiredFlag; // ����, ����������� ��� ����������� ����� ������
}
