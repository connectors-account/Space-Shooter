using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple audio manager singleton. Plays SFX by name.
/// Place AudioClips as children or assign in inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioClip playerShootClip;
    public AudioClip playerHitClip;
    public AudioClip playerDeathClip;
    public AudioClip enemyExplosionClip;
    public AudioClip powerUpClip;

    [Header("Music")]
    public AudioClip backgroundMusic;

    private Dictionary<string, AudioClip> sfxMap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build SFX lookup
        sfxMap = new Dictionary<string, AudioClip>()
        {
            { "PlayerShoot",    playerShootClip },
            { "PlayerHit",      playerHitClip },
            { "PlayerDeath",    playerDeathClip },
            { "EnemyExplosion", enemyExplosionClip },
            { "PowerUp",        powerUpClip }
        };

        // Auto-create audio sources if not assigned
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
            musicSource.volume = 0.4f;
        }
    }

    void Start()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    /// <summary>Static convenience method to play a named SFX.</summary>
    public static void PlaySfx(string name)
    {
        if (Instance == null) return;
        if (Instance.sfxMap.TryGetValue(name, out AudioClip clip) && clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip);
        }
        // Silently ignore if clip is null (audio not yet assigned)
    }

    /// <summary>Play a specific clip directly.</summary>
    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (Instance == null || clip == null) return;
        Instance.sfxSource.PlayOneShot(clip, volume);
    }
}
