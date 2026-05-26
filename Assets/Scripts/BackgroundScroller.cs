using UnityEngine;

/// <summary>
/// Scrolls a tiling background texture to create the illusion of
/// flying through space. Attach to a quad with a tiling material.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private Vector2 scrollDirection = Vector2.down;

    private Renderer _renderer;
    private Vector2 _offset;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        _offset += scrollDirection.normalized * (scrollSpeed * Time.deltaTime);
        if (_renderer != null && _renderer.material != null)
            _renderer.material.mainTextureOffset = _offset;
    }
}
