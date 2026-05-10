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
            bootstrap?.StartBattle();
        }

        public void SetStartWaveButtonVisible(bool visible)
        {
            if (startWaveButton != null)
            {
                startWaveButton.gameObject.SetActive(visible);
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

        public void Bind(Text gold, Text baseHp, Text round)
        {
            goldLabel = gold;
            baseHpLabel = baseHp;
            roundLabel = round;
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
    }
}