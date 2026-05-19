using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all audio: background music and sound effects.
/// Generates procedural sounds at runtime if no audio clips are assigned.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [SerializeField] private float musicVolume = 0.3f;
    [SerializeField] private float sfxVolume = 0.5f;

    [Header("Audio Clips (optional - procedural if empty)")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip playerShootClip;
    [SerializeField] private AudioClip enemyShootClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip waveStartClip;
    [SerializeField] private AudioClip gameOverClip;

    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
        GenerateProceduralSounds();
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }
    }

    private void GenerateProceduralSounds()
    {
        // Generate sounds procedurally if clips not assigned
        if (playerShootClip == null)
            playerShootClip = GenerateLaserSound(0.1f, 800f, 400f);
        sfxClips["PlayerShoot"] = playerShootClip;

        if (enemyShootClip == null)
            enemyShootClip = GenerateLaserSound(0.1f, 500f, 300f);
        sfxClips["EnemyShoot"] = enemyShootClip;

        if (explosionClip == null)
            explosionClip = GenerateExplosionSound(0.4f);
        sfxClips["Explosion"] = explosionClip;

        if (playerHitClip == null)
            playerHitClip = GenerateHitSound(0.15f);
        sfxClips["PlayerHit"] = playerHitClip;

        if (powerUpClip == null)
            powerUpClip = GeneratePowerUpSound(0.3f);
        sfxClips["PowerUp"] = powerUpClip;

        if (waveStartClip == null)
            waveStartClip = GenerateWaveStartSound(0.5f);
        sfxClips["WaveStart"] = waveStartClip;

        if (gameOverClip == null)
            gameOverClip = GenerateGameOverSound(1f);
        sfxClips["GameOver"] = gameOverClip;

        // Generate background music
        if (musicClip == null)
            musicClip = GenerateBackgroundMusic(16f);
    }

    public void PlayMusic()
    {
        if (musicSource == null || musicClip == null) return;
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource?.Stop();
    }

    public void PauseMusic()
    {
        musicSource?.Pause();
    }

    public void ResumeMusic()
    {
        musicSource?.UnPause();
    }

    public void PlaySFX(string name)
    {
        if (sfxSource == null) return;
        if (sfxClips.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }

    // ----- Procedural Sound Generation -----

    private AudioClip GenerateLaserSound(float duration, float startFreq, float endFreq)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t; // Fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("Laser", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateExplosionSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        System.Random rng = new System.Random(42);
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Pow(1f - t, 2f);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            float lowFreq = Mathf.Sin(2f * Mathf.PI * 60f * t * duration);
            samples[i] = (noise * 0.5f + lowFreq * 0.5f) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Explosion", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateHitSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = 1f - t;
            float freq = Mathf.Lerp(600f, 200f, t);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("Hit", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GeneratePowerUpSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(400f, 1200f, t); // Rising tone
            float envelope = Mathf.Sin(Mathf.PI * t); // Bell curve
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("PowerUp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateWaveStartSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq1 = 440f;
            float freq2 = 880f;
            float mix = t < 0.5f ? 0f : 1f;
            float envelope = Mathf.Sin(Mathf.PI * t);
            float wave1 = Mathf.Sin(2f * Mathf.PI * freq1 * t * duration);
            float wave2 = Mathf.Sin(2f * Mathf.PI * freq2 * t * duration);
            samples[i] = Mathf.Lerp(wave1, wave2, mix) * envelope * 0.2f;
        }

        AudioClip clip = AudioClip.Create("WaveStart", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateGameOverSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(440f, 110f, t); // Descending
            float envelope = 1f - t * 0.7f;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("GameOver", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateBackgroundMusic(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Simple arpeggiated synth background
        float[] notes = { 130.81f, 164.81f, 196f, 246.94f, 261.63f, 246.94f, 196f, 164.81f };
        float noteLength = duration / notes.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = (int)(t / noteLength) % notes.Length;
            float noteT = (t % noteLength) / noteLength;

            float freq = notes[noteIndex];
            float envelope = Mathf.Sin(Mathf.PI * noteT) * 0.5f;

            // Main tone + harmony
            float main = Mathf.Sin(2f * Mathf.PI * freq * t);
            float harmony = Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.3f;
            float bass = Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.4f;

            samples[i] = (main + harmony + bass) * envelope * 0.12f;
        }

        AudioClip clip = AudioClip.Create("BGMusic", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
