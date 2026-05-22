using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Wave Control Button")]
        public Button startWaveButton;

        private GameBootstrap bootstrap;

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            Debug.Log($"[HudView] ConfigureBootstrap called with: {(gameBootstrap != null ? gameBootstrap.name : "NULL")} | On GameObject: {gameObject.name} (Scene Valid: {gameObject.scene.IsValid()})");
            bootstrap = gameBootstrap;
            EnsureStartWaveButton();
            
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

            // Detach and destroy existing children instantly to prevent recursive duplication
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
        }
        
        public void ShowGenericMenuCentered(System.Collections.Generic.IEnumerable<System.Action<Button, int>> setupActions)
        {
            EnsureTowerPanelAndPrefab();
            towerPanel.gameObject.SetActive(true);
            isTowerMenuSpecificActive = false;
            UpdateWaveButtonVisibility();
            
            towerPanel.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            
            // Detach and destroy existing children instantly to prevent recursive duplication
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
                var buttonInstance = Instantiate(prefabToUse, towerPanel);
                buttonInstance.gameObject.SetActive(true);
                
                var rect = buttonInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(startX + (buttonWidth + spacing) * index, 0f);
                }
                
                action(buttonInstance, index);
                index++;
            }
        }
    }
}