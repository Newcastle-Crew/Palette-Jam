using UnityEngine;

public class DamageableSection : MonoBehaviour
{
    Breakable breakable;

    // Static defense is to prevent small amounts of damage to pile up. It's a minimum amount that is subtracted from the magnitude of the velocity.
    public float static_defense = 0.2f;
    
    public float defense = 1.0f;

    void Awake()
    {
        breakable = GetComponentInParent<Breakable>();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        var other_hardness = collision.collider.GetComponentInParent<Hardness>();
        if (other_hardness == null) return;
        if (breakable.gameObject == other_hardness.gameObject) return;

        float impulse_force = 0f;
        for (int i = 0; i < collision.contactCount; i++) {
            var contact = collision.GetContact(i);
            impulse_force += contact.normalImpulse;
        }

        float damage = impulse_force * other_hardness.hardness;
        if (damage < static_defense) return;

        Debug.Log(collision.collider.gameObject.name);

        breakable.Damage(damage);
    }
}
