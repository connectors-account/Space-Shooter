// ============================================================
//  CameraShake.cs  –  Screen-shake via coroutine
//  Attach to the Main Camera in the Game scene.
// ============================================================
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Vector3 _originPos;
    bool    _shaking;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _originPos = transform.localPosition;
    }

    /// <summary>
    /// Shake the camera.
    /// <param name="magnitude">Max displacement in world units.</param>
    /// <param name="duration">How long the shake lasts.</param>
    /// </summary>
    public void Shake(float magnitude = 0.15f, float duration = 0.2f)
    {
        if (!_shaking)
            StartCoroutine(DoShake(magnitude, duration));
    }

    IEnumerator DoShake(float magnitude, float duration)
    {
        _shaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t   = elapsed / duration;
            float mag = Mathf.Lerp(magnitude, 0f, t);   // decay
            transform.localPosition = _originPos + (Vector3)Random.insideUnitCircle * mag;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originPos;
        _shaking = false;
    }
}
