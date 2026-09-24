using UnityEngine;

namespace Starward
{
    public sealed class Starfield : MonoBehaviour
    {
        public Sprite star;
        readonly Transform[] stars = new Transform[160];
        readonly float[] speeds = new float[160];
        void Start()
        {
            System.Random random = new System.Random(421);
            for (int i = 0; i < stars.Length; i++)
            {
                int layer = i % 3;
                GameObject go = new GameObject("Star " + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3((float)random.NextDouble() * 22 - 11, (float)random.NextDouble() * 13 - 6.5f, 0);
                go.transform.localScale = Vector3.one * (.035f + layer * .025f);
                SpriteRenderer sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = star;
                sprite.sortingOrder = -20 + layer;
                sprite.color = new Color(.55f + layer * .15f, .75f, 1, .4f + layer * .25f);
                stars[i] = go.transform;
                speeds[i] = .18f + layer * .45f;
            }
        }
        void Update()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 p = stars[i].localPosition;
                p.y -= speeds[i] * Time.deltaTime;
                if (p.y < -6.5f) p.y += 13;
                stars[i].localPosition = p;
            }
        }
    }
}
