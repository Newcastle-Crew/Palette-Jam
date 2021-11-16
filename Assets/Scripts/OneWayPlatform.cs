using System.Collections;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    public Collider2D collider_a;
    int player_layer;

    void Awake() {
        player_layer = LayerMask.NameToLayer("Player");
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        Physics2D.IgnoreCollision(collider_a, other, true);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        StartCoroutine("Ignore", other);
    }

    IEnumerator Ignore(Collider2D other) {
        yield return new WaitForSeconds(0.1f);
        Physics2D.IgnoreCollision(collider_a, other, false);
    }
}
