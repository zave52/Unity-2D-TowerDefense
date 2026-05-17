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

        [Header("Wave Control Button")]
        public Button startWaveButton;

        private GameBootstrap bootstrap;

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            bootstrap = gameBootstrap;
            EnsureStartWaveButton();
        }

        public void OnStartWaveClicked()
        {
            bootstrap?.EndPreparation();
        }

        public void SetStartWaveButtonVisible(bool visible)
        {
            if (startWaveButton != null)
            {
                startWaveButton.gameObject.SetActive(visible);
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

        public void SetAttackerBudget(int value)
        {
            if (attackerBudgetLabel != null)
            {
                attackerBudgetLabel.text = $"Attacker Budget: {value}";
            }
        }

        public void SetGold(int value)
        {
            if (goldLabel != null)
            {
                goldLabel.text = $"Gold: {value}";
            }
        }

        public void SetBaseHp(int value)
        {
            if (baseHpLabel != null)
            {
                baseHpLabel.text = $"Base HP: {value}";
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
                startWaveButton.onClick.AddListener(OnStartWaveClicked);
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
            towerPanel.position = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPosition) : worldPosition;

            foreach (Transform child in towerPanel)
            {
                Destroy(child.gameObject);
            }

            int index = 0;
            foreach (var action in setupActions)
            {
                var buttonInstance = Instantiate(towerButtonPrefab, towerPanel);
                buttonInstance.gameObject.SetActive(true);
                action(buttonInstance, index);
                index++;
            }
        }

        public void HideTowerMenu()
        {
            if (towerPanel != null)
            {
                towerPanel.gameObject.SetActive(false);
            }
        }
        
        public void ShowGenericMenuCentered(System.Collections.Generic.IEnumerable<System.Action<Button, int>> setupActions)
        {
            EnsureTowerPanelAndPrefab();
            towerPanel.gameObject.SetActive(true);
            towerPanel.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            
            foreach (Transform child in towerPanel)
            {
                Destroy(child.gameObject);
            }

            int index = 0;
            
            int count = 0;
            foreach (var a in setupActions) count++;
            float buttonWidth = 100f;
            float spacing = 20f;
            float totalWidth = (buttonWidth * count) + (spacing * (count - 1));
            float startX = -totalWidth / 2f + buttonWidth / 2f;

            foreach (var action in setupActions)
            {
                var buttonInstance = Instantiate(towerButtonPrefab, towerPanel);
                buttonInstance.gameObject.SetActive(true);
                
                var rect = buttonInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(buttonWidth, 44f);
                    rect.anchoredPosition = new Vector2(startX + (buttonWidth + spacing) * index, 0f);
                }
                
                action(buttonInstance, index);
                index++;
            }
        }
    }
}