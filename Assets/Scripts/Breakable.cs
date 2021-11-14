using UnityEngine;

public class Breakable : MonoBehaviour
{
    public float health = 1f;

    public void Damage(float damage) {
        health -= damage;
        
        if (health < 0f) {
            GameObject.Destroy(this.gameObject, 2.0f);
        }
    }
}
