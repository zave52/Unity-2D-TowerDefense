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
        private bool hitProcessed;

        public void Initialize(EnemyController targetEnemy, int projectileDamage, float projectileSpeed, float lifetimeSeconds)
        {
            target = targetEnemy;
            damage = Mathf.Max(1, projectileDamage);
            speed = Mathf.Max(0.1f, projectileSpeed);
            lifetime = Mathf.Max(0.2f, lifetimeSeconds);
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
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

