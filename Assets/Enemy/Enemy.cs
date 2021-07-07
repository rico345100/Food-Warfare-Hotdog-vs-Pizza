using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ObjectPooling;

namespace FoodWarfare {
    public class Enemy : MonoBehaviourPun, IDamagable, IPooledObject, IPunObservable {
        private string m_ExplosionVFXName = "PlayerExplosion";
        private Rigidbody m_Rigidbody;
        private float m_MoveSpeed = 2000;
        private float m_RotateSpeed = 100;
        private bool m_AttackLocked = false;
        private bool m_Dead = false;
        private DamagableEvent m_OnDead = new DamagableEvent();

        private Vector3 m_SynchronizedPosition = Vector3.zero;
        private Quaternion m_SynchronizedRotation = Quaternion.identity;
        private float m_PositionLagCompensation;
        private float m_RotationLagCompensation;

        public Transform Transform => transform;
        public bool Dead => m_Dead;
        public int Health => 0;
        public float MeleeHitDistance => 0;
        public DamagableEvent OnDead => m_OnDead;
        public DamagableEvent OnDamage => null;

        public Rigidbody Rigidbody {
            get {
                if (m_Rigidbody == null) {
                    m_Rigidbody = GetComponent<Rigidbody>();
                }

                return m_Rigidbody;
            }
        }

        void Update() {
            if (PhotonNetwork.IsMasterClient) {
                UpdateAI();
            }
            else {
                SynchronizeTransform();
            }
        }

        void UpdateAI() {
            if (PhotonNetwork.IsMasterClient == false) {
                return;
            }
            else if (InGameManager.Instance == null) {
                return;
            }
            else if (InGameManager.Instance.Allies.Count == 0) {
                return;
            }
            else if (m_Dead) {
                return;
            }

            IDamagable target = GetClosestTarget();

            if (target == null || target.Dead) {
                return;
            }

            Vector3 targetPosition = target.Transform.position;

            HandleMovement(target);
            HandleRotation(target);
            HandleAttack(target);
        }

        IDamagable GetClosestTarget() {
            List<IDamagable> allies = InGameManager.Instance.Allies;
            IDamagable closestTarget = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < allies.Count; i += 1) {
                if (allies[i].Dead) {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, allies[i].Transform.position);

                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestTarget = allies[i];
                }
            }

            return closestTarget;
        }

        void HandleMovement(IDamagable target) {
            Vector3 targetPosition = target.Transform.position;
            Vector3 direction = targetPosition - transform.position;
            Rigidbody.AddForce(direction.normalized * m_MoveSpeed * Time.deltaTime, ForceMode.Acceleration);
        }

        void HandleRotation(IDamagable target) {
            Vector3 targetPosition = target.Transform.position;
            Vector3 direction = targetPosition - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0, lookRot.eulerAngles.y, 0),
                Time.deltaTime * m_RotateSpeed
            );
        }

        void HandleAttack(IDamagable target) {
            float distance = Vector3.Distance(transform.position, target.Transform.position);

            if (distance <= target.MeleeHitDistance) {
                Attack(target);
            }
        }

        void Attack(IDamagable target) {
            if (m_AttackLocked) {
                return;
            }

            target.TakeDamage(1);
            m_AttackLocked = true;

            Invoke("ResetAttackLock", 0.5f);
        }

        void ResetAttackLock() {
            m_AttackLocked = false;
        }

        public void TakeDamage(int value) {
            if (m_Dead) {
                return;
            }

            photonView.RPC("HandleHit", RpcTarget.MasterClient);
            Invoke("Explode", 0.5f);
        }

        [PunRPC]
        void HandleHit() {
            m_Dead = true;
            m_OnDead.Invoke(this);
            
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.AddExplosionForce(1000, transform.position, 3, 100);
        }

        void Explode() {
            photonView.RPC("CreateExplosionVFX", RpcTarget.All);
        }

        [PunRPC]
        void CreateExplosionVFX() {
            AudioManager.CreateAudioObject(transform.position, AudioManager.GetExplosionSound(), 3);

            GameObject explosionVFX = InGamePoolManager.Instance.Get(m_ExplosionVFXName, transform.position, Quaternion.identity);
            InGamePoolManager.Instance.Return(explosionVFX, 5);
            InGamePoolManager.Instance.Return(gameObject);
        }

        void SynchronizeTransform() {
            transform.position = Vector3.MoveTowards(transform.position, m_SynchronizedPosition, m_PositionLagCompensation * (1.0f / PhotonNetwork.SerializationRate));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_SynchronizedRotation, m_RotationLagCompensation * (1.0f / PhotonNetwork.SerializationRate));
        }

        public void OnPooledObjectInstantiated() {}

        public void OnPooledObjectBeforeActive() {
            InGameManager.Instance.Enemies.Add(this);

            m_Dead = false;
            m_SynchronizedPosition = Vector3.zero;
            m_SynchronizedRotation = Quaternion.identity;
        }

        public void OnPooledObjectAfterActive() {}

        public void OnPooledObjectReturn() {
            InGameManager.Instance.Enemies.Remove(this);
        }

        void OnEnable() {
            if (PhotonNetwork.IsMasterClient == false) {
                Rigidbody.isKinematic = true;
                Rigidbody.useGravity = false;
            }
            else {
                Rigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else {
                m_SynchronizedPosition = (Vector3) stream.ReceiveNext();
                m_SynchronizedRotation = (Quaternion) stream.ReceiveNext();

                m_PositionLagCompensation = Vector3.Distance(transform.position, m_SynchronizedPosition);
                m_RotationLagCompensation = Quaternion.Angle(transform.rotation, m_SynchronizedRotation);
            }
        }
    }
}