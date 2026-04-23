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

        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private List<EnemyConfig> enemyConfigs = new();
        [SerializeField] private WaypointPath path;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private int attackerBudget = 100;
        [SerializeField] private float spawnInterval = 1f;

        private readonly List<EnemyController> activeEnemies = new();
        private readonly Queue<EnemyController> enemyPool = new();
        private Coroutine spawnRoutine;

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
        }

        private IEnumerator SpawnWaveRoutine(int roundIndex)
        {
            float difficultyMultiplier = 1f + (roundIndex - 1) * 0.25f;
            CurrentWaveBudget = Mathf.RoundToInt(attackerBudget * difficultyMultiplier);
            int remainingBudget = CurrentWaveBudget;
            
            float currentSpawnInterval = Mathf.Max(0.2f, spawnInterval * Mathf.Pow(0.9f, roundIndex - 1));

            while (remainingBudget > 0)
            {
                var affordableConfigs = enemyConfigs.FindAll(c => c != null && c.SpawnCost <= remainingBudget);
                if (affordableConfigs.Count == 0)
                {
                    break;
                }

                var configToSpawn = affordableConfigs[UnityEngine.Random.Range(0, affordableConfigs.Count)];
                SpawnEnemy(configToSpawn);
                remainingBudget -= configToSpawn.SpawnCost;
                yield return new WaitForSeconds(currentSpawnInterval);
            }

            spawnRoutine = null;
        }

        private void SpawnEnemy(EnemyConfig config)
        {
            if (enemyPrefab == null || config == null || path == null || baseHealth == null)
            {
                Debug.LogWarning($"[EnemySpawner] Missing references. Cannot spawn enemy. Prefab: {enemyPrefab != null}, Config: {config != null}, Path: {path != null}, BaseHp: {baseHealth != null}");
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
        }
    }
}
