using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class PlayerControl : MonoBehaviour
{
    public static PlayerControl Instance = null;

    public float speed = 1.0f;
    public float catnip_speed = 1.5f;
    public float air_speed = 0.02f;
    public float catnip_air_speed = 10f;
    public float air_speed_ineffectiveness_factor = 0.4f;
    public float ground_friction = 0.4f;
    public float normal_jump_gravity_scale = 1.6f;
    public float during_jump_gravity_scale = 0.6f;
    public float max_big_jump_time = 0.4f;

    public float weak_scratch_force = 0.5f;
    public float catnip_scratch_force = 1.5f;
    public float scratch_damage = 5f;
    public float catnip_scratch_damage = 10f;
    public float scratch_rate = 0.3f;

    public float bounce_jump_percent = 0.5f;
    public float bounce_strength = 0.5f;
    public int max_bounces = 3;
    public int bounces = 0;

    // The point in time where a failed jump happened because you weren't on the ground.
    // Used so that if you land just a fraction of a second after you failed a jump, it just jumps anyway.
    float failed_jump_t = -100f;
    public float jump_grace_period = 0.2f;

    bool right = true;
    public Transform scratch_pos;

    float low_grav_jump_timer = 0f;
    float last_jump_t = -100f;

    public float jump_strength = 40f;
    public float catnip_jump_strength = 50f;

    bool on_ground = false;
    bool holding_jump = false;

    float catnip_time = -1f;
    float scratch_time = -100f;

    Vector2 ground_tilt = Vector2.up;

    int bouncy_layer;
    int boss_layer;

    [System.NonSerialized]
    public Health health;
    [System.NonSerialized]
    public Rigidbody2D rb2d;
    Animator animator;

    ContactPoint2D[] contacts;
    List<Collider2D> scratch_results;
    ContactFilter2D scratch_contact_filter;

    void Awake()
    {
        contacts = new ContactPoint2D[4];
        scratch_results = new List<Collider2D>();

        scratch_contact_filter = new ContactFilter2D();
        scratch_contact_filter.layerMask = LayerMask.GetMask("Pushable", "PushableBackground", "Boss");
        scratch_contact_filter.useLayerMask = true;

        bouncy_layer = LayerMask.NameToLayer("Bouncy");
        boss_layer = LayerMask.NameToLayer("Boss");
        health = GetComponent<Health>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb2d.gravityScale = normal_jump_gravity_scale;
        Instance = this;
    }

    public void ActivateCatnip(float time) {
        // TODO: Speed up music as well.

        if (this.catnip_time < 0f) {
            this.catnip_time = time;
        } else {
            this.catnip_time += time;
        }
    }

    void Jump(float percent = 1f) {
        last_jump_t = Time.time;
        // NOTE: Tilted jumps right now have less strength, this may be confusing if we don't have a graphic for it.
        rb2d.AddForce(Vector2.up * Vector2.Dot(ground_tilt, Vector2.up) * (catnip_time > 0f ? catnip_jump_strength : jump_strength) * percent, ForceMode2D.Impulse);

        on_ground = false;
        holding_jump = true;
        animator.SetBool("on_ground", false);
        animator.SetBool("jumping", true);
        rb2d.gravityScale = during_jump_gravity_scale;
        low_grav_jump_timer = max_big_jump_time;
    }

    public void Damage(float damage) {
        this.health.health -= damage;

        if (this.health.health <= 0f) {
            Die();
        } else {
            animator.SetTrigger("damaged");
        }
    }
    
    void Die() {
        // Destroy just the player controller
        Destroy(this);

        // We no longer do manual friction, so make sure it has some amount of friction.
        rb2d.drag = 0.3f;

        animator.SetTrigger("death");
    }

    void Update() {
        // We're on somewhat stable/straight ground, therefore we can jump!
        if (Input.GetButtonDown("Jump")) {
            if (on_ground) {
                Jump();
            } else {
                failed_jump_t = Time.time;
            }
        }

        // "Slash"
        if ((Time.time - scratch_time) >= scratch_rate && Input.GetButtonDown("Action")) {
            scratch_time = Time.time;

            Vector2 maybe_flip_horizontal(Vector2 input, bool flip) {
                return new Vector2(flip ? -input.x : input.x, input.y);
            }

            animator.SetTrigger("weak_slash");

            // Find a slashable object
            var local = maybe_flip_horizontal(scratch_pos.localPosition, !right);
            var num_overlaps = Physics2D.OverlapCircle((Vector2)transform.position + local, .5f, scratch_contact_filter, scratch_results);
            for (int i = 0; i < num_overlaps; i++) {
                var painting_fall = scratch_results[i].GetComponentInParent<PaintingFall>();
                if (painting_fall != null) {
                    painting_fall.Fall();
                }

                var boss = scratch_results[i].GetComponent<Boss>();
                if (boss != null) {
                    boss.Damage(catnip_time > 0f ? catnip_scratch_damage : scratch_damage);
                }

                var breakable = scratch_results[i].GetComponentInParent<Breakable>();
                if (breakable != null) {
                    breakable.combo_counter = 0;
                }

                var overlapping_rb2d = scratch_results[i].GetComponentInParent<Rigidbody2D>();
                if (overlapping_rb2d != null) {
                    Debug.Log("Slashing!!");
                    overlapping_rb2d.AddForceAtPosition(
                        (right ? Vector2.right : Vector2.left) * (catnip_time > 0f ? catnip_scratch_force : weak_scratch_force),
                        (Vector2)transform.position + local,
                        ForceMode2D.Impulse
                    );
                }
            }
        }
    }

    void FixedUpdate() {
        this.catnip_time -= Time.fixedDeltaTime;

        if (holding_jump) {
            low_grav_jump_timer -= Time.fixedDeltaTime;
            if (!Input.GetButton("Jump") || rb2d.velocity.y < 0f || low_grav_jump_timer < 0f) {
                holding_jump = false;
                animator.SetBool("jumping", false);
                rb2d.gravityScale = normal_jump_gravity_scale;
            }
        }

        // Figure out if we're on the ground or not. This is done by going through all of the contact points on the rigidbody,
        // and trying to find a contact point that is vertical. If you can do that, it means we're right on top of the rigidbody.
        // If it's slanted, it means that we're on a slope, and thus, if the slope is sloped enough, we may be currently slipping
        // off. Could be used for feeding into an animation.
        int num_contacts = rb2d.GetContacts(contacts);
        Vector2 best = Vector2.zero;
        float best_angle_diff = 0f;
        bool bouncy = false;
        for (int i = 0; i < num_contacts; i++) {
            var contact_normal = contacts[i].normal;
            var angle_diff = Vector2.Dot(contact_normal, Vector2.up);
            if (angle_diff > best_angle_diff) {
                best = contact_normal;
                best_angle_diff = angle_diff;
            }

            if (contacts[i].collider.gameObject.layer == bouncy_layer || contacts[i].collider.gameObject.layer == boss_layer) {
                bouncy = true;
            }
        }

        var old_on_ground = on_ground;

        on_ground = best_angle_diff > 0.8f;
        if ((Time.time - last_jump_t) < 0.1f) {
            on_ground = false;
        }

        ground_tilt = best;
        if (old_on_ground != on_ground) {
            if (on_ground) {
                if (bouncy) {
                    // Super jump
                    rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                    if (Input.GetButton("Jump")) {
                        bounces += 1;
                        bounces = Mathf.Min(bounces, max_bounces);
                    } else {
                        bounces -= 1;
                        bounces = Mathf.Max(bounces, 0);
                    }

                    Jump(bounces * bounce_strength + bounce_jump_percent);
                    failed_jump_t = -100f;
                } else {
                    bounces = 0;

                    if (Input.GetButton("Jump") && (Time.time - failed_jump_t) <= jump_grace_period) {
                        rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                        Jump();
                        failed_jump_t = -100f;
                    }
                }
            }

            animator.SetBool("on_ground", on_ground);
        }

        if (on_ground) {
            rb2d.velocity = new Vector2(rb2d.velocity.x * Mathf.Exp(-ground_friction * Time.fixedDeltaTime), rb2d.velocity.y);
        }

        var horizontal = Input.GetAxisRaw("Horizontal");

        var ground_controls = on_ground;
        if (ground_controls) {
            // @Performance: We could only update this if you pressed the axis recently.
            animator.SetBool("running", Mathf.Abs(horizontal) > 0.1f);
            if (horizontal < 0f) {
                right = false;
                transform.localScale = new Vector3(-1f, 1f, 1f);
            } else if (horizontal > 0f) {
                right = true;
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        rb2d.velocity = new Vector2(
            rb2d.velocity.x + (horizontal * (
                ground_controls ?
                    (catnip_time > 0f ? catnip_speed : speed) :
                    ((catnip_time > 0f ? catnip_air_speed : air_speed) / (1f + rb2d.velocity.x * rb2d.velocity.x * air_speed_ineffectiveness_factor))
            )) * Time.fixedDeltaTime * Time.fixedDeltaTime,
            rb2d.velocity.y
        );
    }
}
