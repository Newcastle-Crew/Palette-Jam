using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
public class Boss : MonoBehaviour
{
    struct Bark {
        public int index;
        public float time;
        public float wanted_time;
        public float force;
    }

    enum Plan {
        Bark,
        Zoom,
    }

    enum Action {
        Bark,
        Move,
        Bite,
        Tired,
        Charge,
        Default,
    }

    [System.NonSerialized]
    public Health health;
    Rigidbody2D rb2d;
    Animator animator;
    SpriteRenderer sprite;

    [System.NonSerialized]
    public BossStartTrigger start_trigger;

    public Vector2 wanted_position;
    public float acceptable_pos_error = 0.4f;
    public float movement_speed = 2000f;

    public float action_timer = 0f;
    Action action;
    
    // Charge
    public int num_charges = 5;
    public float charge_delay = 0.2f;
    public float charge_backup = -100f;
    public float charge_speed = 6000f;
    public float charge_min_distance = 5f;
    public float charge_friction = 0.5f;
    public SoundEffect charge_sound;
    int charge_counter = 0;

    public float between_charge_tired_time = 1f;
    public float tired_wakeup_time_after_attack = 0.3f;
    public float tired_wakeup_signalling_time = 0.3f;
    public float tired_wakeup_jump = 0.6f;
    bool tired_interruptable = false;

    // Bite variables
    public float bite_rebite_time = 0.3f;
    public float bite_time = 1f;
    public float bite_jump_strength = 100f;
    public float bite_attack_delay = 0.3f;
    public float bite_undirected_jump_strength = 5f;
    public float bite_damage = 6f;
    public SoundEffect bite_sound;

    // Scratch variables
    public float scratch_force = 10f;
    public Transform scratch_pos;
    public float scratch_radius = 0.4f;
    public float ground_friction = 23f;
    Transform in_scratch_region = null;

    // Bark
    public Transform[] bark_target_points;
    public Rigidbody2D vertical_bark_prefab;
    public Rigidbody2D horizontal_bark_prefab;
    public Vector2 bark_angle = Vector2.right + Vector2.up;
    public float bark_buildup_time = 0.4f;
    public float bark_end_time = 1f;
    public float bark_speed = 0.1f;
    public float horizontal_bark_speed = 0.1f;
    public float horizontal_bark_buildup_time = 0.3f;
    public SoundEffect vertical_bark_sound;
    public SoundEffect horizontal_bark_sound;
    public float bark_random_time = 0.4f;
    public int bark_size = 3 * 10;
    public int bark_simultaneous = 3;
    Bark[] bark_places;
    float local_bark_end_time = 0f;

    Plan plan = Plan.Bark;

    public SoundEffect damaged_sound;
    public SoundEffect death_sound;

    bool right = true;
    List<Collider2D> scratch_results = new List<Collider2D>();
    ContactFilter2D scratch_contact_filter;

    // Ground checking stuff
    public bool on_ground = false;
    public Vector2 ground_tilt = Vector2.up;
    ContactPoint2D[] contacts;

    void Awake()
    {
        bark_places = new Bark[bark_size];
        rb2d = GetComponent<Rigidbody2D>();
        wanted_position = rb2d.position;
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        contacts = new ContactPoint2D[4];

        scratch_contact_filter = new ContactFilter2D();
        scratch_contact_filter.layerMask = LayerMask.GetMask("Player");
        scratch_contact_filter.useLayerMask = true;

        BeginTired(false, 0.5f);
    }

