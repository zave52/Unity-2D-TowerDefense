using UnityEngine;

namespace TowerDefense.UI
{
    public class CursorManager : MonoBehaviour
    {
        private static CursorManager _instance;
        public static CursorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<CursorManager>(FindObjectsInactive.Include);
                    if (_instance != null && !_instance.gameObject.activeSelf)
                    {
                        _instance.gameObject.SetActive(true);
                    }
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("Cursors")]
        public Texture2D defaultCursor;
        public Texture2D hoverCursor;
        public Texture2D disabledCursor;

        [Header("Settings")]
        public Vector2 defaultHotspot = Vector2.zero;
        public Vector2 hoverHotspot = Vector2.zero;

        private void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.Log($"[CursorManager] Duplicate CursorManager detected on '{gameObject.name}'. Destroying duplicate GameObject.");
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Start()
        {
            SetDefaultCursor();
            StartCoroutine(LinuxCursorFallbackCoroutine());
        }

        private System.Collections.IEnumerator LinuxCursorFallbackCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            SetDefaultCursor();
            yield return new WaitForSeconds(0.3f);
            SetDefaultCursor();
            yield return new WaitForSeconds(0.7f);
            SetDefaultCursor();
            yield return new WaitForSeconds(1.5f);
            SetDefaultCursor();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetDefaultCursor();
            }
        }

        public void SetDefaultCursor()
        {
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
            }
            else
            {
                Debug.LogWarning("[CursorManager] defaultCursor texture is NULL!");
            }
        }

        public void SetHoverCursor()
        {
            if (hoverCursor != null)
            {
                Cursor.SetCursor(hoverCursor, hoverHotspot, CursorMode.Auto);
            }
        }

        public void SetDisabledCursor()
        {
            if (disabledCursor != null)
            {
                Cursor.SetCursor(disabledCursor, hoverHotspot, CursorMode.Auto);
            }
        }
    }
}
