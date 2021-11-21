using UnityEngine;

public class Health : MonoBehaviour
{
    public GameObject health_bar;
    public Transform health_bar_blocker;
    public float health_bar_pixels = 1f;

    public bool hidden = false;

    float _health = 0f;
    public float health {
        get { return _health; }
        set {
            var local_scale = health_bar_blocker.localScale;
            local_scale.x = Mathf.Min(1f, 1f - (value / max_health)) * health_bar_pixels / 16f;
            health_bar_blocker.localScale = local_scale;

            if (_health > 0f && value <= 0f) {
                var breaking = health_bar.GetComponent<Breakable>();
                if (breaking != null) {
                    breaking.BreakImmediate();
                }
            }

            _health = value;
        }
    }

    public float max_health = 1f;

    void Awake() {
        health = max_health;
        
        if (hidden) health_bar.SetActive(false);
    }

    public void Activate() {
        health_bar.SetActive(true);
    }
}
