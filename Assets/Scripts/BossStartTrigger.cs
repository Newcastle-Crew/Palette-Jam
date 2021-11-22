using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    public GameObject boss;
    public int required_score = 30;
    public float fade_speed = 1f;
    public GameObject fragment_prefab;
    bool started = false;

    public GameObject[] disable_when_boss;
    public GameObject[] enable_when_boss;
    public GameObject[] break_when_boss;
    public GameObject[] disable_after_boss;
    public GameObject[] enable_after_boss;

    void OnTriggerEnter2D(Collider2D other) {
        if (started) return;
        var player = other.GetComponent<PlayerControl>();
        if (player == null) return;
        if (Score.GetScore() < required_score) return;

        started = true;

        MusicController.StartBossMusic();
        Timer.Disable();
        boss.SetActive(true);
        boss.GetComponent<Boss>().start_trigger = this;
        foreach(var obj in disable_when_boss) if (obj != null) obj.SetActive(false);
        foreach(var obj in enable_when_boss) if (obj != null) obj.SetActive(true);

        foreach(var obj in break_when_boss) {
            if (obj == null) continue;

            var breakable = obj.GetComponent<Breakable>();
            if (breakable == null) breakable = obj.AddComponent<Breakable>();
            breakable.score = 0;
            breakable.combo_counter = 0;
            breakable.fragmentPrefab = fragment_prefab;
            breakable.BreakImmediate();
        }
    }

    public void Win() {
        foreach(var obj in disable_after_boss) if (obj != null) obj.SetActive(false);
        foreach(var obj in enable_after_boss) if (obj != null) obj.SetActive(true);
    }
}
