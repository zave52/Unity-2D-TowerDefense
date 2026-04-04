using UnityEngine;

namespace TowerDefense.Enemies
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "TowerDefense/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private int rewardGold = 10;

        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int BaseDamage => baseDamage;
        public int RewardGold => rewardGold;
    }
}

