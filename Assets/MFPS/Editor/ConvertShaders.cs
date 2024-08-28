using UnityEngine;
using UnityEditor;

public class FixMissingShaders : MonoBehaviour
{
    [MenuItem("Tools/Fix Missing Shaders")]
    static void FixAllMissingShaders()
    {
        // Найти все материалы в проекте
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();

        foreach (Material mat in allMaterials)
        {
            // Если у материала отсутствует шейдер
            if (mat.shader.name == "Hidden/InternalErrorShader" || mat.shader == null)
            {
                // Заменить на стандартный шейдер
                mat.shader = Shader.Find("Standard");
                Debug.Log("Обновлен материал: " + mat.name);
            }
        }

        // Принудительное обновление и сохранение изменений
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
