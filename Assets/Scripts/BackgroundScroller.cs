using UnityEngine;

/// <summary>
/// Parallax scrolling background. Attach to each background layer sprite.
/// Uses material offset for seamless tiling — requires Wrap Mode = Repeat on the texture.
/// Alternatively, duplicates the sprite for seamless scrolling.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Speed (higher = closer layer)")]
    public float scrollSpeed = 0.5f;

    [Header("Method")]
    public bool useMaterialOffset = false; // true if texture has Repeat wrap

    SpriteRenderer sr;
    float spriteHeight;
    Vector3 startPos;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteHeight = sr.bounds.size.y;
        }
        startPos = transform.position;

        if (!useMaterialOffset && sr != null)
        {
            // Create a duplicate above for seamless loop
            GameObject dup = new GameObject(gameObject.name + "_dup");
            dup.transform.SetParent(transform.parent);
            var dupSR = dup.AddComponent<SpriteRenderer>();
            dupSR.sprite = sr.sprite;
            dupSR.color = sr.color;
            dupSR.sortingLayerName = sr.sortingLayerName;
            dupSR.sortingOrder = sr.sortingOrder;
            dup.transform.position = transform.position + Vector3.up * spriteHeight;
            dup.transform.localScale = transform.localScale;
        }
    }

    void Update()
    {
        if (useMaterialOffset)
        {
            // Offset the material
            float offset = Time.time * scrollSpeed;
            sr.material.mainTextureOffset = new Vector2(0, offset);
        }
        else
        {
            // Move the transform, reset when out of view
            transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);
            if (transform.position.y <= startPos.y - spriteHeight)
            {
                transform.position = startPos;
            }
        }
    }
}
