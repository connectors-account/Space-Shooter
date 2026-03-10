using UnityEngine;

namespace SpaceShooter.Effects
{
    public class StarField : MonoBehaviour
    {
        [Header("Star Settings")]
        [SerializeField] private int starCount = 100;
        [SerializeField] private float fieldWidth = 20f;
        [SerializeField] private float fieldHeight = 15f;
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float minSize = 0.02f;
        [SerializeField] private float maxSize = 0.1f;

        [Header("Visual")]
        [SerializeField] private Color[] starColors;
        [SerializeField] private Sprite starSprite;

        private Star[] stars;

        private class Star
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public float speed;
            public float size;
        }

        private void Start()
        {
            GenerateStars();
        }

        private void GenerateStars()
        {
            stars = new Star[starCount];

            if (starColors == null || starColors.Length == 0)
            {
                starColors = new Color[] { Color.white, new Color(0.8f, 0.8f, 1f), new Color(1f, 1f, 0.8f) };
            }

            for (int i = 0; i < starCount; i++)
            {
                GameObject starObj = new GameObject($"Star_{i}");
                starObj.transform.SetParent(transform);

                float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                float y = Random.Range(-fieldHeight / 2f, fieldHeight / 2f);
                starObj.transform.position = new Vector3(x, y, 10f);

                SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
                sr.sprite = starSprite;
                sr.color = starColors[Random.Range(0, starColors.Length)];
                sr.sortingLayerName = "Background";
                sr.sortingOrder = -100;

                float size = Random.Range(minSize, maxSize);
                starObj.transform.localScale = Vector3.one * size;

                stars[i] = new Star
                {
                    transform = starObj.transform,
                    renderer = sr,
                    speed = Random.Range(minSpeed, maxSpeed),
                    size = size
                };
            }
        }

        private void Update()
        {
            if (stars == null) return;

            foreach (var star in stars)
            {
                if (star.transform == null) continue;

                Vector3 pos = star.transform.position;
                pos.y -= star.speed * Time.deltaTime;

                if (pos.y < -fieldHeight / 2f)
                {
                    pos.y = fieldHeight / 2f;
                    pos.x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                }

                star.transform.position = pos;
            }
        }

        public void SetSpeed(float multiplier)
        {
            if (stars == null) return;

            foreach (var star in stars)
            {
                star.speed = Random.Range(minSpeed, maxSpeed) * multiplier;
            }
        }
    }
}
