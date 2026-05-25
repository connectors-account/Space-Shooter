using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages sound effects using procedurally generated audio.
/// Singleton pattern — access via AudioManager.Instance.
/// No external audio files needed; all sounds are synthesized at runtime.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private int sfxSourceCount = 8;
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 0.5f;

    private AudioSource[] sfxSources;
    private int currentSourceIndex;
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create audio source pool
        sfxSources = new AudioSource[sfxSourceCount];
        for (int i = 0; i < sfxSourceCount; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.volume = masterVolume;
            sfxSources[i] = src;
        }

        // Pre-generate all sound effects
        GenerateAllClips();
    }

    /// <summary>
    /// Plays a named sound effect. Silently ignores unknown names.
    /// </summary>
    public void PlaySFX(string clipName)
    {
        if (!clipCache.ContainsKey(clipName)) return;

        AudioSource src = sfxSources[currentSourceIndex];
        src.clip = clipCache[clipName];
        src.volume = masterVolume;
        src.Play();

        currentSourceIndex = (currentSourceIndex + 1) % sfxSources.Length;
    }

    /// <summary>
    /// Generates all game sound effects procedurally.
    /// </summary>
    private void GenerateAllClips()
    {
        int sampleRate = 44100;

        // Player shoot — short high-pitched blip
        clipCache["PlayerShoot"] = GenerateTone(sampleRate, 0.08f, 880f, 1200f, 0.3f);

        // Enemy shoot — lower pitched blip
        clipCache["EnemyShoot"] = GenerateTone(sampleRate, 0.1f, 330f, 220f, 0.2f);

        // Player hit — descending noise burst
        clipCache["PlayerHit"] = GenerateNoiseBurst(sampleRate, 0.2f, 0.5f);

        // Player death — long descending tone
        clipCache["PlayerDeath"] = GenerateTone(sampleRate, 0.5f, 440f, 80f, 0.6f);

        // Enemy death — short burst
        clipCache["EnemyDeath"] = GenerateNoiseBurst(sampleRate, 0.15f, 0.3f);

        // Power up — ascending arpeggio
        clipCache["PowerUp"] = GenerateArpeggio(sampleRate, 0.3f, new float[] { 523f, 659f, 784f, 1047f }, 0.4f);

        // Shield break — descending sweep
        clipCache["ShieldBreak"] = GenerateTone(sampleRate, 0.25f, 1200f, 200f, 0.4f);
    }

    /// <summary>
    /// Generates a simple tone that sweeps between two frequencies.
    /// </summary>
    private AudioClip GenerateTone(int sampleRate, float duration, float startFreq, float endFreq, float volume)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            float envelope = 1f - t; // Linear fade out
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates a noise burst with decay — used for explosion/hit sounds.
    /// </summary>
    private AudioClip GenerateNoiseBurst(int sampleRate, float duration, float volume)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = (1f - t) * (1f - t); // Quadratic decay
            samples[i] = Random.Range(-1f, 1f) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generates a rapid arpeggio — used for power-up pickup sound.
    /// </summary>
    private AudioClip GenerateArpeggio(int sampleRate, float duration, float[] frequencies, float volume)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        int noteDuration = sampleCount / frequencies.Length;

        for (int n = 0; n < frequencies.Length; n++)
        {
            float freq = frequencies[n];
            int start = n * noteDuration;
            int end = Mathf.Min(start + noteDuration, sampleCount);

            for (int i = start; i < end; i++)
            {
                float noteT = (float)(i - start) / noteDuration;
                float envelope = 1f - noteT * 0.5f; // Gentle decay per note
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * volume * envelope;
            }
        }

        AudioClip clip = AudioClip.Create("Arpeggio", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void SetVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
    }
}
