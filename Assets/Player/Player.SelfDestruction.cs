using UnityEngine;

namespace FoodWarfare {
    public partial class Player {
        private KeyCode m_SelfDestructionKey = KeyCode.R;
        private bool m_SelfDestructionAvailable = true;

        void HandleSelfDestruction() {
            if (m_SelfDestructionAvailable == false) {
                return;
            }

            if (Input.GetKey(m_SelfDestructionKey)) {
                TakeDamage(int.MaxValue);
            }
        }

        void SetSelfDestructionAvailable() {
            m_SelfDestructionAvailable = true;
        }
    }
}
