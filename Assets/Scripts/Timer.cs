#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#endregion

public class Timer : MonoBehaviour
{
    float countdown = 180.0f; // Counts down for 3 minutes.
    public Text timerUI;

    void Update()
    {
        if (countdown > 0)
        { countdown -= Time.deltaTime; }


        double b = System.Math.Round(countdown, 2);
        timerUI.text = b.ToString();
        if (countdown < 0)
        {
            Debug.Log("Completed");
            SceneManager.LoadScene(sceneBuildIndex: 0);
        }
    }
}