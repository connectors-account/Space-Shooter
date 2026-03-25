// =============================================================================
// ExplosionEffect.cs — Self-destroying explosion animation
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Plays an explosion animation/particle effect and destroys itself.
    /// Can scale up and fade out for a simple visual effect.
    /// </summary>
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float maxScale = 2f;
        [SerializeField] private bool fadeOut = true;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float timer;
        private SpriteRenderer sr;
        private Vector3 initialScale;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;
            timer = 0f;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Scale up
            float scaleMultiplier = scaleCurve.Evaluate(t) * maxScale;
            transform.localScale = initialScale * scaleMultiplier;

            // Fade out
            if (fadeOut && sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }
        }
    }
}
