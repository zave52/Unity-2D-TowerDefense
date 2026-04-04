using TowerDefense.Enemies;
using TowerDefense.UI;
using TowerDefense.World;
using UnityEngine;

namespace TowerDefense.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private UIScreenRouter screenRouter;
        [SerializeField] private HudView hudView;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private int startGold = 300;
        [SerializeField] private bool debugAutoStart = true;
        [SerializeField] private float debugPreparationSeconds = 1.5f;
        [SerializeField] private float debugRoundEndSeconds = 1.0f;

        private GameStateMachine stateMachine;
        private float stateTimer;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            stateMachine = new GameStateMachine();
            stateMachine.StateChanged += OnStateChanged;

            if (baseHealth != null)
            {
                baseHealth.HealthChanged += OnBaseHealthChanged;
                baseHealth.Depleted += OnBaseDepleted;
            }
        }

        private void Start()
        {
            hudView?.SetGold(startGold);
            if (baseHealth != null)
            {
                hudView?.SetBaseHp(baseHealth.CurrentHealth);
            }

            stateMachine.TrySetState(GameState.Menu);
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.StateChanged -= OnStateChanged;
            }

            if (baseHealth != null)
            {
                baseHealth.HealthChanged -= OnBaseHealthChanged;
                baseHealth.Depleted -= OnBaseDepleted;
            }
        }

        public void StartRun()
        {
            stateMachine.TrySetState(GameState.Preparation);
        }

        public void StartBattle()
        {
            stateMachine.TrySetState(GameState.Battle);
        }

        public void EndBattle(bool gameOver)
        {
            stateMachine.TrySetState(gameOver ? GameState.GameOver : GameState.RoundEnd);
        }

        public void NextRound()
        {
            stateMachine.TrySetState(GameState.Preparation);
        }

        public void BackToMenu()
        {
            stateMachine.TrySetState(GameState.Menu);
        }

        private void Update()
        {
            if (!debugAutoStart || stateMachine == null)
            {
                return;
            }

            stateTimer += Time.deltaTime;

            switch (stateMachine.CurrentState)
            {
                case GameState.Menu:
                    StartRun();
                    break;
                case GameState.Preparation:
                    if (stateTimer >= debugPreparationSeconds)
                    {
                        StartBattle();
                    }

                    break;
                case GameState.RoundEnd:
                    if (stateTimer >= debugRoundEndSeconds)
                    {
                        NextRound();
                    }

                    break;
            }
        }

        private void OnStateChanged(GameState previous, GameState next)
        {
            stateTimer = 0f;
            screenRouter?.ShowForState(next);
            hudView?.SetRound(stateMachine.CurrentRound);

            if (next == GameState.Battle)
            {
                enemySpawner?.StartWave();
            }

            if (next == GameState.RoundEnd || next == GameState.GameOver || next == GameState.Menu)
            {
                enemySpawner?.StopWave();
            }

            if (next == GameState.Menu)
            {
                baseHealth?.ResetHealth();
            }

            Debug.Log($"[GameState] {previous} -> {next} | Round: {stateMachine.CurrentRound}");
        }

        private void OnBaseHealthChanged(int current, int max)
        {
            hudView?.SetBaseHp(current);
        }

        private void OnBaseDepleted()
        {
            stateMachine.TrySetState(GameState.GameOver);
        }

        public void Setup(UIScreenRouter router, HudView hud, EnemySpawner spawner, BaseHealth health)
        {
            screenRouter = router;
            hudView = hud;
            enemySpawner = spawner;
            baseHealth = health;
        }
    }
}

