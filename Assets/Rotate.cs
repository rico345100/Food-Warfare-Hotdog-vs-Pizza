using UnityEngine;

public class Rotate : MonoBehaviour {
    [SerializeField] private Vector3 m_Velocity = Vector3.zero;

    void Update() {
        transform.Rotate(m_Velocity * Time.deltaTime);
    }
}
