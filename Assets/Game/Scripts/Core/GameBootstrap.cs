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
        public UIScreenRouter screenRouter;
        public MenuView menuView;
        public HudView hudView;
        public EnemySpawner enemySpawner;
        public BaseHealth baseHealth;
        public TowerPlacementSystem towerPlacementSystem;
        [SerializeField] private int startGold = 300;
        [SerializeField] private Color cameraBackgroundColor = new Color(0.11f, 0.15f, 0.2f, 1f);
        [SerializeField] private float roundEndDelay = 1.5f; // Renamed for clarity
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
                enemySpawner.WaveCompleted += OnWaveCompleted;
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
            
            if (menuView != null)
            {
                menuView.ConfigureBootstrap(this);
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
                enemySpawner.WaveCompleted -= OnWaveCompleted;
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
            if (stateMachine.CurrentState == GameState.RoundEnd)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= roundEndDelay)
                {
                    NextRound();
                }
            }
        }

        private void OnStateChanged(GameState previous, GameState next)
        {
            stateTimer = 0f;
            screenRouter?.ShowForState(next);
            hudView?.SetRound(stateMachine.CurrentRound);
            hudView?.SetStartWaveButtonVisible(next == GameState.Preparation);

            if (next == GameState.Battle)
            {
                enemySpawner?.StartWave(Mathf.Max(1, stateMachine.CurrentRound));
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

        private void OnWaveCompleted()
        {
            EndBattle(false); // Wave completed, not game over
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

        public void Setup(UIScreenRouter router, HudView hud, MenuView menu, EnemySpawner spawner, BaseHealth health)
        {
            screenRouter = router;
            hudView = hud;
            menuView = menu;
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