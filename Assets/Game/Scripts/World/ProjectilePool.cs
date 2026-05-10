using UnityEngine;
using UnityEngine.Pool;

namespace TowerDefense.World
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }
        
        public GameObject prefab; // Made public for SceneAutoSetup
        
        private ObjectPool<ProjectileController> pool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
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
            if (prefab == null)
            {
                Debug.LogError("[ProjectilePool] Prefab is not set! Please assign a projectile prefab in the Inspector.", this);
                // Create a fallback primitive so the game doesn't crash
                var fallbackGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallbackGo.transform.localScale = Vector3.one * 0.2f;
                return fallbackGo.AddComponent<ProjectileController>();
            }
            
            var projectileObject = Instantiate(prefab, transform);
            var projectile = projectileObject.GetComponent<ProjectileController>();
            return projectile;
        }
    }
}
