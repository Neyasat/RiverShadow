using UnityEngine;

public class FlagDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            PrintAllFlags();
        }
    }

    public void PrintAllFlags()
    {
        if (DialogueManager.Instance != null)
        {
            foreach (var flag in DialogueManager.Instance.GetAllFlags())
            {
                Debug.Log($"Flag: {flag.Key}, Value: {flag.Value}");
            }
        }
        else
        {
            Debug.LogError("DialogueManager.Instance не найден. Убедитесь, что DialogueManager инициализирован.");
        }
    }
}
