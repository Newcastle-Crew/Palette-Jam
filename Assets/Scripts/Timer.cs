using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    static Timer instance;
    float countdown = 155.0f;
    static bool active;
    public Text timerUI;
    public SoundEffect timeout_sound;

    void Awake() {
        instance = this;
    }

    public static void Disable() {
        if (instance == null) return;

        instance.timerUI.enabled = false;
        instance.enabled = false;
    }

    void Update()
    {
        if (countdown > 0)
        { countdown -= Time.deltaTime; }

        int displayTimer = Mathf.CeilToInt(countdown);

        string minutes = ((int)displayTimer / 60).ToString();
        string seconds = (displayTimer % 60).ToString("00");

        double b = System.Math.Round(countdown, 2);
        timerUI.text = minutes + ":" + seconds;

        if (countdown < 0) {
            StartCoroutine("LoadNewScene");
        }
    }

    IEnumerator LoadNewScene() {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(2);
    }
}