using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class AutoFixer
{
    [MenuItem("Tools/Fix Pink Background & Text")]
    public static void FixProject()
    {
        Debug.Log("<color=yellow><b>AUTO-FIXER:</b> Fixing Render Pipeline and TextMesh Pro issues...</color>");

        // 1. Force GraphicsSettings to use URP to fix the broken Converter Window and pink backgrounds
        string urpPath = AssetDatabase.GUIDToAssetPath("681886c5eb7344803b6206f758bf0b1c");
        if (!string.IsNullOrEmpty(urpPath))
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(urpPath);
            if (pipeline != null)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
                QualitySettings.renderPipeline = pipeline;
                Debug.Log("-> Restored URP Render Pipeline Asset successfully.");
            }
        }

        // 2. Fix any missing or Standard materials to URP 2D Shader automatically
        int fixedMaterials = 0;
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        Shader spriteLit = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (spriteLit == null) spriteLit = Shader.Find("Sprites/Default");

        foreach (var mGuid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(mGuid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                if (mat.shader == null || mat.shader.name.Contains("InternalError") || mat.shader.name == "Standard")
                {
                    if (spriteLit != null)
                    {
                        mat.shader = spriteLit;
                        EditorUtility.SetDirty(mat);
                        fixedMaterials++;
                    }
                }
            }
        }
        Debug.Log($"-> Fixed {fixedMaterials} pink/broken materials.");

        // 3. Force re-import TextMesh Pro to apply the corrected .meta files we restored
        AssetDatabase.ImportAsset("Assets/TextMesh Pro", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        Debug.Log("-> Re-imported TextMesh Pro to restore fonts.");

        AssetDatabase.SaveAssets();

        Debug.Log("<color=green><b>PROJECT AUTO-FIXED SUCCESSFULLY!</b></color> All materials and fonts should be restored.");
    }
}
