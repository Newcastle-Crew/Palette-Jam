using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    Collider2D blocking_collider = null;
    Collider2D trigger_collider = null;

    void Awake() {
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
            Physics2D.IgnoreCollision(blocking_collider, collision.collider, true);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        // If they are already touching, e.g. the collider and the trigger got activated at the same time,
        // we don't disable it.
        if (!blocking_collider.bounds.Intersects(other.bounds)) {
            Physics2D.IgnoreCollision(blocking_collider, other, true);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        Physics2D.IgnoreCollision(blocking_collider, other, false);
    }
}
