using UnityEngine;

namespace NovaGuard
{
    /// <summary>Short pooled-within-a-burst geometric sparks; owned by the current run.</summary>
    public sealed class HitBurst : MonoBehaviour
    {
        private Transform[] pieces;
        private SpriteRenderer[] images;
        private Vector2[] velocities;
        private float age;
        private Sprite sprite;
        private Texture2D texture;
        private Color tint;

        public static void Create(Vector3 position, Color color, int count, Transform parent)
        {
            var root = new GameObject("Hit sparks");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.AddComponent<HitBurst>().Initialize(color, count);
        }

        private void Initialize(Color color, int count)
        {
            tint = color;
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            pieces = new Transform[count];
            images = new SpriteRenderer[count];
            velocities = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                var item = new GameObject("Spark", typeof(SpriteRenderer));
                item.transform.SetParent(transform, false);
                item.transform.localScale = Vector3.one * Random.Range(.04f, .1f);
                pieces[i] = item.transform;
                images[i] = item.GetComponent<SpriteRenderer>();
                images[i].sprite = sprite;
                images[i].sortingOrder = 20;
                images[i].color = tint;
                velocities[i] = Random.insideUnitCircle.normalized * Random.Range(1, 5);
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - age / .45f);
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].localPosition += (Vector3)(velocities[i] * Time.deltaTime);
                images[i].color = new Color(tint.r, tint.g, tint.b, alpha);
            }
            if (age >= .45f) Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if (sprite != null) Destroy(sprite);
            if (texture != null) Destroy(texture);
        }
    }
}
