using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance;

    public List<TowerData> towers;
    public List<EnemyData> enemies;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log($"[AssetManager] Duplicate AssetManager detected on '{gameObject.name}'. Destroying duplicate GameObject.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public TowerData GetTowerData(string towerName)
    {
        return towers.Find(tower => tower.towerName == towerName);
    }

    public EnemyData GetEnemyData(string enemyName)
    {
        return enemies.Find(enemy => enemy.enemyName == enemyName);
    }
}
