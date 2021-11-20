using System.Collections;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    public Collider2D collider_a;
    int player_layer;

    int players_inside = 0;
    bool ignoring = false;

    void Awake() {
        player_layer = LayerMask.NameToLayer("Player");
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        // If they are already touching, e.g. the collider and the trigger got activated at the same time,
        // we don't disable it.
        if (!collider_a.bounds.Intersects(other.bounds)) {
            Physics2D.IgnoreCollision(collider_a, other, true);
            ignoring = true;
        }

        players_inside += 1;
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;
        players_inside -= 1;

        if (ignoring) {
            StartCoroutine("Ignore", other);
        }
    }

    IEnumerator Ignore(Collider2D other) {
        yield return new WaitForSeconds(0.1f);

        // Deals with the edge case of the player re-entering the trigger within 0.1 seconds
        // of exiting, which would cause the IgnoreCollision to be reset while they're still inside.
        if (players_inside == 0) {
            Physics2D.IgnoreCollision(collider_a, other, false);
            ignoring = false;
        }
    }
}
