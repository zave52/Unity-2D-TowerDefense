using UnityEngine;

namespace TowerDefense.World
{
    [CreateAssetMenu(fileName = "TowerConfig", menuName = "TowerDefense/Tower Config")]
    public sealed class TowerConfig : ScriptableObject
    {
        [SerializeField] private string displayName = "Basic Tower";
        [SerializeField] private int cost = 100;
        [SerializeField] private Color previewColor = new Color(0.25f, 0.75f, 0.95f, 1f);

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public int Cost => Mathf.Max(0, cost);
        public Color PreviewColor => previewColor;
    }
}

