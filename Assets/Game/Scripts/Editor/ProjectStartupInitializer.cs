using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TowerDefense.Editor
{
    [InitializeOnLoad]
    public static class ProjectStartupInitializer
    {
        static ProjectStartupInitializer()
        {
            var mainSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scenes/Game.unity");
            if (mainSceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = mainSceneAsset;
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlaying)
                {
                    return;
                }

                var activeScene = SceneManager.GetActiveScene();
                if (string.IsNullOrEmpty(activeScene.path) || activeScene.path.Contains("SampleScene"))
                {
                    if (mainSceneAsset != null)
                    {
                        UnityEngine.Debug.Log("[ProjectStartupInitializer] Automatically opening main Game scene for you.");
                        EditorSceneManager.OpenScene("Assets/Game/Scenes/Game.unity", OpenSceneMode.Single);
                    }
                }
            };
        }
    }
}
