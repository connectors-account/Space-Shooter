using UnityEngine;

namespace OrbitBreaker
{
    /// <summary>Short sprite-based explosion; owned by the run root so restarting cleans every spark.</summary>
    public sealed class BurstEffect : MonoBehaviour
    {
        private readonly SpriteRenderer[] sparks = new SpriteRenderer[10];
        private readonly Vector3[] velocities = new Vector3[10];
        private Color tint;
        private float age;

        public void Initialize(Sprite sprite, Color color)
        {
            tint = color;
            for (int i = 0; i < sparks.Length; i++)
            {
                var go = new GameObject("Spark");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * .8f;
                sparks[i] = go.AddComponent<SpriteRenderer>();
                sparks[i].sprite = sprite;
                sparks[i].color = color;
                sparks[i].sortingOrder = 15;
                float angle = i * Mathf.PI * 2f / sparks.Length;
                velocities[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (1.5f + (i % 3) * .6f);
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= .45f) { Destroy(gameObject); return; }
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i].transform.localPosition += velocities[i] * Time.deltaTime;
                Color c = tint; c.a = 1f - age / .45f;
                sparks[i].color = c;
            }
        }
    }
}
