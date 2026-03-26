using UnityEngine;

/// <summary>
/// Singleton audio manager with a pool of AudioSources for concurrent SFX.
/// Assign clips in the Inspector or load them from Resources at runtime.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips – assign in Inspector")]
    public AudioClip shootClip;
    public AudioClip explosionClip;
    public AudioClip powerupClip;
    public AudioClip playerHitClip;
    public AudioClip menuClickClip;
    public AudioClip bossWarningClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    public int poolSize = 5;

    private AudioSource[] pool;
    private int poolIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build the AudioSource pool
        pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }
    }

    // ── Convenience methods ────────────────────────────────────────────
    public void PlayShoot()       => Play(shootClip);
    public void PlayExplosion()   => Play(explosionClip);
    public void PlayPowerup()     => Play(powerupClip);
    public void PlayPlayerHit()   => Play(playerHitClip);
    public void PlayMenuClick()   => Play(menuClickClip);
    public void PlayBossWarning() => Play(bossWarningClip);

    /// <summary>Play an arbitrary clip through the pool.</summary>
    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        AudioSource src = pool[poolIndex];
        src.clip   = clip;
        src.volume = sfxVolume;
        src.Play();
        poolIndex = (poolIndex + 1) % pool.Length;
    }

    /// <summary>Set master SFX volume (0-1).</summary>
    public void SetVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
    }
}
