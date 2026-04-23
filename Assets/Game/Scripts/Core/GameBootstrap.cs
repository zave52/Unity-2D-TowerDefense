using TowerDefense.Enemies;
using TowerDefense.UI;
using TowerDefense.World;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace TowerDefense.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private UIScreenRouter screenRouter;
        [SerializeField] private HudView hudView;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private TowerPlacementSystem towerPlacementSystem;
        [SerializeField] private int startGold = 300;
        [SerializeField] private Color cameraBackgroundColor = new Color(0.11f, 0.15f, 0.2f, 1f);
        [SerializeField] private bool debugAutoStart = false;
        [SerializeField] private float debugPreparationSeconds = 1.5f;
        [SerializeField] private float debugRoundEndSeconds = 1.0f;
        [SerializeField] private int maxRounds = 10;

        private GameStateMachine stateMachine;
        private float stateTimer;

        public int CurrentGold { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureEventSystemInputModule();
            stateMachine = new GameStateMachine();
            stateMachine.StateChanged += OnStateChanged;

            if (baseHealth != null)
            {
                baseHealth.HealthChanged += OnBaseHealthChanged;
                baseHealth.Depleted += OnBaseDepleted;
            }

            if (enemySpawner != null)
            {
                enemySpawner.EnemyKilled += OnEnemyKilled;
            }
        }

        private void Start()
        {
            EnsureCameraBackground();
            CurrentGold = startGold;
            hudView?.SetGold(CurrentGold);
            if (baseHealth != null)
            {
                hudView?.SetBaseHp(baseHealth.CurrentHealth);
            }

            EnsureTowerPlacementSystem();
            EnsureEffectsManager();

            stateMachine.TrySetState(GameState.Menu);
            
            if (hudView != null)
            {
                hudView.ConfigureBootstrap(this);
            }

            if (debugAutoStart)
            {
                StartRun();
                StartBattle();
            }
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

            if (enemySpawner != null)
            {
                enemySpawner.EnemyKilled -= OnEnemyKilled;
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
            hudView?.SetNextWaveButtonVisible(next == GameState.Preparation);

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
                CurrentGold = startGold;
                hudView?.SetGold(CurrentGold);
                DestroyAllTowers();
                enemySpawner?.ClearEnemies();
            }

            if (next == GameState.RoundEnd)
            {
                if (stateMachine.CurrentRound >= maxRounds)
                {
                    stateMachine.TrySetState(GameState.GameWon);
                }
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

        private void OnEnemyKilled(int reward)
        {
            AddGold(reward);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            CurrentGold += amount;
            hudView?.SetGold(CurrentGold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentGold >= amount)
            {
                CurrentGold -= amount;
                hudView?.SetGold(CurrentGold);
                return true;
            }
            return false;
        }

        public void Setup(UIScreenRouter router, HudView hud, EnemySpawner spawner, BaseHealth health)
        {
            screenRouter = router;
            hudView = hud;
            enemySpawner = spawner;
            baseHealth = health;
        }

        private void EnsureTowerPlacementSystem()
        {
            if (towerPlacementSystem == null)
            {
                towerPlacementSystem = GetComponent<TowerPlacementSystem>();
            }

            if (towerPlacementSystem == null)
            {
                towerPlacementSystem = gameObject.AddComponent<TowerPlacementSystem>();
            }

            var waypointPath = FindAnyObjectByType<WaypointPath>();
            towerPlacementSystem.Configure(waypointPath, hudView, TrySpendGold, () => CurrentGold);
        }

        private void DestroyAllTowers()
        {
            var towers = FindObjectsByType<PlacedTower>(FindObjectsInactive.Exclude);
            foreach (var tower in towers)
            {
                if (tower != null)
                {
                    Destroy(tower.gameObject);
                }
            }
        }

        public void RestartGame()
        {
            stateMachine.TrySetState(GameState.Menu);
        }

        private void EnsureEffectsManager()
        {
            if (EffectsManager.Instance == null)
            {
                var effectsManagerObj = new GameObject("EffectsManager");
                effectsManagerObj.AddComponent<EffectsManager>();
            }
        }

        private void EnsureCameraBackground()
        {
            var cameraInstance = Camera.main;
            if (cameraInstance == null)
            {
                return;
            }

            cameraInstance.clearFlags = CameraClearFlags.SolidColor;
            cameraInstance.backgroundColor = cameraBackgroundColor;
        }

        private static void EnsureEventSystemInputModule()
        {
            var eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var standalone = eventSystem.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                Destroy(standalone);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#endif
        }
    }
}
