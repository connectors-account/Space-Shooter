using UnityEngine;

public class StarfieldGenerator : MonoBehaviour
{
    [Header("Star Settings")]
    public int starCount = 100;
    public float fieldWidth = 20f;
    public float fieldHeight = 15f;
    public float minStarSize = 0.02f;
    public float maxStarSize = 0.1f;
    public float minScrollSpeed = 0.5f;
    public float maxScrollSpeed = 2f;

    [Header("Visual")]
    public Color starColor = Color.white;
    public Sprite starSprite;

    private Star[] stars;

    private class Star
    {
        public Transform transform;
        public float speed;
        public float size;
    }

    private void Start()
    {
        GenerateStarfield();
    }

    private void GenerateStarfield()
    {
        stars = new Star[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject starObj = new GameObject($"Star_{i}");
            starObj.transform.parent = transform;

            SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
            sr.sprite = starSprite;
            sr.color = starColor;
            sr.sortingOrder = -10;

            Star star = new Star();
            star.transform = starObj.transform;
            star.speed = Random.Range(minScrollSpeed, maxScrollSpeed);
            star.size = Random.Range(minStarSize, maxStarSize);

            // Random position
            float x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
            float y = Random.Range(-fieldHeight / 2f, fieldHeight / 2f);
            starObj.transform.position = new Vector3(x, y, 1f);
            starObj.transform.localScale = Vector3.one * star.size;

            // Vary alpha based on size (smaller = dimmer)
            float alpha = Mathf.Lerp(0.3f, 1f, (star.size - minStarSize) / (maxStarSize - minStarSize));
            sr.color = new Color(starColor.r, starColor.g, starColor.b, alpha);

            stars[i] = star;
        }
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        foreach (Star star in stars)
        {
            if (star?.transform == null) continue;

            // Move star down
            star.transform.Translate(Vector3.down * star.speed * Time.deltaTime);

            // Wrap around
            if (star.transform.position.y < -fieldHeight / 2f)
            {
                Vector3 newPos = star.transform.position;
                newPos.y = fieldHeight / 2f;
                newPos.x = Random.Range(-fieldWidth / 2f, fieldWidth / 2f);
                star.transform.position = newPos;
            }
        }
    }
}
