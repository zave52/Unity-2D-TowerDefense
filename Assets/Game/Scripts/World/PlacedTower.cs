using UnityEngine;

namespace TowerDefense.World
{
    public sealed class PlacedTower : MonoBehaviour
    {
        private TowerConfig config;

        public TowerConfig Config => config;

        public void Configure(TowerConfig towerConfig)
        {
            config = towerConfig;
            name = towerConfig != null ? towerConfig.DisplayName : name;
        }
    }
}