    void FixedUpdate()
    {
        // @Duplicate: This is roughly the same as in PlayerControl, but as we only have one type of boss and one type of player, it doesn't really matter...
        // Figure out if we're on the ground or not. This is done by going through all of the contact points on the rigidbody,
        // and trying to find a contact point that is vertical. If you can do that, it means we're right on top of the rigidbody.
        // If it's slanted, it means that we're on a slope, and thus, if the slope is sloped enough, we may be currently slipping
        // off. Could be used for feeding into an animation.
        int num_contacts = rb2d.GetContacts(contacts);
        Vector2 best = Vector2.zero;
        float best_angle_diff = 0f;
        for (int i = 0; i < num_contacts; i++) {
            var contact_normal = contacts[i].normal;
            var angle_diff = Vector2.Dot(contact_normal, Vector2.up);
            if (angle_diff > best_angle_diff) {
                best = contact_normal;
                best_angle_diff = angle_diff;
            }
        }
        var old_on_ground = on_ground;
        on_ground = best_angle_diff > 0.8f;

        var old_action_timer = action_timer;
        action_timer += Time.fixedDeltaTime;

        if (in_scratch_region != null && !(action == Action.Bite || action == Action.Tired)) {
            Bite();
        }

        switch (action) {
            case Action.Tired:
                if (old_action_timer < -tired_wakeup_signalling_time && action_timer >= -tired_wakeup_signalling_time) {
                    animator.SetTrigger("tired_wake_up");
                    rb2d.AddForce(Vector2.up * tired_wakeup_jump, ForceMode2D.Impulse);
                }

                if (action_timer >= 0f) {
                    ResumeDefaultAction();
                }
                break;
            case Action.Bite:
                if (old_action_timer < bite_attack_delay && action_timer >= bite_attack_delay) {
                    // Do the actual attack
                    Vector2 maybe_flip_horizontal(Vector2 input, bool flip) {
                        return new Vector2(flip ? -input.x : input.x, input.y);
                    }

                    // Find a slashable object
                    var local = maybe_flip_horizontal(scratch_pos.localPosition, !right);
                    var num_overlaps = Physics2D.OverlapCircle((Vector2)transform.position + local, scratch_radius, scratch_contact_filter, scratch_results);
                    for (int i = 0; i < num_overlaps; i++) {
                        var overlapping_rb2d = scratch_results[i].GetComponentInParent<Rigidbody2D>();
                        if (overlapping_rb2d != null) {
                            overlapping_rb2d.velocity = (Vector2.up * 0.7f + (right ? Vector2.right : Vector2.left)) * scratch_force;
                        }

                        var player = scratch_results[i].GetComponentInParent<PlayerControl>();
                        if (player != null) {
                            player.Damage(bite_damage);
                        }
                    }
                }

                if (in_scratch_region != null && action_timer >= bite_rebite_time) {
                    Bite();
                }

                if (action_timer >= bite_time) {
                    ResumeDefaultAction();
                }
                break;
            case Action.Move: {
                var position_error = wanted_position - rb2d.position;
                if (position_error.sqrMagnitude > acceptable_pos_error * acceptable_pos_error) {
                    SetOrientation(position_error.x > 0f);
                    rb2d.velocity = new Vector2(
                        rb2d.velocity.x + Mathf.Sign(position_error.x) * movement_speed * Time.fixedDeltaTime * Time.fixedDeltaTime,
                        rb2d.velocity.y
                    );
                } else {
                    ResumeDefaultAction();
                }
                break;
            }
            case Action.Charge: {
                var position_error = wanted_position - rb2d.position;
                if (position_error.sqrMagnitude > acceptable_pos_error * acceptable_pos_error) {
                    SetOrientation(position_error.x > 0f);
                    rb2d.velocity = new Vector2(
                        rb2d.velocity.x + Mathf.Sign(position_error.x) * (action_timer >= charge_delay ? charge_speed : charge_backup) * Time.fixedDeltaTime * Time.fixedDeltaTime,
                        rb2d.velocity.y
                    );
                } else {
                    BeginTired(true, between_charge_tired_time);
                }
                break;
            }
            case Action.Default:
                switch (plan) {
                    case Plan.Zoom:
                        if(charge_counter >= num_charges) {
                            BeginBark();
                            break;
                        }

                        charge_counter += 1;
                        ChargeToRandomSpot();

                        break;
                    case Plan.Bark:
                        for (int i = 0; i < bark_places.Length; i++) {
                            var wanted_time = bark_places[i].wanted_time;
                            if (old_action_timer < wanted_time && action_timer >= wanted_time) {
                                vertical_bark_sound.Play();
                                animator.SetTrigger("vertical_bark");
                                var vel = new Vector2(right ? bark_angle.x : -bark_angle.x, bark_angle.y).normalized;

                                var angle = Mathf.Rad2Deg * Mathf.Atan2(vel.y, vel.x);
                                var instance = Instantiate(vertical_bark_prefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, right ? angle : angle + 180f)));
                                instance.velocity = vel * bark_places[i].force;
                                instance.angularVelocity = right ? (-2f * angle / bark_places[i].time) : (2f * (180f - angle) / bark_places[i].time);
                            }
                        }

                        var horizontal_bark_time = (float)(bark_places.Length / bark_simultaneous) * bark_speed;
                        if (old_action_timer < horizontal_bark_time && action_timer >= horizontal_bark_time) {
                            horizontal_bark_sound.Play();
                            animator.SetTrigger("horizontal_bark_buildup");
                        }

                        if (old_action_timer < horizontal_bark_time + horizontal_bark_buildup_time && action_timer >= horizontal_bark_time + horizontal_bark_buildup_time) {
                            animator.SetTrigger("horizontal_bark");
                            var instance = Instantiate(horizontal_bark_prefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
                            instance.velocity = (right ? Vector2.right : Vector2.left) * horizontal_bark_speed;
                        }

                        if (action_timer >= local_bark_end_time + bark_end_time) {
                            BeginCharge();
                        }
                        
                        break;
                }
                break;
        }

