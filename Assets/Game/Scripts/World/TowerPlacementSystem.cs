using System;
using System.Collections.Generic;
using TowerDefense.UI;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TowerDefense.World
{
    public sealed class TowerPlacementSystem : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int gridWidth = 12;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2 origin = new(-6f, -4f);

        [Header("Economy")]
        [SerializeField] private int startGold = 300;

        [Header("Placement Menu")]
        [SerializeField] private List<TowerConfig> towerConfigs = new();

        [Header("UI Dependencies")]
        [SerializeField] private GameObject radialMenuPrefab;

        private static Sprite fallbackSprite;

        private readonly HashSet<Vector2Int> occupiedCells = new();
        private readonly HashSet<Vector2Int> blockedCells = new();

        private HudView hudView;
        private WaypointPath waypointPath;
        private int currentGold;
        private bool menuOpen;
        private Vector2Int selectedCell;

        // IMGUI rect replaced by using real unity UI partially or just completely removing it to rely on HUD
        
        public void Configure(WaypointPath path, HudView hud)
        {
            waypointPath = path;
            hudView = hud;
            RebuildBlockedCells();
        }

        public void ConfigureTowerConfigs(List<TowerConfig> configs)
        {
            towerConfigs = configs != null ? new List<TowerConfig>(configs) : new List<TowerConfig>();
            if (towerConfigs.Count == 0)
            {
                towerConfigs.AddRange(CreateRuntimeDefaults());
            }
        }

        private void Awake()
        {
            if (towerConfigs.Count == 0)
            {
                towerConfigs.AddRange(CreateRuntimeDefaults());
            }
        }

        private void Start()
        {
            currentGold = Mathf.Max(0, startGold);
            hudView?.SetGold(currentGold);
            RebuildBlockedCells();
        }

        private void Update()
        {
            if (!IsPrimaryPointerPressedThisFrame() || IsPointerOverUi())
            {
                return;
            }

            if (!TryGetCellUnderCursor(out var cell))
            {
                CloseMenu();
                return;
            }

            if (IsBlocked(cell) || occupiedCells.Contains(cell))
            {
                CloseMenu();
                return;
            }

            selectedCell = cell;
            OpenMenu(cell);
        }

        private void OpenMenu(Vector2Int cell)
        {
            menuOpen = true;
            if (hudView != null)
            {
                // We show UI relative to the cell center
                var center = GetCellCenter(cell);
                var actions = new List<Action<UnityEngine.UI.Button, int>>();

                for (int i = 0; i < towerConfigs.Count; i++)
                {
                    var config = towerConfigs[i];
                    if (config == null) continue;

                    actions.Add((btn, index) =>
                    {
                        var text = btn.GetComponentInChildren<UnityEngine.UI.Text>();
                        if (text != null)
                        {
                            text.text = $"{config.DisplayName}\n({config.Cost}G)";
                        }

                        btn.onClick.AddListener(() => TryPurchase(config));
                        
                        // Layout positioning logic for a simple radial/grid menu around center
                        var rect = btn.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.sizeDelta = new Vector2(100f, 44f);
                            // simple layout offset based on index
                            float angle = index * (Mathf.PI * 2f / towerConfigs.Count);
                            float radius = 80f; // UI pixels
                            rect.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                        }
                    });
                }
                
                // Add Cancel button
                actions.Add((btn, index) =>
                {
                     var text = btn.GetComponentInChildren<UnityEngine.UI.Text>();
                     if (text != null)
                     {
                         text.text = "Cancel";
                     }
                     btn.onClick.AddListener(CloseMenu);
                     var rect = btn.GetComponent<RectTransform>();
                     if (rect != null)
                     {
                         rect.sizeDelta = new Vector2(80f, 32f);
                         rect.anchoredPosition = Vector2.zero; // center
                     }
                });

                hudView.ShowTowerMenu(center, actions);
            }
        }

        private void CloseMenu()
        {
            menuOpen = false;
            if (hudView != null)
            {
                hudView.HideTowerMenu();
            }
        }

        // Remove OnGUI completely
        // Remove DrawPlacementMenu completely

        private bool TryPurchase(TowerConfig config)
        {
            if (config == null || config.Cost > currentGold || IsBlocked(selectedCell) || occupiedCells.Contains(selectedCell))
            {
                CloseMenu();
                return false;
            }

            SpawnTowerPlaceholder(selectedCell, config);
            occupiedCells.Add(selectedCell);
            currentGold -= config.Cost;
            hudView?.SetGold(currentGold);
            CloseMenu();
            return true;
        }

        private static List<TowerConfig> CreateRuntimeDefaults()
        {
            return new List<TowerConfig>
            {
                CreateRuntimeTowerConfig(TowerType.Archer, "Archer", 100, 10, 2.7f, 1.2f, 0.75f, new Color(0.35f, 0.9f, 0.35f, 1f)),
                CreateRuntimeTowerConfig(TowerType.Mage, "Mage", 150, 14, 2.1f, 0.8f, 0.82f, new Color(0.5f, 0.6f, 1f, 1f)),
                CreateRuntimeTowerConfig(TowerType.Freezer, "Freezer", 120, 8, 2.8f, 1.0f, 0.78f, new Color(0.45f, 0.95f, 1f, 1f)),
                CreateRuntimeTowerConfig(TowerType.Cannon, "Cannon", 200, 24, 3.3f, 0.55f, 0.9f, new Color(0.55f, 0.62f, 0.72f, 1f))
            };
        }

        private static TowerConfig CreateRuntimeTowerConfig(
            TowerType type,
            string displayName,
            int cost,
            int damage,
            float range,
            float attacksPerSecond,
            float previewScale,
            Color previewColor)
        {
            var config = ScriptableObject.CreateInstance<TowerConfig>();
            config.name = $"Runtime{type}Config";
            config.SetRuntimeData(type, displayName, cost, damage, range, attacksPerSecond, previewScale, previewColor);
            return config;
        }

        private void SpawnTowerPlaceholder(Vector2Int cell, TowerConfig config)
        {
            var tower = new GameObject($"{config.Type}_Tower_{cell.x}_{cell.y}");
            tower.transform.position = GetCellCenter(cell);

            var renderer = tower.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFallbackSprite();
            renderer.color = config.PreviewColor;
            renderer.sortingOrder = 2;

            var scale = cellSize * config.PreviewScale;
            tower.transform.localScale = new Vector3(scale, scale, 1f);

            var top = new GameObject("TypeTop");
            top.transform.SetParent(tower.transform, false);
            top.transform.localPosition = new Vector3(0f, scale * 0.38f, 0f);
            top.transform.localScale = GetTypeTopScale(config.Type, scale);
            var topRenderer = top.AddComponent<SpriteRenderer>();
            topRenderer.sprite = GetFallbackSprite();
            topRenderer.color = Color.Lerp(config.PreviewColor, Color.white, 0.25f);
            topRenderer.sortingOrder = 3;

            var placedTower = tower.AddComponent<PlacedTower>();
            placedTower.Configure(config);

            var combatController = tower.AddComponent<TowerCombatController>();
            combatController.Configure(config);
        }

        private static Vector3 GetTypeTopScale(TowerType type, float baseScale)
        {
            return type switch
            {
                TowerType.Archer => new Vector3(baseScale * 0.35f, baseScale * 0.18f, 1f),
                TowerType.Mage => new Vector3(baseScale * 0.22f, baseScale * 0.35f, 1f),
                TowerType.Freezer => new Vector3(baseScale * 0.42f, baseScale * 0.12f, 1f),
                TowerType.Cannon => new Vector3(baseScale * 0.48f, baseScale * 0.22f, 1f),
                _ => new Vector3(baseScale * 0.25f, baseScale * 0.25f, 1f)
            };
        }

        private void RebuildBlockedCells()
        {
            blockedCells.Clear();
            if (waypointPath == null)
            {
                return;
            }

            for (var i = 0; i < waypointPath.Count; i++)
            {
                var cell = WorldToCell(waypointPath.GetPosition(i));
                if (IsInsideGrid(cell))
                {
                    blockedCells.Add(cell);
                }
            }
        }

        private bool TryGetCellUnderCursor(out Vector2Int cell)
        {
            if (!TryGetPointerScreenPosition(out var pointerPosition))
            {
                cell = default;
                return false;
            }

            if (Camera.main == null)
            {
                cell = default;
                return false;
            }

            var world = Camera.main.ScreenToWorldPoint(new Vector3(pointerPosition.x, pointerPosition.y, 0f));
            world.z = 0f;
            cell = WorldToCell(world);
            return IsInsideGrid(cell);
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private static bool IsPrimaryPointerPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
#else
            screenPosition = Input.mousePosition;
            return true;
#endif
        }

        /*
        private bool IsPointerOverPlacementMenu()
        {
            return false; // Using full UI now, UI clicks are stopped by IsPointerOverUi()
        }
        */
        private Vector2Int WorldToCell(Vector3 world)
        {
            var x = Mathf.FloorToInt((world.x - origin.x) / cellSize);
            var y = Mathf.FloorToInt((world.y - origin.y) / cellSize);
            return new Vector2Int(x, y);
        }

        private Vector3 GetCellCenter(Vector2Int cell)
        {
            var x = origin.x + (cell.x + 0.5f) * cellSize;
            var y = origin.y + (cell.y + 0.5f) * cellSize;
            return new Vector3(x, y, 0f);
        }

        private bool IsBlocked(Vector2Int cell)
        {
            return blockedCells.Contains(cell);
        }

        private bool IsInsideGrid(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return fallbackSprite;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.25f, 0.9f, 0.9f, 0.6f);
            for (var y = 0; y < gridHeight; y++)
            {
                for (var x = 0; x < gridWidth; x++)
                {
                    var center = GetCellCenter(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
                }
            }

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.8f);
            foreach (var cell in blockedCells)
            {
                var center = GetCellCenter(cell);
                Gizmos.DrawCube(center, new Vector3(cellSize * 0.25f, cellSize * 0.25f, 0f));
            }
        }
    }
}
