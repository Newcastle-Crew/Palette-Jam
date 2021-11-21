#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#endregion

public class Timer : MonoBehaviour
{
    static Timer instance;
    float countdown = 183.0f; // Counts down for 3 minutes and 3 seconds.
    static bool active;
    public Text timerUI;

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

        string minutes = ((int)countdown / 60).ToString();
        string seconds = (countdown % 60).ToString("00");

        double b = System.Math.Round(countdown, 2);
        timerUI.text = minutes + ":" + seconds;

        if (countdown < 0) // Sends you back to the main menu when the timer hits 0.
        { SceneManager.LoadScene(sceneBuildIndex: 0); }
    }
}