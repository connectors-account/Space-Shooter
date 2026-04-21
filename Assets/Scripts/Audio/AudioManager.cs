using UnityEngine;

namespace SpaceShooter.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX Clips")]
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip powerUpClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip uiClickClip;

        [Header("Sources")]
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void PlayShoot() => PlayClip(shootClip);
        public void PlayExplosion() => PlayClip(explosionClip);
        public void PlayPlayerHit() => PlayClip(playerHitClip);
        public void PlayPowerUp() => PlayClip(powerUpClip);
        public void PlayGameOver() => PlayClip(gameOverClip);
        public void PlayWaveStart() => PlayClip(waveStartClip);
        public void PlayUIClick() => PlayClip(uiClickClip);

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }
    }
}
