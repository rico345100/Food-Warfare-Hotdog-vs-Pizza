using UnityEngine;

public class DisplayUIElementOverCanvas : MonoBehaviour {
    [SerializeField] private Transform m_Target = null;
    
    public Transform Target {
        get => m_Target;
        set => m_Target = value;
    }

    void Update() {
        if (m_Target == null) {
            return;
        }
        else if (InGameManager.Instance == null) {
            return;
        }

        Camera mainCamera = InGameManager.Instance.MainCamera;
        Vector3 viewerPosition = mainCamera.transform.position;
        Vector3 direction = (m_Target.position - viewerPosition).normalized;
        float dot = Vector3.Dot(direction, mainCamera.transform.forward);

        if (dot > 0) {
            transform.localScale = Vector3.one;
        }
        else {
            transform.localScale = Vector3.zero;
        }

        Vector3 worldPosition = mainCamera.WorldToScreenPoint(m_Target.position);
        transform.position = worldPosition;
    }
}
