using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private Button pveButton;
        [SerializeField] private Button pvpButton;

        private GameBootstrap bootstrap;

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            bootstrap = gameBootstrap;
        }

        private void Awake()
        {
            if (pveButton != null && pveButton.gameObject.GetComponent<HoverCursor>() == null)
                pveButton.gameObject.AddComponent<HoverCursor>();
            
            if (pvpButton != null && pvpButton.gameObject.GetComponent<HoverCursor>() == null)
                pvpButton.gameObject.AddComponent<HoverCursor>();
        }

        private void OnEnable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.AddListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.AddListener(OnPvPClicked);
            }
        }

        private void OnDisable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.RemoveListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.RemoveListener(OnPvPClicked);
            }
        }

        private void OnPvEClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
            if (bootstrap != null)
            {
                bootstrap.StartRun(GameMode.PvE);
            }
        }

        private void OnPvPClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
            if (bootstrap != null)
            {
                bootstrap.StartRun(GameMode.PvP);
            }
        }

        public void Bind(Button pve, Button pvp)
        {
            pveButton = pve;
            pvpButton = pvp;
        }
    }
}
