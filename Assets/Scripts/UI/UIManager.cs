using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Registry of named UI panels. Show one exclusively or hide them all.
    /// Panels register themselves (or are assigned in the inspector / by the setup script).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [System.Serializable]
        public struct NamedPanel
        {
            public string name;
            public GameObject panel;
        }

        public List<NamedPanel> panels = new List<NamedPanel>();

        private readonly Dictionary<string, GameObject> _map = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var p in panels)
                if (p.panel != null && !_map.ContainsKey(p.name))
                    _map[p.name] = p.panel;
        }

        public void RegisterPanel(string name, GameObject panel)
        {
            if (panel == null) return;
            _map[name] = panel;
        }

        public void ShowPanel(string name)
        {
            foreach (var kv in _map)
                kv.Value.SetActive(kv.Key == name);
        }

        public void HideAll()
        {
            foreach (var kv in _map)
                kv.Value.SetActive(false);
        }

        public GameObject GetPanel(string name)
        {
            return _map.TryGetValue(name, out var go) ? go : null;
        }
    }
}
