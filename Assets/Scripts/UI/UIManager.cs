using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Central UI coordinator for the game scene. Manages panel show/hide with
    /// lightweight coroutine animations (no external tween dependency).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [System.Serializable]
        public class Panel
        {
            public string name;
            public CanvasGroup group;
            public GameObject root;
            [System.NonSerialized] public Coroutine animRoutine;
        }

        [Header("Panels")]
        [SerializeField] private List<Panel> panels = new List<Panel>();

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.25f;

        private readonly Dictionary<string, Panel> panelLookup = new Dictionary<string, Panel>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (Panel p in panels)
            {
                if (!string.IsNullOrEmpty(p.name)) panelLookup[p.name] = p;
            }
        }

        public void ShowPanel(string panelName)
        {
            if (!panelLookup.TryGetValue(panelName, out Panel panel)) return;
            if (panel.root != null) panel.root.SetActive(true);
            if (panel.group != null)
            {
                StopCoroutineFor(panel);
                panel.group.blocksRaycasts = true;
                panel.group.interactable = true;
                panel.animRoutine = StartCoroutine(Fade(panel.group, 1f));
            }
        }

        public void HidePanel(string panelName)
        {
            if (!panelLookup.TryGetValue(panelName, out Panel panel)) return;
            if (panel.group != null)
            {
                StopCoroutineFor(panel);
                panel.group.blocksRaycasts = false;
                panel.group.interactable = false;
                panel.animRoutine = StartCoroutine(FadeAndDisable(panel));
            }
            else if (panel.root != null)
            {
                panel.root.SetActive(false);
            }
        }

        private void StopCoroutineFor(Panel panel)
        {
            if (panel.animRoutine != null) StopCoroutine(panel.animRoutine);
        }

        private IEnumerator Fade(CanvasGroup group, float target)
        {
            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }
            group.alpha = target;
        }

        private IEnumerator FadeAndDisable(Panel panel)
        {
            yield return Fade(panel.group, 0f);
            if (panel.root != null) panel.root.SetActive(false);
        }

        public bool IsPanelVisible(string panelName)
        {
            if (!panelLookup.TryGetValue(panelName, out Panel panel)) return false;
            return panel.root != null && panel.root.activeSelf && (panel.group == null || panel.group.alpha > 0.5f);
        }
    }
}
