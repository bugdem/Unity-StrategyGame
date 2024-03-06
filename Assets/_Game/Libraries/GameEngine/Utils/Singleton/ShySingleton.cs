using UnityEngine;

namespace GameEngine.Library.Utils
{
    /// <summary>
    /// Singleton pattern.
    /// </summary>
    public class ShySingleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }
                return _instance;
            }
        }

		public static T Get()
		{
			return Application.isPlaying ? Instance : FindObjectOfType<T>();
		}

		/// <summary>
		/// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
		/// </summary>
		protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _instance = this as T;
        }
    }
}