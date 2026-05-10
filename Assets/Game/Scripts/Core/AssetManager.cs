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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
