using UnityEngine;

namespace TowerDefense.World
{
    public enum TowerType
    {
        Archer = 0,
        Mage = 1,
        Freezer = 2,
        Cannon = 3
    }

    [CreateAssetMenu(fileName = "TowerConfig", menuName = "TowerDefense/Tower Config")]
    public sealed class TowerConfig : ScriptableObject
    {
        [SerializeField] private TowerType towerType = TowerType.Archer;
        [SerializeField] private string displayName = "Basic Tower";
        [SerializeField] private int cost = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private float range = 2.5f;
        [SerializeField] private float attacksPerSecond = 1f;
        [SerializeField] private float previewScale = 0.75f;
        [SerializeField] private Color previewColor = new Color(0.25f, 0.75f, 0.95f, 1f);
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip projectileHitSound;

        public TowerType Type => towerType;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public int Cost => Mathf.Max(0, cost);
        public int Damage => Mathf.Max(1, damage);
        public float Range => Mathf.Max(0.25f, range);
        public float AttacksPerSecond => Mathf.Max(0.1f, attacksPerSecond);
        public float PreviewScale => Mathf.Max(0.25f, previewScale);
        public Color PreviewColor => previewColor;
        public GameObject TowerPrefab => towerPrefab;
        public AudioClip ShootSound => shootSound;
        public AudioClip ProjectileHitSound => projectileHitSound;

        public void SetRuntimeData(
            TowerType type,
            string towerDisplayName,
            int towerCost,
            int towerDamage,
            float towerRange,
            float towerAttacksPerSecond,
            float towerPreviewScale,
            Color towerPreviewColor,
            GameObject prefab = null)
        {
            towerType = type;
            displayName = towerDisplayName;
            cost = towerCost;
            damage = towerDamage;
            range = towerRange;
            attacksPerSecond = towerAttacksPerSecond;
            previewScale = towerPreviewScale;
            previewColor = towerPreviewColor;
            towerPrefab = prefab;
        }
    }
}

