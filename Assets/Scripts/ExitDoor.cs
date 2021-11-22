using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    public GameObject exit_signifier;
    bool active = false;

    void Update() {
        if (!active) return;
        if (Input.GetButtonDown("Enter")) {
            // Save the score
            int score = Score.GetScore();
            int high_score = 0;
            if (PlayerPrefs.HasKey("HighScore")) {
                high_score = PlayerPrefs.GetInt("HighScore");
            }

            PlayerPrefs.SetInt("Score", score);

            if (score > high_score) {
                PlayerPrefs.SetInt("HighScore", score);
            }

            SceneManager.LoadScene(3);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        exit_signifier.SetActive(true);
        active = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        exit_signifier.SetActive(false);
        active = false;
    }
}
