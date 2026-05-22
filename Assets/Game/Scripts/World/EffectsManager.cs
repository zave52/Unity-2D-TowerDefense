using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.World
{
    public sealed class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip shotSfx;
        [SerializeField] private AudioClip hitSfx;
        [SerializeField] private AudioClip deathSfx;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

        [Header("Visual Effects Prefabs")]
        [SerializeField] private GameObject shotVfxPrefab;
        [SerializeField] private GameObject hitVfxPrefab;
        [SerializeField] private GameObject deathVfxPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlayShot(Vector3 position, TowerConfig config)
        {
            if (TowerDefense.Core.AudioManager.Instance != null && config != null)
            {
                TowerDefense.Core.AudioManager.Instance.PlayTowerShoot(config, position);
            }
            else
            {
                PlaySound(shotSfx, position);
            }
            SpawnVfx(shotVfxPrefab, position);
        }

        public void PlayHit(Vector3 position, EnemyConfig enemyConfig, TowerConfig towerConfig)
        {
            if (TowerDefense.Core.AudioManager.Instance != null)
            {
                if (towerConfig != null && towerConfig.ProjectileHitSound != null)
                    TowerDefense.Core.AudioManager.Instance.PlayProjectileHit(towerConfig, position);
                else
                    PlaySound(hitSfx, position);
            }
            else
            {
                PlaySound(hitSfx, position);
            }
            SpawnVfx(hitVfxPrefab, position);
        }

        public void PlayDeath(Vector3 position, EnemyConfig config)
        {
            if (TowerDefense.Core.AudioManager.Instance != null && config != null)
            {
                TowerDefense.Core.AudioManager.Instance.PlayEnemyDeath(config, position);
            }
            else
            {
                PlaySound(deathSfx, position);
            }
            SpawnVfx(deathVfxPrefab, position);
        }

        private void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volume);
            }
        }

        private void SpawnVfx(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                var vfx = Instantiate(prefab, position, Quaternion.identity);
                var ps = vfx.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(vfx, 2f);
                }
            }
        }
    }
}
