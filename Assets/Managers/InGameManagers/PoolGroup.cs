using System.Collections.Generic;
using UnityEngine;

namespace ObjectPooling {
    [System.Serializable]
    public class PoolGroup {
        public string name;
        public GameObject targetPrefab;
        public int defaultSize = 10;
        public int extendSize = 10;
        [HideInInspector] public List<GameObject> objectList = new List<GameObject>();
    }
}
