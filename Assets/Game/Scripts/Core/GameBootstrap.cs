using TowerDefense.Enemies;
using TowerDefense.UI;
using TowerDefense.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
        [SerializeField] private int startGold = 500;
        [SerializeField] private Color cameraBackgroundColor = new Color(0.11f, 0.15f, 0.2f, 1f);
        [SerializeField] private float roundEndDelay = 1.5f;
        [SerializeField] private int maxRounds = 10;

        private GameStateMachine stateMachine;
        private float stateTimer;

        public GameMode CurrentMode { get; private set; }
        public int CurrentGold { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
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
            EnsureMenuStartButton();

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

        public void StartRun(GameMode mode)
        {
            CurrentMode = mode;
            stateMachine.TrySetState(GameState.Preparation);
        }

        public void EndPreparation()
        {
            if (CurrentMode == GameMode.PvP && stateMachine.CurrentState == GameState.Preparation)
            {
                stateMachine.TrySetState(GameState.AttackerPreparation);
            }
            else if (CurrentMode == GameMode.PvP && stateMachine.CurrentState == GameState.AttackerPreparation)
            {
                if (enemySpawner != null && enemySpawner.PvPWaveQueue.Count == 0)
                {
                    Debug.Log("Cannot start wave: Attacker has not selected any enemies!");
                    return;
                }
                stateMachine.TrySetState(GameState.Battle);
            }
            else
            {
                stateMachine.TrySetState(GameState.Battle);
            }
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
            hudView?.SetStartWaveButtonVisible(next == GameState.Preparation || next == GameState.AttackerPreparation);
            
            if (hudView != null)
            {
                if (next == GameState.Preparation)
                {
                    enemySpawner?.PrepareBudgetForRound(Mathf.Max(1, stateMachine.CurrentRound));
                    hudView.SetAttackerBudget(enemySpawner != null ? enemySpawner.RemainingBudget : 0);
                }

                string btnText = (CurrentMode == GameMode.PvP && next == GameState.Preparation) ? "Next (Attacker)" : "Start Wave";
                hudView.SetStartWaveButtonText(btnText);
                hudView.SetAttackerUIVisible(next == GameState.AttackerPreparation);
                hudView.SetAttackerBannerVisible(CurrentMode == GameMode.PvP);
                
                if (towerPlacementSystem != null)
                {
                    towerPlacementSystem.enabled = (next == GameState.Preparation);
                }

                if (next == GameState.AttackerPreparation)
                {
                    var actions = new System.Collections.Generic.List<System.Action<UnityEngine.UI.Button, int>>();
                    if (enemySpawner != null)
                    {
                        foreach (var enemy in enemySpawner.enemyConfigs)
                        {
                            if (enemy == null) continue;
                            var capturedEnemy = enemy;
                            actions.Add((btn, idx) =>
                            {
                                var txt = btn.GetComponentInChildren<UnityEngine.UI.Text>();
                                if (txt != null) txt.text = $"{capturedEnemy.Type}\n({capturedEnemy.SpawnCost})";
                                var tmpTxt = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                                if (tmpTxt != null) tmpTxt.text = $"{capturedEnemy.Type}\n({capturedEnemy.SpawnCost})";
                                
                                var hover = btn.gameObject.GetComponent<TowerDefense.UI.HoverCursor>();
                                if (hover == null) hover = btn.gameObject.AddComponent<TowerDefense.UI.HoverCursor>();
                                hover.IsAffordable = () => enemySpawner != null && enemySpawner.RemainingBudget >= capturedEnemy.SpawnCost;
                                
                                btn.onClick.AddListener(() => 
                                {
                                    if (enemySpawner.TryEnqueueEnemy(capturedEnemy))
                                    {
                                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySpendGold();
                                        hudView.SetAttackerBudget(enemySpawner.RemainingBudget);
                                    }
                                    else
                                    {
                                        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickError();
                                    }
                                });
                            });
                        }
                    }
                    hudView.ShowGenericMenuCentered(actions);
                }
            }

            if (next == GameState.Battle)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayGameMusic();
                enemySpawner?.StartWave(Mathf.Max(1, stateMachine.CurrentRound), CurrentMode == GameMode.PvE);
            }
            else if (next == GameState.Menu || next == GameState.Preparation || next == GameState.AttackerPreparation || next == GameState.GameOver || next == GameState.GameWon)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();
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
            EndBattle(false);
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

        private void EnsureMenuStartButton()
        {
            var menuRoot = screenRouter != null ? screenRouter.MenuScreen : null;
            if (menuRoot == null)
            {
                return;
            }

            if (menuRoot.GetComponentInChildren<Button>(true) != null)
            {
                return;
            }

            var buttonGo = new GameObject(
                "PlayButton",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            buttonGo.transform.SetParent(menuRoot.transform, false);

            var rt = (RectTransform)buttonGo.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(240f, 64f);

            var image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.0f, 0.85f, 0.15f, 1f);
            image.raycastTarget = true;

            var button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => 
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
                StartRun(GameMode.PvE);
            });

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = Vector2.zero;

            var text = textGo.GetComponent<Text>();
            text.text = "Play";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = 28;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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