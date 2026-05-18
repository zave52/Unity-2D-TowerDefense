using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool checkInteractable = true;
        private Button button;

        public System.Func<bool> IsAffordable = () => true;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CursorManager.Instance == null) return;

            bool canClick = true;
            if (checkInteractable && button != null && !button.interactable)
            {
                canClick = false;
            }
            if (!IsAffordable())
            {
                canClick = false;
            }

            if (canClick)
                CursorManager.Instance.SetHoverCursor();
            else
                CursorManager.Instance.SetDisabledCursor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetDefaultCursor();
        }

        private void OnDisable()
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetDefaultCursor();
        }
    }
}
