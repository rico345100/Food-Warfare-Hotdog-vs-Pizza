using UnityEngine;
using Photon.Pun;

namespace FoodWarfare {
    public partial class Player {
        [SerializeField] private Transform m_ShootPoint = null;
        private KeyCode m_FireKey = KeyCode.Space;
        private string m_ProjectileName = "PlayerProjectile";
        private bool m_FireLock = false;
        private float m_FireRate = 0.4f;
        private float m_ProjectileLaunchPower = 50;

        void HandleFire() {
            if (m_FireLock) return;

            if (Input.GetKey(m_FireKey)) {
                GameObject projectileObject = PhotonNetwork.Instantiate(m_ProjectileName, m_ShootPoint.position, m_ShootPoint.rotation);
                Projectile projectile = projectileObject.GetComponent<Projectile>();
                float projectileLaunchPower = GetProjectileLaunchPower();

                Vector3 pushForce = m_ShootPoint.forward.normalized * projectileLaunchPower;
                float spread = GetProjectileSpread();
                float spreadWidth = Random.Range(-spread, spread);
                Vector3 verticalSpread = m_ShootPoint.transform.right * spreadWidth;

                pushForce += verticalSpread;

                projectile.Rigidbody.AddForce(pushForce, ForceMode.VelocityChange);

                AudioSource.PlayOneShot(AudioManager.GetFireSound());

                m_FireLock = true;
                Invoke("ResetFireLock", GetFireRate());
            }
        }

        void ResetFireLock() {
            m_FireLock = false;
        }

        float GetProjectileLaunchPower() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.ProjectileSpeed);
            return m_ProjectileLaunchPower + (upgradeLevel * 20);
        }

        float GetFireRate() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.AttackSpeed);
            return m_FireRate - (upgradeLevel * 0.03f);
        }

        float GetProjectileSpread() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.ProjectileSpread);
            return upgradeLevel * 2.5f;
        }
    }
}
