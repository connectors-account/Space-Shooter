using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton audio manager for playing sound effects.
/// Generates simple procedural audio clips at runtime so no external audio files are needed.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private int sfxSourceCount = 8;
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    private AudioSource[] sfxSources;
    private int currentSourceIndex = 0;
    private Dictionary<string, AudioClip> sfxClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        GenerateSoundEffects();
    }

    private void InitializeAudioSources()
    {
        sfxSources = new AudioSource[sfxSourceCount];
        for (int i = 0; i < sfxSourceCount; i++)
        {
            GameObject sourceObj = new GameObject("SFXSource_" + i);
            sourceObj.transform.SetParent(transform);
            sfxSources[i] = sourceObj.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
        }
    }

    /// <summary>
    /// Play a named sound effect.
    /// </summary>
    public void PlaySFX(string clipName)
    {
        if (sfxClips == null || !sfxClips.ContainsKey(clipName)) return;

        AudioSource source = sfxSources[currentSourceIndex];
        source.clip = sfxClips[clipName];
        source.volume = masterVolume * sfxVolume;
        source.pitch = Random.Range(0.95f, 1.05f); // Slight pitch variation
        source.Play();

        currentSourceIndex = (currentSourceIndex + 1) % sfxSources.Length;
    }

    /// <summary>
    /// Set master volume (0-1).
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Set SFX volume (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // ====== Procedural Sound Generation ======
    // All sounds are synthesized at runtime - no external audio files required.

    private void GenerateSoundEffects()
    {
        sfxClips = new Dictionary<string, AudioClip>();

        sfxClips["PlayerShoot"] = GenerateShootSound(0.1f, 800f, 400f);
        sfxClips["EnemyShoot"] = GenerateShootSound(0.12f, 300f, 150f);
        sfxClips["PlayerHit"] = GenerateHitSound(0.2f, 200f);
        sfxClips["PlayerDeath"] = GenerateExplosionSound(0.6f, 100f);
        sfxClips["EnemyDeath"] = GenerateExplosionSound(0.3f, 250f);
        sfxClips["ShieldBreak"] = GenerateShieldSound(0.25f);
        sfxClips["PowerUpHealth"] = GeneratePowerUpSound(0.3f, 600f);
        sfxClips["PowerUpShield"] = GeneratePowerUpSound(0.3f, 800f);
        sfxClips["PowerUpRapidFire"] = GeneratePowerUpSound(0.3f, 1000f);
        sfxClips["UIClick"] = GenerateClickSound(0.05f);
    }

    private AudioClip GenerateShootSound(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t; // Linear decay
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.4f;
        }

        return CreateClip("Shoot", samples, sampleRate);
    }

    private AudioClip GenerateHitSound(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = (1f - t) * (1f - t);
            float noise = Random.Range(-0.3f, 0.3f);
            samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t * duration) + noise) * envelope * 0.3f;
        }

        return CreateClip("Hit", samples, sampleRate);
    }

    private AudioClip GenerateExplosionSound(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float currentFreq = freq * (1f - t * 0.8f);
            float envelope = Mathf.Pow(1f - t, 2f);
            float noise = Random.Range(-1f, 1f) * 0.5f;
            float tone = Mathf.Sin(2f * Mathf.PI * currentFreq * t * duration);
            samples[i] = (tone * 0.4f + noise * 0.6f) * envelope * 0.35f;
        }

        return CreateClip("Explosion", samples, sampleRate);
    }

    private AudioClip GenerateShieldSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = 500f + Mathf.Sin(t * 20f) * 200f;
            float envelope = Mathf.Pow(1f - t, 1.5f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        return CreateClip("Shield", samples, sampleRate);
    }

    private AudioClip GeneratePowerUpSound(float duration, float baseFreq)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Rising arpeggio effect
            float freq = baseFreq * (1f + t * 0.5f);
            float envelope = Mathf.Sin(t * Mathf.PI); // Bell curve
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.25f;
        }

        return CreateClip("PowerUp", samples, sampleRate);
    }

    private AudioClip GenerateClickSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Pow(1f - t, 4f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * 1200f * t * duration) * envelope * 0.3f;
        }

        return CreateClip("Click", samples, sampleRate);
    }

    private AudioClip CreateClip(string name, float[] samples, int sampleRate)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
