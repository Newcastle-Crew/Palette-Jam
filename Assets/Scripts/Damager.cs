using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Breakable))]
public class Damager : MonoBehaviour
{
    public float damage = 4f;
    public float force = 2f;

    // Update is called once per frame
    void OnCollisionEnter2D(Collision2D collision) {
        var player = collision.collider.gameObject.GetComponent<PlayerControl>();
        if (player != null) {
            player.Damage(damage);
            player.rb2d.AddForce(GetComponent<Rigidbody2D>().velocity.normalized * force, ForceMode2D.Impulse);
            GetComponent<Breakable>().BreakImmediate();
        }
    }
}
