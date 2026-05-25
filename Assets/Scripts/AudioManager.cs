using UnityEngine;

/// <summary>
/// Manages procedurally generated sound effects using AudioSource.
/// No audio files needed - generates beeps and boops at runtime.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.3f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayShoot()
    {
        PlayTone(800f, 0.05f, 0.15f);
    }

    public void PlayHit()
    {
        PlayTone(200f, 0.1f, 0.3f);
    }

    public void PlayExplosion()
    {
        PlayTone(100f, 0.15f, 0.4f);
    }

    public void PlayPowerUp()
    {
        PlayTone(1200f, 0.1f, 0.25f);
    }

    void PlayTone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration); // Simple fade out
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;

            // Add some noise for explosion-like sounds
            if (frequency < 300f)
            {
                data[i] += Random.Range(-0.1f, 0.1f) * envelope;
            }
        }

        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip, volume);
    }
}
