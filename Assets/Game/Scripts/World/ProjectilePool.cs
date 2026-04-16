using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.World
{
    public class ProjectilePool : MonoBehaviour
    {
        private static ProjectilePool instance;
        public static ProjectilePool Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[ProjectilePool]");
                    instance = go.AddComponent<ProjectilePool>();
                    DontDestroyOnLoad(go);
                    instance.Init();
                }
                return instance;
            }
        }

        private ObjectPool<ProjectileController> pool;
        private static Sprite fallbackSprite;

        private void Init()
        {
            pool = new ObjectPool<ProjectileController>(
                createFunc: CreateProjectile,
                actionOnGet: proj => proj.gameObject.SetActive(true),
                actionOnRelease: proj => proj.gameObject.SetActive(false),
                actionOnDestroy: proj => Destroy(proj.gameObject),
                collectionCheck: false,
                defaultCapacity: 50,
                maxSize: 200
            );
        }

        public ProjectileController Get() => pool.Get();

        public void Release(ProjectileController projectile) => pool.Release(projectile);

        private ProjectileController CreateProjectile()
        {
            var projectileObject = new GameObject("PooledProjectile");
            projectileObject.transform.SetParent(transform);

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateFallbackSprite();
            renderer.sortingOrder = 4;
            projectileObject.transform.localScale = new Vector3(0.18f, 0.18f, 1f);

            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var projectile = projectileObject.AddComponent<ProjectileController>();
            return projectile;
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

