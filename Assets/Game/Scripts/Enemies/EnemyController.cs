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
        private float slowMultiplier = 1f;
        private float slowTimer = 0f;
        private Coroutine tintCoroutine;

        public bool IsActive => active && gameObject.activeInHierarchy;
        public int CurrentHealth => currentHealth;
        public EnemyConfig Config => config;

        public float DistanceToGoal()
        {
            if (!active || path == null) return float.MaxValue;
            var targetIndex = Mathf.Min(currentWaypoint + 1, path.Count - 1);
            var dist = Vector3.Distance(transform.position, path.GetPosition(targetIndex));
            for (var i = targetIndex; i < path.Count - 1; i++)
            {
                dist += Vector3.Distance(path.GetPosition(i), path.GetPosition(i + 1));
            }
            return dist;
        }

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
            slowMultiplier = 1f;
            slowTimer = 0f;
            active = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = config != null && config.EnemySprite != null ? config.EnemySprite : GetFallbackSprite();
                if (config != null && config.EnemySprite != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }



            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            
            if (config != null)
            {
                if (config.AnimatorController != null)
                {
                    animator.enabled = true;
                    animator.runtimeAnimatorController = config.AnimatorController;
                }
                else
                {
                    animator.enabled = false;
                }
            }

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

        public void TakeDamage(int damage, TowerType sourceTowerType)
        {
            TakeDamage(damage);
            if (!active) return;

            if (config != null && config.Type == EnemyType.Ghost && sourceTowerType == TowerType.Freezer)
            {
                sourceTowerType = TowerType.Archer;
            }

            Color flashColor = Color.white;
            float duration = 0.25f;

            switch (sourceTowerType)
            {
                case TowerType.Archer:
                    flashColor = new Color(1f, 0.8f, 0.4f, 1f);
                    duration = 0.2f;
                    break;
                case TowerType.Mage:
                    flashColor = new Color(1f, 0.2f, 0.2f, 1f);
                    duration = 0.45f;
                    break;
                case TowerType.Freezer:
                    flashColor = new Color(0.2f, 0.6f, 1f, 1f);
                    duration = 0.65f;
                    break;
                case TowerType.Cannon:
                    flashColor = new Color(1f, 0.45f, 0f, 1f);
                    duration = 0.35f;
                    break;
            }

            FlashColor(flashColor, duration);
        }

        public void FlashColor(Color targetColor, float duration)
        {
            if (spriteRenderer == null) return;

            if (tintCoroutine != null)
            {
                StopCoroutine(tintCoroutine);
            }
            tintCoroutine = StartCoroutine(FlashColorRoutine(targetColor, duration));
        }

        private System.Collections.IEnumerator FlashColorRoutine(Color targetColor, float duration)
        {
            float elapsed = 0f;
            Color baseColor = (config != null && config.EnemySprite != null) ? Color.white : new Color(0.9f, 0.25f, 0.25f, 1f);

            spriteRenderer.color = targetColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(targetColor, baseColor, t);
                }
                yield return null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }
            tintCoroutine = null;
        }

        public void ApplySlow(float amount, float duration)
        {
            if (!active || config == null || config.Type == EnemyType.Ghost)
            {
                return;
            }

            var multiplier = Mathf.Clamp01(1f - amount);
            if (multiplier < slowMultiplier || slowTimer <= 0f)
            {
                slowMultiplier = multiplier;
                slowTimer = duration;
            }
        }

        private void Update()
        {
            if (!active || config == null || path == null)
            {
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "Decorations";
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            }

            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f)
                {
                    slowMultiplier = 1f;
                }
            }

            var targetIndex = Mathf.Min(currentWaypoint + 1, path.Count - 1);
            var targetPosition = path.GetPosition(targetIndex);
            var step = config.MoveSpeed * slowMultiplier * Time.deltaTime;
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
            if (!reachedBase && EffectsManager.Instance != null)
            {
                EffectsManager.Instance.PlayDeath(transform.position, config);
            }

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
