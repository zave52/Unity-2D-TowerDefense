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

        public void ClearEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            activeEnemies.Clear();
        }

        private IEnumerator SpawnWaveRoutine()
        {
            CurrentWaveBudget = attackerBudget;
            int remainingBudget = CurrentWaveBudget;

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
                yield return new WaitForSeconds(spawnInterval);
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

            var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);
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
        }
    }
}
