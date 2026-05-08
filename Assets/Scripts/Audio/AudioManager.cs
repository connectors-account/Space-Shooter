using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Audio manager singleton. Generates simple beep/boop sound effects programmatically
/// using AudioClip.Create and sine wave synthesis. No external audio files needed.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private int sampleRate = 44100;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // SFX audio source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.5f;

        // Music audio source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.2f;

        GenerateAllSFX();
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 0.7f);
    }

    /// <summary>
    /// Play a named sound effect.
    /// </summary>
    public void PlaySFX(string name)
    {
        if (sfxClips.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxSource.volume);
        }
    }

    /// <summary>
    /// Start background music.
    /// </summary>
    public void PlayMusic()
    {
        if (!musicSource.isPlaying && sfxClips.ContainsKey("Music"))
        {
            musicSource.clip = sfxClips["Music"];
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // =============================================
    // Procedural Audio Generation
    // =============================================

    private void GenerateAllSFX()
    {
        // Player shoot - short high-pitched blip
        sfxClips["PlayerShoot"] = GenerateTone(880f, 0.06f, ToneType.Square, fadeOut: true);

        // Enemy death - descending noise burst
        sfxClips["EnemyDeath"] = GenerateNoiseBurst(0.15f, 600f, 100f);

        // Player hit - low thud
        sfxClips["PlayerHit"] = GenerateTone(150f, 0.2f, ToneType.Sine, fadeOut: true);

        // Player death - long descending
        sfxClips["PlayerDeath"] = GenerateSweep(800f, 80f, 0.6f, ToneType.Square);

        // Power up pickup - ascending arpeggio
        sfxClips["PowerUp"] = GenerateArpeggio(new float[] { 440, 554, 659, 880 }, 0.08f);

        // Shield up - bright shimmer
        sfxClips["ShieldUp"] = GenerateSweep(400f, 1200f, 0.3f, ToneType.Sine);

        // Shield break - crunch
        sfxClips["ShieldBreak"] = GenerateNoiseBurst(0.2f, 800f, 200f);

        // Heal - gentle ascending
        sfxClips["Heal"] = GenerateArpeggio(new float[] { 523, 659, 784 }, 0.1f);

        // Background music - simple bass loop
        sfxClips["Music"] = GenerateSimpleLoop(4f);
    }

    private enum ToneType { Sine, Square, Triangle, Noise }

    private AudioClip GenerateTone(float frequency, float duration, ToneType type, bool fadeOut = false)
    {
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float value = GetWaveform(t, frequency, type);
            if (fadeOut)
                value *= 1f - ((float)i / samples);
            data[i] = value * 0.3f;
        }

        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateSweep(float freqStart, float freqEnd, float duration, ToneType type)
    {
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float freq = Mathf.Lerp(freqStart, freqEnd, progress);
            float value = GetWaveform(t, freq, type);
            value *= 1f - progress; // Fade out
            data[i] = value * 0.25f;
        }

        AudioClip clip = AudioClip.Create("sweep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateNoiseBurst(float duration, float filterStart, float filterEnd)
    {
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        float phase = 0;
        for (int i = 0; i < samples; i++)
        {
            float progress = (float)i / samples;
            float freq = Mathf.Lerp(filterStart, filterEnd, progress);
            phase += freq / sampleRate;
            float noise = (Random.value * 2f - 1f) * 0.5f;
            float tone = Mathf.Sin(phase * 2f * Mathf.PI) * 0.3f;
            float value = (noise + tone) * (1f - progress);
            data[i] = value * 0.3f;
        }

        AudioClip clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateArpeggio(float[] frequencies, float noteLength)
    {
        int totalSamples = (int)(sampleRate * noteLength * frequencies.Length);
        float[] data = new float[totalSamples];
        int samplesPerNote = (int)(sampleRate * noteLength);

        for (int note = 0; note < frequencies.Length; note++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                int idx = note * samplesPerNote + i;
                if (idx >= totalSamples) break;

                float t = (float)i / sampleRate;
                float progress = (float)i / samplesPerNote;
                float value = Mathf.Sin(2f * Mathf.PI * frequencies[note] * t);
                value *= 1f - progress; // Fade each note
                data[idx] = value * 0.25f;
            }
        }

        AudioClip clip = AudioClip.Create("arp", totalSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateSimpleLoop(float duration)
    {
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        // Simple bass pattern with drums
        float bassFreq = 55f; // A1
        float[] pattern = { 55, 55, 73.4f, 55, 82.4f, 55, 73.4f, 55 };

        int beatSamples = samples / pattern.Length;

        for (int beat = 0; beat < pattern.Length; beat++)
        {
            for (int i = 0; i < beatSamples; i++)
            {
                int idx = beat * beatSamples + i;
                if (idx >= samples) break;

                float t = (float)i / sampleRate;
                float progress = (float)i / beatSamples;

                // Bass tone
                float bass = Mathf.Sin(2f * Mathf.PI * pattern[beat] * t) * 0.2f;
                bass *= Mathf.Max(0, 1f - progress * 2f); // Quick decay

                // Subtle kick at beat start
                float kick = 0;
                if (i < sampleRate * 0.05f)
                {
                    float kickT = (float)i / (sampleRate * 0.05f);
                    kick = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(150, 40, kickT) * t) * (1f - kickT) * 0.15f;
                }

                data[idx] = bass + kick;
            }
        }

        AudioClip clip = AudioClip.Create("music", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private float GetWaveform(float t, float frequency, ToneType type)
    {
        float phase = t * frequency;
        switch (type)
        {
            case ToneType.Sine:
                return Mathf.Sin(2f * Mathf.PI * phase);
            case ToneType.Square:
                return Mathf.Sin(2f * Mathf.PI * phase) > 0 ? 0.5f : -0.5f;
            case ToneType.Triangle:
                return Mathf.PingPong(phase * 2f, 1f) * 2f - 1f;
            case ToneType.Noise:
                return Random.value * 2f - 1f;
            default:
                return 0f;
        }
    }
}
