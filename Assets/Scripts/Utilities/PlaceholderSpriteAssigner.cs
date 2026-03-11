using UnityEngine;

/// <summary>
/// Attach to objects to automatically assign placeholder sprites if none exists.
/// Remove this script once you have proper art assets.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlaceholderSpriteAssigner : MonoBehaviour
{
    public enum SpriteShape { Square, Circle, Triangle, Diamond, Bullet }

    [Header("Placeholder Settings")]
    public SpriteShape shape = SpriteShape.Square;
    public int size = 32;
    public int bulletHeight = 16;
    public Color color = Color.white;
    public bool assignOnAwake = true;

    private void Awake()
    {
        if (assignOnAwake)
        {
            AssignPlaceholder();
        }
    }

    [ContextMenu("Assign Placeholder Sprite")]
    public void AssignPlaceholder()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Only assign if no sprite exists
        if (sr.sprite != null && !assignOnAwake) return;

        sr.sprite = shape switch
        {
            SpriteShape.Square => SpriteGenerator.CreateSquare(size, color),
            SpriteShape.Circle => SpriteGenerator.CreateCircle(size, color),
            SpriteShape.Triangle => SpriteGenerator.CreateTriangle(size, color),
            SpriteShape.Diamond => SpriteGenerator.CreateDiamond(size, color),
            SpriteShape.Bullet => SpriteGenerator.CreateBullet(size / 2, bulletHeight, color),
            _ => SpriteGenerator.CreateSquare(size, color)
        };
    }
}
