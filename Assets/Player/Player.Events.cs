namespace FoodWarfare {
    public partial class Player {
        private DamagableEvent m_OnDamage = new DamagableEvent();
        private DamagableEvent m_OnDead  = new DamagableEvent();

        public DamagableEvent OnDamage => m_OnDamage;
        public DamagableEvent OnDead => m_OnDead;
    }
}
