using System.Collections;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    Collider2D blocking_collider = null;
    Collider2D trigger_collider = null;
    int player_layer;

    bool ignoring = false;

    void Awake() {
        player_layer = LayerMask.NameToLayer("Player");

        foreach(var v in GetComponents<Collider2D>()) {
            if (v.isTrigger) {
                trigger_collider = v;
            } else {
                blocking_collider = v;
            }
        }

        Debug.Assert(blocking_collider != null);
        Debug.Assert(trigger_collider != null);
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (Physics2D.IsTouching(collision.collider, trigger_collider)) {
            if (!ignoring) {
                Physics2D.IgnoreCollision(blocking_collider, collision.collider, true);
                ignoring = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        // If they are already touching, e.g. the collider and the trigger got activated at the same time,
        // we don't disable it.
        if (!blocking_collider.bounds.Intersects(other.bounds)) {
            Physics2D.IgnoreCollision(blocking_collider, other, true);
            ignoring = true;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer != player_layer)  return;

        if (ignoring) {
            Physics2D.IgnoreCollision(blocking_collider, other, false);
            ignoring = false;
        }
    }
}
