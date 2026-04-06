using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central audio manager. Generates simple beep/blip sounds procedurally
/// since no external audio assets are needed. Provides named sound playback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    private AudioSource sfxSource;
    private Dictionary<string, AudioClip> soundClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        GenerateSoundEffects();
    }

    /// <summary>
    /// Procedurally generate simple sound effects as AudioClips.
    /// </summary>
    private void GenerateSoundEffects()
    {
        soundClips = new Dictionary<string, AudioClip>();

        // Player shoot: short high-pitched blip
        soundClips["PlayerShoot"] = GenerateBeep(0.05f, 880f, 0.3f);

        // Enemy shoot: lower pitched blip
        soundClips["EnemyShoot"] = GenerateBeep(0.08f, 330f, 0.2f);

        // Player hit: descending tone
        soundClips["PlayerHit"] = GenerateSweep(0.15f, 600f, 200f, 0.4f);

        // Player death: long descending sweep
        soundClips["PlayerDeath"] = GenerateSweep(0.4f, 800f, 100f, 0.5f);

        // Enemy hit: short crunch
        soundClips["EnemyHit"] = GenerateNoise(0.05f, 0.2f);

        // Enemy death: medium pop
        soundClips["EnemyDeath"] = GenerateSweep(0.1f, 500f, 100f, 0.3f);

        // Power-up collect: ascending arpeggio
        soundClips["PowerUp"] = GenerateSweep(0.2f, 400f, 1200f, 0.4f);

        // Shield break: burst
        soundClips["ShieldBreak"] = GenerateNoise(0.15f, 0.4f);
    }

    /// <summary>
    /// Generate a simple sine wave beep.
    /// </summary>
    private AudioClip GenerateBeep(float duration, float frequency, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / sampleCount; // Linear decay
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate a frequency sweep (ascending or descending tone).
    /// </summary>
    private AudioClip GenerateSweep(float duration, float startFreq, float endFreq, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, progress);
            float envelope = 1f - progress;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Sweep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate white noise burst (for explosions, impacts).
    /// </summary>
    private AudioClip GenerateNoise(float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float envelope = 1f - (float)i / sampleCount;
            samples[i] = Random.Range(-1f, 1f) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Play a named sound effect.
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (soundClips == null || !soundClips.ContainsKey(soundName)) return;
        if (sfxSource == null) return;

        sfxSource.volume = masterVolume * sfxVolume;
        sfxSource.PlayOneShot(soundClips[soundName]);
    }
}
