using UnityEngine;

public abstract class LocalSingleton<T> : MonoBehaviour where T : LocalSingleton<T> {
	private static T m_Instance = null;
	public static T Instance {
		get {
			if (m_Instance == null) {
                Initialize();
			}

			return m_Instance;
		}
	}

    protected virtual void OnInit() {}

	void Awake() {
		Initialize();
	}

	public static void Initialize() {
		if (m_Instance != null) {
			return;
		}
		
		m_Instance = FindObjectOfType<T>();

		if (m_Instance == null) {
			throw new System.Exception("Failed to get instance of type " + typeof(T).Name);
		}

		m_Instance.OnInit();
	}
}
