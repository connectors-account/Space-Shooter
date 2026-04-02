using UnityEngine;

/// <summary>
/// Generates a scrolling starfield particle effect for the background.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Starfield Settings")]
    [SerializeField] private int starCount = 100;
    [SerializeField] private float fieldWidth = 20f;
    [SerializeField] private float fieldHeight = 15f;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float minSize = 0.02f;
    [SerializeField] private float maxSize = 0.08f;

    private struct Star
    {
        public GameObject obj;
        public float speed;
    }

    private Star[] stars;

    private void Start()
    {
        stars = new Star[starCount];

        for (int i = 0; i < starCount; i++)
        {
            CreateStar(i, true);
        }
    }

    private void CreateStar(int index, bool randomY)
    {
        GameObject star = new GameObject("Star_" + index);
        star.transform.SetParent(transform);

        SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
        sr.sprite = CreateStarSprite();
        sr.sortingOrder = -10;

        float brightness = Random.Range(0.3f, 1f);
        sr.color = new Color(brightness, brightness, brightness + Random.Range(0f, 0.2f), brightness);

        float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
        float y = randomY ? Random.Range(-fieldHeight / 2f, fieldHeight / 2f) : fieldHeight / 2f + Random.Range(0f, 2f);
        star.transform.position = new Vector3(x, y, 5f);

        float size = Random.Range(minSize, maxSize);
        star.transform.localScale = new Vector3(size, size, 1f);

        stars[index].obj = star;
        stars[index].speed = Random.Range(minSpeed, maxSpeed);
    }

    private Sprite CreateStarSprite()
    {
        // Create a simple 4x4 white pixel texture for star
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    private void Update()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i].obj == null) continue;

            stars[i].obj.transform.Translate(Vector3.down * stars[i].speed * Time.deltaTime, Space.World);

            if (stars[i].obj.transform.position.y < -fieldHeight / 2f)
            {
                Destroy(stars[i].obj);
                CreateStar(i, false);
            }
        }
    }
}
