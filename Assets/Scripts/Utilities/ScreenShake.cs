using UnityEngine;
using System.Collections;

/// <summary>
/// ScreenShake adds camera shake effects for impacts.
/// Attach to the main camera.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [Header("Shake Settings")]
    [Tooltip("Default shake duration")]
    public float defaultDuration = 0.1f;
    
    [Tooltip("Default shake magnitude")]
    public float defaultMagnitude = 0.1f;

    private Vector3 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger screen shake with default values
    /// </summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    /// <summary>
    /// Trigger screen shake with custom values
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }
}
