using UnityEngine;

namespace Starward
{
    public enum SoundId { Laser, EnemyShot, Explosion, Hit, Pickup, UI, GameOver, Wave }
    public sealed class AudioBank : MonoBehaviour
    {
        public AudioClip[] clips;
        public bool Muted { get; private set; }
        readonly AudioSource[] voices = new AudioSource[12];
        int cursor;
        void Awake()
        {
            Muted = PlayerPrefs.GetInt("Starward.Mute", 0) != 0;
            for (int i = 0; i < voices.Length; i++)
            {
                voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0;
                voices[i].ignoreListenerPause = true;
                voices[i].volume = .45f;
                voices[i].mute = Muted;
            }
        }
        public void Play(SoundId id)
        {
            int index = (int)id;
            if (Muted || clips == null || index >= clips.Length || clips[index] == null) return;
            AudioSource source = voices[cursor++ % voices.Length];
            source.clip = clips[index];
            source.Play();
        }
        public void ToggleMute()
        {
            Muted = !Muted;
            foreach (AudioSource source in voices) source.mute = Muted;
            PlayerPrefs.SetInt("Starward.Mute", Muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
