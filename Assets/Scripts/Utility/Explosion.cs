// ============================================================
//  Explosion.cs  –  Scale-up + fade-out visual effect
//  Call Explosion.Spawn(position, large) from anywhere.
// ============================================================
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float duration   = 0.35f;
    public float maxScale   = 2.0f;
    bool _large;

    SpriteRenderer _sr;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void Init(bool large)
    {
        _large = large;
        maxScale = large ? 4.5f : 2.0f;
        transform.localScale = Vector3.zero;
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        float elapsed = 0f;
        Color startCol = _large ? Color.yellow : new Color(1f, 0.6f, 0.1f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float s = Mathf.Lerp(0f, maxScale, Mathf.Sqrt(t));
            transform.localScale = Vector3.one * s;

            if (_sr)
                _sr.color = new Color(startCol.r, startCol.g, startCol.b, 1f - t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    // ── Static factory ───────────────────────────────────────

    public static void Spawn(Vector3 position, bool large = false)
    {
        var go = new GameObject("Explosion");
        go.transform.position = position;

        var sr  = go.AddComponent<SpriteRenderer>();
        sr.sprite       = SpriteFactory.CreateSolidRect(16, 16, Color.white);
        sr.sortingOrder = 10;

        var ex = go.AddComponent<Explosion>();
        ex.Init(large);

        if (large)
            CameraShake.Instance?.Shake(0.3f, 0.35f);
    }
}
