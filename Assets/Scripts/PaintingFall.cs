using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HingeJoint2D))]
public class PaintingFall : MonoBehaviour
{
    HingeJoint2D hinge;
    public float fall_delay = 1.5f;

    void Awake() {
        hinge = GetComponent<HingeJoint2D>();
    }

    IEnumerator FallInternal() {
        yield return new WaitForSeconds(fall_delay);
        hinge.enabled = false;
    }

    public void Fall() {
        StartCoroutine("FallInternal");
    }
}
