using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class TowerCombatController : MonoBehaviour
    {
        private static Sprite fallbackSprite;

        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetimeSeconds = 3f;

        private TowerConfig config;
        private float shotCooldown;

        public void Configure(TowerConfig towerConfig)
        {
            config = towerConfig;
            shotCooldown = 0f;
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            shotCooldown -= Time.deltaTime;
            if (shotCooldown > 0f)
            {
                return;
            }

            var target = FindNearestTargetInRange(config.Range);
            if (target == null)
            {
                return;
            }

            FireProjectile(target);
            shotCooldown = 1f / config.AttacksPerSecond;
        }

        private EnemyController FindNearestTargetInRange(float range)
        {
            var enemies = FindObjectsOfType<EnemyController>();
            var rangeSqr = range * range;
            EnemyController best = null;
            var bestDistance = float.MaxValue;
            var origin = transform.position;

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                var distanceSqr = (enemy.transform.position - origin).sqrMagnitude;
                if (distanceSqr > rangeSqr || distanceSqr >= bestDistance)
                {
                    continue;
                }

                bestDistance = distanceSqr;
                best = enemy;
            }

            return best;
        }

        private void FireProjectile(EnemyController target)
        {
            var projectileObject = new GameObject($"Projectile_{config.Type}");
            projectileObject.transform.position = transform.position;

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateFallbackSprite();
            renderer.color = Color.Lerp(config.PreviewColor, Color.white, 0.15f);
            renderer.sortingOrder = 4;

            projectileObject.transform.localScale = new Vector3(0.18f, 0.18f, 1f);

            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var projectile = projectileObject.AddComponent<ProjectileController>();
            projectile.Initialize(target, config.Damage, projectileSpeed, projectileLifetimeSeconds);
        }

        private static Sprite CreateFallbackSprite()
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


