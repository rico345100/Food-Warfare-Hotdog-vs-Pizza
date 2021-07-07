using UnityEngine;
using Photon.Pun;
using ObjectPooling;

namespace FoodWarfare {
    [RequireComponent(typeof(AudioClip))]
    public partial class Player : MonoBehaviourPun, IDamagable, IPooledObject {
        private static Player m_Instance = null;
        public static Player Instance => m_Instance;

        private Rigidbody m_Rigidbody;
        private AudioSource m_AudioSource;
        private Transform m_SpawnPoint;

        [SerializeField] private Camera m_Camera = null;
        [SerializeField] private GameObject m_Mesh = null;
        [SerializeField] private Transform m_DisplayNameOffset = null;
        
        private string m_ExplosionVFXName = "PlayerExplosion";
        private float m_ExplosionRadius = 5;
        [SerializeField] private LayerMask m_ExplosionMask = default;

        private int m_Health = 10;
        private int m_DefaultMaxHealth = 10;

        private GameObject m_PlayerIndicator;

        public Camera Camera => m_Camera;

        public Transform Transform => transform;
        public int Health {
            get => m_Health;
            set => m_Health = value;
        }

        public float MeleeHitDistance => 7;

        public Rigidbody Rigidbody {
            get {
                if (m_Rigidbody == null) {
                    m_Rigidbody = GetComponent<Rigidbody>();
                }

                return m_Rigidbody;
            }
        }

        public AudioSource AudioSource {
            get {
                if (m_AudioSource == null) {
                    m_AudioSource = GetComponent<AudioSource>();
                }

                return m_AudioSource;
            }
        }

        public Transform SpawnPoint {
            get => m_SpawnPoint;
            set => m_SpawnPoint = value;
        }

        public bool Dead {
            get => m_Health <= 0;
        }

        public void Reinitialize() {
            m_Instance = this;

            Rigidbody.velocity = Vector3.zero;
            transform.SetPositionAndRotation(SpawnPoint.position, SpawnPoint.rotation);

            int maxHealth = GetMaxHealth();

            m_Health = maxHealth;
            InGameUIManager.Instance.SetHealthText(maxHealth);
            
            InGameUIManager.Instance.ShowPlayerUI();
            InGameUIManager.Instance.Reset();

            Invoke("SetSelfDestructionAvailable", 2);

            photonView.RPC("HandleReinitializeOtherPlayer", RpcTarget.Others, SpawnPoint.position, SpawnPoint.rotation, m_Health);
        }

        [PunRPC]
        void HandleReinitializeOtherPlayer(Vector3 position, Quaternion rotation, int newHealth) {
            if (m_PlayerIndicator == null) {
                m_PlayerIndicator = InGameUIManager.Instance.CreatePlayerIndicator(
                    photonView.Owner,
                    m_DisplayNameOffset
                );
            }

            transform.SetPositionAndRotation(position, rotation);
            m_PlayerIndicator.SetActive(true);

            m_Health = newHealth;
        }

        void Update() {
            if (photonView.IsMine == false) {
                return;
            }
            else if (Dead) {
                return;
            }
            else if (InGameUIManager.Instance.SendMessageContainerVisible == true) {
                return;
            }

            HandleMovement();
            HandleRotation();
            HandleSelfDestruction();
            HandleFire();
        }

        void HandleDead() {
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, m_ExplosionRadius, m_ExplosionMask);

            foreach (Collider hit in colliders) {
                if (hit.attachedRigidbody) {
                    hit.attachedRigidbody.AddExplosionForce(10000, transform.position, m_ExplosionRadius, 1000);
                }

                Enemy enemy = hit.GetComponent<Enemy>();

                if (enemy != null) {
                    enemy.TakeDamage(1);
                }
            }

            CreatePlayerExplosionVFX(explosionPos);

            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;

            m_SelfDestructionAvailable = false;

            InGameUIManager.Instance.HidePlayerUI();
            OnDead.Invoke(this);

            photonView.RPC("HandleDead", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber);

            PhotonNetwork.Destroy(gameObject);

            m_Instance = null;
        }

        [PunRPC]
        void HandleDead(int actorNumber) {
            m_Health = 0;

            CreatePlayerExplosionVFX(transform.position);
            m_PlayerIndicator.SetActive(false);

            InGameUIManager.Instance.DeletePlayerIndicator(actorNumber);
        }

        void CreatePlayerExplosionVFX(Vector3 position) {
            AudioManager.CreateAudioObject(transform.position, AudioManager.GetExplosionSound(), 3);

            GameObject explosionVFX = InGamePoolManager.Instance.Get(m_ExplosionVFXName, transform.position, Quaternion.identity);
            InGamePoolManager.Instance.Return(explosionVFX, 5);
        }

        public void TakeDamage(int damage) {
            if (photonView.IsMine) {
                HandleHit(damage);
            }
            else {
                photonView.RPC("HandleHit", photonView.Owner, damage);
            }
        }

        [PunRPC]
        void HandleHit(int damage) {
            OnDamage.Invoke(this);
            
            m_Health -= damage;

            InGameUIManager.Instance.ShowHit();
            InGameUIManager.Instance.SetHealthText(m_Health);
            
            if (m_Health <= 0) {
                HandleDead();
            }
        }

        int GetMaxHealth() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.MaxHP);
            return m_DefaultMaxHealth + (upgradeLevel * 5);
        }

        public void OnPooledObjectInstantiated() {}

        public void OnPooledObjectBeforeActive() {
            m_Mesh.SetActive(true);
        }

        public void OnPooledObjectAfterActive() {}

        public void OnPooledObjectReturn() {}

        void OnEnable() {
            if (photonView.IsMine == false) {
                Rigidbody.isKinematic = true;
                Rigidbody.useGravity = false;
                m_Camera.gameObject.SetActive(false);                
            }
            else {
                Rigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
                m_Camera.gameObject.SetActive(true); 
            }

            InGameManager.Instance.Allies.Add(this);
        }

        void OnDisable() {
            InGameManager.Instance.Allies.Remove(this);
        }
    }
}
