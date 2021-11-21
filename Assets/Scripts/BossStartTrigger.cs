using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    public GameObject boss;
    public int required_score = 30;
    public float fade_speed = 1f;

    public GameObject[] disable_when_boss;
    public GameObject[] enable_when_boss;
    public GameObject[] disable_after_boss;
    public GameObject[] enable_after_boss;

    void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponent<PlayerControl>();
        if (player == null) return;
        if (Score.GetScore() < required_score) return;

        Timer.Disable();
        boss.SetActive(true);
        boss.GetComponent<Boss>().start_trigger = this;
        foreach(var obj in disable_when_boss) if (obj != null) obj.SetActive(false);
        foreach(var obj in enable_when_boss) if (obj != null) obj.SetActive(true);
    }

    public void Win() {
        foreach(var obj in disable_after_boss) if (obj != null) obj.SetActive(false);
        foreach(var obj in enable_after_boss) if (obj != null) obj.SetActive(true);
    }
}
