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
        public event Action WaveCompleted;

        public EnemyController enemyPrefab;
        public List<EnemyConfig> enemyConfigs = new();
        public WaypointPath path;
        public BaseHealth baseHealth;
        [SerializeField] private int attackerBudget = 200;
        [SerializeField] private float spawnInterval = 1.25f;

        private readonly List<EnemyController> activeEnemies = new();
        private readonly Queue<EnemyController> enemyPool = new();
        private Coroutine spawnRoutine;
        private int enemiesToSpawnInCurrentWave;
        
        public List<EnemyConfig> PvPWaveQueue { get; private set; } = new();

        public int ActiveEnemyCount => activeEnemies.Count;

        public int CurrentWaveBudget { get; set; } = 100;
        public int RemainingBudget { get; private set; }

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

        public void StartWave(int roundIndex = 1, bool autoGenerate = true)
        {
            if (spawnRoutine != null)
            {
                return;
            }

            enemiesToSpawnInCurrentWave = 0;
            spawnRoutine = StartCoroutine(SpawnWaveRoutine(roundIndex, autoGenerate));
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
            enemiesToSpawnInCurrentWave = 0;
            PvPWaveQueue.Clear();
        }

        public void PrepareBudgetForRound(int roundIndex)
        {
            float difficultyMultiplier = 1f + (roundIndex - 1) * 0.45f;
            CurrentWaveBudget = Mathf.RoundToInt(attackerBudget * difficultyMultiplier);
            RemainingBudget = CurrentWaveBudget;
            PvPWaveQueue.Clear();
        }

        public bool TryEnqueueEnemy(EnemyConfig config)
        {
            if (config != null && config.SpawnCost <= RemainingBudget && PvPWaveQueue.Count < MaxEnemiesPerWave)
            {
                PvPWaveQueue.Add(config);
                RemainingBudget -= config.SpawnCost;
                return true;
            }
            return false;
        }

        private IEnumerator SpawnWaveRoutine(int roundIndex, bool autoGenerate)
        {
            if (CurrentWaveBudget == 0)
            {
                PrepareBudgetForRound(roundIndex);
            }
            
            if (autoGenerate)
            {
                PvPWaveQueue.Clear();
                while (RemainingBudget > 0 && PvPWaveQueue.Count < MaxEnemiesPerWave)
                {
                    var affordableConfigs = enemyConfigs.FindAll(c => c != null && c.SpawnCost <= RemainingBudget);
                    if (affordableConfigs.Count == 0)
                    {
                        break;
                    }

                    var configToSpawn = affordableConfigs[UnityEngine.Random.Range(0, affordableConfigs.Count)];
                    TryEnqueueEnemy(configToSpawn);
                }
            }

            int enemiesSpawnedThisRoutine = 0;

            foreach (var configToSpawn in PvPWaveQueue)
            {
                SpawnEnemy(configToSpawn);
                enemiesSpawnedThisRoutine++;
                enemiesToSpawnInCurrentWave++;
                yield return new WaitForSeconds(spawnInterval);
            }
            
            PvPWaveQueue.Clear();

            spawnRoutine = null;
            CheckWaveCompletion();
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

            CheckWaveCompletion();
        }

        private void CheckWaveCompletion()
        {
            if (spawnRoutine == null && activeEnemies.Count == 0 && enemiesToSpawnInCurrentWave > 0)
            {
                Debug.Log("[EnemySpawner] Wave completed!");
                WaveCompleted?.Invoke();
                enemiesToSpawnInCurrentWave = 0;
            }
        }
    }
}
