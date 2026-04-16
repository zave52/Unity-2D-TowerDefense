using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class TowerCombatController : MonoBehaviour
    {
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetimeSeconds = 3f;
        [SerializeField] private float mageAoeRadius = 2.4f;

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
            var enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude);
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
            var projectile = ProjectilePool.Instance.Get();
            projectile.transform.position = transform.position;

            if (projectile.Renderer != null)
            {
                projectile.Renderer.color = Color.Lerp(config.PreviewColor, Color.white, 0.15f);
            }

            var isMageAoe = config.Type == TowerType.Mage;
            var effectiveAoeRadius = isMageAoe ? Mathf.Max(mageAoeRadius, config.Range * 0.75f) : 0f;
            
            var slowAmount = 0f;
            var slowDuration = 0f;
            if (config.Type == TowerType.Freezer)
            {
                slowAmount = 0.5f;
                slowDuration = 2f;
            }

            projectile.Initialize(target, config.Damage, projectileSpeed, projectileLifetimeSeconds, isMageAoe, effectiveAoeRadius, slowAmount, slowDuration);
        }
    }
}
