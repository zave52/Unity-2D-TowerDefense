using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private Button pveButton;

        private GameBootstrap bootstrap;

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            bootstrap = gameBootstrap;
        }

        private void OnEnable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.AddListener(OnPvEClicked);
            }
        }

        private void OnDisable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.RemoveListener(OnPvEClicked);
            }
        }

        private void OnPvEClicked()
        {
            if (bootstrap != null)
            {
                bootstrap.StartRun();
            }
        }

        public void Bind(Button pve)
        {
            pveButton = pve;
        }
    }
}
