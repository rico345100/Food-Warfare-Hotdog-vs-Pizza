using UnityEngine;

public static class Utils {
    public static void RemoveAllChildren(Transform container) {
        foreach(Transform child in container) {
            Object.Destroy(child.gameObject);
        }
    }    
}
