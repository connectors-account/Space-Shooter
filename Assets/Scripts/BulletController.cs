using UnityEngine;

/// <summary>
/// Controls a single bullet (player or enemy). Moves in a fixed direction,
/// self-destructs when off-screen, and applies damage on impact.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    private Vector2 direction = Vector2.up;
    private float speed = 12f;
    private bool isPlayerBullet = true;
    private int damage = 20;

    [Tooltip("Distance beyond which the bullet destroys itself.")]
    public float despawnDistance = 12f;

    /// <summary>Configure the bullet after instantiation.</summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        damage = dmg;

        // Tag drives collision handling on the receiving objects.
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Clean up bullets that have left the play area.
        if (Mathf.Abs(transform.position.y) > despawnDistance ||
            Mathf.Abs(transform.position.x) > despawnDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullets only hurt enemies.
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullets only hurt the player. The player script also
            // handles this collision, so we just remove the bullet here.
            if (other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}

/// <summary>
/// Utility to build a simple square bullet at runtime when no prefab exists.
/// Keeps the project runnable even before art/prefabs are wired up in the editor.
/// </summary>
public static class BulletFactory
{
    public static GameObject CreateBullet(Vector3 position, Color color)
    {
        GameObject go = new GameObject("Bullet");
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.12f, 0.4f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimitiveSprite.Square();
        sr.color = color;
        sr.sortingOrder = 5;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        // Required so kinematic bodies report trigger events against other
        // kinematic bodies (player/enemy ships are also kinematic).
        rb.useFullKinematicContacts = true;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        return go;
    }
}

/// <summary>
/// Generates reusable 1x1 primitive sprites (square, triangle, circle) in code
/// so the game works without any imported textures.
/// </summary>
public static class PrimitiveSprite
{
    private static Sprite squareSprite;
    private static Sprite triangleSprite;
    private static Sprite circleSprite;

    public static Sprite Square()
    {
        if (squareSprite != null) return squareSprite;

        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        return squareSprite;
    }

    public static Sprite Triangle()
    {
        if (triangleSprite != null) return triangleSprite;

        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Fill an upward-pointing triangle.
                float halfWidth = (y / (float)size) * (size / 2f);
                bool inside = x >= (size / 2f - halfWidth) && x <= (size / 2f + halfWidth);
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        triangleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return triangleSprite;
    }

    public static Sprite Circle()
    {
        if (circleSprite != null) return circleSprite;

        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
