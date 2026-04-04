using System;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public sealed class EnemyController : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        private EnemyConfig config;
        private WaypointPath path;
        private BaseHealth baseHealth;
        private Action<EnemyController, bool> completedCallback;
        private SpriteRenderer spriteRenderer;

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
        }

        public void TakeDamage(int damage)
        {
            if (!active || damage <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
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
            completedCallback?.Invoke(this, reachedBase);
            gameObject.SetActive(false);
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


