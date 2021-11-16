using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    static CameraControl Instance = null;
    static List<Room> rooms = new List<Room>();
    Camera cam;
    
    Vector2 extents = Vector2.positiveInfinity;
    Vector2 center = Vector2.zero;
    bool use_extents = false;

    public float travel_speed = 3f;

    void Awake() {
        cam = GetComponent<Camera>();
        Instance = this;

        UpdateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerControl.Instance) return;

        var target_pos = (Vector2)PlayerControl.Instance.transform.position;
        if (use_extents)
            target_pos = Vector2.Min(center + extents, Vector2.Max(center - extents, target_pos));

        var travel_speed_t = travel_speed * Time.deltaTime;
        var difference = target_pos - (Vector2)this.transform.position;
        if (difference.sqrMagnitude > travel_speed_t * travel_speed_t)
            difference = difference.normalized * travel_speed_t;
        this.transform.position += (Vector3)difference;
    }

    void UpdateBounds() {
        if (rooms.Count == 1) {
            use_extents = true;
            var bounds = rooms[0].box.bounds;
            extents = (Vector2)bounds.extents - new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
            extents = Vector2.Max(Vector2.zero, extents);
            center = (Vector2)bounds.center;
        } else if (rooms.Count == 0) {
            use_extents = false;
        }
    }

    public static void AddRoomBound(Room room) {
        rooms.Add(room);

        if (Instance == null) return;
        Instance.UpdateBounds();
    }
    
    public static void RemoveRoomBound(Room room) {
        rooms.Remove(room);

        if (Instance == null) return;
        Instance.UpdateBounds();
    }
}
