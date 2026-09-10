using UnityEngine;

namespace Starfall
{
    public sealed class Starfield : MonoBehaviour
    {
        public Sprite starSprite;
        public int starCount = 100;
        private Transform[] stars;
        private float[] speeds;
        private SpriteRenderer[] visuals;
        private float width;
        private float clock;

        private void Start()
        {
            width = Camera.main.orthographicSize * Camera.main.aspect;
            stars = new Transform[starCount];
            speeds = new float[starCount];
            visuals = new SpriteRenderer[starCount];
            var random = new System.Random(73419);
            for (int i = 0; i < starCount; i++)
            {
                GameObject star = new GameObject("Star " + i);
                star.transform.SetParent(transform, false);
                star.transform.position = new Vector3((float)random.NextDouble() * width * 2 - width, (float)random.NextDouble() * 22 - 11, 0);
                int layer = i % 3;
                float size = 0.018f + layer * 0.017f;
                star.transform.localScale = new Vector3(size, size * (1f + layer * 0.35f), 1);
                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = starSprite;
                renderer.sortingOrder = -20 + layer;
                renderer.color = new Color(0.55f + layer * 0.15f, 0.75f + layer * 0.10f, 1f, 0.45f + layer * 0.2f);
                stars[i] = star.transform;
                speeds[i] = 0.3f + layer * 0.65f;
                visuals[i] = renderer;
            }
        }

        private void Update()
        {
            if (stars == null) return;
            clock += Time.deltaTime;
            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 position = stars[i].position;
                position.y -= speeds[i] * Time.deltaTime;
                if (position.y < -11) position.y += 22;
                stars[i].position = position;
                Color color = visuals[i].color;
                color.a = 0.5f + 0.25f * Mathf.Sin(clock * 1.4f + i);
                visuals[i].color = color;
            }
        }
    }
}
