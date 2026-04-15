using UnityEngine;

namespace TowerDefense.Enemies
{
    public enum EnemyType { Goblin, Orc, Ghost }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "TowerDefense/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private EnemyType type = EnemyType.Goblin;
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private int rewardGold = 10;

        public EnemyType Type => type;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int BaseDamage => baseDamage;
        public int RewardGold => rewardGold;

        public void SetRuntimeData(int health, float speed, int damage, int reward)
        {
            maxHealth = Mathf.Max(1, health);
            moveSpeed = Mathf.Max(0.1f, speed);
            baseDamage = Mathf.Max(0, damage);
            rewardGold = Mathf.Max(0, reward);
        }
    }
}
