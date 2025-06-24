using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartHotkey : MonoBehaviour {
    void Update() {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl && Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(0); // Restart the whole game (scene 0)
        }

        if (ctrl && Input.GetKeyDown(KeyCode.T)) {
            SceneManager.LoadScene(1); // Restart gameplay (scene 1)
        }
    }
}
