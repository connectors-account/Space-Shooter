using UnityEngine;

namespace Starfall
{
    public sealed class SpaceBackdrop : MonoBehaviour
    {
        private readonly Transform[] stars = new Transform[110];
        private readonly float[] speeds = new float[110];
        public void Create(Sprite dot)
        {
            var random = new System.Random(710);
            for (int i = 0; i < stars.Length; i++)
            {
                int layer = i % 3;
                var go = new GameObject("Star");
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = dot;
                renderer.sortingOrder = -10;
                renderer.color = Color.Lerp(new Color(.15f,.28f,.5f), new Color(.7f,.91f,1), layer / 2f);
                stars[i] = go.transform;
                stars[i].position = new Vector3((float)random.NextDouble()*18-9, (float)random.NextDouble()*10-5, 0);
                float size = .025f + layer * .022f;
                stars[i].localScale = new Vector3(size, size * (1+layer*.6f), 1);
                speeds[i] = .18f + layer * .45f;
            }
        }
        public void Tick(float dt)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 p = stars[i].position;
                p.y -= speeds[i] * dt;
                if (p.y < -5.2f) p.y = 5.2f;
                stars[i].position = p;
            }
        }
    }
}
