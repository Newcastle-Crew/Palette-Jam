using UnityEngine;

public class DamageableSection : MonoBehaviour
{
    public Breakable breakable;

    // Static defense is to prevent small amounts of damage to pile up. It's a minimum amount that is subtracted from the magnitude of the velocity.
    public float static_defense = 0.2f;
    
    public float defense = 1.0f;

    void Awake()
    {
        breakable = GetComponentInParent<Breakable>();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        var other_breakable = collision.collider.GetComponentInParent<Breakable>();
        if (other_breakable != null && other_breakable.combo_counter != -1) {
            if (breakable.combo_counter == -1) {
                breakable.combo_counter = other_breakable.combo_counter + 1;
            } else {
                breakable.combo_counter = Mathf.Min(breakable.combo_counter, other_breakable.combo_counter + 1);
            }
        }

        var other_hardness = collision.collider.GetComponentInParent<Hardness>();
        float hardness = 1f;
        if (other_hardness != null) hardness = other_hardness.hardness;

        float impulse_force = 0f;
        for (int i = 0; i < collision.contactCount; i++) {
            var contact = collision.GetContact(i);
            impulse_force += contact.normalImpulse;
        }

        float damage = impulse_force * hardness;
        if (damage < static_defense) return;

        breakable.Damage(damage);
    }
}
