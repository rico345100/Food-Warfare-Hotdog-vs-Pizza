using UnityEngine;

public class DeadZone : MonoBehaviour {
    void OnCollisionEnter(Collision other) {
        IDamagable damagable = other.transform.GetComponent<IDamagable>();

        if (damagable != null) {
            damagable.TakeDamage(int.MaxValue);
        }
    }
}
