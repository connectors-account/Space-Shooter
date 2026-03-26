// ============================================================================
// SoundManager.cs — Manages all game audio (singleton)
// Plays SFX via pool, manages background music with crossfade
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitchMin = 0.95f;
    [Range(0.5f, 1.5f)] public float pitchMax = 1.05f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Library")]
    [SerializeField] private SoundClip[] soundEffects;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip gameOverJingle;
    [SerializeField] private AudioClip victoryJingle;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 16;

    // Internal
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource[] sfxPool;
    private int sfxPoolIndex;
    private Dictionary<string, SoundClip> soundDict;
    private bool usingSourceA = true;

    // =========================================================================
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
        BuildSoundDictionary();
        LoadVolumePrefs();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    // =========================================================================
    // Init
    // =========================================================================
    private void InitAudioSources()
    {
        // Music sources
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;

        // SFX pool
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
        }
    }

    private void BuildSoundDictionary()
    {
        soundDict = new Dictionary<string, SoundClip>();
        if (soundEffects == null) return;

        foreach (var sc in soundEffects)
        {
            if (!string.IsNullOrEmpty(sc.name) && sc.clip != null)
            {
                soundDict[sc.name.ToLower()] = sc;
            }
        }
    }

    // =========================================================================
    // SFX Playback
    // =========================================================================
    public void PlaySFX(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;

        string key = soundName.ToLower();
        if (!soundDict.ContainsKey(key))
        {
            Debug.LogWarning($"[SoundManager] Sound '{soundName}' not found.");
            return;
        }

        SoundClip sc = soundDict[key];
        PlayClip(sc.clip, sc.volume * sfxVolume * masterVolume,
                 Random.Range(sc.pitchMin, sc.pitchMax));
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        PlayClip(clip, volumeScale * sfxVolume * masterVolume, Random.Range(0.95f, 1.05f));
    }

    private void PlayClip(AudioClip clip, float vol, float pitch)
    {
        AudioSource source = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;

        source.clip = clip;
        source.volume = vol;
        source.pitch = pitch;
        source.Play();
    }

    // =========================================================================
    // Music
    // =========================================================================
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        AudioSource active = usingSourceA ? musicSourceA : musicSourceB;
        active.clip = clip;
        active.volume = musicVolume * masterVolume;
        active.loop = loop;
        active.Play();
    }

    public void StopMusic()
    {
        musicSourceA.Stop();
        musicSourceB.Stop();
    }

    // =========================================================================
    // State-Driven Music
    // =========================================================================
    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                PlayMusic(menuMusic);
                break;
            case GameState.Playing:
                PlayMusic(gameMusic);
                break;
            case GameState.GameOver:
                StopMusic();
                if (gameOverJingle != null)
                    PlaySFX(gameOverJingle, 0.8f);
                break;
            case GameState.Victory:
                StopMusic();
                if (victoryJingle != null)
                    PlaySFX(victoryJingle, 0.8f);
                break;
        }
    }

    // =========================================================================
    // Volume Controls
    // =========================================================================
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        UpdateMusicVolume();
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        UpdateMusicVolume();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    private void UpdateMusicVolume()
    {
        float vol = musicVolume * masterVolume;
        musicSourceA.volume = vol;
        musicSourceB.volume = vol;
    }

    private void LoadVolumePrefs()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    }

    public float MasterVolume => masterVolume;
    public float SFXVolume => sfxVolume;
    public float MusicVolume => musicVolume;
}
