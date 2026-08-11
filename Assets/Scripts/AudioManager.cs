using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Central place to play one-shot sound effects. Attach to a GameObject in the
    /// gameplay scene with an AudioSource, then assign the three clips in the Inspector.
    /// Clips are supplied by the user (.wav/.mp3) and dropped into the slots.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sound Effects (assign in Inspector)")]
        [Tooltip("Played when the player fires a bullet.")]
        [SerializeField] private AudioClip shootClip;

        [Tooltip("Played when an enemy is destroyed.")]
        [SerializeField] private AudioClip explosionClip;

        [Tooltip("Played once when the game is over.")]
        [SerializeField] private AudioClip gameOverClip;

        [Header("Volumes")]
        [Range(0f, 1f)][SerializeField] private float shootVolume = 0.5f;
        [Range(0f, 1f)][SerializeField] private float explosionVolume = 0.7f;
        [Range(0f, 1f)][SerializeField] private float gameOverVolume = 0.8f;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlayShoot()
        {
            PlayClip(shootClip, shootVolume);
        }

        public void PlayExplosion()
        {
            PlayClip(explosionClip, explosionVolume);
        }

        public void PlayGameOver()
        {
            PlayClip(gameOverClip, gameOverVolume);
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }
            // PlayOneShot ignores Time.timeScale, so the game-over sound still plays when paused.
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
