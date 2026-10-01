using UnityEngine;

namespace NovaGuard
{
    /// <summary>Three repeatable depth layers of geometry-only luminous points.</summary>
    public sealed class Starfield : MonoBehaviour
    {
        private Transform[] stars;
        private float[] speeds;
        private Sprite point;
        private Texture2D texture;
        private const int Count = 150;

        private void Awake()
        {
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            point = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            stars = new Transform[Count];
            speeds = new float[Count];
            var random = new System.Random(7401);
            for (int i = 0; i < Count; i++)
            {
                int layer = i % 3;
                var star = new GameObject("Star " + i, typeof(SpriteRenderer));
                star.transform.SetParent(transform, false);
                star.transform.position = new Vector3((float)random.NextDouble() * 32 - 16,
                    (float)random.NextDouble() * 20 - 10, 2);
                float size = .025f + layer * .018f;
                star.transform.localScale = new Vector3(size, size * (1 + layer * .35f), 1);
                var renderer = star.GetComponent<SpriteRenderer>();
                renderer.sprite = point;
                renderer.color = new Color(.5f + layer * .18f, .65f + layer * .13f, 1, .35f + layer * .25f);
                renderer.sortingOrder = -100 + layer;
                stars[i] = star.transform;
                speeds[i] = .35f + layer * .7f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 position = stars[i].position;
                position.y -= speeds[i] * dt;
                if (position.y < -10) position.y += 20;
                stars[i].position = position;
            }
        }
        private void OnDestroy()
        {
            if (point != null) Destroy(point);
            if (texture != null) Destroy(texture);
        }
    }
}
