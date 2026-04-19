using UnityEngine;

namespace SpaceShooter.Audio
{
    /// <summary>
    /// Centralized one-shot SFX manager.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip powerUpClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip gameOverClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void PlayShoot() => Play(shootClip);
        public void PlayExplosion() => Play(explosionClip);
        public void PlayPlayerHit() => Play(playerHitClip);
        public void PlayPowerUp() => Play(powerUpClip);
        public void PlayWaveStart() => Play(waveStartClip);
        public void PlayGameOver() => Play(gameOverClip);

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
