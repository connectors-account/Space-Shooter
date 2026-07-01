using UnityEngine;

/// <summary>
/// Creates and scrolls a parallax starfield background. Two layers of stars
/// move at different speeds to give a sense of depth. The starfield is
/// generated procedurally so no textures are required.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Layer Speeds (units/sec)")]
    public float farLayerSpeed = 0.6f;
    public float nearLayerSpeed = 1.6f;

    [Header("Star Counts")]
    public int farStarCount = 60;
    public int nearStarCount = 30;

    [Header("Bounds (world units)")]
    public float topY = 6f;
    public float bottomY = -6f;
    public float halfWidth = 9f;

    private Transform[] farStars;
    private Transform[] nearStars;

    private void Start()
    {
        farStars = CreateLayer(farStarCount, 0.05f, new Color(1f, 1f, 1f, 0.5f), "FarStar", -20);
        nearStars = CreateLayer(nearStarCount, 0.1f, Color.white, "NearStar", -19);
    }

    private void Update()
    {
        ScrollLayer(farStars, farLayerSpeed);
        ScrollLayer(nearStars, nearLayerSpeed);
    }

    private Transform[] CreateLayer(int count, float size, Color color, string namePrefix, int sortingOrder)
    {
        Transform[] stars = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            GameObject star = new GameObject(namePrefix + i);
            star.transform.SetParent(transform, false);
            star.transform.position = RandomStarPosition(spawnAnywhere: true);
            star.transform.localScale = new Vector3(size, size, 1f);

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = PrimitiveSprite.Circle();
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            stars[i] = star.transform;
        }
        return stars;
    }

    private void ScrollLayer(Transform[] stars, float speed)
    {
        if (stars == null) return;

        foreach (Transform star in stars)
        {
            if (star == null) continue;
            Vector3 pos = star.position;
            pos.y -= speed * Time.deltaTime;

            // Recycle stars that scroll past the bottom back to the top.
            if (pos.y < bottomY)
            {
                pos = RandomStarPosition(spawnAnywhere: false);
            }
            star.position = pos;
        }
    }

    private Vector3 RandomStarPosition(bool spawnAnywhere)
    {
        float x = Random.Range(-halfWidth, halfWidth);
        float y = spawnAnywhere ? Random.Range(bottomY, topY) : topY;
        return new Vector3(x, y, 10f);
    }
}
