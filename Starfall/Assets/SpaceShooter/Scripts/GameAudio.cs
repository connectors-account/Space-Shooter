using UnityEngine;

namespace Starfall
{
    public enum SoundCue { Laser, Hit, Explosion, Pickup, GameOver }

    [RequireComponent(typeof(AudioSource))]
    public sealed class GameAudio : MonoBehaviour
    {
        public AudioClip[] clips;
        public bool Muted { get; private set; }
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = 0.6f;
            Muted = PlayerPrefs.GetInt("Starfall.Muted", 0) == 1;
            source.mute = Muted;
        }

        public void Play(SoundCue cue)
        {
            int index = (int)cue;
            if (!Muted && index < clips.Length && clips[index] != null)
                source.PlayOneShot(clips[index], cue == SoundCue.Laser ? 0.28f : 0.75f);
        }

        public void StopEffects() { source.Stop(); }

        public void ToggleMute()
        {
            Muted = !Muted;
            source.mute = Muted;
            PlayerPrefs.SetInt("Starfall.Muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
