using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Central audio hub. Owns a looping music source and a one-shot SFX source,
    /// exposes named clips, and persists volume settings to PlayerPrefs.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        public const string MusicVolumeKey = "SpaceShooter.MusicVolume";
        public const string SFXVolumeKey = "SpaceShooter.SFXVolume";

        [Header("Music Clips")]
        public AudioClip bgMusic;
        public AudioClip bossMusic;
        public AudioClip menuMusic;

        [Header("SFX Clips")]
        public AudioClip shootSFX;
        public AudioClip explosionSFX;
        public AudioClip powerUpSFX;
        public AudioClip hitSFX;

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        public AudioSource MusicSource => musicSource;
        public AudioSource SFXSource => sfxSource;

        /// <summary>The most recent clip passed to <see cref="PlaySFX"/> (for tests/UI).</summary>
        public AudioClip LastPlayedSFX { get; private set; }

        private bool _initialized;

        protected override void Awake()
        {
            persistAcrossScenes = true;
            base.Awake();
            Initialize();
        }

        /// <summary>Creates the audio sources (if needed) and loads saved volumes.</summary>
        public void Initialize()
        {
            RegisterSingleton();
            if (_initialized) return;

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }

            musicSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.6f);
            sfxSource.volume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.8f);
            _initialized = true;
        }

        /// <summary>Plays a one-shot sound effect.</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            Initialize();
            LastPlayedSFX = clip;
            sfxSource.PlayOneShot(clip, sfxSource.volume);
        }

        /// <summary>Plays looping music, replacing whatever is currently playing.</summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            Initialize();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>Stops the current music track.</summary>
        public void StopMusic()
        {
            Initialize();
            musicSource.Stop();
        }

        /// <summary>Sets and persists the music volume (0-1).</summary>
        public void SetMusicVolume(float v)
        {
            Initialize();
            v = Mathf.Clamp01(v);
            musicSource.volume = v;
            PlayerPrefs.SetFloat(MusicVolumeKey, v);
            PlayerPrefs.Save();
        }

        /// <summary>Sets and persists the SFX volume (0-1).</summary>
        public void SetSFXVolume(float v)
        {
            Initialize();
            v = Mathf.Clamp01(v);
            sfxSource.volume = v;
            PlayerPrefs.SetFloat(SFXVolumeKey, v);
            PlayerPrefs.Save();
        }
    }
}
