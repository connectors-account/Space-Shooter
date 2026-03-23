using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Centralized audio manager using singleton pattern.
    /// Manages all game sound effects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;        // For one-shot sound effects
        [SerializeField] private AudioSource musicSource;      // For background music

        [Header("Sound Effects")]
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip powerUpClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip waveStartClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip buttonClickClip;

        [Header("Background Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources if not assigned
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

            sfxSource.volume = sfxVolume;
            musicSource.volume = musicVolume;
        }

        // ========== SOUND EFFECT METHODS ==========

        /// <summary>Play the shooting sound effect.</summary>
        public void PlayShootSound()
        {
            PlaySFX(shootClip, 0.5f);
        }

        /// <summary>Play the explosion sound effect.</summary>
        public void PlayExplosionSound()
        {
            PlaySFX(explosionClip);
        }

        /// <summary>Play the power-up pickup sound.</summary>
        public void PlayPowerUpSound()
        {
            PlaySFX(powerUpClip);
        }

        /// <summary>Play the player hit sound.</summary>
        public void PlayPlayerHitSound()
        {
            PlaySFX(playerHitClip);
        }

        /// <summary>Play the wave start sound.</summary>
        public void PlayWaveStartSound()
        {
            PlaySFX(waveStartClip);
        }

        /// <summary>Play the game over sound.</summary>
        public void PlayGameOverSound()
        {
            PlaySFX(gameOverClip);
        }

        /// <summary>Play button click sound.</summary>
        public void PlayButtonClickSound()
        {
            PlaySFX(buttonClickClip, 0.6f);
        }

        // ========== MUSIC METHODS ==========

        /// <summary>Play menu background music.</summary>
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        /// <summary>Play gameplay background music.</summary>
        public void PlayGameMusic()
        {
            PlayMusic(gameMusic);
        }

        /// <summary>Stop background music.</summary>
        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        // ========== VOLUME CONTROL ==========

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        // ========== INTERNAL HELPERS ==========

        private void PlaySFX(AudioClip clip, float volumeScale = -1f)
        {
            if (clip == null || sfxSource == null) return;

            float vol = volumeScale < 0 ? sfxVolume : volumeScale;
            sfxSource.PlayOneShot(clip, vol);
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume;

            if (clip != null)
                musicSource.Play();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
