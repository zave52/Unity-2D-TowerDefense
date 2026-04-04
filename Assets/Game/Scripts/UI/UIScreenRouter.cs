using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.UI
{
    public sealed class UIScreenRouter : MonoBehaviour
    {
        [SerializeField] private GameObject menuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;

        public void ShowForState(GameState state)
        {
            SetActive(menuScreen, state == GameState.Menu);
            SetActive(gameOverScreen, state == GameState.GameOver);
            SetActive(hudScreen, state is GameState.Preparation or GameState.Battle or GameState.RoundEnd);
        }

        public void Configure(GameObject menu, GameObject hud, GameObject gameOver)
        {
            menuScreen = menu;
            hudScreen = hud;
            gameOverScreen = gameOver;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}

