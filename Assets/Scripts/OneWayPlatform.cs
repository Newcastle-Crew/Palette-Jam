using System.Collections;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    public Collider2D collider_a;
    int player_layer;

    int players_inside = 0;

    void Awake() {
        player_layer = LayerMask.NameToLayer("Player");
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        Physics2D.IgnoreCollision(collider_a, other, true);
        players_inside += 1;
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;
        players_inside -= 1;

        StartCoroutine("Ignore", other);
    }

    IEnumerator Ignore(Collider2D other) {
        yield return new WaitForSeconds(0.1f);

        // Deals with the edge case of the player re-entering the trigger within 0.1 seconds
        // of exiting, which would cause the IgnoreCollision to be reset while they're still inside.
        if (players_inside == 0) {
            Physics2D.IgnoreCollision(collider_a, other, false);
        }
    }
}
