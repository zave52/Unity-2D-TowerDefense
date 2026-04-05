using System;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public sealed class EnemyController : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        [Header("Health Bar")]
        [SerializeField] private float hpBarOffsetY = 0.9f;
        [SerializeField] private float hpBarWidth = 0.9f;
        [SerializeField] private float hpBarHeight = 0.12f;
        [SerializeField] private Color hpBarBackgroundColor = new Color(0f, 0f, 0f, 0.75f);
        [SerializeField] private Color hpBarFillColor = new Color(0.2f, 0.9f, 0.3f, 0.95f);

        private EnemyConfig config;
        private WaypointPath path;
        private BaseHealth baseHealth;
        private Action<EnemyController, bool> completedCallback;
        private SpriteRenderer spriteRenderer;
        private Transform hpBarRoot;
        private SpriteRenderer hpBarFill;

        private int currentWaypoint;
        private int currentHealth;
        private bool active;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = GetFallbackSprite();
                spriteRenderer.color = new Color(0.9f, 0.25f, 0.25f, 1f);
            }

            EnsureHealthBar();
            SetHealthBarVisible(false);
        }

        public void Initialize(EnemyConfig enemyConfig, WaypointPath waypointPath, BaseHealth targetBase, Action<EnemyController, bool> onCompleted)
        {
            config = enemyConfig;
            path = waypointPath;
            baseHealth = targetBase;
            completedCallback = onCompleted;
            currentWaypoint = 0;
            currentHealth = config != null ? config.MaxHealth : 1;
            active = true;

            if (path != null && path.TryGetPosition(0, out var startPosition))
            {
                transform.position = startPosition;
            }

            EnsureHealthBar();
            SetHealthBarVisible(true);
            UpdateHealthBar();
        }

        public void TakeDamage(int damage)
        {
            if (!active || damage <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
            UpdateHealthBar();
            if (currentHealth == 0)
            {
                Finish(false);
            }
        }

        private void Update()
        {
            if (!active || config == null || path == null)
            {
                return;
            }

            var targetIndex = Mathf.Min(currentWaypoint + 1, path.Count - 1);
            var targetPosition = path.GetPosition(targetIndex);
            var step = config.MoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) <= 0.02f)
            {
                currentWaypoint = targetIndex;
                if (currentWaypoint >= path.Count - 1)
                {
                    if (baseHealth != null)
                    {
                        baseHealth.ApplyDamage(config.BaseDamage);
                    }

                    Finish(true);
                }
            }
        }

        private void Finish(bool reachedBase)
        {
            if (!active)
            {
                return;
            }

            active = false;
            SetHealthBarVisible(false);
            completedCallback?.Invoke(this, reachedBase);
            gameObject.SetActive(false);
        }

        private void EnsureHealthBar()
        {
            if (hpBarRoot != null && hpBarFill != null)
            {
                return;
            }

            var root = new GameObject("HealthBar");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, hpBarOffsetY, 0f);
            hpBarRoot = root.transform;

            var background = new GameObject("Background");
            background.transform.SetParent(hpBarRoot, false);
            var backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = GetFallbackSprite();
            backgroundRenderer.color = hpBarBackgroundColor;
            backgroundRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;
            background.transform.localScale = new Vector3(hpBarWidth, hpBarHeight, 1f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(hpBarRoot, false);
            hpBarFill = fill.AddComponent<SpriteRenderer>();
            hpBarFill.sprite = GetFallbackSprite();
            hpBarFill.color = hpBarFillColor;
            hpBarFill.sortingOrder = backgroundRenderer.sortingOrder + 1;
            fill.transform.localScale = new Vector3(hpBarWidth, hpBarHeight * 0.8f, 1f);
        }

        private void UpdateHealthBar()
        {
            if (hpBarFill == null)
            {
                return;
            }

            var maxHealth = config != null ? Mathf.Max(1, config.MaxHealth) : 1;
            var normalized = Mathf.Clamp01(currentHealth / (float)maxHealth);
            var fillWidth = hpBarWidth * normalized;
            hpBarFill.transform.localScale = new Vector3(fillWidth, hpBarHeight * 0.8f, 1f);
            hpBarFill.transform.localPosition = new Vector3(-(hpBarWidth - fillWidth) * 0.5f, 0f, 0f);
        }

        private void SetHealthBarVisible(bool visible)
        {
            if (hpBarRoot != null)
            {
                hpBarRoot.gameObject.SetActive(visible);
            }
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
    }
}


