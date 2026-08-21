using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Singleton audio manager. All clips are generated procedurally with AudioClip.Create,
    /// so no external audio assets are required. Handles SFX, looping music and volume control.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private int sfxSourceCount = 8;

        private AudioSource[] sfxSources;
        private AudioSource musicSource;
        private int sfxIndex;

        private readonly Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
        private AudioClip musicClip;

        private const int SampleRate = 44100;

        private float sfxVolume = 1f;
        private float musicVolume = 0.5f;

        private const string SfxVolKey = "SpaceShooter_SFXVol";
        private const string MusicVolKey = "SpaceShooter_MusicVol";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxVolume = PlayerPrefs.GetFloat(SfxVolKey, 1f);
            musicVolume = PlayerPrefs.GetFloat(MusicVolKey, 0.5f);

            SetupSources();
            GenerateAllClips();
        }

        private void Start()
        {
            PlayMusic("BackgroundMusic");
        }

        private void SetupSources()
        {
            sfxSources = new AudioSource[Mathf.Max(1, sfxSourceCount)];
            for (int i = 0; i < sfxSources.Length; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = sfxVolume;
                sfxSources[i] = src;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        private void GenerateAllClips()
        {
            sfxClips["PlayerShoot"] = GenerateLaser(0.10f, 440f, 880f);
            sfxClips["EnemyShoot"] = GenerateLaser(0.10f, 660f, 330f);
            sfxClips["Explosion"] = GenerateExplosion(0.5f);
            sfxClips["PowerUp"] = GenerateArpeggio(0.30f, new float[] { 261.63f, 329.63f, 392.00f });
            sfxClips["PlayerHit"] = GeneratePlayerHit(0.20f);
            sfxClips["BossAlert"] = GenerateSweep(1.0f, 880f, 110f);
            sfxClips["GameOver"] = GenerateChord(1.5f, new float[] { 261.63f, 311.13f, 392.00f }, true);
            sfxClips["MenuClick"] = GenerateTick(0.05f, 800f);

            musicClip = GenerateMusicLoop();
        }

        // ---------------- Playback API ----------------

        public void PlaySFX(string name)
        {
            if (!sfxClips.TryGetValue(name, out AudioClip clip) || clip == null) return;
            AudioSource src = sfxSources[sfxIndex];
            sfxIndex = (sfxIndex + 1) % sfxSources.Length;
            src.volume = sfxVolume;
            src.PlayOneShot(clip, sfxVolume);
        }

        public void PlayMusic(string name)
        {
            if (musicClip == null) return;
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            foreach (AudioSource src in sfxSources)
            {
                src.volume = sfxVolume;
            }
            PlayerPrefs.SetFloat(SfxVolKey, sfxVolume);
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (musicSource != null) musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(MusicVolKey, musicVolume);
        }

        public float GetSFXVolume() => sfxVolume;
        public float GetMusicVolume() => musicVolume;

        // ---------------- Procedural generators ----------------

        private AudioClip GenerateLaser(float duration, float startFreq, float endFreq)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Exp(-4f * t); // decay
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate)) * env * 0.5f;
            }
            return CreateClip("Laser", data);
        }

        private AudioClip GenerateExplosion(float duration)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Exp(-5f * t);
                float noise = Random.Range(-1f, 1f);
                // Add a low rumble.
                float rumble = Mathf.Sin(2f * Mathf.PI * 60f * (i / (float)SampleRate));
                data[i] = (noise * 0.7f + rumble * 0.3f) * env * 0.6f;
            }
            return CreateClip("Explosion", data);
        }

        private AudioClip GenerateArpeggio(float duration, float[] notes)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            int noteSamples = samples / notes.Length;
            for (int i = 0; i < samples; i++)
            {
                int noteIndex = Mathf.Min(notes.Length - 1, i / noteSamples);
                float localT = (float)(i - noteIndex * noteSamples) / noteSamples;
                float env = Mathf.Sin(Mathf.PI * localT); // soft in/out per note
                data[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * (i / (float)SampleRate)) * env * 0.4f;
            }
            return CreateClip("Arpeggio", data);
        }

        private AudioClip GeneratePlayerHit(float duration)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Exp(-8f * t);
                float thud = Mathf.Sin(2f * Mathf.PI * 90f * (i / (float)SampleRate));
                float noise = Random.Range(-1f, 1f) * 0.4f;
                data[i] = (thud * 0.6f + noise) * env * 0.6f;
            }
            return CreateClip("PlayerHit", data);
        }

        private AudioClip GenerateSweep(float duration, float startFreq, float endFreq)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Sin(Mathf.PI * t);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate)) * env * 0.5f;
            }
            return CreateClip("Sweep", data);
        }

        private AudioClip GenerateChord(float duration, float[] freqs, bool descending)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float bend = descending ? Mathf.Lerp(1f, 0.6f, t) : 1f;
                float env = Mathf.Exp(-2f * t);
                float sample = 0f;
                foreach (float f in freqs)
                {
                    sample += Mathf.Sin(2f * Mathf.PI * f * bend * (i / (float)SampleRate));
                }
                data[i] = (sample / freqs.Length) * env * 0.5f;
            }
            return CreateClip("Chord", data);
        }

        private AudioClip GenerateTick(float duration, float freq)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Exp(-30f * t);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate)) * env * 0.5f;
            }
            return CreateClip("Tick", data);
        }

        private AudioClip GenerateMusicLoop()
        {
            // Simple 8-note melody looped over a duration.
            float noteDur = 0.35f;
            float[] melody = { 261.63f, 329.63f, 392.00f, 329.63f, 293.66f, 349.23f, 440.00f, 392.00f };
            float bassFreq = 130.81f;
            int noteSamples = Mathf.CeilToInt(SampleRate * noteDur);
            int totalSamples = noteSamples * melody.Length;
            float[] data = new float[totalSamples];

            for (int n = 0; n < melody.Length; n++)
            {
                for (int i = 0; i < noteSamples; i++)
                {
                    int idx = n * noteSamples + i;
                    float localT = (float)i / noteSamples;
                    float env = Mathf.Sin(Mathf.PI * localT) * 0.5f + 0.2f;
                    float lead = Mathf.Sin(2f * Mathf.PI * melody[n] * (idx / (float)SampleRate));
                    float bass = Mathf.Sin(2f * Mathf.PI * bassFreq * (idx / (float)SampleRate));
                    data[idx] = (lead * 0.35f + bass * 0.25f) * env;
                }
            }
            return CreateClip("Music", data);
        }

        private AudioClip CreateClip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
