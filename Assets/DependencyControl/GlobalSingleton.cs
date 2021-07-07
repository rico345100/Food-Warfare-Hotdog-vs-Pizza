using UnityEngine;

public abstract class GlobalSingleton<T> : MonoBehaviour where T : GlobalSingleton<T> {
	private static T m_Instance = null;
	public static T Instance {
		get {
			if (m_Instance == null) {
                Initialize();
			}

			return m_Instance;
		}
	}

    void Awake() {
        if (m_Instance != null && m_Instance != this) {
            Destroy(gameObject);
        }
        
        Initialize();
    }

    protected virtual void OnInit() {}

    public static void Initialize() {
        if (m_Instance != null) {
			return;
		}

        m_Instance = FindObjectOfType<T>();

        if (m_Instance == null) {
            throw new System.Exception("Failed to get instance of type " + typeof(T).Name);
        }

        m_Instance.transform.parent = null;
        DontDestroyOnLoad(m_Instance.gameObject);

        m_Instance.OnInit();
    }
}
