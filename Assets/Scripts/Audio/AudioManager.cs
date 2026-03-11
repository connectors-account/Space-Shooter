using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;
    public bool randomizePitch = false;
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip bossMusic;
    public AudioClip victoryMusic;
    public AudioClip gameOverMusic;

    [Header("Sound Effects")]
    public List<SoundEffect> soundEffects;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    public float fadeDuration = 1f;

    private Dictionary<string, SoundEffect> sfxDictionary;
    private bool isFading = false;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.parent = transform;
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.parent = transform;
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // Build SFX dictionary
        sfxDictionary = new Dictionary<string, SoundEffect>();
        if (soundEffects != null)
        {
            foreach (var sfx in soundEffects)
            {
                if (!string.IsNullOrEmpty(sfx.name) && !sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary[sfx.name] = sfx;
                }
            }
        }

        // Load saved volume settings
        LoadVolumeSettings();

        // Subscribe to game state changes
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void Start()
    {
        // Play appropriate music based on current state
        if (GameManager.Instance != null)
        {
            OnGameStateChanged(GameManager.Instance.CurrentState);
        }
        else
        {
            PlayMusic(menuMusic);
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.MainMenu:
                PlayMusic(menuMusic);
                break;
            case GameManager.GameState.Playing:
                PlayMusic(gameMusic);
                break;
            case GameManager.GameState.GameOver:
                PlayMusic(gameOverMusic);
                break;
            case GameManager.GameState.Victory:
                PlayMusic(victoryMusic);
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (isFading)
        {
            StopAllCoroutines();
        }

        StartCoroutine(FadeToNewMusic(clip));
    }

    private System.Collections.IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        isFading = true;

        // Fade out current music
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (fadeDuration / 2f));
                yield return null;
            }
        }

        // Switch to new clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music
        float targetVolume = musicVolume * masterVolume;
        float elapsedIn = 0f;

        while (elapsedIn < fadeDuration / 2f)
        {
            elapsedIn += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedIn / (fadeDuration / 2f));
            yield return null;
        }

        musicSource.volume = targetVolume;
        isFading = false;
    }

    public void PlaySFX(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName) || sfxSource == null)
            return;

        if (sfxDictionary.TryGetValue(sfxName, out SoundEffect sfx))
        {
            PlaySFX(sfx);
        }
    }

    public void PlaySFX(SoundEffect sfx)
    {
        if (sfx == null || sfx.clip == null || sfxSource == null)
            return;

        float pitch = sfx.pitch;
        if (sfx.randomizePitch)
        {
            pitch += UnityEngine.Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
        }

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfx.clip, sfx.volume * sfxVolume * masterVolume);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
    }

    public void PlayBossMusic()
    {
        if (bossMusic != null)
        {
            PlayMusic(bossMusic);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null && !isFading)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
