using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager; // Ссылка на менеджер диалогов
    public string dialogId; // ID диалога, который будет запускаться при взаимодействии
    public float interactionDistance = 5f; // Дистанция, на которой можно взаимодействовать
    private bool isInteracting = false; // Флаг, указывающий, идет ли взаимодействие

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryStartDialogue();
        }

        // Если игрок взаимодействует и больше не смотрит на NPC или отошел - завершить диалог
        if (isInteracting && (IsPlayerTooFar() || !IsPlayerLookingAtNPC()))
        {
            dialogueManager.EndDialogue();
            isInteracting = false;
        }
    }

    void TryStartDialogue()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            NPCInteraction npc = hit.collider.GetComponent<NPCInteraction>();

            if (npc != null && npc == this)
            {
                dialogueManager.StartDialogue(dialogId);
                isInteracting = true;
            }
        }
    }

    bool IsPlayerLookingAtNPC()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        return Physics.Raycast(ray, out hit, interactionDistance) && hit.collider.GetComponent<NPCInteraction>() == this;
    }

    bool IsPlayerTooFar()
    {
        return Vector3.Distance(transform.position, Camera.main.transform.position) > interactionDistance;
    }
}
