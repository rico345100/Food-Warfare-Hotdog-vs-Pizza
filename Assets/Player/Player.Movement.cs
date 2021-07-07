using UnityEngine;

namespace FoodWarfare { 
    public partial class Player {
        private float m_MoveSpeed = 1000;
        private float m_RotateSpeed = 200;

        private KeyCode m_MoveForwardKey = KeyCode.UpArrow;
        private KeyCode m_MoveBackwardKey = KeyCode.DownArrow;
        private KeyCode m_RotateLeftKey = KeyCode.LeftArrow;
        private KeyCode m_RotateRightKey = KeyCode.RightArrow;

        void HandleMovement() {
            float forwardMovement = 0;
            float movespeed = GetMoveSpeed();

            if (Input.GetKey(m_MoveForwardKey)) {
                forwardMovement += movespeed * Time.deltaTime;
            }
            else if (Input.GetKey(m_MoveBackwardKey)) {
                forwardMovement -= movespeed * Time.deltaTime;
            }

            Rigidbody.AddForce(transform.forward * forwardMovement, ForceMode.Acceleration);
        }

        void HandleRotation() {
            float rotateSpeed = GetRotateSpeed();

            if (Input.GetKey(m_RotateLeftKey)) {
                transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
            }
            else if (Input.GetKey(m_RotateRightKey)) {
                transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            }
        }

        float GetMoveSpeed() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.MoveSpeed);
            return m_MoveSpeed + (upgradeLevel * 200);
        }

        float GetRotateSpeed() {
            int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.RotatingSpeed);
            return m_RotateSpeed + (upgradeLevel * 40);
        }
    }
}
