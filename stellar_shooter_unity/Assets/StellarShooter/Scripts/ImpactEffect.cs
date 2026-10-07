using UnityEngine;

namespace StellarShooter
{
    public sealed class ImpactEffect : MonoBehaviour
    {
        public SpriteRenderer[] sparks;
        private float age, size = 1;
        private Color tint = Color.white;
        public void Initialize(Color color, float radius) { tint = color; size = radius; }
        private void Update()
        {
            age += Time.deltaTime;
            float progress = age / .45f;
            if (progress >= 1) { Destroy(gameObject); return; }
            for (int i = 0; i < sparks.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / sparks.Length;
                sparks[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (progress * size);
                sparks[i].transform.localScale = Vector3.one * (.18f * (1-progress));
                sparks[i].color = new Color(tint.r, tint.g, tint.b, 1-progress);
            }
        }
    }
}
