using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Code-configured particle burst explosion. No external assets required.
    /// Scales to explosion size and auto-returns to the pool when finished.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color enemyColorStart = new Color(1f, 0.7f, 0.1f);
        [SerializeField] private Color enemyColorEnd = new Color(1f, 0.2f, 0f);
        [SerializeField] private Color playerColorStart = new Color(0.6f, 0.8f, 1f);
        [SerializeField] private Color playerColorEnd = new Color(1f, 1f, 1f);

        private ParticleSystem ps;
        private string poolTag = "Explosion";
        private float lifetime;
        private float timer;
        private bool active;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            ConfigureBase();
        }

        private void ConfigureBase()
        {
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        /// <summary>Play the explosion. sizeScale scales the burst; isPlayer switches colours.</summary>
        public void Play(float sizeScale, bool isPlayer, string poolTag)
        {
            this.poolTag = poolTag;
            transform.localScale = Vector3.one * sizeScale;

            var main = ps.main;
            main.startSpeed = 5f * sizeScale;
            main.startSize = 0.3f * sizeScale;

            Color start = isPlayer ? playerColorStart : enemyColorStart;
            Color end = isPlayer ? playerColorEnd : enemyColorEnd;

            var col = ps.colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            int burstCount = Mathf.RoundToInt(30 * sizeScale);
            ps.Emit(burstCount);
            ps.Play();

            lifetime = main.startLifetime.constant + main.duration;
            timer = lifetime;
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                active = false;
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (ObjectPool.Instance != null && ObjectPool.Instance.HasPool(poolTag))
            {
                ObjectPool.Instance.ReturnObject(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
