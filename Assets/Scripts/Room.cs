using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    public BoxCollider2D box;

    void Awake() {
        box = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        CameraControl.AddRoomBound(this);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        CameraControl.RemoveRoomBound(this);
    }
}
