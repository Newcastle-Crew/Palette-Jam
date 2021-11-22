using UnityEngine;
using UnityEngine.UI;

public class ShowHighScore : MonoBehaviour
{
    public GameObject high_score;
    public Text high_score_text;

    public GameObject last_score;
    public Text last_score_text;

    void Start()
    {
        int high_score = 0;
        if (PlayerPrefs.HasKey("HighScore")) {
            high_score = PlayerPrefs.GetInt("HighScore");
        }
        high_score_text.text = high_score.ToString();

        if (PlayerPrefs.HasKey("Score")) {
            last_score.SetActive(true);
            last_score_text.text = PlayerPrefs.GetInt("Score").ToString();
        }
    }
}
