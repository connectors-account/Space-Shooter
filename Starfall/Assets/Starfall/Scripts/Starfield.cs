using UnityEngine;

namespace Starfall
{
    public sealed class Starfield : MonoBehaviour
    {
        Transform[] stars;
        float[] speeds;
        Camera view;
        public void Initialize(Sprite sprite)
        {
            view = Camera.main;
            stars = new Transform[150]; speeds = new float[stars.Length];
            var rng = new System.Random(4317);
            var root = new GameObject("Three-depth starfield").transform; root.SetParent(transform);
            for (int i = 0; i < stars.Length; i++)
            {
                int layer = i % 3;
                var go = new GameObject("Star " + i); stars[i] = go.transform; stars[i].SetParent(root);
                stars[i].position = new Vector3((float)rng.NextDouble() * 18 - 9, (float)rng.NextDouble() * 11 - 5.5f, 0);
                stars[i].localScale = Vector3.one * (0.22f + layer * 0.17f);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = -20 + layer;
                sr.color = new Color(0.52f + layer * 0.18f, 0.7f + layer * 0.1f, 1, 0.35f + layer * 0.25f);
                speeds[i] = 0.18f + layer * 0.45f;
            }
        }
        void Update()
        {
            // Preserve the same arena and UI on ultrawide and portrait windows.
            if (view != null)
            {
                float ratio = (float)Screen.width / Mathf.Max(1, Screen.height) / (16f / 9f);
                view.rect = ratio >= 1 ? new Rect((1 - 1 / ratio) / 2, 0, 1 / ratio, 1) : new Rect(0, (1 - ratio) / 2, 1, ratio);
            }
            if (stars == null) return;
            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 p = stars[i].position; p.y -= speeds[i] * Time.deltaTime;
                if (p.y < -5.5f) p.y += 11;
                stars[i].position = p;
            }
        }
    }
}
