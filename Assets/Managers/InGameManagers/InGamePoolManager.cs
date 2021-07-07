using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ObjectPooling;

public class InGamePoolManager : LocalSingleton<InGamePoolManager>, IPunPrefabPool {
    [Tooltip("List of Pool Objects. Update this pool only in original prefab so that every scene can share same pool objects. Also don't put the scene specific pool objects, use SceneOnlyPoolGroups instead.")]
    [SerializeField] private PoolGroup[] m_PoolGroups = null;
    [Tooltip("List of Pool Objects that used in only this scene.")]
    [SerializeField] private PoolGroup[] m_SceneOnlyPoolGroups = null;
    private Dictionary<GameObject, IPooledObject> m_CachedInterfaces = new Dictionary<GameObject, IPooledObject>();
    private Dictionary<GameObject, IEnumerator> m_AutoDestroyCoroutines = new Dictionary<GameObject, IEnumerator>();
    private Dictionary<GameObject, Rigidbody> m_CachedRigidbodies = new Dictionary<GameObject, Rigidbody>();

    private PoolGroup[] m_TotalPoolGroups = null;

    protected override void OnInit() {
        PhotonNetwork.PrefabPool = this;

        List<PoolGroup> totalPoolGroups = new List<PoolGroup>();
        totalPoolGroups.AddRange(m_PoolGroups);
        totalPoolGroups.AddRange(m_SceneOnlyPoolGroups);

        m_TotalPoolGroups = totalPoolGroups.ToArray();

        foreach (PoolGroup poolGroup in totalPoolGroups) {
            ExtendPool(poolGroup, poolGroup.defaultSize);
        }
    }

    void ExtendPool(PoolGroup poolGroup, int size) {
        for (int i = 0; i < size; i++) {
            GameObject gameObject = Instantiate(poolGroup.targetPrefab);
            gameObject.SetActive(false);

            IPooledObject poolObjectInterface = gameObject.GetComponent<IPooledObject>();

            if (poolObjectInterface != null) {
                poolObjectInterface.OnPooledObjectInstantiated();
                m_CachedInterfaces.Add(gameObject, poolObjectInterface);
            }

            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            if (rigidbody != null) {
                m_CachedRigidbodies.Add(gameObject, rigidbody);
            }

            poolGroup.objectList.Add(gameObject);
        }
    }

    PoolGroup GetPoolGroup(string name) {
         PoolGroup poolGroup = null;

        for (int i = 0; i < m_TotalPoolGroups.Length; i++) {
            if (m_TotalPoolGroups[i].name.Equals(name)) {
                poolGroup = m_TotalPoolGroups[i];
                break;
            }
        }

        return poolGroup;
    }

    GameObject GetAvailableFromPool(PoolGroup poolGroup) {
        GameObject availableObject = null;

        for (int i = 0; i < poolGroup.objectList.Count; i++) {
            if (poolGroup.objectList[i].activeSelf == false) {
                availableObject = poolGroup.objectList[i];
                break;
            }
        }

        return availableObject;
    }

    public GameObject Get(string name, Vector3 position, Quaternion rotation) {
        GameObject availableObject = InternalGet(name, position, rotation);

        bool hasRigidbody = m_CachedRigidbodies.ContainsKey(availableObject);
        bool isKinematic = hasRigidbody && m_CachedRigidbodies[availableObject].isKinematic;
        
        if (hasRigidbody) {
            m_CachedRigidbodies[availableObject].isKinematic = true;
        }

        availableObject.transform.SetPositionAndRotation(position, rotation);

        if (hasRigidbody && isKinematic == false) {
            m_CachedRigidbodies[availableObject].isKinematic = false;
        }

        IPooledObject pooledObject = null;

        if (m_CachedInterfaces.ContainsKey(availableObject)) {
            pooledObject = m_CachedInterfaces[availableObject];
            pooledObject.OnPooledObjectBeforeActive();
        }

        availableObject.SetActive(true);

        if (pooledObject != null) {
            pooledObject.OnPooledObjectAfterActive();
        }

        return availableObject;
    }

    GameObject NetworkGet(string name, Vector3 position, Quaternion rotation) {
        GameObject availableObject = InternalGet(name, position, rotation);

        bool hasRigidbody = m_CachedRigidbodies.ContainsKey(availableObject);
        bool isKinematic = hasRigidbody && m_CachedRigidbodies[availableObject].isKinematic;
        
        if (hasRigidbody) {
            m_CachedRigidbodies[availableObject].isKinematic = true;
        }

        availableObject.transform.SetPositionAndRotation(position, rotation);

        if (hasRigidbody && isKinematic == false) {
            m_CachedRigidbodies[availableObject].isKinematic = false;
        }

        IPooledObject pooledObject = null;

        if (m_CachedInterfaces.ContainsKey(availableObject)) {
            pooledObject = m_CachedInterfaces[availableObject];
            pooledObject.OnPooledObjectBeforeActive();
        }

        return availableObject;
    }

    GameObject InternalGet(string name, Vector3 position, Quaternion rotation) {
        PoolGroup poolGroup = GetPoolGroup(name);

        if (poolGroup == null) {
            throw new System.Exception("Can't find PoolGroup named " + name.ToString());
        }

        GameObject availableObject = GetAvailableFromPool(poolGroup);

        if (availableObject == null) {
            Debug.Log("Extending Pool for: " + name);
            // All pooled object are using! Extend it.
            ExtendPool(poolGroup, poolGroup.extendSize);
            availableObject = GetAvailableFromPool(poolGroup);
        }

        return availableObject;
    }

    public void Return(GameObject go) {
        go.transform.SetParent(null);
        go.SetActive(false);

        if (m_CachedInterfaces.ContainsKey(go)) {
            IPooledObject pooledObject = m_CachedInterfaces[go];
            pooledObject.OnPooledObjectReturn();
        }

        RemoveAutoDestroyCoroutineIfExists(go);
    }

    public void Return(GameObject go, float time) {
        RemoveAutoDestroyCoroutineIfExists(go);

        IEnumerator co = CoReturnAfterTime(go, time);
        m_AutoDestroyCoroutines.Add(go, co);
        StartCoroutine(co);
    }

    IEnumerator CoReturnAfterTime(GameObject go, float time) {
        yield return new WaitForSeconds(time);
        Return(go);
    }

    void RemoveAutoDestroyCoroutineIfExists(GameObject go) {
        if (m_AutoDestroyCoroutines.ContainsKey(go)) {
            StopCoroutine(m_AutoDestroyCoroutines[go]);
            m_AutoDestroyCoroutines.Remove(go);
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) {
        return NetworkGet(prefabId, position, rotation);
    }

    public void Destroy(GameObject gameObject) {
        Return(gameObject);
    }
}
