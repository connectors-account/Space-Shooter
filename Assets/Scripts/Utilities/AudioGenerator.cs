using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generates all AudioClips procedurally from PCM sample arrays.
    /// Mono, 44100 Hz. Every method returns a ready-to-play AudioClip.
    /// </summary>
    public static class AudioGenerator
    {
        #region Constants
        private const int SR = GameConstants.AUDIO_SAMPLE_RATE;
        private const float TWO_PI = Mathf.PI * 2f;
        #endregion

        #region Waveform Helpers
        private static float Sine(float freq, float t) => Mathf.Sin(TWO_PI * freq * t);

        private static float Square(float freq, float t) => Mathf.Sign(Mathf.Sin(TWO_PI * freq * t));

        private static float Noise() => Random.Range(-1f, 1f);

        private static AudioClip Build(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static int SamplesFor(float seconds) => Mathf.Max(1, Mathf.RoundToInt(seconds * SR));

        /// <summary>Simple linear attack/exponential release envelope.</summary>
        private static float Envelope(int i, int total, float attackFrac = 0.05f)
        {
            float t = (float)i / total;
            float attack = Mathf.Clamp01(t / Mathf.Max(0.0001f, attackFrac));
            float release = Mathf.Exp(-3f * t);
            return attack * release;
        }
        #endregion

        #region SFX
        /// <summary>Short rising sine chirp 440->880 Hz over 0.1 s.</summary>
        public static AudioClip GenerateShootSfx()
        {
            float dur = 0.1f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float freq = Mathf.Lerp(440f, 880f, t / dur);
                s[i] = Sine(freq, t) * Envelope(i, n) * 0.5f;
            }
            return Build("Shoot", s);
        }

        /// <summary>Descending chirp 880->440 Hz over 0.12 s.</summary>
        public static AudioClip GenerateEnemyShootSfx()
        {
            float dur = 0.12f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float freq = Mathf.Lerp(880f, 440f, t / dur);
                s[i] = Sine(freq, t) * Envelope(i, n) * 0.45f;
            }
            return Build("EnemyShoot", s);
        }

        /// <summary>White-noise burst with exponential decay over 0.4 s.</summary>
        public static AudioClip GenerateExplosionSfx()
        {
            float dur = 0.4f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            float lastLow = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float decay = Mathf.Exp(-5f * t);
                // Low-pass filtered noise for a rumbly boom.
                float noise = Noise();
                lastLow = Mathf.Lerp(lastLow, noise, 0.4f);
                s[i] = lastLow * decay * 0.8f;
            }
            return Build("Explosion", s);
        }

        /// <summary>Dissonant beating buzz (220 + 233 Hz) over 0.2 s.</summary>
        public static AudioClip GeneratePlayerHitSfx()
        {
            float dur = 0.2f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float a = Sine(220f, t);
                float b = Sine(233f, t);
                s[i] = (a + b) * 0.5f * Envelope(i, n, 0.02f) * 0.6f;
            }
            return Build("PlayerHit", s);
        }

        /// <summary>Ascending arpeggio C4-E4-G4-C5, 0.05 s per note.</summary>
        public static AudioClip GeneratePowerUpSfx()
        {
            float noteDur = 0.05f;
            float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f };
            int perNote = SamplesFor(noteDur);
            int n = perNote * freqs.Length;
            float[] s = new float[n];
            for (int note = 0; note < freqs.Length; note++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    int idx = note * perNote + i;
                    float t = (float)i / SR;
                    s[idx] = Sine(freqs[note], t) * Envelope(i, perNote, 0.05f) * 0.5f;
                }
            }
            return Build("PowerUp", s);
        }

        /// <summary>Low sweep 80->40 Hz with harmonics over 1 s.</summary>
        public static AudioClip GenerateBossRoarSfx()
        {
            float dur = 1f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float prog = t / dur;
                float freq = Mathf.Lerp(80f, 40f, prog);
                float fundamental = Sine(freq, t);
                float h2 = Sine(freq * 2f, t) * 0.5f;
                float h3 = Sine(freq * 3f, t) * 0.25f;
                float env = Mathf.Sin(Mathf.PI * prog); // fade in and out
                s[i] = (fundamental + h2 + h3) * env * 0.4f;
            }
            return Build("BossRoar", s);
        }

        /// <summary>Triumphant rising 3-note chord over 0.6 s.</summary>
        public static AudioClip GenerateWaveCompleteSfx()
        {
            float dur = 0.6f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            // C major triad notes that enter in sequence.
            float[] freqs = { 261.63f, 329.63f, 392.00f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float prog = t / dur;
                float val = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    float enter = k * 0.15f;
                    if (prog >= enter)
                        val += Sine(freqs[k], t);
                }
                val /= freqs.Length;
                float env = Mathf.Min(1f, prog * 6f) * Mathf.Exp(-1.5f * prog);
                s[i] = val * env * 0.5f;
            }
            return Build("WaveComplete", s);
        }

        /// <summary>Brief 1000 Hz tick over 0.05 s.</summary>
        public static AudioClip GenerateButtonClickSfx()
        {
            float dur = 0.05f;
            int n = SamplesFor(dur);
            float[] s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                s[i] = Square(1000f, t) * Envelope(i, n, 0.02f) * 0.35f;
            }
            return Build("ButtonClick", s);
        }
        #endregion

        #region Music
        /// <summary>
        /// 30 s looping synth track: 16 bars using square-wave notes drawn from a
        /// C minor pentatonic scale, with a simple bass line underneath.
        /// </summary>
        public static AudioClip GenerateBackgroundMusic()
        {
            float dur = 30f;
            int n = SamplesFor(dur);
            float[] s = new float[n];

            // C minor pentatonic: C, Eb, F, G, Bb (octave 4).
            float[] scale = { 261.63f, 311.13f, 349.23f, 392.00f, 466.16f, 523.25f };
            // Bass notes an octave+ down.
            float[] bass = { 65.41f, 77.78f, 87.31f, 98.00f };

            float bpm = 120f;
            float beat = 60f / bpm;         // seconds per beat
            float noteLen = beat * 0.5f;    // eighth notes for melody
            int melodySamples = SamplesFor(noteLen);
            int bassSamples = SamplesFor(beat * 2f); // half-note bass

            // Deterministic pseudo-random sequence so the loop is stable.
            System.Random rng = new System.Random(1234);

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;

                // Melody note index changes every melodySamples.
                int melStep = i / melodySamples;
                float melFreq = scale[SeededIndex(rng, melStep, scale.Length)];
                float melEnvT = (float)(i % melodySamples) / melodySamples;
                float melEnv = Mathf.Min(1f, melEnvT * 8f) * Mathf.Exp(-2.5f * melEnvT);
                float melody = Square(melFreq, t) * melEnv * 0.18f;

                // Bass note changes every bassSamples.
                int bassStep = i / bassSamples;
                float bassFreq = bass[SeededIndex(rng, bassStep + 100, bass.Length)];
                float bassEnvT = (float)(i % bassSamples) / bassSamples;
                float bassEnv = Mathf.Min(1f, bassEnvT * 4f) * Mathf.Exp(-1.2f * bassEnvT);
                float bassLine = Square(bassFreq, t) * bassEnv * 0.14f;

                s[i] = melody + bassLine;
            }

            return Build("BackgroundMusic", s);
        }

        // Stable index for a given step using a hash so the pattern repeats predictably.
        private static int SeededIndex(System.Random rng, int step, int range)
        {
            unchecked
            {
                int hash = step * 2654435761u.GetHashCode();
                hash ^= (step + 7) * 40503;
                int idx = Mathf.Abs(hash) % range;
                return idx;
            }
        }
        #endregion
    }
}
