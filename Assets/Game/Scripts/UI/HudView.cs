using TowerDefense.Core;
using TowerDefense.World;
using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Enemies;

namespace TowerDefense.UI
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Text goldLabel;
        [SerializeField] private Text baseHpLabel;
        [SerializeField] private Text roundLabel;
        [SerializeField] private Text attackerBudgetLabel;
        
        [Header("Tower Panel")]
        [SerializeField] private RectTransform towerPanel;
        [SerializeField] private Button towerButtonPrefab;
        [SerializeField] private Button attackerCardPrefab;

        [Header("PvP Queue Scroll View")]
        [SerializeField] private ScrollRect queueScrollView;
        [SerializeField] private RectTransform queueContentContainer;
        [SerializeField] private GameObject queueCardPrefab;

        [Header("Wave Control Button")]
        public Button startWaveButton;

        [Header("HUD Time Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button targetButton;
        [SerializeField] private GameObject pauseImage;
        [SerializeField] private GameObject resumeImage;

        private GameBootstrap bootstrap;
        private RectTransform selectionMenuContainer;

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            Debug.Log($"[HudView] ConfigureBootstrap called with: {(gameBootstrap != null ? gameBootstrap.name : "NULL")} | On GameObject: {gameObject.name} (Scene Valid: {gameObject.scene.IsValid()})");
            bootstrap = gameBootstrap;
            EnsureStartWaveButton();
            EnsureTimeControls();
            
            if (startWaveButton != null && startWaveButton.gameObject.GetComponent<HoverCursor>() == null)
            {
                startWaveButton.gameObject.AddComponent<HoverCursor>();
            }
        }

        public void OnStartWaveClicked()
        {
            if (bootstrap == null)
            {
                bootstrap = FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
                Debug.Log($"[HudView] Start Wave Click: bootstrap was NULL, dynamically resolved to {(bootstrap != null ? bootstrap.name : "NULL")}");
            }
            bootstrap?.EndPreparation();
        }

        private bool isWaveButtonIntendedVisible;
        private bool isTowerMenuSpecificActive;

        public void SetStartWaveButtonVisible(bool visible)
        {
            isWaveButtonIntendedVisible = visible;
            UpdateWaveButtonVisibility();
        }

        private void UpdateWaveButtonVisibility()
        {
            if (startWaveButton != null)
            {
                startWaveButton.gameObject.SetActive(isWaveButtonIntendedVisible && !isTowerMenuSpecificActive);
            }
        }

        public void SetStartWaveButtonText(string text)
        {
            if (startWaveButton != null)
            {
                var txt = startWaveButton.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = text;
                }
            }
        }

        public void SetAttackerUIVisible(bool visible)
        {
            if (!visible)
            {
                HideTowerMenu();
            }
        }

        public void SetAttackerBannerVisible(bool visible)
        {
            if (attackerBudgetLabel != null && attackerBudgetLabel.transform.parent != null)
            {
                attackerBudgetLabel.transform.parent.gameObject.SetActive(visible);
            }
        }

        public void SetAttackerBudget(int value)
        {
            if (attackerBudgetLabel != null)
            {
                attackerBudgetLabel.text = value.ToString();
            }
        }

        public void SetGold(int value)
        {
            if (goldLabel != null)
            {
                goldLabel.text = value.ToString();
            }
        }

        public void SetBaseHp(int value)
        {
            if (baseHpLabel != null)
            {
                baseHpLabel.text = value.ToString();
            }
        }

        public void SetRound(int value)
        {
            if (roundLabel != null)
            {
                roundLabel.text = $"Round: {value}";
            }
        }

        public void Bind(Text gold, Text baseHp, Text round, Text attackerBudget = null)
        {
            goldLabel = gold;
            baseHpLabel = baseHp;
            roundLabel = round;
            attackerBudgetLabel = attackerBudget;
        }

        private void EnsureStartWaveButton()
        {
            if (startWaveButton != null)
            {
                startWaveButton.onClick.RemoveAllListeners();
                startWaveButton.onClick.AddListener(() => {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
                    OnStartWaveClicked();
                });
            }
        }

        private void EnsureTimeControls()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(() => {
                    if (bootstrap != null)
                    {
                        bootstrap.TogglePause();
                    }
                });

                if (pauseButton.gameObject.GetComponent<HoverCursor>() == null)
                {
                    pauseButton.gameObject.AddComponent<HoverCursor>();
                }
            }

            if (speedButton != null)
            {
                speedButton.onClick.RemoveAllListeners();
                speedButton.onClick.AddListener(() => {
                    if (bootstrap != null)
                    {
                        bootstrap.CycleSpeed();
                    }
                });

                if (speedButton.gameObject.GetComponent<HoverCursor>() == null)
                {
                    speedButton.gameObject.AddComponent<HoverCursor>();
                }
            }

            if (targetButton != null)
            {
                targetButton.onClick.RemoveAllListeners();
                targetButton.onClick.AddListener(() => {
                    if (bootstrap != null)
                    {
                        bootstrap.CycleTargetingMode();
                    }
                });

                if (targetButton.gameObject.GetComponent<HoverCursor>() == null)
                {
                    targetButton.gameObject.AddComponent<HoverCursor>();
                }
            }
        }

        public void UpdatePauseUI(bool isPaused)
        {
            if (pauseButton != null)
            {
                GameObject pauseIcon = pauseImage;
                GameObject resumeIcon = resumeImage;

                if (pauseIcon == null)
                {
                    var pTrans = pauseButton.transform.Find("PauseImage") ?? 
                                 pauseButton.transform.Find("Pause") ?? 
                                 pauseButton.transform.Find("PauseIcon");
                    if (pTrans != null) pauseIcon = pTrans.gameObject;
                }
                
                if (resumeIcon == null)
                {
                    var rTrans = pauseButton.transform.Find("ResumeImage") ?? 
                                 pauseButton.transform.Find("Resume") ?? 
                                 pauseButton.transform.Find("Play") ?? 
                                 pauseButton.transform.Find("PlayImage") ?? 
                                 pauseButton.transform.Find("PlayIcon");
                    if (rTrans != null) resumeIcon = rTrans.gameObject;
                }

                if (pauseIcon != null && resumeIcon != null)
                {
                    pauseIcon.SetActive(!isPaused);
                    resumeIcon.SetActive(isPaused);
                }
                else if (pauseButton.transform.childCount >= 2)
                {
                    var child0 = pauseButton.transform.GetChild(0);
                    var child1 = pauseButton.transform.GetChild(1);
                    if (child0 != null) child0.gameObject.SetActive(!isPaused);
                    if (child1 != null) child1.gameObject.SetActive(isPaused);
                }
                else
                {
                    Debug.LogWarning($"[HudView] PauseButton '{pauseButton.name}' does not have Pause/Resume images configured or children to toggle!");
                }

                Transform labelTrans = pauseButton.transform.Find("PauseButtonLabel") ??
                                       pauseButton.transform.Find("Label") ??
                                       pauseButton.transform.Find("Text") ??
                                       pauseButton.transform;
                                       
                var legacyText = labelTrans.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = isPaused ? "Resume" : "Pause";
                }
                else
                {
                    var tmproText = labelTrans.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmproText != null)
                    {
                        tmproText.text = isPaused ? "Resume" : "Pause";
                    }
                    else
                    {
                        var tmproUGUI = labelTrans.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (tmproUGUI != null)
                        {
                            tmproUGUI.text = isPaused ? "Resume" : "Pause";
                        }
                    }
                }
            }
        }

        public void UpdateSpeedUI(float speed)
        {
            if (speedButton != null)
            {
                var txt = speedButton.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = $"{speed}x";
                }
                else
                {
                    var tmp = speedButton.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmp != null)
                    {
                        tmp.text = $"{speed}x";
                    }
                }
            }
        }

        public void UpdateTargetingModeUI(TargetingMode mode)
        {
            if (targetButton != null)
            {
                string modeText = "Target: Nearest to Base";
                switch (mode)
                {
                    case TargetingMode.Nearest: modeText = "Target: Nearest to Base"; break;
                    case TargetingMode.Furthest: modeText = "Target: Furthest to Base"; break;
                    case TargetingMode.NearestToTower: modeText = "Target: Nearest to Tower"; break;
                    case TargetingMode.FurthestToTower: modeText = "Target: Furthest to Tower"; break;
                    case TargetingMode.Weakest: modeText = "Target: Weakest"; break;
                    case TargetingMode.Strongest: modeText = "Target: Strongest"; break;
                }

                var txt = targetButton.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = modeText;
                }
                else
                {
                    var tmp = targetButton.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmp != null)
                    {
                        tmp.text = modeText;
                    }
                }
            }
        }

        private void EnsureTowerPanelAndPrefab()
        {
            if (towerPanel == null)
            {
                var panelGo = new GameObject("TowerPanel");
                towerPanel = panelGo.AddComponent<RectTransform>();
                
                var parentTransform = goldLabel != null ? goldLabel.transform.parent : transform;
                towerPanel.SetParent(parentTransform, false);
                
                towerPanel.sizeDelta = new Vector2(200f, 200f);
                panelGo.SetActive(false);
            }

            if (towerButtonPrefab == null)
            {
                var btnGo = new GameObject("TowerButtonPrefab", typeof(RectTransform));
                btnGo.SetActive(false);
                var btnImg = btnGo.AddComponent<Image>();
                btnImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
                towerButtonPrefab = btnGo.AddComponent<Button>();
                towerButtonPrefab.targetGraphic = btnImg;
                
                var textGo = new GameObject("Text", typeof(RectTransform));
                textGo.transform.SetParent(btnGo.transform, false);
                var textRt = textGo.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                var text = textGo.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                text.fontSize = 14;
            }
        }

        public void ShowTowerMenu(Vector3 worldPosition, System.Collections.Generic.IEnumerable<System.Action<Button, int>> setupActions)
        {
            EnsureTowerPanelAndPrefab();

            towerPanel.gameObject.SetActive(true);
            isTowerMenuSpecificActive = true;
            UpdateWaveButtonVisibility();
            towerPanel.position = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPosition) : worldPosition;

            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in towerPanel)
            {
                children.Add(child.gameObject);
            }
            towerPanel.DetachChildren();
            foreach (var childGo in children)
            {
                Destroy(childGo);
            }

            int count = 0;
            foreach (var a in setupActions) count++;
            float buttonWidth = 60f;
            if (towerButtonPrefab != null)
            {
                var pRect = towerButtonPrefab.GetComponent<RectTransform>();
                if (pRect != null) buttonWidth = pRect.sizeDelta.x;
            }
            
            float radius = Mathf.Max(90f, buttonWidth * 1.1f);
            
            int radialCount = Mathf.Max(1, count - 1); 
            float angleStep = 360f / radialCount;
            int index = 0;
            
            foreach (var action in setupActions)
            {
                var buttonInstance = Instantiate(towerButtonPrefab, towerPanel);
                buttonInstance.gameObject.SetActive(true);
                
                var rect = buttonInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    if (index == count - 1)
                    {
                        rect.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        float angle = (90f - index * angleStep) * Mathf.Deg2Rad;
                        float x = Mathf.Cos(angle) * radius;
                        float y = Mathf.Sin(angle) * radius;
                        
                        rect.anchoredPosition = new Vector2(x, y);
                    }
                }

                action(buttonInstance, index);
                index++;
            }
        }

        public void HideTowerMenu()
        {
            if (towerPanel != null)
            {
                towerPanel.gameObject.SetActive(false);
                isTowerMenuSpecificActive = false;
                UpdateWaveButtonVisibility();
            }
            if (selectionMenuContainer != null)
            {
                selectionMenuContainer.gameObject.SetActive(false);
            }
        }
        
        public void ShowGenericMenuCentered(System.Collections.Generic.IEnumerable<System.Action<Button, int>> setupActions)
        {
            if (selectionMenuContainer == null)
            {
                var containerGo = new GameObject("SelectionMenuContainer");
                selectionMenuContainer = containerGo.AddComponent<RectTransform>();
                
                var parentTransform = towerPanel != null ? towerPanel.parent : transform;
                selectionMenuContainer.SetParent(parentTransform, false);
                selectionMenuContainer.sizeDelta = new Vector2(Screen.width, 200f);
            }

            selectionMenuContainer.gameObject.SetActive(true);
            isTowerMenuSpecificActive = false;
            UpdateWaveButtonVisibility();
            
            if (towerPanel != null)
            {
                towerPanel.gameObject.SetActive(false);
            }
            
            selectionMenuContainer.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            
            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in selectionMenuContainer)
            {
                children.Add(child.gameObject);
            }
            selectionMenuContainer.DetachChildren();
            foreach (var childGo in children)
            {
                Destroy(childGo);
            }

            int index = 0;
            
            int count = 0;
            foreach (var a in setupActions) count++;
            var prefabToUse = attackerCardPrefab != null ? attackerCardPrefab : towerButtonPrefab;
            float buttonWidth = 120f;
            if (prefabToUse != null)
            {
                var pRect = prefabToUse.GetComponent<RectTransform>();
                if (pRect != null) buttonWidth = pRect.sizeDelta.x;
            }

            float spacing = 20f;
            float totalWidth = (buttonWidth * count) + (spacing * (count - 1));
            float startX = -totalWidth / 2f + buttonWidth / 2f;

            foreach (var action in setupActions)
            {
                var buttonInstance = Instantiate(prefabToUse, selectionMenuContainer);
                buttonInstance.gameObject.SetActive(true);
                
                var rect = buttonInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(startX + (buttonWidth + spacing) * index, 100f);
                }
                
                action(buttonInstance, index);
                index++;
            }
        }

        public void ShowAttackerQueueList(EnemySpawner spawner, System.Action<int> onRemoveClicked)
        {
            if (queueScrollView == null || queueContentContainer == null || queueCardPrefab == null)
            {
                Debug.LogWarning($"[HudView] PvP Queue UI elements or prefab are not assigned in the Inspector! queueScrollView={queueScrollView != null}, queueContentContainer={queueContentContainer != null}, queueCardPrefab={queueCardPrefab != null}");
                return;
            }

            Debug.Log($"[HudView] ShowAttackerQueueList drawing! Queue size: {(spawner != null ? spawner.PvPWaveQueue.Count : 0)}");
            queueScrollView.gameObject.SetActive(true);
            queueScrollView.transform.SetAsLastSibling();

            var layoutGroup = queueContentContainer.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = queueContentContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            layoutGroup.padding = new RectOffset(15, 15, 10, 10);
            layoutGroup.spacing = 15f;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            var fitter = queueContentContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = queueContentContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in queueContentContainer)
            {
                children.Add(child.gameObject);
            }
            queueContentContainer.DetachChildren();
            foreach (var childGo in children)
            {
                Destroy(childGo);
            }

            if (spawner == null) return;

            for (int i = 0; i < spawner.PvPWaveQueue.Count; i++)
            {
                var enemyConfig = spawner.PvPWaveQueue[i];
                if (enemyConfig == null) continue;

                int capturedIndex = i;
                var cardInstance = Instantiate(queueCardPrefab, queueContentContainer);
                cardInstance.gameObject.SetActive(true);

                var iconImg = cardInstance.transform.Find("EnemyIcon")?.GetComponent<Image>();
                if (iconImg == null)
                {
                    var images = cardInstance.GetComponentsInChildren<Image>(true);
                    var mainImg = cardInstance.GetComponent<Image>();
                    foreach (var img in images)
                    {
                        if (img != mainImg)
                        {
                            iconImg = img;
                            break;
                        }
                    }
                }
                if (iconImg != null)
                {
                    iconImg.sprite = enemyConfig.EnemySprite;
                    iconImg.gameObject.SetActive(enemyConfig.EnemySprite != null);
                }

                Transform labelTransform = cardInstance.transform.Find("EnemyLabel");
                Transform searchRoot = labelTransform != null ? labelTransform : cardInstance.transform;

                var legacyText = searchRoot.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = $"{enemyConfig.Type} ({enemyConfig.SpawnCost})";
                }
                else
                {
                    var tmproText = searchRoot.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmproText != null)
                    {
                        tmproText.text = $"{enemyConfig.Type} ({enemyConfig.SpawnCost})";
                    }
                    else
                    {
                        var tmproUGUI = searchRoot.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (tmproUGUI != null)
                        {
                            tmproUGUI.text = $"{enemyConfig.Type} ({enemyConfig.SpawnCost})";
                        }
                    }
                }

                var deleteBtn = cardInstance.transform.Find("DeleteButton")?.GetComponent<Button>();
                if (deleteBtn == null)
                {
                    deleteBtn = cardInstance.GetComponentInChildren<Button>();
                }
                if (deleteBtn != null)
                {
                    var hover = deleteBtn.gameObject.GetComponent<HoverCursor>();
                    if (hover == null) hover = deleteBtn.gameObject.AddComponent<HoverCursor>();
                    hover.IsAffordable = () => true;

                    deleteBtn.onClick.RemoveAllListeners();
                    deleteBtn.onClick.AddListener(() =>
                    {
                        onRemoveClicked?.Invoke(capturedIndex);
                    });
                }
                else
                {
                    Debug.LogWarning("[HudView] No DeleteButton or Button component found in the instantiated queue card prefab!");
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(queueContentContainer);
        }

        public void HideAttackerQueueList()
        {
            if (queueScrollView != null)
            {
                queueScrollView.gameObject.SetActive(false);
            }
        }
    }
}