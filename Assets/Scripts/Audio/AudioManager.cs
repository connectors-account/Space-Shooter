using UnityEngine;

namespace SpaceShooter.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _audioSource;
        private AudioClip _shootClip;
        private AudioClip _explosionClip;
        private AudioClip _powerUpClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0.6f;

            _shootClip = BuildToneClip(950f, 0.05f);
            _explosionClip = BuildToneClip(160f, 0.18f);
            _powerUpClip = BuildToneClip(540f, 0.13f);
        }

        public void PlayShoot()
        {
            _audioSource.PlayOneShot(_shootClip, 0.35f);
        }

        public void PlayExplosion()
        {
            _audioSource.PlayOneShot(_explosionClip, 0.45f);
        }

        public void PlayPowerUp()
        {
            _audioSource.PlayOneShot(_powerUpClip, 0.4f);
        }

        private static AudioClip BuildToneClip(float frequency, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.RoundToInt(sampleRate * duration);
            var data = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Lerp(1f, 0f, i / (float)sampleCount);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.25f;
            }

            var clip = AudioClip.Create($"Tone_{frequency}", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
