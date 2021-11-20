using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    [System.NonSerialized]
    public BoxCollider2D box;

    void Awake() {
        box = GetComponent<BoxCollider2D>();
    }

    void OnDestroy() {
        CameraControl.RemoveRoomBound(this);
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
