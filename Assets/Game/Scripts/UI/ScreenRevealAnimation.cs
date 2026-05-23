using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public sealed class ScreenRevealAnimation : MonoBehaviour
    {
        private RectTransform mainPanel;
        private RectTransform subPanel;
        private CanvasGroup mainTextGroup;
        private CanvasGroup subTextGroup;

        private Vector2 originalMainSize;
        private Vector2 originalSubSize;

        private Coroutine activeCoroutine;
        private bool isInitialized = false;

        private void Initialize()
        {
            if (isInitialized) return;

            Transform mainTransform = transform.Find("GameOverImage");
            if (mainTransform == null)
            {
                mainTransform = transform.Find("GameWonImage");
            }
            
            if (mainTransform != null)
            {
                mainPanel = mainTransform.GetComponent<RectTransform>();
                originalMainSize = mainPanel.sizeDelta;

                var mainText = mainTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                if (mainText != null)
                {
                    mainTextGroup = mainText.gameObject.GetComponent<CanvasGroup>();
                    if (mainTextGroup == null) mainTextGroup = mainText.gameObject.AddComponent<CanvasGroup>();
                }
                else
                {
                    var legacyText = mainTransform.GetComponentInChildren<Text>(true);
                    if (legacyText != null)
                    {
                        mainTextGroup = legacyText.gameObject.GetComponent<CanvasGroup>();
                        if (mainTextGroup == null) mainTextGroup = legacyText.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }

            Transform subTransform = transform.Find("StartAgainImage");
            if (subTransform != null)
            {
                subPanel = subTransform.GetComponent<RectTransform>();
                originalSubSize = subPanel.sizeDelta;

                var subText = subTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                if (subText != null)
                {
                    subTextGroup = subText.gameObject.GetComponent<CanvasGroup>();
                    if (subTextGroup == null) subTextGroup = subText.gameObject.AddComponent<CanvasGroup>();
                }
                else
                {
                    var legacySubText = subTransform.GetComponentInChildren<Text>(true);
                    if (legacySubText != null)
                    {
                        subTextGroup = legacySubText.gameObject.GetComponent<CanvasGroup>();
                        if (subTextGroup == null) subTextGroup = legacySubText.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }

            isInitialized = true;
        }

        private void OnEnable()
        {
            Initialize();
            StartReveal();
        }

        private void OnDisable()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }
            ResetToDefault();
        }

        public void StartReveal()
        {
            Initialize();
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }
            activeCoroutine = StartCoroutine(RevealSequence());
        }

        private void ResetToDefault()
        {
            if (mainPanel != null) mainPanel.sizeDelta = originalMainSize;
            if (subPanel != null) subPanel.sizeDelta = originalSubSize;
            if (mainTextGroup != null) mainTextGroup.alpha = 1f;
            if (subTextGroup != null) subTextGroup.alpha = 1f;
        }

        private IEnumerator RevealSequence()
        {
            if (mainPanel != null) mainPanel.sizeDelta = new Vector2(0f, originalMainSize.y);
            if (subPanel != null) subPanel.sizeDelta = new Vector2(0f, originalSubSize.y);
            if (mainTextGroup != null) mainTextGroup.alpha = 0f;
            if (subTextGroup != null) subTextGroup.alpha = 0f;

            float mainExpandDuration = 0.6f;
            float elapsed = 0f;
            while (elapsed < mainExpandDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mainExpandDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (mainPanel != null)
                {
                    mainPanel.sizeDelta = new Vector2(Mathf.Lerp(0f, originalMainSize.x, smoothT), originalMainSize.y);
                }
                yield return null;
            }
            if (mainPanel != null) mainPanel.sizeDelta = originalMainSize;

            yield return new WaitForSeconds(0.15f);

            float mainTextFadeDuration = 0.35f;
            elapsed = 0f;
            while (elapsed < mainTextFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mainTextFadeDuration);
                if (mainTextGroup != null) mainTextGroup.alpha = t;
                yield return null;
            }
            if (mainTextGroup != null) mainTextGroup.alpha = 1f;

            yield return new WaitForSeconds(0.4f);

            float subExpandDuration = 0.5f;
            elapsed = 0f;
            while (elapsed < subExpandDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / subExpandDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (subPanel != null)
                {
                    subPanel.sizeDelta = new Vector2(Mathf.Lerp(0f, originalSubSize.x, smoothT), originalSubSize.y);
                }
                yield return null;
            }
            if (subPanel != null) subPanel.sizeDelta = originalSubSize;

            yield return new WaitForSeconds(0.15f);

            float subTextFadeDuration = 0.3f;
            elapsed = 0f;
            while (elapsed < subTextFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / subTextFadeDuration);
                if (subTextGroup != null) subTextGroup.alpha = t;
                yield return null;
            }
            if (subTextGroup != null) subTextGroup.alpha = 1f;

            activeCoroutine = null;
        }
    }
}
