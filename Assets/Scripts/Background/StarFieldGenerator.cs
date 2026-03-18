using UnityEngine;

/// <summary>
/// Generates procedural star particles for the background.
/// Creates a multi-layer starfield with parallax effect using particle systems.
/// </summary>
public class StarFieldGenerator : MonoBehaviour
{
    [Header("Star Layers")]
    [SerializeField] private int farStarCount = 100;
    [SerializeField] private int midStarCount = 50;
    [SerializeField] private int nearStarCount = 25;

    [SerializeField] private float farStarSpeed = 0.5f;
    [SerializeField] private float midStarSpeed = 1.5f;
    [SerializeField] private float nearStarSpeed = 3f;

    [SerializeField] private Color farStarColor = new Color(0.5f, 0.5f, 0.6f, 0.5f);
    [SerializeField] private Color midStarColor = new Color(0.7f, 0.7f, 0.8f, 0.7f);
    [SerializeField] private Color nearStarColor = new Color(1f, 1f, 1f, 1f);

    private Camera mainCam;
    private float halfHeight;
    private float halfWidth;

    // Star data
    private struct Star
    {
        public GameObject obj;
        public float speed;
    }

    private Star[] stars;

    private void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) return;

        halfHeight = mainCam.orthographicSize + 1f;
        halfWidth = halfHeight * mainCam.aspect + 1f;

        int totalStars = farStarCount + midStarCount + nearStarCount;
        stars = new Star[totalStars];
        int idx = 0;

        idx = CreateStarLayer(idx, farStarCount, farStarSpeed, farStarColor, 0.02f, 0.04f);
        idx = CreateStarLayer(idx, midStarCount, midStarSpeed, midStarColor, 0.04f, 0.07f);
        idx = CreateStarLayer(idx, nearStarCount, nearStarSpeed, nearStarColor, 0.06f, 0.1f);
    }

    private int CreateStarLayer(int startIdx, int count, float speed, Color color, float minSize, float maxSize)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject star = new GameObject("Star");
            star.transform.parent = transform;

            float x = Random.Range(-halfWidth, halfWidth);
            float y = Random.Range(-halfHeight, halfHeight);
            star.transform.position = new Vector3(x, y, 5f); // behind everything

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarSprite();
            sr.color = color;
            sr.sortingOrder = -10;

            float size = Random.Range(minSize, maxSize);
            star.transform.localScale = new Vector3(size, size, 1f);

            stars[startIdx + i] = new Star { obj = star, speed = speed };
        }
        return startIdx + count;
    }

    private Sprite CreateStarSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    private void Update()
    {
        if (stars == null) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i].obj == null) continue;

            stars[i].obj.transform.Translate(Vector3.down * stars[i].speed * Time.deltaTime, Space.World);

            // Wrap around
            if (stars[i].obj.transform.position.y < -halfHeight)
            {
                Vector3 pos = stars[i].obj.transform.position;
                pos.y = halfHeight;
                pos.x = Random.Range(-halfWidth, halfWidth);
                stars[i].obj.transform.position = pos;
            }
        }
    }
}
