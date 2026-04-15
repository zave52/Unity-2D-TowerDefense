using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class ProjectileController : MonoBehaviour
    {
        private EnemyController target;
        private int damage;
        private float speed;
        private float lifetime;
        private bool isAoe;
        private float aoeRadius;
        private float slowAmount;
        private float slowDuration;
        private bool hitProcessed;

        public void Initialize(
            EnemyController targetEnemy,
            int projectileDamage,
            float projectileSpeed,
            float lifetimeSeconds,
            bool useAoe,
            float aoeHitRadius,
            float projectileSlowAmount = 0f,
            float projectileSlowDuration = 0f)
        {
            target = targetEnemy;
            damage = Mathf.Max(1, projectileDamage);
            speed = Mathf.Max(0.1f, projectileSpeed);
            lifetime = Mathf.Max(0.2f, lifetimeSeconds);
            isAoe = useAoe;
            aoeRadius = Mathf.Max(0.1f, aoeHitRadius);
            slowAmount = Mathf.Max(0f, projectileSlowAmount);
            slowDuration = Mathf.Max(0f, projectileSlowDuration);
        }

        private void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (target == null || !target.IsActive)
            {
                Destroy(gameObject);
                return;
            }

            var nextPosition = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            transform.position = nextPosition;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hitProcessed)
            {
                return;
            }

            var enemy = other.GetComponent<EnemyController>();
            if (enemy == null || enemy != target || !enemy.IsActive)
            {
                return;
            }

            hitProcessed = true;
            if (isAoe)
            {
                ApplyAoeDamage(transform.position);
            }
            else
            {
                enemy.TakeDamage(damage);
                if (slowAmount > 0f)
                {
                    enemy.ApplySlow(slowAmount, slowDuration);
                }
            }

            Destroy(gameObject);
        }

        private void ApplyAoeDamage(Vector3 center)
        {
            var hits = Physics2D.OverlapCircleAll(center, aoeRadius);
            for (var i = 0; i < hits.Length; i++)
            {
                var enemy = hits[i].GetComponent<EnemyController>();
                if (enemy == null || !enemy.IsActive)
                {
                    continue;
                }

                enemy.TakeDamage(damage);
                if (slowAmount > 0f)
                {
                    enemy.ApplySlow(slowAmount, slowDuration);
                }
            }
        }
    }
}
