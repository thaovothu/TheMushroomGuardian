using UnityEngine;

/// <summary>
/// Generic Singleton base class
/// Kế thừa từ lớp này để tạo Singleton: 
/// public class MyManager : BaseSingleton<MyManager> { }
/// </summary>
/// <typeparam name="T">Type của Singleton class</typeparam>
public class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tìm instance trong scene
                _instance = FindObjectOfType<T>();

                // Nếu chưa có, tạo GameObject mới
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    
                    Debug.Log($"[Singleton] Created new {typeof(T).Name} instance");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = GetComponent<T>();
            // DontDestroyOnLoad chỉ chấp nhận GameObject gốc. Nếu singleton là con của
            // object khác, dùng root để tránh cảnh báo và đảm bảo nó thật sự persist.
            DontDestroyOnLoad(transform.root.gameObject);
            Debug.Log($"[Singleton] {typeof(T).Name} initialized");
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Another instance of {typeof(T).Name} already exists. Destroying duplicate.");
            Destroy(gameObject);
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

/// <summary>
/// Non-MonoBehaviour Singleton - cho những class không cần attach vào GameObject
/// </summary>
public class SingletonNonMono<T> where T : SingletonNonMono<T>, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            _instance ??= new T();
            return _instance;
        }
    }

    protected SingletonNonMono()
    {
        Debug.Log($"[Singleton] {typeof(T).Name} (Non-Mono) initialized");
    }
}
