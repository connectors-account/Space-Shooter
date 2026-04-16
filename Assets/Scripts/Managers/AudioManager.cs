using UnityEngine;

namespace SpaceShooter.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfx;

        private AudioClip _shoot;
        private AudioClip _explosion;
        private AudioClip _powerUp;
        private AudioClip _hit;
        private AudioClip _ui;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.volume = 0.55f;

            _shoot = GenerateTone(900f, 0.05f);
            _explosion = GenerateNoise(0.16f);
            _powerUp = GenerateTone(520f, 0.18f);
            _hit = GenerateTone(180f, 0.1f);
            _ui = GenerateTone(700f, 0.06f);
        }

        public void PlayShoot() => _sfx.PlayOneShot(_shoot, 0.45f);
        public void PlayExplosion() => _sfx.PlayOneShot(_explosion, 0.5f);
        public void PlayPowerUp() => _sfx.PlayOneShot(_powerUp, 0.5f);
        public void PlayHit() => _sfx.PlayOneShot(_hit, 0.5f);
        public void PlayUi() => _sfx.PlayOneShot(_ui, 0.45f);

        private static AudioClip GenerateTone(float frequency, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.3f * envelope;
            }

            var clip = AudioClip.Create($"tone_{frequency}_{duration}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateNoise(float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var envelope = 1f - (i / (float)sampleCount);
                samples[i] = Random.Range(-0.8f, 0.8f) * envelope * 0.25f;
            }

            var clip = AudioClip.Create($"noise_{duration}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
