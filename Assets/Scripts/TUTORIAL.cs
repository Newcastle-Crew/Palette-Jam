#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class TUTORIAL : MonoBehaviour
{
    public Image image;
    public bool tutorialShowing;

    private void Start()
    {
        image.enabled = true;
        tutorialShowing = true;
    }

    private void Update()
    {
        if (tutorialShowing && Input.anyKey) // Checks for any keyboard input while tutorial is showing.
        {
            HideTutorial();
        }
    }

    public void HideTutorial()
    {
        Debug.Log("Something has been pressed! Now hide the tutorial!");

        tutorialShowing = false;
        image.enabled = !image.enabled;
    }
}
