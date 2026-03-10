using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Managers
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float pitch = 1f;
        public bool loop = false;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private int sfxPoolSize = 10;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private AudioClip bossMusic;

        [Header("Sound Effects")]
        [SerializeField] private List<SoundEffect> soundEffects;

        [Header("Volume Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        private Dictionary<string, SoundEffect> soundDictionary;
        private List<AudioSource> sfxPool;
        private int currentSfxIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadVolumeSettings();
        }

        private void Initialize()
        {
            soundDictionary = new Dictionary<string, SoundEffect>();
            
            if (soundEffects != null)
            {
                foreach (var sound in soundEffects)
                {
                    if (!soundDictionary.ContainsKey(sound.name))
                    {
                        soundDictionary.Add(sound.name, sound);
                    }
                }
            }

            CreateSfxPool();

            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
        }

        private void CreateSfxPool()
        {
            sfxPool = new List<AudioSource>();
            
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(source);
            }
        }

        private void LoadVolumeSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            ApplyVolumeSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }

        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;
            
            if (soundDictionary != null && soundDictionary.TryGetValue(soundName, out SoundEffect sound))
            {
                if (sound.clip != null)
                {
                    AudioSource source = GetAvailableSfxSource();
                    source.clip = sound.clip;
                    source.volume = sound.volume * sfxVolume * masterVolume;
                    source.pitch = sound.pitch;
                    source.loop = sound.loop;
                    source.Play();
                }
            }
        }

        public void PlaySoundAtPosition(string soundName, Vector3 position)
        {
            if (string.IsNullOrEmpty(soundName)) return;
            
            if (soundDictionary != null && soundDictionary.TryGetValue(soundName, out SoundEffect sound))
            {
                if (sound.clip != null)
                {
                    AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
                }
            }
        }

        private AudioSource GetAvailableSfxSource()
        {
            if (sfxPool == null || sfxPool.Count == 0)
            {
                CreateSfxPool();
            }

            AudioSource source = sfxPool[currentSfxIndex];
            currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Count;
            return source;
        }

        public void PlayMenuMusic()
        {
            if (menuMusic != null)
            {
                PlayMusic(menuMusic);
            }
        }

        public void PlayGameMusic()
        {
            if (gameMusic != null)
            {
                PlayMusic(gameMusic);
            }
        }

        public void PlayBossMusic()
        {
            if (bossMusic != null)
            {
                PlayMusic(bossMusic);
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (musicSource != null)
                musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (musicSource != null)
                musicSource.UnPause();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            ApplyVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        public void FadeMusic(float targetVolume, float duration)
        {
            StartCoroutine(FadeMusicCoroutine(targetVolume, duration));
        }

        private System.Collections.IEnumerator FadeMusicCoroutine(float targetVolume, float duration)
        {
            if (musicSource == null) yield break;

            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume * masterVolume, elapsed / duration);
                yield return null;
            }

            musicSource.volume = targetVolume * masterVolume;
        }
    }
}
