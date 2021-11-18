using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catnip : MonoBehaviour
{
    public float SpeedUpgrade = 2f;
    public float JumpUpgrade = 1.5f;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            var animator = GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger("collected");
            } else {
                Destroy(gameObject);
            }
        }
    }
}
