using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public sealed class UIScreenRouter : MonoBehaviour
    {
        [SerializeField] private GameObject menuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject gameWonScreen;

        public GameObject MenuScreen => menuScreen;

        private void Start()
        {
            EnsureAnimationComponent(gameOverScreen);
            EnsureAnimationComponent(gameWonScreen);
        }

        private void EnsureAnimationComponent(GameObject screen)
        {
            if (screen != null && screen.GetComponent<ScreenRevealAnimation>() == null)
            {
                screen.AddComponent<ScreenRevealAnimation>();
            }
        }

        public void ShowForState(GameState state, GameMode mode)
        {
            if (state == GameState.GameOver)
            {
                EnsureAnimationComponent(gameOverScreen);
            }
            else if (state == GameState.GameWon)
            {
                EnsureAnimationComponent(gameWonScreen);
            }

            SetActive(menuScreen, state == GameState.Menu);
            SetActive(gameOverScreen, state == GameState.GameOver);
            SetActive(gameWonScreen, state == GameState.GameWon);
            SetActive(hudScreen, state is GameState.Preparation or GameState.AttackerPreparation or GameState.Battle or GameState.RoundEnd);

            if (state == GameState.GameOver)
            {
                SetupClickToRestart(gameOverScreen);
                UpdateLabel(gameOverScreen, mode == GameMode.PvP 
                    ? "Attacker won" 
                    : "Game Over");
            }
            else if (state == GameState.GameWon)
            {
                SetupClickToRestart(gameWonScreen);
                UpdateLabel(gameWonScreen, mode == GameMode.PvP 
                    ? "Defender won" 
                    : "Victory!");
            }
        }

        public void Configure(GameObject menu, GameObject hud, GameObject gameOver, GameObject gameWon)
        {
            menuScreen = menu;
            hudScreen = hud;
            gameOverScreen = gameOver;
            gameWonScreen = gameWon;
            EnsureAnimationComponent(gameOverScreen);
            EnsureAnimationComponent(gameWonScreen);
        }

        private void SetupClickToRestart(GameObject screen)
        {
            if (screen == null) return;

            var img = screen.GetComponent<Image>();
            if (img == null)
            {
                img = screen.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0f);
            }
            img.raycastTarget = true;

            var button = screen.GetComponent<Button>();
            if (button == null)
            {
                button = screen.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (GameBootstrap.Instance != null)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
                    GameBootstrap.Instance.RestartGame();
                }
            });
        }

        private void UpdateLabel(GameObject screen, string text)
        {
            if (screen == null) return;

            var texts = screen.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                if (tmp.gameObject.name == "GameOverLabel" || tmp.gameObject.name == "GameWonLabel")
                {
                    tmp.text = text;
                    return;
                }
            }

            var legacyTexts = screen.GetComponentsInChildren<Text>(true);
            foreach (var legacy in legacyTexts)
            {
                if (legacy.gameObject.name == "GameOverLabel" || legacy.gameObject.name == "GameWonLabel")
                {
                    legacy.text = text;
                    return;
                }
            }

            if (texts.Length > 0)
            {
                texts[0].text = text;
            }
            else if (legacyTexts.Length > 0)
            {
                legacyTexts[0].text = text;
            }
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
