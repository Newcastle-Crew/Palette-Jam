using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Score : MonoBehaviour
{
    static Score Instance = null;

    public GameObject score_animation_prefab;
    public float score_animation_time = 1.05f;

    int score = 0;
    Text text;

    void Awake() {
        text = GetComponent<Text>();
        Instance = this;
        text.text = score.ToString();
    }

    void _AddScore(Vector2 pos, int addition) {
        score += addition;
        text.text = score.ToString();
        var instance = Instantiate(score_animation_prefab, pos, Quaternion.identity);
        instance.GetComponentInChildren<Text>().text = addition.ToString();
        Destroy(instance, score_animation_time);
    }

    public static void AddScore(Vector2 pos, int addition) {
        if (Instance == null) return;

        Instance._AddScore(pos, addition);
    }

}
