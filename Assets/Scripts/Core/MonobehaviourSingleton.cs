using UnityEngine;

namespace Core
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool isPersistent = true;

        private static T instance;
        private static bool _isShuttingDown;

        public static T Instance
        {
            get
            {
                if (_isShuttingDown) return null;

                if (!instance)
                    instance = FindFirstObjectByType<T>();

                if (!instance)
                {
                    var singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                }

                return instance;
            }
        }

        void Awake()
        {
            _isShuttingDown = false;

            if (Instance != this)
                Destroy(gameObject);
            else if (isPersistent)
                DontDestroyOnLoad(gameObject);
        }

        void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }

        void OnDestroy()
        {
            _isShuttingDown = true;
        }
    }
}