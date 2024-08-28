using UnityEngine;
using UnityEditor;

public class FixMissingShaders : MonoBehaviour
{
    [MenuItem("Tools/Fix Missing Shaders")]
    static void FixAllMissingShaders()
    {
        // ����� ��� ��������� � �������
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();

        foreach (Material mat in allMaterials)
        {
            // ���� � ��������� ����������� ������
            if (mat.shader.name == "Hidden/InternalErrorShader" || mat.shader == null)
            {
                // �������� �� ����������� ������
                mat.shader = Shader.Find("Standard");
                Debug.Log("�������� ��������: " + mat.name);
            }
        }

        // �������������� ���������� � ���������� ���������
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
