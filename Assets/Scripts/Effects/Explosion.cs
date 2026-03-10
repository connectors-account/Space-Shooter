using UnityEngine;
using System;

namespace SpaceShooter.Effects
{
    public class Explosion : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private AnimationCurve alphaCurve;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem particles;

        private Action onComplete;
        private float timer;
        private bool isPlaying;
        private Vector3 initialScale;
        private Color initialColor;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (particles == null)
                particles = GetComponent<ParticleSystem>();

            if (spriteRenderer != null)
            {
                initialScale = transform.localScale;
                initialColor = spriteRenderer.color;
            }

            if (scaleCurve == null || scaleCurve.keys.Length == 0)
            {
                scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            if (alphaCurve == null || alphaCurve.keys.Length == 0)
            {
                alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
            }
        }

        public void Play(Action onComplete = null)
        {
            this.onComplete = onComplete;
            timer = 0f;
            isPlaying = true;

            if (particles != null)
            {
                particles.Play();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        private void Update()
        {
            if (!isPlaying) return;

            timer += Time.deltaTime;
            float progress = timer / duration;

            if (spriteRenderer != null)
            {
                float scale = scaleCurve.Evaluate(progress);
                transform.localScale = initialScale * scale;

                float alpha = alphaCurve.Evaluate(progress);
                Color color = initialColor;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            if (timer >= duration)
            {
                isPlaying = false;
                onComplete?.Invoke();
            }
        }

        private void OnDisable()
        {
            timer = 0f;
            isPlaying = false;
            transform.localScale = initialScale;
            
            if (spriteRenderer != null)
                spriteRenderer.color = initialColor;
        }
    }
}
