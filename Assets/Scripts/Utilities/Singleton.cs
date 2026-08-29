using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generic MonoBehaviour singleton base class.
    /// Derive like: public class GameManager : Singleton&lt;GameManager&gt; { ... }
    /// Survives scene loads via DontDestroyOnLoad and guarantees a single instance.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Whether the object should be kept alive across scene loads via DontDestroyOnLoad.
        /// Override in a derived class and return false for a scene-local singleton.
        /// </summary>
        protected virtual bool PersistAcrossScenes => true;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject(typeof(T).Name + " (AutoCreated)");
                            _instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Returns true if an instance already exists without creating one.
        /// </summary>
        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (PersistAcrossScenes && transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }

                OnAwakeInitialize();
            }
            else if (_instance != this)
            {
                // A duplicate exists — destroy this one.
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Override for one-time initialization on the surviving instance.
        /// </summary>
        protected virtual void OnAwakeInitialize() { }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
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
