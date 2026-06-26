using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Centralised sound effect and music playback. To keep the project fully playable without
    /// imported audio assets, short SFX clips are synthesised procedurally at startup. If real
    /// <see cref="AudioClip"/> assets are assigned via <see cref="RegisterClip"/> they take priority,
    /// so this manager doubles as the integration point for custom audio.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        [Range(0f, 1f)] private float _sfxVolume = 0.5f;
        [Range(0f, 1f)] private float _musicVolume = 0.25f;

        private const int SampleRate = 44100;

        /// <summary>
        /// Initialises audio sources and synthesises the default SFX set. Called once by the bootstrap.
        /// </summary>
        public void Initialize()
        {
            Instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = _musicVolume;

            BuildDefaultClips();
        }

        private void BuildDefaultClips()
        {
            RegisterClip("player_shot", GenerateTone(880f, 0.08f, WaveShape.Square, 0.3f));
            RegisterClip("enemy_shot", GenerateTone(330f, 0.10f, WaveShape.Saw, 0.25f));
            RegisterClip("explosion", GenerateNoise(0.35f, 0.5f));
            RegisterClip("player_hit", GenerateTone(160f, 0.18f, WaveShape.Square, 0.4f));
            RegisterClip("powerup", GenerateSweep(440f, 1320f, 0.25f));
            RegisterClip("ui_click", GenerateTone(660f, 0.06f, WaveShape.Square, 0.3f));
            RegisterClip("wave", GenerateSweep(523f, 1046f, 0.4f));
            RegisterClip("music", GenerateMusicLoop());
        }

        /// <summary>
        /// Registers (or overrides) a named clip. Assign imported assets here to replace the synthesised defaults.
        /// </summary>
        /// <param name="key">Logical name (e.g. "explosion").</param>
        /// <param name="clip">The audio clip to associate.</param>
        public void RegisterClip(string key, AudioClip clip)
        {
            if (clip != null)
            {
                _clips[key] = clip;
            }
        }

        private void Play(string key, float volumeScale = 1f)
        {
            if (_sfxSource != null && _clips.TryGetValue(key, out AudioClip clip))
            {
                _sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        /// <summary>Plays the player firing sound.</summary>
        public void PlayPlayerShot() => Play("player_shot", 0.6f);

        /// <summary>Plays the enemy firing sound.</summary>
        public void PlayEnemyShot() => Play("enemy_shot", 0.4f);

        /// <summary>Plays an explosion sound.</summary>
        public void PlayExplosion() => Play("explosion", 0.8f);

        /// <summary>Plays the player-took-damage sound.</summary>
        public void PlayPlayerHit() => Play("player_hit", 0.8f);

        /// <summary>Plays the power-up collected sound.</summary>
        public void PlayPowerUp() => Play("powerup");

        /// <summary>Plays a UI button click.</summary>
        public void PlayUiClick() => Play("ui_click", 0.7f);

        /// <summary>Plays the new-wave fanfare.</summary>
        public void PlayWaveStart() => Play("wave");

        /// <summary>Starts looping background music.</summary>
        public void StartMusic()
        {
            if (_musicSource != null && _clips.TryGetValue("music", out AudioClip clip))
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }

        /// <summary>Stops the background music.</summary>
        public void StopMusic()
        {
            _musicSource?.Stop();
        }

        /// <summary>Sets SFX volume (0..1).</summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (_sfxSource != null)
            {
                _sfxSource.volume = _sfxVolume;
            }
        }

        /// <summary>Sets music volume (0..1).</summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        private enum WaveShape { Sine, Square, Saw }

        private static AudioClip GenerateTone(float frequency, float duration, WaveShape shape, float amplitude)
        {
            int samples = Mathf.Max(1, (int)(SampleRate * duration));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float phase = frequency * t;
                float value;
                switch (shape)
                {
                    case WaveShape.Square:
                        value = Mathf.Sign(Mathf.Sin(phase * 2f * Mathf.PI));
                        break;
                    case WaveShape.Saw:
                        value = 2f * (phase - Mathf.Floor(phase + 0.5f));
                        break;
                    default:
                        value = Mathf.Sin(phase * 2f * Mathf.PI);
                        break;
                }
                float envelope = 1f - ((float)i / samples); // linear decay
                data[i] = value * amplitude * envelope;
            }
            return ClipFrom(data, "tone");
        }

        private static AudioClip GenerateSweep(float startFreq, float endFreq, float duration)
        {
            int samples = Mathf.Max(1, (int)(SampleRate * duration));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, progress);
                float t = (float)i / SampleRate;
                float envelope = Mathf.Sin(progress * Mathf.PI); // fade in/out
                data[i] = Mathf.Sin(freq * t * 2f * Mathf.PI) * 0.35f * envelope;
            }
            return ClipFrom(data, "sweep");
        }

        private static AudioClip GenerateNoise(float duration, float amplitude)
        {
            int samples = Mathf.Max(1, (int)(SampleRate * duration));
            float[] data = new float[samples];
            var rng = new System.Random();
            for (int i = 0; i < samples; i++)
            {
                float envelope = 1f - ((float)i / samples);
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = white * amplitude * envelope * envelope;
            }
            return ClipFrom(data, "noise");
        }

        private static AudioClip GenerateMusicLoop()
        {
            // Simple arpeggiated loop (A minor) so the game has ambient background music.
            float[] notes = { 220f, 261.63f, 329.63f, 261.63f };
            float noteDur = 0.25f;
            int perNote = (int)(SampleRate * noteDur);
            int samples = perNote * notes.Length;
            float[] data = new float[samples];
            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    int idx = n * perNote + i;
                    float t = (float)i / SampleRate;
                    float envelope = Mathf.Sin(((float)i / perNote) * Mathf.PI);
                    data[idx] = Mathf.Sin(notes[n] * t * 2f * Mathf.PI) * 0.18f * envelope;
                }
            }
            return ClipFrom(data, "music");
        }

        private static AudioClip ClipFrom(float[] data, string name)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
