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

        private static Sprite fallbackSprite;

        private readonly HashSet<Vector2Int> occupiedCells = new();
        private readonly HashSet<Vector2Int> blockedCells = new();

        private HudView hudView;
        private WaypointPath waypointPath;
        private int currentGold;
        private bool menuOpen;
        private Vector2Int selectedCell;
        private Rect menuRect = new(12f, 12f, 260f, 180f);

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
                towerConfigs.Add(CreateRuntimeFallbackConfig());
            }
        }

        private void Awake()
        {
            if (towerConfigs.Count == 0)
            {
                towerConfigs.Add(CreateRuntimeFallbackConfig());
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

            if (menuOpen && IsPointerOverPlacementMenu())
            {
                // Let IMGUI window consume click events (Buy/Cancel/drag) without world placement logic.
                return;
            }

            if (!TryGetCellUnderCursor(out var cell))
            {
                menuOpen = false;
                return;
            }

            if (IsBlocked(cell) || occupiedCells.Contains(cell))
            {
                menuOpen = false;
                return;
            }

            selectedCell = cell;
            menuOpen = true;
        }

        private void OnGUI()
        {
            if (!menuOpen)
            {
                return;
            }

            menuRect = GUI.Window(737, menuRect, DrawPlacementMenu, "Tower Purchase");
        }

        private void DrawPlacementMenu(int windowId)
        {
            var y = 28f;
            GUI.Label(new Rect(12f, y, 236f, 20f), $"Cell: ({selectedCell.x}, {selectedCell.y})");
            y += 24f;
            GUI.Label(new Rect(12f, y, 236f, 20f), $"Gold: {currentGold}");
            y += 28f;

            for (var i = 0; i < towerConfigs.Count; i++)
            {
                var config = towerConfigs[i];
                if (config == null)
                {
                    continue;
                }

                var buttonLabel = $"Buy {config.DisplayName} ({config.Cost})";
                if (GUI.Button(new Rect(12f, y, 236f, 28f), buttonLabel))
                {
                    TryPurchase(config);
                }

                y += 34f;
            }

            if (GUI.Button(new Rect(12f, y, 236f, 28f), "Cancel"))
            {
                menuOpen = false;
            }

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private bool TryPurchase(TowerConfig config)
        {
            if (config == null || config.Cost > currentGold || IsBlocked(selectedCell) || occupiedCells.Contains(selectedCell))
            {
                return false;
            }

            SpawnTowerPlaceholder(selectedCell, config.PreviewColor);
            occupiedCells.Add(selectedCell);
            currentGold -= config.Cost;
            hudView?.SetGold(currentGold);
            menuOpen = false;
            return true;
        }

        private static TowerConfig CreateRuntimeFallbackConfig()
        {
            var config = ScriptableObject.CreateInstance<TowerConfig>();
            config.name = "RuntimeTowerConfig";
            return config;
        }

        private void SpawnTowerPlaceholder(Vector2Int cell, Color color)
        {
            var tower = new GameObject($"Tower_{cell.x}_{cell.y}");
            tower.transform.position = GetCellCenter(cell);

            var renderer = tower.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFallbackSprite();
            renderer.color = color;
            renderer.sortingOrder = 2;

            var scale = cellSize * 0.75f;
            tower.transform.localScale = new Vector3(scale, scale, 1f);
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

        private bool IsPointerOverPlacementMenu()
        {
            if (!menuOpen || !TryGetPointerScreenPosition(out var pointerScreen))
            {
                return false;
            }

            // Input System screen origin is bottom-left; IMGUI uses top-left.
            var guiY = Screen.height - pointerScreen.y;
            return menuRect.Contains(new Vector2(pointerScreen.x, guiY));
        }

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



