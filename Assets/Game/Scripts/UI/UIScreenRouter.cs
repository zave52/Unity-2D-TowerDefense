using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.UI
{
    public sealed class UIScreenRouter : MonoBehaviour
    {
        [SerializeField] private GameObject menuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject gameWonScreen;

        public void ShowForState(GameState state)
        {
            SetActive(menuScreen, state == GameState.Menu);
            SetActive(gameOverScreen, state == GameState.GameOver);
            SetActive(gameWonScreen, state == GameState.GameWon);
            SetActive(hudScreen, state is GameState.Preparation or GameState.AttackerPreparation or GameState.Battle or GameState.RoundEnd);
        }

        public void Configure(GameObject menu, GameObject hud, GameObject gameOver, GameObject gameWon)
        {
            menuScreen = menu;
            hudScreen = hud;
            gameOverScreen = gameOver;
            gameWonScreen = gameWon;
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