        if (on_ground) {
            // Friction
            rb2d.velocity = new Vector2(rb2d.velocity.x * Mathf.Exp(-(action == Action.Charge ? charge_friction : ground_friction) * Time.fixedDeltaTime), rb2d.velocity.y);
        }
    }

    public void Damage(float damage) {
        damaged_sound.Play();
        health.health -= damage;

        if (health.health < 0f) {
            Die();
        } else {
            if (action == Action.Tired && tired_interruptable) {
                action_timer = Mathf.Max(action_timer, -tired_wakeup_time_after_attack);
            }
        }
    }

    void Die() {
        death_sound.Play();
        start_trigger.Win();

        rb2d.drag = 1.6f;
        animator.SetTrigger("dead");
        Destroy(this);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            in_scratch_region = other.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            in_scratch_region = null;
        }
    }

    void BeginTired(bool interruptable, float time) {
        action = Action.Tired;
        action_timer = -time;
        tired_interruptable = interruptable;

        animator.SetTrigger("tired");
    }

    void ResumeDefaultAction() {
        action = Action.Default;
        action_timer = 0f;

        switch (plan) {
            case Plan.Bark:
                var right_i = bark_target_points.Length - 1;
                var target_i = 
                      ((Vector2)bark_target_points[0      ].position - rb2d.position).sqrMagnitude
                    < ((Vector2)bark_target_points[right_i].position - rb2d.position).sqrMagnitude
                ? 0 : right_i;
                var target = (Vector2)bark_target_points[target_i].position;

                if (!MoveTo(target)) break;

                animator.SetTrigger("barking");

                // If we're on the left side, face right, otherwise, face left
                SetOrientation(target_i == 0);

                float min_time = 10000f;
                local_bark_end_time = -10000f;
                for (int i = 0; i < bark_places.Length; i++) {
                    var last_full_set = (i / bark_simultaneous) * bark_simultaneous;
                    var num_picks = last_full_set + (bark_target_points.Length - 1) - i;
                    var pick = Random.Range(0, num_picks);

                    if (pick >= target_i) pick += 1;
                    for (int j = last_full_set; j < i; j++) {
                        if (pick >= bark_places[j].index) pick += 1;
                    }

                    bark_places[i].index = pick;

                    var target_pos = (Vector2)bark_target_points[pick].position - rb2d.position;
                    var g = Physics2D.gravity.y * vertical_bark_prefab.gravityScale;

                    var vel = new Vector2(right ? bark_angle.x : -bark_angle.x, bark_angle.y).normalized;

                    var inner = -2f * (target_pos.x * vel.y / vel.x - target_pos.y) / g;
                    if (inner < 0f) {
                        Debug.LogError("No valid bark strength to position");
                        continue;
                    }

                    var t = Mathf.Sqrt(inner);

                    var wanted_end_time = (float)(i / bark_simultaneous) * bark_speed + Random.Range(0f, bark_random_time);
                    var wanted_time = wanted_end_time - t;
                    min_time = Mathf.Min(wanted_time, min_time);
                    local_bark_end_time = Mathf.Max(local_bark_end_time, wanted_end_time);

                    bark_places[i].wanted_time = wanted_time;
                    bark_places[i].time = t;
                    bark_places[i].force = target_pos.x / (vel.x * t);;
                }

                action_timer = min_time - bark_buildup_time;

                break;
        }
    }

    void BeginBark() {
        plan = Plan.Bark;
        ResumeDefaultAction();
    }

    void BeginCharge() {
        plan = Plan.Zoom;

        charge_counter = 0;
        ChargeToRandomSpot();
    }

    void ChargeToRandomSpot() {
        charge_sound.Play();
        int num = 0;
        for (int i = 0; i < bark_target_points.Length; i++) {
            if (i >= 2 && i < bark_target_points.Length - 2) continue;

            if (((Vector2)bark_target_points[i].position - rb2d.position).sqrMagnitude >= charge_min_distance * charge_min_distance) {
                num += 1;
            }
        }

        int picked = Random.Range(0, num);
        for (int i = 0; i <= picked; i++) {
            if (i >= 2 && i < bark_target_points.Length - 2) { picked += 1; continue; }
            if (((Vector2)bark_target_points[i].position - rb2d.position).sqrMagnitude < charge_min_distance * charge_min_distance) {
                picked += 1;
            }
        }

        animator.SetTrigger("zooming");
        action_timer = 0f;
        wanted_position = (Vector2)bark_target_points[picked].position;
        action = Action.Charge;
    }

    // Moves to a target, returns true if we're already there.
    bool MoveTo(Vector2 position) {
        if ((position - rb2d.position).sqrMagnitude > acceptable_pos_error * acceptable_pos_error) {
            animator.SetTrigger("moving");
            wanted_position = position;
            action = Action.Move;
            return false;
        } else {
            return true;
        }
    }

    void Bite() {
        bite_sound.Play();
        animator.SetTrigger("slash");

        // Bite!
        var error = (Vector2)in_scratch_region.position - rb2d.position;
        SetOrientation(error.x > 0f);
        rb2d.AddForce(error.normalized * bite_jump_strength + Vector2.up * bite_undirected_jump_strength, ForceMode2D.Impulse);
        action = Action.Bite;
        action_timer = 0f;
    }

    void SetOrientation(bool right) {
        this.right = right;
        sprite.flipX = !right;
    }
}
