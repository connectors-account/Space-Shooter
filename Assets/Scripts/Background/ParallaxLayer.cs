// ============================================================
//  ParallaxLayer.cs  –  Infinite-scroll background layer
//
//  Usage:
//   1. Create a GameObject with a SpriteRenderer (e.g. a solid dark colour).
//   2. Attach this script.
//   3. Duplicate it 2–3 times with decreasing speed for a parallax effect.
//
//  The script tiles the sprite vertically so it appears infinite.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeed = 1.5f;

    [Header("Tiling")]
    public bool autoTileHeight = true;

    SpriteRenderer _sr;
    float          _spriteHeight;
    float          _startY;
    Transform      _cam;

    void Start()
    {
        _sr           = GetComponent<SpriteRenderer>();
        _spriteHeight = _sr.bounds.size.y;
        _startY       = transform.position.y;

        _cam = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

        // Re-loop: when the sprite centre goes below the camera bottom,
        // jump it back above.
        if (_cam == null) return;

        float camBottom = _cam.position.y - Camera.main.orthographicSize;
        if (transform.position.y + _spriteHeight * 0.5f < camBottom)
        {
            float offset = _spriteHeight;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + offset * 2f,
                transform.position.z);
        }
    }
}
