using UnityEngine;

namespace Core
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool isPersistent = true;

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindFirstObjectByType<T>();

                if (!_instance)
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();

                return _instance;
            }
        }

        void Awake()
        {
            if (Instance != this)
                Destroy(gameObject);
            else if (isPersistent)
                DontDestroyOnLoad(gameObject);
        }
    }
}