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

        [Header("Placement Menu")]
        public List<TowerConfig> towerConfigs = new();

        [Header("UI Dependencies")]
        [SerializeField] private GameObject radialMenuPrefab;

        private static Sprite fallbackSprite;

        private readonly HashSet<Vector2Int> occupiedCells = new();
        private readonly HashSet<Vector2Int> blockedCells = new();

        private HudView hudView;
        private WaypointPath waypointPath;
        private Vector2Int selectedCell;

        private Func<int, bool> spendGoldCallback;
        private Func<int> getGoldCallback;
        
        public void Configure(WaypointPath path, HudView hud, Func<int, bool> spendGold, Func<int> getGold)
        {
            waypointPath = path;
            hudView = hud;
            spendGoldCallback = spendGold;
            getGoldCallback = getGold;
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
            if (hudView != null)
            {
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
                        var tmpText = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (tmpText != null)
                        {
                            tmpText.text = $"{config.DisplayName}\n({config.Cost}G)";
                        }

                        var hover = btn.gameObject.GetComponent<TowerDefense.UI.HoverCursor>();
                        if (hover == null) hover = btn.gameObject.AddComponent<TowerDefense.UI.HoverCursor>();
                        hover.IsAffordable = () => getGoldCallback != null && getGoldCallback() >= config.Cost;

                        btn.onClick.AddListener(() => TryPurchase(config));
                    });
                }
                
                actions.Add((btn, index) =>
                {
                     var text = btn.GetComponentInChildren<UnityEngine.UI.Text>();
                     if (text != null)
                     {
                         text.text = "Cancel";
                     }
                     var tmpText = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                     if (tmpText != null)
                     {
                         tmpText.text = "Cancel";
                     }
                     
                     var hover = btn.gameObject.GetComponent<TowerDefense.UI.HoverCursor>();
                     if (hover == null) hover = btn.gameObject.AddComponent<TowerDefense.UI.HoverCursor>();
                     hover.IsAffordable = () => true;

                     btn.onClick.AddListener(CloseMenu);
                     
                     var rect = btn.GetComponent<RectTransform>();
                     if (rect != null)
                     {
                         rect.anchoredPosition = Vector2.zero;
                     }
                });

                hudView.ShowTowerMenu(center, actions);
            }
        }

        private void CloseMenu()
        {
            if (hudView != null)
            {
                hudView.HideTowerMenu();
            }
        }

        private bool TryPurchase(TowerConfig config)
        {
            if (config == null || IsBlocked(selectedCell) || occupiedCells.Contains(selectedCell))
            {
                CloseMenu();
                return false;
            }

            int currentGold = getGoldCallback != null ? getGoldCallback() : 0;
            if (config.Cost > currentGold)
            {
                CloseMenu();
                return false;
            }

            if (spendGoldCallback != null && !spendGoldCallback(config.Cost))
            {
                CloseMenu();
                return false;
            }

            SpawnTowerPlaceholder(selectedCell, config);
            occupiedCells.Add(selectedCell);
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
            GameObject tower;
            if (config.TowerPrefab != null)
            {
                tower = Instantiate(config.TowerPrefab, GetCellCenter(cell), Quaternion.identity);
                tower.name = $"{config.Type}_Tower_{cell.x}_{cell.y}";
            }
            else
            {
                tower = new GameObject($"{config.Type}_Tower_{cell.x}_{cell.y}");
                tower.transform.position = GetCellCenter(cell);

                var renderer = tower.AddComponent<SpriteRenderer>();
                renderer.sprite = GetFallbackSprite();
                renderer.color = config.PreviewColor;
                renderer.sortingOrder = 2;

                var scale = cellSize * config.PreviewScale;
                tower.transform.localScale = new Vector3(scale, scale, 1f);
            }

            var placedTower = tower.AddComponent<PlacedTower>();
            placedTower.Configure(config);

            var combatController = tower.AddComponent<TowerCombatController>();
            combatController.Configure(config);
        }

        private void RebuildBlockedCells()
        {
            blockedCells.Clear();
            if (waypointPath == null || waypointPath.Count < 2)
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

            for (int i = 0; i < waypointPath.Count - 1; i++)
            {
                Vector2Int startCell = WorldToCell(waypointPath.GetPosition(i));
                Vector2Int endCell = WorldToCell(waypointPath.GetPosition(i + 1));

                int dx = Mathf.Abs(endCell.x - startCell.x);
                int dy = Mathf.Abs(endCell.y - startCell.y);

                int sx = (startCell.x < endCell.x) ? 1 : -1;
                int sy = (startCell.y < endCell.y) ? 1 : -1;

                int err = dx - dy;

                Vector2Int currentCell = startCell;

                while (true)
                {
                    if (IsInsideGrid(currentCell))
                    {
                        blockedCells.Add(currentCell);
                    }

                    if (currentCell.x == endCell.x && currentCell.y == endCell.y)
                    {
                        break;
                    }

                    int e2 = 2 * err;
                    if (e2 > -dy)
                    {
                        err -= dy;
                        currentCell.x += sx;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        currentCell.y += sy;
                    }
                }
            }

            blockedCells.Add(WorldToCell(new Vector3(5.5f, 2.5f, 0f)));
            blockedCells.Add(WorldToCell(new Vector3(5.5f, 3.5f, 0f)));
            blockedCells.Add(WorldToCell(new Vector3(4.5f, 3.5f, 0f)));
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
            if (EventSystem.current == null) return false;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 pointerPos = Vector2.zero;
                if (!TryGetPointerScreenPosition(out pointerPos))
                {
                    return false;
                }

                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = pointerPos
                };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (var result in results)
                {
                    if (result.gameObject != null)
                    {
                        var name = result.gameObject.name.ToLower();
                        if (name.Contains("button") || name.Contains("menu") || name.Contains("panel") || result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsPrimaryPointerPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }
            return false;
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
                for (var x = 0; x < gridWidth; x++)
                {
                    var center = GetCellCenter(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
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
