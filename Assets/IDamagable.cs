using UnityEngine;
using UnityEngine.Events;

public class DamagableEvent: UnityEvent<IDamagable> {}

public interface IDamagable {
    Transform Transform { get; }
    int Health { get; }
    bool Dead { get; }
    float MeleeHitDistance { get; }
    DamagableEvent OnDamage { get; }
    DamagableEvent OnDead { get; }
    void TakeDamage(int value);
}