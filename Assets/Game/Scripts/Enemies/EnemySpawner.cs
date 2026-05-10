using System.Collections;
using System.Collections.Generic;
using System;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const int MaxEnemiesPerWave = 50;
        private const int MinEnemiesPerWave = 1;
        private const float MinSpawnInterval = 0.05f;

        public event Action<int> EnemyKilled;
        public event Action WaveCompleted; // New event

        public EnemyController enemyPrefab;
        public List<EnemyConfig> enemyConfigs = new();
        public WaypointPath path;
        public BaseHealth baseHealth;
        [SerializeField] private int attackerBudget = 80;
        [SerializeField] private float spawnInterval = 1.25f;

        private readonly List<EnemyController> activeEnemies = new();
        private readonly Queue<EnemyController> enemyPool = new();
        private Coroutine spawnRoutine;
        private int enemiesToSpawnInCurrentWave; // New field to track total enemies for the wave

        public int ActiveEnemyCount => activeEnemies.Count;

        public int CurrentWaveBudget { get; set; } = 100;

        private void Start()
        {
        }

        private void OnValidate()
        {
            attackerBudget = Mathf.Max(10, attackerBudget);
            spawnInterval = Mathf.Max(MinSpawnInterval, spawnInterval);
        }

        public void Configure(EnemyController prefab, List<EnemyConfig> configs, WaypointPath waypointPath, BaseHealth targetBase)
        {
            enemyPrefab = prefab;
            enemyConfigs = configs != null ? new List<EnemyConfig>(configs) : new List<EnemyConfig>();
            path = waypointPath;
            baseHealth = targetBase;
        }

        public void StartWave(int roundIndex = 1)
        {
            if (spawnRoutine != null)
            {
                return;
            }

            enemiesToSpawnInCurrentWave = 0; // Reset count for new wave
            spawnRoutine = StartCoroutine(SpawnWaveRoutine(roundIndex));
        }

        public void StopWave()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        public void ClearEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(false);
                    enemyPool.Enqueue(enemy);
                }
            }
            activeEnemies.Clear();
            enemiesToSpawnInCurrentWave = 0; // Reset count
        }

        private IEnumerator SpawnWaveRoutine(int roundIndex)
        {
            float difficultyMultiplier = 1f + (roundIndex - 1) * 0.45f;
            CurrentWaveBudget = Mathf.RoundToInt(attackerBudget * difficultyMultiplier);
            int remainingBudget = CurrentWaveBudget;
            
            float currentSpawnInterval = Mathf.Max(0.2f, spawnInterval * Mathf.Pow(0.85f, roundIndex - 1));

            int enemiesSpawnedThisRoutine = 0;
            while (remainingBudget > 0 && enemiesSpawnedThisRoutine < MaxEnemiesPerWave)
            {
                var affordableConfigs = enemyConfigs.FindAll(c => c != null && c.SpawnCost <= remainingBudget);
                if (affordableConfigs.Count == 0)
                {
                    break;
                }

                var configToSpawn = affordableConfigs[UnityEngine.Random.Range(0, affordableConfigs.Count)];
                SpawnEnemy(configToSpawn);
                enemiesSpawnedThisRoutine++;
                enemiesToSpawnInCurrentWave++; // Increment total enemies for the wave
                remainingBudget -= configToSpawn.SpawnCost;
                yield return new WaitForSeconds(currentSpawnInterval);
            }

            spawnRoutine = null;
            CheckWaveCompletion(); // Check if wave is completed immediately after spawning finishes
        }

        private void SpawnEnemy(EnemyConfig config)
        {
            if (enemyPrefab == null || config == null || path == null || baseHealth == null || path.Count == 0)
            {
                Debug.LogWarning($"[EnemySpawner] Missing references. Cannot spawn enemy. Prefab: {enemyPrefab != null}, Config: {config != null}, Path: {path != null}, Path has points: {(path != null && path.Count > 0)}, BaseHp: {baseHealth != null}");
                return;
            }

            EnemyController enemy;
            if (enemyPool.Count > 0)
            {
                enemy = enemyPool.Dequeue();
                enemy.transform.position = transform.position;
                enemy.gameObject.SetActive(true);
            }
            else
            {
                enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
            }

            enemy.Initialize(config, path, baseHealth, HandleEnemyCompleted);
            activeEnemies.Add(enemy);
        }

        private void HandleEnemyCompleted(EnemyController enemy, bool reachedBase)
        {
            activeEnemies.Remove(enemy);
            if (!reachedBase && enemy.Config != null)
            {
                EnemyKilled?.Invoke(enemy.Config.RewardGold);
            }
            enemyPool.Enqueue(enemy);

            CheckWaveCompletion(); // Check after each enemy is completed
        }

        private void CheckWaveCompletion()
        {
            // Wave is completed if all enemies that were supposed to spawn have spawned,
            // AND all active enemies have been removed (either destroyed or reached base).
            if (spawnRoutine == null && activeEnemies.Count == 0 && enemiesToSpawnInCurrentWave > 0)
            {
                Debug.Log("[EnemySpawner] Wave completed!");
                WaveCompleted?.Invoke();
                enemiesToSpawnInCurrentWave = 0; // Reset for next wave
            }
        }
    }
}
