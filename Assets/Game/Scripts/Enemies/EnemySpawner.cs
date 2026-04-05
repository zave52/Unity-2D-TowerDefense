using System.Collections;
using System.Collections.Generic;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const int MaxEnemiesPerWave = 50;
        private const int MinEnemiesPerWave = 1;
        private const float MinSpawnInterval = 0.05f;

        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private EnemyConfig defaultConfig;
        [SerializeField] private WaypointPath path;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private int enemiesPerWave = 10;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private bool autoStartWave;

        private readonly List<EnemyController> activeEnemies = new();
        private Coroutine spawnRoutine;

        public int ActiveEnemyCount => activeEnemies.Count;

        private void Start()
        {
            if (autoStartWave)
            {
                StartWave();
            }
        }

        private void OnValidate()
        {
            enemiesPerWave = Mathf.Clamp(enemiesPerWave, MinEnemiesPerWave, MaxEnemiesPerWave);
            spawnInterval = Mathf.Max(MinSpawnInterval, spawnInterval);
        }

        public void Configure(EnemyController prefab, EnemyConfig config, WaypointPath waypointPath, BaseHealth targetBase)
        {
            enemyPrefab = prefab;
            defaultConfig = config;
            path = waypointPath;
            baseHealth = targetBase;
        }

        public void StartWave()
        {
            if (spawnRoutine != null)
            {
                return;
            }

            spawnRoutine = StartCoroutine(SpawnWaveRoutine());
        }

        public void StopWave()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private IEnumerator SpawnWaveRoutine()
        {
            var waveCount = GetValidatedWaveCount();
            for (var i = 0; i < waveCount; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            spawnRoutine = null;
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || defaultConfig == null || path == null || baseHealth == null)
            {
                Debug.LogWarning("[EnemySpawner] Missing references. Cannot spawn enemy.");
                return;
            }

            var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            enemy.Initialize(defaultConfig, path, baseHealth, HandleEnemyCompleted);
            activeEnemies.Add(enemy);
        }

        private int GetValidatedWaveCount()
        {
            return Mathf.Clamp(enemiesPerWave, MinEnemiesPerWave, MaxEnemiesPerWave);
        }

        private void HandleEnemyCompleted(EnemyController enemy, bool reachedBase)
        {
            activeEnemies.Remove(enemy);
        }
    }
}

