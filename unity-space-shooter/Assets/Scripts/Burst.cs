using UnityEngine;

namespace Starward
{
    public sealed class Burst : MonoBehaviour
    {
        public Sprite spark;
        readonly Transform[] parts = new Transform[10];
        readonly SpriteRenderer[] renderers = new SpriteRenderer[10];
        readonly Vector3[] directions = new Vector3[10];
        float age;
        Color tint;
        public void Initialize(Color color)
        {
            tint = color;
            for (int i = 0; i < parts.Length; i++)
            {
                GameObject p = new GameObject("Spark");
                p.transform.SetParent(transform, false);
                parts[i] = p.transform;
                parts[i].localScale = Vector3.one * .2f;
                renderers[i] = p.AddComponent<SpriteRenderer>();
                renderers[i].sprite = spark;
                renderers[i].sortingOrder = 15;
                float a = i * Mathf.PI * 2 / parts.Length;
                directions[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * (1.5f + i % 3);
            }
        }
        void Update()
        {
            age += Time.deltaTime;
            if (age > .45f) { Destroy(gameObject); return; }
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                parts[i].localPosition += directions[i] * Time.deltaTime;
                Color c = tint; c.a = 1 - age / .45f;
                renderers[i].color = c;
            }
        }
    }
}
