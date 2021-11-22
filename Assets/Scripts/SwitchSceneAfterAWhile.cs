using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchSceneAfterAWhile : MonoBehaviour
{
    public float time = 3f;
    public string new_scene;

    void Start() {
        StartCoroutine("Switch");
    }

    IEnumerator Switch() {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(new_scene);
    }
}
