#if UNITY_EDITOR
using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.UI;
using TowerDefense.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace TowerDefense.EditorTools
{
    public static class GameSetupWizard
    {
        private const string GameRootPath = "Assets/Game";
        private const string BootstrapScenePath = "Assets/Game/Scenes/Bootstrap.unity";
        private const string SmokeScenePath = "Assets/Game/Scenes/Tech_WebGL_Smoke.unity";
        private const string EnemyPrefabPath = "Assets/Game/Prefabs/Enemies/Enemy.prefab";
        private const string EnemyDataFolderPath = "Assets/Game/Data/Enemies";
        private const string TowerDataFolderPath = "Assets/Game/Data/Towers";

        [MenuItem("Tools/Tower Defense/Setup/Create Core Scenes")]
        public static void SetupAll()
        {
            EnsureFolderTree();
            CreateBootstrapScene();
            CreateSmokeScene();
            AddScenesToBuildSettings();
            ApplyWebGlDefaults();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GameSetup] Done. Core scenes and runtime skeleton are ready.");
        }

        private static void EnsureFolderTree()
        {
            EnsureFolder("Assets", "Game");
            EnsureFolder(GameRootPath, "Art");
            EnsureFolder(GameRootPath, "Data");
            EnsureFolder(GameRootPath, "Prefabs");
            EnsureFolder(GameRootPath + "/Prefabs", "Core");
            EnsureFolder(GameRootPath + "/Prefabs", "Enemies");
            EnsureFolder(GameRootPath + "/Prefabs", "UI");
            EnsureFolder(GameRootPath + "/Prefabs", "World");
            EnsureFolder(GameRootPath, "Scenes");
            EnsureFolder(GameRootPath, "Scripts");
            EnsureFolder(GameRootPath + "/Scripts", "Core");
            EnsureFolder(GameRootPath + "/Scripts", "Enemies");
            EnsureFolder(GameRootPath + "/Scripts", "World");
            EnsureFolder(GameRootPath + "/Scripts", "UI");
            EnsureFolder(GameRootPath + "/Scripts", "Editor");
            EnsureFolder(GameRootPath + "/Data", "Enemies");
            EnsureFolder(GameRootPath + "/Data", "Towers");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var fullPath = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5;
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.11f, 0.15f, 0.2f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif

            var canvas = CreateCanvasRoot();
            var menuScreen = CreatePanel(canvas.transform, "MenuScreen", new Vector2(0f, 120f));
            var hudScreen = CreatePanel(canvas.transform, "HUDScreen", new Vector2(0f, -220f));
            var gameOverScreen = CreatePanel(canvas.transform, "GameOverScreen", new Vector2(0f, -120f));
            var gameWonScreen = CreatePanel(canvas.transform, "GameWonScreen", new Vector2(0f, -120f));

            CreateLabel(menuScreen.transform, "MenuLabel", "Tower Defense", Vector2.zero);
            var pveButton = CreateButton(menuScreen.transform, "PlayPvEButton", "Play Game", new Vector2(0f, -80f));

            var gold = CreateLabel(hudScreen.transform, "GoldLabel", "Gold: 300", new Vector2(-220f, 0f));
            var hp = CreateLabel(hudScreen.transform, "BaseHpLabel", "Base HP: 20", Vector2.zero);
            var round = CreateLabel(hudScreen.transform, "RoundLabel", "Round: 1", new Vector2(220f, 0f));
            CreateLabel(gameOverScreen.transform, "GameOverLabel", "Game Over", Vector2.zero);
            CreateLabel(gameWonScreen.transform, "GameWonLabel", "Victory!", Vector2.zero);

            var worldRoot = new GameObject("WorldRoot");
            var pathObject = new GameObject("WaypointPath");
            pathObject.transform.SetParent(worldRoot.transform, false);
            var path = pathObject.AddComponent<WaypointPath>();
            path.SetWaypoints(CreateDefaultWaypoints(pathObject.transform));

            var baseObject = new GameObject("DefenderBase");
            baseObject.transform.SetParent(worldRoot.transform, false);
            baseObject.transform.position = new Vector3(4f, 0f, 0f);
            var baseHealth = baseObject.AddComponent<BaseHealth>();

            var spawnerObject = new GameObject("EnemySpawner");
            spawnerObject.transform.SetParent(worldRoot.transform, false);
            spawnerObject.transform.position = new Vector3(-4f, 0f, 0f);
            var spawner = spawnerObject.AddComponent<EnemySpawner>();

            var enemyPrefab = GetOrCreateEnemyPrefab();
            // Ensure three enemy configs exist: Goblin, Orc, Ghost. Use Goblin as the default spawner config.
            var goblin = GetOrCreateEnemyConfig("Goblin", 30, 2f, 1, 10, 10);
            var orc = GetOrCreateEnemyConfig("Orc", 50, 1.4f, 2, 20, 20);
            var ghost = GetOrCreateEnemyConfig("Ghost", 20, 2.6f, 1, 15, 15);
            spawner.Configure(enemyPrefab, goblin, path, baseHealth);

            var gameRoot = new GameObject("GameRoot");
            var hudView = gameRoot.AddComponent<HudView>();
            hudView.Bind(gold, hp, round);
            var menuView = gameRoot.AddComponent<MenuView>();
            menuView.Bind(pveButton);

            var towerPlacement = gameRoot.AddComponent<TowerPlacementSystem>();
            towerPlacement.ConfigureTowerConfigs(GetOrCreateTowerConfigs());
            var screenRouter = gameRoot.AddComponent<UIScreenRouter>();
            screenRouter.Configure(menuScreen, hudScreen, gameOverScreen, gameWonScreen);
            var bootstrap = gameRoot.AddComponent<GameBootstrap>();
            bootstrap.Setup(screenRouter, hudView, spawner, baseHealth);
            menuView.ConfigureBootstrap(bootstrap);

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static List<Transform> CreateDefaultWaypoints(Transform parent)
        {
            var points = new List<Transform>();
            var positions = new[]
            {
                new Vector3(-4f, 0f, 0f),
                new Vector3(-2f, 0f, 0f),
                new Vector3(0f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(4f, 0f, 0f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var point = new GameObject($"WP_{i:00}");
                point.transform.SetParent(parent, false);
                point.transform.position = positions[i];
                points.Add(point.transform);
            }

            return points;
        }

        private static EnemyController GetOrCreateEnemyPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyController>(EnemyPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("Enemy");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<CircleCollider2D>();
            var enemy = go.AddComponent<EnemyController>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, EnemyPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<EnemyController>();
        }

        private static EnemyConfig GetOrCreateEnemyConfig(string name, int maxHealth, float moveSpeed, int baseDamage, int rewardGold, int spawnCost)
        {
            var assetPath = $"{EnemyDataFolderPath}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyConfig>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var config = ScriptableObject.CreateInstance<EnemyConfig>();
            config.name = name;
            config.SetRuntimeData(maxHealth, moveSpeed, baseDamage, rewardGold, spawnCost);
            AssetDatabase.CreateAsset(config, assetPath);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static List<TowerConfig> GetOrCreateTowerConfigs()
        {
            var configs = new List<TowerConfig>
            {
                GetOrCreateTowerConfig(TowerType.Archer, "Archer", 100, 10, 2.7f, 1.2f, 0.75f, new Color(0.35f, 0.9f, 0.35f, 1f)),
                GetOrCreateTowerConfig(TowerType.Mage, "Mage", 150, 14, 2.1f, 0.8f, 0.82f, new Color(0.5f, 0.6f, 1f, 1f)),
                GetOrCreateTowerConfig(TowerType.Freezer, "Freezer", 120, 8, 2.8f, 1f, 0.78f, new Color(0.45f, 0.95f, 1f, 1f)),
                GetOrCreateTowerConfig(TowerType.Cannon, "Cannon", 200, 24, 3.3f, 0.55f, 0.9f, new Color(0.55f, 0.62f, 0.72f, 1f))
            };

            return configs;
        }

        private static TowerConfig GetOrCreateTowerConfig(
            TowerType type,
            string towerName,
            int cost,
            int damage,
            float range,
            float attacksPerSecond,
            float previewScale,
            Color previewColor)
        {
            var assetPath = $"{TowerDataFolderPath}/{towerName}.asset";
            var config = AssetDatabase.LoadAssetAtPath<TowerConfig>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TowerConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }

            config.SetRuntimeData(type, towerName, cost, damage, range, attacksPerSecond, previewScale, previewColor);
            EditorUtility.SetDirty(config);

            return config;
        }

        private static void CreateSmokeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "SmokeTestMarker";
            marker.transform.position = Vector3.zero;

            var light = new GameObject("Main Light");
            light.AddComponent<Light>().type = LightType.Directional;

            EditorSceneManager.SaveScene(scene, SmokeScenePath);
        }

        private static GameObject CreateCanvasRoot()
        {
            var canvasObject = new GameObject("HUDCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500f, 80f);
            rectTransform.anchoredPosition = anchoredPosition;
            return panel;
        }

        private static Text CreateLabel(Transform parent, string name, string text, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(460f, 60f);
            rectTransform.anchoredPosition = anchoredPosition;

            return label;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var button = go.AddComponent<Button>();
            var textComponent = go.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 24;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 60f);
            rectTransform.anchoredPosition = anchoredPosition;

            button.targetGraphic = textComponent;

            return button;
        }

        private static void AddScenesToBuildSettings()
        {
            var sceneMap = new Dictionary<string, EditorBuildSettingsScene>();
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                sceneMap[buildScene.path] = buildScene;
            }

            sceneMap[BootstrapScenePath] = new EditorBuildSettingsScene(BootstrapScenePath, true);
            sceneMap[SmokeScenePath] = new EditorBuildSettingsScene(SmokeScenePath, true);

            EditorBuildSettings.scenes = new List<EditorBuildSettingsScene>(sceneMap.Values).ToArray();
        }

        private static void ApplyWebGlDefaults()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.runInBackground = true;
        }
    }
}
#endif
