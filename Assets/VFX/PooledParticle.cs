using UnityEngine;

namespace ObjectPooling {
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticle : MonoBehaviour, IPooledObject {
        private ParticleSystem m_ParticleSystem = null;

        public ParticleSystem ParticleSystem {
            get {
                if (m_ParticleSystem == null) {
                    m_ParticleSystem = GetComponent<ParticleSystem>();
                }

                return m_ParticleSystem;
            }
        }

        public void OnPooledObjectInstantiated() {
            m_ParticleSystem = GetComponent<ParticleSystem>();
            m_ParticleSystem.Stop();
            m_ParticleSystem.Play();
        }

        public void OnPooledObjectBeforeActive() {}

        public void OnPooledObjectAfterActive() {}

        public void OnPooledObjectReturn() {}

        void OnEnable() {
            transform.localScale = Vector3.one;
            ParticleSystem.Stop();
            ParticleSystem.Play();
        }
    }
}
