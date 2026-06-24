using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Centralised sound effect and music manager (singleton).
    /// All gameplay scripts call into this so audio is decoupled from logic.
    /// Assign clips in the Inspector; any unassigned clip is simply skipped,
    /// so the game runs fine even before audio assets are added.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip playerShootClip;
        [SerializeField] private AudioClip enemyShootClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip powerUpClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip buttonClickClip;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-create audio sources if not assigned in the Inspector.
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // ---------- Public SFX API ----------
        public void PlayPlayerShoot() => PlaySfx(playerShootClip);
        public void PlayEnemyShoot() => PlaySfx(enemyShootClip);
        public void PlayExplosion() => PlaySfx(explosionClip);
        public void PlayPowerUp() => PlaySfx(powerUpClip);
        public void PlayWaveStart() => PlaySfx(waveStartClip);
        public void PlayGameOver() => PlaySfx(gameOverClip);
        public void PlayButtonClick() => PlaySfx(buttonClickClip);

        // ---------- Music ----------
        public void PlayMusic()
        {
            if (backgroundMusic == null || musicSource == null) return;
            if (musicSource.isPlaying) return;
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying) musicSource.Stop();
        }

        public void SetSfxVolume(float v) => sfxVolume = Mathf.Clamp01(v);
        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (musicSource != null) musicSource.volume = musicVolume;
        }
    }
}
