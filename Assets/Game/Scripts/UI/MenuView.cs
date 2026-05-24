using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private Button pveButton;
        [SerializeField] private Button pvpButton;

        private GameBootstrap bootstrap;

        private RectTransform panelRectTransform;
        private Vector2 originalPanelSize;
        private CanvasGroup pveCanvasGroup;
        private CanvasGroup pvpCanvasGroup;
        private CanvasGroup labelCanvasGroup;
        private Coroutine revealCoroutine;
        private bool isInitialized = false;

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "NULL";
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        public void ConfigureBootstrap(GameBootstrap gameBootstrap)
        {
            bootstrap = gameBootstrap;
        }

        private void Awake()
        {
            
            Transform panelTransform = transform.Find("MenuImage");
            if (panelTransform != null)
            {
                panelRectTransform = panelTransform.GetComponent<RectTransform>();
            }
            if (panelRectTransform == null)
            {
                panelRectTransform = GetComponent<RectTransform>();
            }

            if (panelRectTransform != null)
            {
                originalPanelSize = panelRectTransform.sizeDelta;
                isInitialized = true;

                if (panelRectTransform.gameObject.GetComponent<RectMask2D>() == null)
                {
                    panelRectTransform.gameObject.AddComponent<RectMask2D>();
                }
            }

            Transform labelTransform = transform.Find("MenuImage/MenuLabel");
            if (labelTransform == null)
            {
                foreach (var child in GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "MenuLabel") { labelTransform = child; break; }
                }
            }
            if (labelTransform != null)
            {
                labelCanvasGroup = labelTransform.GetComponent<CanvasGroup>();
                if (labelCanvasGroup == null)
                {
                    labelCanvasGroup = labelTransform.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (pveButton == null)
            {
                pveButton = transform.Find("PvEModeButton")?.GetComponent<Button>();
                if (pveButton == null)
                {
                    foreach (var btn in GetComponentsInChildren<Button>(true))
                    {
                        if (btn.name.Contains("PvE")) { pveButton = btn; break; }
                    }
                }
            }

            if (pvpButton == null)
            {
                pvpButton = transform.Find("PvPModeButton")?.GetComponent<Button>();
                if (pvpButton == null)
                {
                    foreach (var btn in GetComponentsInChildren<Button>(true))
                    {
                        if (btn.name.Contains("PvP")) { pvpButton = btn; break; }
                    }
                }
            }

            if (pveButton != null && pveButton.gameObject.GetComponent<HoverCursor>() == null)
                pveButton.gameObject.AddComponent<HoverCursor>();
            
            if (pvpButton != null && pvpButton.gameObject.GetComponent<HoverCursor>() == null)
                pvpButton.gameObject.AddComponent<HoverCursor>();
        }

        private void OnEnable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.AddListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.AddListener(OnPvPClicked);
            }

            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
            }
            revealCoroutine = StartCoroutine(RevealRoutine());
        }

        private void OnDisable()
        {
            if (pveButton != null)
            {
                pveButton.onClick.RemoveListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.RemoveListener(OnPvPClicked);
            }

            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
                revealCoroutine = null;
            }

            if (isInitialized && panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = originalPanelSize;
            }

            if (pveCanvasGroup != null)
            {
                pveCanvasGroup.alpha = 1f;
                pveCanvasGroup.interactable = true;
                pveCanvasGroup.blocksRaycasts = true;
            }

            if (pvpCanvasGroup != null)
            {
                pvpCanvasGroup.alpha = 1f;
                pvpCanvasGroup.interactable = true;
                pvpCanvasGroup.blocksRaycasts = true;
            }

            if (labelCanvasGroup != null)
            {
                labelCanvasGroup.alpha = 1f;
                labelCanvasGroup.interactable = true;
                labelCanvasGroup.blocksRaycasts = true;
            }
        }

        private void OnPvEClicked()
        {
            if (bootstrap == null)
            {
                bootstrap = FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
                Debug.Log($"[MenuView] PvE Click: bootstrap was NULL, dynamically resolved to {(bootstrap != null ? bootstrap.name : "NULL")}");
            }
            Debug.Log($"[MenuView] PvE Mode Button Clicked! On GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)} | Bootstrap reference: {(bootstrap != null ? bootstrap.name : "NULL")}");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
            if (bootstrap != null)
            {
                bootstrap.StartRun(GameMode.PvE);
            }
        }

        private void OnPvPClicked()
        {
            if (bootstrap == null)
            {
                bootstrap = FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include);
                Debug.Log($"[MenuView] PvP Click: bootstrap was NULL, dynamically resolved to {(bootstrap != null ? bootstrap.name : "NULL")}");
            }
            Debug.Log($"[MenuView] PvP Mode Button Clicked! On GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)} | Bootstrap reference: {(bootstrap != null ? bootstrap.name : "NULL")}");
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSuccess();
            if (bootstrap != null)
            {
                bootstrap.StartRun(GameMode.PvP);
            }
        }

        public void Bind(Button pve, Button pvp)
        {
            pveButton = pve;
            pvpButton = pvp;
        }

        private System.Collections.IEnumerator RevealRoutine()
        {
            if (!isInitialized)
            {
                Transform panelTransform = transform.Find("MenuImage");
                if (panelTransform != null)
                {
                    panelRectTransform = panelTransform.GetComponent<RectTransform>();
                }
                if (panelRectTransform == null)
                {
                    panelRectTransform = GetComponent<RectTransform>();
                }

                if (panelRectTransform != null)
                {
                    originalPanelSize = panelRectTransform.sizeDelta;
                    isInitialized = true;

                    if (panelRectTransform.gameObject.GetComponent<RectMask2D>() == null)
                    {
                        panelRectTransform.gameObject.AddComponent<RectMask2D>();
                    }
                }
            }

            if (pveButton != null && pveCanvasGroup == null)
            {
                pveCanvasGroup = pveButton.GetComponent<CanvasGroup>();
                if (pveCanvasGroup == null)
                {
                    pveCanvasGroup = pveButton.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (pvpButton != null && pvpCanvasGroup == null)
            {
                pvpCanvasGroup = pvpButton.GetComponent<CanvasGroup>();
                if (pvpCanvasGroup == null)
                {
                    pvpCanvasGroup = pvpButton.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (labelCanvasGroup == null)
            {
                Transform labelTransform = transform.Find("MenuImage/MenuLabel");
                if (labelTransform == null)
                {
                    foreach (var child in GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == "MenuLabel") { labelTransform = child; break; }
                    }
                }
                if (labelTransform != null)
                {
                    labelCanvasGroup = labelTransform.GetComponent<CanvasGroup>();
                    if (labelCanvasGroup == null)
                    {
                        labelCanvasGroup = labelTransform.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = new Vector2(0f, originalPanelSize.y);
            }

            if (pveCanvasGroup != null)
            {
                pveCanvasGroup.alpha = 0f;
                pveCanvasGroup.interactable = false;
                pveCanvasGroup.blocksRaycasts = false;
            }

            if (pvpCanvasGroup != null)
            {
                pvpCanvasGroup.alpha = 0f;
                pvpCanvasGroup.interactable = false;
                pvpCanvasGroup.blocksRaycasts = false;
            }

            if (labelCanvasGroup != null)
            {
                labelCanvasGroup.alpha = 0f;
                labelCanvasGroup.interactable = false;
                labelCanvasGroup.blocksRaycasts = false;
            }

            float expandDuration = 0.6f;
            float elapsed = 0f;

            while (elapsed < expandDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / expandDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (panelRectTransform != null)
                {
                    panelRectTransform.sizeDelta = new Vector2(Mathf.Lerp(0f, originalPanelSize.x, smoothT), originalPanelSize.y);
                }
                yield return null;
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = originalPanelSize;
            }

            yield return new WaitForSeconds(0.4f);

            float fadeDuration = 0.4f;
            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                if (pveCanvasGroup != null)
                {
                    pveCanvasGroup.alpha = t;
                }
                if (pvpCanvasGroup != null)
                {
                    pvpCanvasGroup.alpha = t;
                }
                if (labelCanvasGroup != null)
                {
                    labelCanvasGroup.alpha = t;
                }
                yield return null;
            }

            if (pveCanvasGroup != null)
            {
                pveCanvasGroup.alpha = 1f;
                pveCanvasGroup.interactable = true;
                pveCanvasGroup.blocksRaycasts = true;
            }

            if (pvpCanvasGroup != null)
            {
                pvpCanvasGroup.alpha = 1f;
                pvpCanvasGroup.interactable = true;
                pvpCanvasGroup.blocksRaycasts = true;
            }

            if (labelCanvasGroup != null)
            {
                labelCanvasGroup.alpha = 1f;
                labelCanvasGroup.interactable = true;
                labelCanvasGroup.blocksRaycasts = true;
            }

            revealCoroutine = null;
        }
    }
}
