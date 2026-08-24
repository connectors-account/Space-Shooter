using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Handles all audio playback: a looping music source and a one-shot SFX source.
    /// Volumes are persisted to PlayerPrefs.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("SFX Clips")]
        public AudioClip shootSFX;
        public AudioClip explosionSFX;
        public AudioClip powerUpSFX;
        public AudioClip playerHitSFX;
        public AudioClip bossAlertSFX;

        [Header("Music Clips")]
        public AudioClip menuMusicClip;
        public AudioClip gameMusicClip;
        public AudioClip gameOverMusicClip;

        [Header("Default Volumes")]
        [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float defaultSFXVolume = 0.8f;

        private float _musicVolume;
        private float _sfxVolume;

        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;

        protected override void OnAwakeInitialize()
        {
            // Create audio sources automatically if not assigned in the inspector.
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            _musicVolume = PlayerPrefs.GetFloat(Constants.PrefKeys.MusicVolume, defaultMusicVolume);
            _sfxVolume = PlayerPrefs.GetFloat(Constants.PrefKeys.SFXVolume, defaultSFXVolume);

            musicSource.volume = _musicVolume;
            sfxSource.volume = _sfxVolume;
        }

        /// <summary>
        /// Plays a one-shot sound effect at the current SFX volume.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>
        /// Plays a sound effect with a per-call volume scale.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(_sfxVolume * volumeScale));
        }

        /// <summary>
        /// Starts looping the given music clip. Ignores if the same clip is already playing.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null)
            {
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.volume = _musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = _musicVolume;
            }

            PlayerPrefs.SetFloat(Constants.PrefKeys.MusicVolume, _musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = _sfxVolume;
            }

            PlayerPrefs.SetFloat(Constants.PrefKeys.SFXVolume, _sfxVolume);
            PlayerPrefs.Save();
        }
    }
}
