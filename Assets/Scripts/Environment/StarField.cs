using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Generates a simple procedural star field using a particle system.
    /// Creates the illusion of moving through space.
    /// </summary>
    public class StarField : MonoBehaviour
    {
        [Header("Star Field Settings")]
        [SerializeField] private int starCount = 200;
        [SerializeField] private float fieldWidth = 20f;
        [SerializeField] private float fieldHeight = 15f;
        [SerializeField] private float minStarSpeed = 1f;
        [SerializeField] private float maxStarSpeed = 4f;
        [SerializeField] private float minStarSize = 0.02f;
        [SerializeField] private float maxStarSize = 0.08f;

        private struct Star
        {
            public GameObject obj;
            public float speed;
        }

        private Star[] stars;

        private void Start()
        {
            GenerateStarField();
        }

        private void GenerateStarField()
        {
            stars = new Star[starCount];

            for (int i = 0; i < starCount; i++)
            {
                // Create a simple white square for each star
                GameObject star = new GameObject("Star_" + i);
                star.transform.parent = transform;

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = CreateStarSprite();
                sr.sortingOrder = -10;

                // Random position
                float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                float y = Random.Range(-fieldHeight / 2f, fieldHeight / 2f);
                star.transform.position = new Vector3(x, y, 5f);

                // Random size
                float size = Random.Range(minStarSize, maxStarSize);
                star.transform.localScale = new Vector3(size, size, 1f);

                // Random brightness
                float brightness = Random.Range(0.3f, 1f);
                sr.color = new Color(brightness, brightness, brightness + Random.Range(0f, 0.1f), brightness);

                stars[i] = new Star
                {
                    obj = star,
                    speed = Random.Range(minStarSpeed, maxStarSpeed)
                };
            }
        }

        private void Update()
        {
            if (stars == null) return;

            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].obj == null) continue;

                // Move star downward
                stars[i].obj.transform.position += Vector3.down * stars[i].speed * Time.deltaTime;

                // Wrap around when below screen
                if (stars[i].obj.transform.position.y < -fieldHeight / 2f)
                {
                    float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                    stars[i].obj.transform.position = new Vector3(x, fieldHeight / 2f, 5f);
                }
            }
        }

        /// <summary>Creates a simple 1x1 white sprite at runtime.</summary>
        private Sprite CreateStarSprite()
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
