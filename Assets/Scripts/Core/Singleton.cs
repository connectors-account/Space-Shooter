using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Generic MonoBehaviour singleton base class.
    /// Any component deriving from this exposes a static <see cref="Instance"/>.
    /// Set <see cref="persistAcrossScenes"/> to keep the object alive between scene loads.
    /// </summary>
    /// <typeparam name="T">The concrete singleton type.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        /// <summary>The active instance of this singleton, or null if none exists.</summary>
        public static T Instance => _instance;

        [Tooltip("Keep this object alive when a new scene is loaded.")]
        [SerializeField] protected bool persistAcrossScenes = false;

        protected virtual void Awake()
        {
            RegisterSingleton();
        }

        /// <summary>
        /// Registers this component as the active singleton instance.
        /// Duplicate instances are destroyed at runtime; in the editor (tests)
        /// the reference is simply overwritten so components can be created freely.
        /// </summary>
        protected void RegisterSingleton()
        {
            if (_instance != null && _instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            _instance = (T)this;

            if (persistAcrossScenes && Application.isPlaying && transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
