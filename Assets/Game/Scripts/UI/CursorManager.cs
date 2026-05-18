using UnityEngine;

namespace TowerDefense.UI
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursors")]
        public Texture2D defaultCursor;
        public Texture2D hoverCursor;
        public Texture2D disabledCursor;

        [Header("Settings")]
        public Vector2 defaultHotspot = Vector2.zero;
        public Vector2 hoverHotspot = Vector2.zero;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetDefaultCursor();
        }

        public void SetDefaultCursor()
        {
            if (defaultCursor != null)
                Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
        }

        public void SetHoverCursor()
        {
            if (hoverCursor != null)
                Cursor.SetCursor(hoverCursor, hoverHotspot, CursorMode.Auto);
        }

        public void SetDisabledCursor()
        {
            if (disabledCursor != null)
                Cursor.SetCursor(disabledCursor, hoverHotspot, CursorMode.Auto);
        }
    }
}
