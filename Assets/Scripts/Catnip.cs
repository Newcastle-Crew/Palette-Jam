using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catnip : MonoBehaviour
{
    public float time = 30f;

    void OnTriggerEnter2D(Collider2D other) {
        var player_control = other.gameObject.GetComponent<PlayerControl>();

        if (player_control != null) {
            player_control.ActivateCatnip(time);

            var animator = GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger("collected");
            } else {
                Destroy(gameObject);
            }
        }
    }
}
