using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextAsset jsonFile; // Ссылка на JSON файл с диалогами
    private Dictionary<string, Dialogue> dialogues; // Словарь для хранения всех диалогов
    private string currentDialogId; // Текущий ID диалога
    private Dictionary<string, bool> flags = new Dictionary<string, bool>(); // Словарь для хранения флагов
    public GameObject dialogueUI; // Ссылка на объект UI для диалога
    public TMP_Text dialogueText; // TMP_Text элемент для отображения текста диалога
    public TMP_Text option1Text; // TMP_Text элемент для первого варианта ответа
    public TMP_Text option2Text; // TMP_Text элемент для второго варианта ответа
    public TMP_Text option3Text; // TMP_Text элемент для третьего варианта ответа
    public TMP_Text characterNameText; // TMP_Text элемент для отображения имени персонажа

    void Start()
    {
        LoadDialogues(); // Загружаем диалоги из JSON файла
        HideDialogueUI(); // Скрываем UI диалога при старте
    }

    void LoadDialogues()
    {
        DialogueContainer dialogueContainer = JsonUtility.FromJson<DialogueContainer>(jsonFile.text);
        dialogues = new Dictionary<string, Dialogue>();
        foreach (var dialogue in dialogueContainer.dialogs)
        {
            dialogues[dialogue.id] = dialogue; // Сохраняем каждый диалог в словарь по его ID
        }
    }

    public void StartDialogue(string dialogId)
    {
        currentDialogId = dialogId;
        ShowCurrentDialogue();
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true); // Показываем UI диалога
        }
    }

    void ShowCurrentDialogue()
    {
        if (dialogues.ContainsKey(currentDialogId))
        {
            var dialogue = dialogues[currentDialogId];
            dialogueText.text = dialogue.text;

            // Отображаем имя персонажа
            if (characterNameText != null)
            {
                characterNameText.text = dialogue.character;
            }

            option1Text.text = dialogue.responses.Length > 0 && CheckRequiredFlag(dialogue.responses[0]) ? dialogue.responses[0].text : "";
            option2Text.text = dialogue.responses.Length > 1 && CheckRequiredFlag(dialogue.responses[1]) ? dialogue.responses[1].text : "";
            option3Text.text = dialogue.responses.Length > 2 && CheckRequiredFlag(dialogue.responses[2]) ? dialogue.responses[2].text : "";

            // Активация или деактивация текстовых элементов в зависимости от наличия ответа
            option1Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option1Text.text));
            option2Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option2Text.text));
            option3Text.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(option3Text.text));
        }
        else
        {
            Debug.LogError("Диалог не найден: " + currentDialogId);
        }
    }

    bool CheckRequiredFlag(Response response)
    {
        if (string.IsNullOrEmpty(response.requiredFlag))
        {
            return true; // Если флаг не указан, ответ доступен
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
                    flags[response.flag] = true; // Сохранение флага выбранного ответа

                    Debug.Log("Флаг установлен: " + response.flag);
                    Debug.Log("Значение флага " + response.flag + ": " + flags[response.flag]);

                    StartDialogue(response.nextDialog); // Переход к следующему диалогу
                }
                else
                {
                    Debug.LogError("Требуемый флаг отсутствует: " + response.requiredFlag);
                }
            }
            else
            {
                Debug.LogError("Неверный индекс ответа");
            }
        }
    }

    public void EndDialogue()
    {
        Debug.Log("Диалог завершен.");
        currentDialogId = null; // Обнуляем текущий ID диалога
        HideDialogueUI(); // Скрываем UI диалога
    }

    private void HideDialogueUI()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false); // Скрываем UI диалога
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChooseResponse(0); // Выбор первого ответа
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChooseResponse(1); // Выбор второго ответа
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChooseResponse(2); // Выбор третьего ответа
        }
    }
}

[System.Serializable]
public class DialogueContainer
{
    public Dialogue[] dialogs; // Массив всех диалогов в JSON
}

[System.Serializable]
public class Dialogue
{
    public string id; // ID диалога
    public string character; // Имя персонажа
    public string text; // Текст диалога
    public Response[] responses; // Массив возможных ответов
}

[System.Serializable]
public class Response
{
    public string text; // Текст ответа
    public string nextDialog; // ID следующего диалога
    public string flag; // Флаг, который устанавливается при выборе этого ответа
    public string requiredFlag; // Флаг, необходимый для отображения этого ответа
}
