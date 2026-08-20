using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Resources
{
    /// <summary>
    /// Optional helper: if you drop real .wav/.ogg files into Assets/Resources/Audio with names
    /// matching the SFX keys ("shoot", "explosion", "powerup", "hit", "boss_shoot",
    /// "wave_complete", "game_over", "menu_click", or "music"), this component loads and registers
    /// them with the AudioManager at startup, overriding the procedurally generated tones.
    ///
    /// If no files are present the game simply keeps using the synthesized sounds — nothing breaks.
    /// Attach this to the same GameObject as the AudioManager (the setup script does this for you).
    /// </summary>
    public class AudioClipPlaceholder : MonoBehaviour
    {
        private static readonly string[] ClipNames =
        {
            "shoot", "explosion", "powerup", "hit",
            "boss_shoot", "wave_complete", "game_over", "menu_click", "music"
        };

        private void Start()
        {
            if (AudioManager.Instance == null) return;

            foreach (var name in ClipNames)
            {
                // Resources.Load searches Assets/Resources/**; path is relative & extension-less.
                var clip = UnityEngine.Resources.Load<AudioClip>($"Audio/{name}");
                if (clip != null)
                {
                    AudioManager.Instance.RegisterClip(name, clip);
                    Debug.Log($"[AudioClipPlaceholder] Loaded custom clip '{name}'.");
                }
            }
        }
    }
}
