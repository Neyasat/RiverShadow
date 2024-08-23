using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager; // ������ �� �������� ��������
    public string dialogId; // ID �������, ������� ����� ����������� ��� ��������������
    public float interactionDistance = 5f; // ���������, �� ������� ����� �����������������
    private bool isInteracting = false; // ����, �����������, ���� �� ��������������

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryStartDialogue();
        }

        // ���� ����� ��������������� � ������ �� ������� �� NPC ��� ������ - ��������� ������
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
