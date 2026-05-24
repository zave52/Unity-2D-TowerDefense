using TowerDefense.Enemies;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class TowerCombatController : MonoBehaviour
    {
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetimeSeconds = 3f;
        [SerializeField] private float mageAoeRadius = 1.2f;

        private TowerConfig config;
        private float shotCooldown;
        private Animator animator;
        private EnemySpawner spawner;
        private bool hasHasTargetParam;
        private bool hasAttackParam;

        public void Configure(TowerConfig towerConfig)
        {
            config = towerConfig;
            shotCooldown = 0f;
            animator = GetComponentInChildren<Animator>();
            if (GameBootstrap.Instance != null)
            {
                spawner = GameBootstrap.Instance.enemySpawner;
            }
            hasHasTargetParam = HasParameter(animator, "HasTarget");
            hasAttackParam = HasParameter(animator, "Attack");
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            var target = FindTargetInRange(config.Range);
            
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                if (hasHasTargetParam)
                {
                    animator.SetBool("HasTarget", target != null);
                }
            }

            shotCooldown -= Time.deltaTime;
            if (shotCooldown > 0f || target == null)
            {
                return;
            }

            FireProjectile(target);
            shotCooldown = 1f / config.AttacksPerSecond;
        }

        private EnemyController FindTargetInRange(float range)
        {
            if (spawner == null && GameBootstrap.Instance != null)
            {
                spawner = GameBootstrap.Instance.enemySpawner;
            }

            if (spawner == null)
            {
                return null;
            }

            var enemies = spawner.ActiveEnemies;
            var rangeSqr = range * range;
            var origin = transform.position;

            EnemyController best = null;
            var bestDistanceToGoal = float.MaxValue; // For Nearest
            var bestDistanceToGoalMax = float.MinValue; // For Furthest
            var bestDistanceSqr = float.MaxValue; // For NearestToTower
            var bestDistanceSqrMax = float.MinValue; // For FurthestToTower
            var bestHealth = float.MaxValue; // For Weakest
            var bestHealthMax = float.MinValue; // For Strongest

            var mode = TargetingMode.Nearest;
            if (GameBootstrap.Instance != null)
            {
                mode = GameBootstrap.Instance.CurrentTargetingMode;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                var distToTowerSqr = (enemy.transform.position - origin).sqrMagnitude;
                if (distToTowerSqr > rangeSqr)
                {
                    continue;
                }

                switch (mode)
                {
                    case TargetingMode.Nearest:
                        var distToGoal = enemy.DistanceToGoal();
                        if (distToGoal < bestDistanceToGoal)
                        {
                            bestDistanceToGoal = distToGoal;
                            best = enemy;
                        }
                        break;

                    case TargetingMode.Furthest:
                        var distToGoalMax = enemy.DistanceToGoal();
                        if (distToGoalMax > bestDistanceToGoalMax)
                        {
                            bestDistanceToGoalMax = distToGoalMax;
                            best = enemy;
                        }
                        break;

                    case TargetingMode.NearestToTower:
                        if (distToTowerSqr < bestDistanceSqr)
                        {
                            bestDistanceSqr = distToTowerSqr;
                            best = enemy;
                        }
                        break;

                    case TargetingMode.FurthestToTower:
                        if (distToTowerSqr > bestDistanceSqrMax)
                        {
                            bestDistanceSqrMax = distToTowerSqr;
                            best = enemy;
                        }
                        break;

                    case TargetingMode.Weakest:
                        var hp = enemy.CurrentHealth;
                        if (hp < bestHealth)
                        {
                            bestHealth = hp;
                            best = enemy;
                        }
                        break;

                    case TargetingMode.Strongest:
                        var maxHp = enemy.CurrentHealth;
                        if (maxHp > bestHealthMax)
                        {
                            bestHealthMax = maxHp;
                            best = enemy;
                        }
                        break;
                }
            }

            return best;
        }

        private void FireProjectile(EnemyController target)
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                if (hasAttackParam)
                {
                    animator.SetTrigger("Attack");
                }
            }

            var spawnPos = transform.position;
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            SpriteRenderer mainRenderer = null;
            float maxHeight = 0f;
            foreach (var r in renderers)
            {
                if (r.bounds.size.y > maxHeight)
                {
                    maxHeight = r.bounds.size.y;
                    mainRenderer = r;
                }
            }

            if (mainRenderer != null)
            {
                spawnPos.y = mainRenderer.bounds.min.y + mainRenderer.bounds.size.y * 0.7f;
            }
            else
            {
                spawnPos.y += 0.7f;
            }

            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.PlayShot(spawnPos, config);
            }

            var projectile = ProjectilePool.Instance.Get();
            projectile.transform.position = spawnPos;

            if (projectile.Renderer != null)
            {
                if (config.ProjectileSprite != null)
                {
                    projectile.Renderer.color = Color.white;
                }
                else
                {
                    projectile.Renderer.color = Color.Lerp(config.PreviewColor, Color.white, 0.15f);
                }
            }

            var isMageAoe = config.Type == TowerType.Mage;
            var effectiveAoeRadius = isMageAoe ? Mathf.Max(mageAoeRadius, config.Range * 0.5f) : 0f;
            
            var slowAmount = 0f;
            var slowDuration = 0f;
            if (config.Type == TowerType.Freezer)
            {
                slowAmount = 0.5f;
                slowDuration = 2f;
            }

            projectile.Initialize(target, config, projectileSpeed, projectileLifetimeSeconds, isMageAoe, effectiveAoeRadius, slowAmount, slowDuration);
        }
        private static bool HasParameter(Animator animator, string paramName)
        {
            if (animator == null) return false;
            foreach (var param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
