using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Centralized audio manager with procedurally generated placeholder sounds.
    /// Replace AudioClips with real assets for production.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;

        private Dictionary<string, AudioClip> sfxClips;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupAudioSources();
            GeneratePlaceholderSounds();
        }

        private void SetupAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            sfxSource.volume = sfxVolume;
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Generate simple procedural beep sounds as placeholders.
        /// Each sound has a distinct frequency/duration.
        /// </summary>
        private void GeneratePlaceholderSounds()
        {
            sfxClips = new Dictionary<string, AudioClip>();

            sfxClips["PlayerShoot"] = GenerateBeep(0.08f, 880f, 0.5f);   // short high beep
            sfxClips["EnemyShoot"] = GenerateBeep(0.1f, 440f, 0.3f);     // lower beep
            sfxClips["EnemyDeath"] = GenerateBeep(0.15f, 220f, 0.6f);    // low boom
            sfxClips["PlayerHit"] = GenerateBeep(0.2f, 330f, 0.7f);      // medium hit
            sfxClips["PlayerDeath"] = GenerateNoise(0.5f, 0.8f);         // noise burst
            sfxClips["PowerUp"] = GenerateSweep(0.25f, 440f, 880f, 0.5f); // ascending sweep
            sfxClips["ShieldBreak"] = GenerateNoise(0.2f, 0.5f);         // short noise
            sfxClips["WaveStart"] = GenerateSweep(0.3f, 330f, 660f, 0.4f);
        }

        private AudioClip GenerateBeep(float duration, float frequency, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // linear fade out
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create($"Beep_{frequency}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateNoise(float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration);
                samples[i] = Random.Range(-1f, 1f) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateSweep(float duration, float startFreq, float endFreq, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(startFreq, endFreq, t / duration);
                float envelope = 1f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create("Sweep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Play a named sound effect. Falls back gracefully if the clip doesn't exist.
        /// </summary>
        public void PlaySFX(string clipName)
        {
            if (sfxClips != null && sfxClips.TryGetValue(clipName, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void SetSFXVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
            sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            musicSource.volume = musicVolume;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
