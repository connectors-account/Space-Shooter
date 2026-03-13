using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio system managing background music and sound effects.
/// Singleton pattern for global access. Generates placeholder audio at runtime.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.3f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    // Generated audio clips
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

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
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        // Generate procedural sound effects
        GenerateSFX();
        GenerateMusic();
    }

    private void GenerateSFX()
    {
        sfxClips["PlayerShoot"] = GenerateShootSound(0.1f, 800f, 400f);
        sfxClips["EnemyShoot"] = GenerateShootSound(0.1f, 400f, 200f);
        sfxClips["PlayerHit"] = GenerateImpactSound(0.15f, 300f);
        sfxClips["PlayerExplosion"] = GenerateExplosionSound(0.4f);
        sfxClips["EnemyExplosion"] = GenerateExplosionSound(0.25f);
        sfxClips["BossExplosion"] = GenerateExplosionSound(0.6f);
        sfxClips["PowerUp"] = GeneratePowerUpSound(0.2f);
        sfxClips["ShieldBreak"] = GenerateShieldBreakSound(0.2f);
    }

    private void GenerateMusic()
    {
        // Generate a simple ambient loop
        int sampleRate = 44100;
        float duration = 8f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("BGMusic", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Low bass drone
            float bass = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.15f;

            // Ambient pad
            float pad = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.05f;
            pad += Mathf.Sin(2f * Mathf.PI * 165f * t) * 0.03f;
            pad += Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.02f;

            // Slow LFO modulation
            float lfo = (Mathf.Sin(2f * Mathf.PI * 0.1f * t) + 1f) * 0.5f;

            // Simple arpeggio
            float noteFreq = GetArpeggioNote(t);
            float arp = Mathf.Sin(2f * Mathf.PI * noteFreq * t) * 0.04f * lfo;

            samples[i] = (bass + pad + arp) * 0.6f;
        }

        clip.SetData(samples, 0);

        musicSource.clip = clip;
        musicSource.Play();
    }

    private float GetArpeggioNote(float time)
    {
        float[] notes = { 220f, 261.63f, 329.63f, 392f, 329.63f, 261.63f };
        float noteLength = 0.5f;
        int noteIndex = (int)(time / noteLength) % notes.Length;
        return notes[noteIndex];
    }

    private AudioClip GenerateShootSound(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Shoot", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t; // Linear decay
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateImpactSound(float duration, float freq)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Impact", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 8f);
            float noise = Random.Range(-0.3f, 0.3f);
            samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t * duration) + noise) * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateExplosionSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Explosion", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 4f);
            float noise = Random.Range(-1f, 1f);
            float lowRumble = Mathf.Sin(2f * Mathf.PI * 60f * t * duration) * 0.5f;
            samples[i] = (noise * 0.5f + lowRumble) * envelope * 0.4f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GeneratePowerUpSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("PowerUp", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(400f, 1200f, t);
            float envelope = Mathf.Sin(t * Mathf.PI);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateShieldBreakSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("ShieldBreak", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 6f);
            float glass = Mathf.Sin(2f * Mathf.PI * 2000f * t * duration) * 0.3f;
            float shatter = Random.Range(-0.5f, 0.5f) * Mathf.Exp(-t * 3f);
            samples[i] = (glass + shatter) * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    public void PlaySFX(string sfxName)
    {
        if (sfxClips.ContainsKey(sfxName) && sfxSource != null)
        {
            sfxSource.PlayOneShot(sfxClips[sfxName], sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying) musicSource.Play();
    }
}
