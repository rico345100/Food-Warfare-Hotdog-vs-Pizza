namespace ObjectPooling {
    public interface IPooledObject {
        // Invokes when Object is instantiated
        void OnPooledObjectInstantiated();
        // Invokes when pooled object before active
        void OnPooledObjectBeforeActive();
        // Invokes when pooled object after active
        void OnPooledObjectAfterActive();
        // Invokes when the pooled object retured to the pool
        void OnPooledObjectReturn();
    }
}
