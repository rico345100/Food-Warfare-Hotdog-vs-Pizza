using UnityEngine;
using UnityEngine.SceneManagement;

public class BootScene : MonoBehaviour {
    void Awake() {
        // Do some task must be done before game starts:
        SceneManager.LoadScene("Start");
    }
}
