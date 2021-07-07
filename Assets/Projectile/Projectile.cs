using UnityEngine;
using ObjectPooling;
using System.Collections;
using Photon.Pun;

namespace FoodWarfare {
    public class Projectile : MonoBehaviourPun, IPooledObject {
        private Rigidbody m_Rigidbody;
        private string m_ExplosionVFXName = "ProjectileExplosion";
        private bool m_Exploded = false;
        private IEnumerator m_CoExplode = null;
        private WaitForSeconds m_WaitExplosion = new WaitForSeconds(5);

        public Rigidbody Rigidbody {
            get {
                if (m_Rigidbody == null) {
                    m_Rigidbody = GetComponent<Rigidbody>();
                }

                return m_Rigidbody;
            }
        }

        void OnCollisionEnter(Collision other) {
            if (photonView.IsMine == false) {
                return;
            }
            else if (m_Exploded) {
                return;
            }

            IDamagable damagable = other.transform.GetComponent<IDamagable>();

            if (damagable != null) {
                damagable.TakeDamage(1);
            }

            Explode();
        }

        void Explode() {
            if (m_Exploded) {
                return;
            }

            m_Exploded = true;

            photonView.RPC("CreateExplosionVFX", RpcTarget.All, transform.position);
            PhotonNetwork.Destroy(gameObject);
        }

        [PunRPC]
        void CreateExplosionVFX(Vector3 position) {
            AudioManager.CreateAudioObject(position, AudioManager.GetExplosionSound(), 3);

            GameObject explosionVFX = InGamePoolManager.Instance.Get(m_ExplosionVFXName, position, Quaternion.identity);
            InGamePoolManager.Instance.Return(explosionVFX, 5);
        }

        IEnumerator CoExplode() {
            yield return m_WaitExplosion;
            Explode();
        }

        public void OnPooledObjectInstantiated() {}

        public void OnPooledObjectBeforeActive() {}

        public void OnPooledObjectAfterActive() {}

        public void OnPooledObjectReturn() {
            Rigidbody.velocity = Vector3.zero;

            if (m_CoExplode != null) {
                StopCoroutine(m_CoExplode);
                return;
            }
        }

        void OnEnable() {
            if (photonView.IsMine == false) {
                Rigidbody.isKinematic = true;
            }
            else {
                Rigidbody.isKinematic = false;
            }

            m_Exploded = false;

            m_CoExplode = CoExplode();
            StartCoroutine(m_CoExplode);
        }
    }
}
