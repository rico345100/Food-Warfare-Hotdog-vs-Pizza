using UnityEngine;
using Photon.Pun;
using ObjectPooling;

namespace FoodWarfare {
    public class HQ : MonoBehaviour, IDamagable, IPooledObject {
        private int m_Health = 200;
        private string m_ExplosionVFXName = "PlayerExplosion";

        public Transform Transform => transform;
        public int Health => m_Health;
        public bool Dead => m_Health <= 0;
        public float MeleeHitDistance => 15;

        private DamagableEvent m_OnDamage = new DamagableEvent();
        private DamagableEvent m_OnDead = new DamagableEvent();

        public DamagableEvent OnDamage => m_OnDamage;
        public DamagableEvent OnDead => m_OnDead;

        public void TakeDamage(int value) {
            if (Dead) {
                return;
            }

            m_Health -= value;

            m_OnDamage.Invoke(this);

            if (m_Health <= 0) {
                HandleDead();
            }
        }

        void HandleDead() {
            m_OnDead.Invoke(this);

            AudioManager.CreateAudioObject(transform.position, AudioManager.GetExplosionSound(), 3);
            
            GameObject explosionVFX = InGamePoolManager.Instance.Get(m_ExplosionVFXName, transform.position, Quaternion.identity);
            InGamePoolManager.Instance.Return(explosionVFX, 5);
            
            PhotonNetwork.Destroy(gameObject);
        }

        public void OnPooledObjectInstantiated() {}

        public void OnPooledObjectBeforeActive() {
            InGameManager.Instance.Allies.Add(this);
        }

        public void OnPooledObjectAfterActive() {}

        public void OnPooledObjectReturn() {
            InGameManager.Instance.Allies.Remove(this);
        }
    }
}