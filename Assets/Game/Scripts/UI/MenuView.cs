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
            Debug.Log($"[MenuView] ConfigureBootstrap called on GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)} | Scene Valid: {gameObject.scene.IsValid()} | GameBootstrap: {(gameBootstrap != null ? gameBootstrap.name : "NULL")}");
            bootstrap = gameBootstrap;
        }

        private void Awake()
        {
            Debug.Log($"[MenuView] Awake called on GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)} | Scene Valid: {gameObject.scene.IsValid()}");
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
            Debug.Log($"[MenuView] OnEnable called on GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)} | pveButton: {(pveButton != null ? pveButton.name : "NULL")}, pvpButton: {(pvpButton != null ? pvpButton.name : "NULL")}");
            if (pveButton != null)
            {
                pveButton.onClick.AddListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.AddListener(OnPvPClicked);
            }
        }

        private void OnDisable()
        {
            Debug.Log($"[MenuView] OnDisable called on GameObject: {gameObject.name} (HashCode: {gameObject.GetHashCode()}) | Path: {GetGameObjectPath(gameObject)}");
            if (pveButton != null)
            {
                pveButton.onClick.RemoveListener(OnPvEClicked);
            }
            if (pvpButton != null)
            {
                pvpButton.onClick.RemoveListener(OnPvPClicked);
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
    }
}
